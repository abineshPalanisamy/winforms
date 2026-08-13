// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Windows.Forms;

namespace Microsoft.Extensions.WinForms;

/// <summary>
///  The default implementation of <see cref="IFormProvider"/> that creates DI-scoped
///  <see cref="Form"/> instances on the Windows Forms UI thread.
/// </summary>
/// <remarks>
///  <para>
///   Each call to <see cref="ShowForm{TForm}"/> or <see cref="ShowFormAsync{TForm}"/>
///   creates a new <see cref="IServiceScope"/>, resolves the requested form type from
///   that scope, and disposes the scope when the form is closed. A
///   <see cref="SemaphoreSlim"/> limits the number of simultaneously open forms to
///   <see cref="WinFormsHostingOptions.MaxConcurrentForms"/>.
///  </para>
/// </remarks>
internal sealed class FormProvider : IFormProvider
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IGuiContext _guiContext;
    private readonly SemaphoreSlim _throttle;

    /// <summary>
    ///  Initializes a new instance of <see cref="FormProvider"/>.
    /// </summary>
    /// <param name="scopeFactory">The scope factory used to create per-form DI scopes.</param>
    /// <param name="guiContext">The GUI context used to dispatch work to the UI thread.</param>
    /// <param name="options">The hosting options that configure form-provider behavior.</param>
    public FormProvider(
        IServiceScopeFactory scopeFactory,
        IGuiContext guiContext,
        IOptions<WinFormsHostingOptions> options)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(guiContext);
        ArgumentNullException.ThrowIfNull(options);

        _scopeFactory = scopeFactory;
        _guiContext = guiContext;
        _throttle = new SemaphoreSlim(
            initialCount: options.Value.MaxConcurrentForms,
            maxCount: options.Value.MaxConcurrentForms);
    }

    /// <inheritdoc/>
    public TForm ShowForm<TForm>() where TForm : Form
    {
        if (!_guiContext.IsUiThread)
        {
            throw new InvalidOperationException(
                SR.FormProvider_MustBeCalledOnUiThread);
        }

        _throttle.Wait();

        IServiceScope scope = _scopeFactory.CreateScope();
        TForm form;

        try
        {
            form = scope.ServiceProvider.GetRequiredService<TForm>();

            // If the form supports receiving the service provider after construction,
            // assign it now so the form can resolve additional services.
            if (form is IServiceProviderAssignable assignable)
            {
                assignable.SetServiceProvider(scope.ServiceProvider);
            }
        }
        catch
        {
            scope.Dispose();
            _throttle.Release();
            throw;
        }

        form.FormClosed += (_, _) =>
        {
            scope.Dispose();
            _throttle.Release();
        };

        form.Show();
        return form;
    }

    /// <inheritdoc/>
    public async Task<DialogResult> ShowFormAsync<TForm>(CancellationToken cancellationToken = default)
        where TForm : Form
    {
        await _throttle.WaitAsync(cancellationToken).ConfigureAwait(false);

        // All UI work must be done on the UI thread.
        TaskCompletionSource<DialogResult> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        _guiContext.SynchronizationContext.Post(
            static state =>
            {
                var (provider, token, tcsInner) = ((FormProvider, CancellationToken, TaskCompletionSource<DialogResult>))state!;
                provider.ShowFormAsyncCore<TForm>(tcsInner, token);
            },
            (this, cancellationToken, tcs));

        return await tcs.Task.ConfigureAwait(false);
    }

    private void ShowFormAsyncCore<TForm>(
        TaskCompletionSource<DialogResult> tcs,
        CancellationToken cancellationToken)
        where TForm : Form
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _throttle.Release();
            tcs.TrySetCanceled(cancellationToken);
            return;
        }

        IServiceScope scope = _scopeFactory.CreateScope();
        TForm form;

        try
        {
            form = scope.ServiceProvider.GetRequiredService<TForm>();

            if (form is IServiceProviderAssignable assignable)
            {
                assignable.SetServiceProvider(scope.ServiceProvider);
            }
        }
        catch (Exception ex)
        {
            scope.Dispose();
            _throttle.Release();
            tcs.TrySetException(ex);
            return;
        }

        form.FormClosed += (_, e) =>
        {
            scope.Dispose();
            _throttle.Release();
            tcs.TrySetResult(form.DialogResult);
        };

        form.Show();
    }
}
