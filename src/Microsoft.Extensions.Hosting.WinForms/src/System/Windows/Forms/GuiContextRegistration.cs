// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Extensions.WinForms;

/// <summary>
///  A singleton service that acts as a bridge between the DI container and the
///  <see cref="GuiContext"/> created on the Windows Forms STA thread.
/// </summary>
/// <remarks>
///  <para>
///   Because the <see cref="GuiContext"/> cannot be created until the STA thread starts
///   (so that it captures the correct <see cref="SynchronizationContext"/>), this
///   registration object is registered in the container first and then populated by
///   <see cref="WinFormsHostedService"/> once the UI thread is running.
///  </para>
/// </remarks>
internal sealed class GuiContextRegistration
{
    private IGuiContext? _guiContext;
    private readonly TaskCompletionSource<IGuiContext> _readySource =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    ///  Gets the registered <see cref="IGuiContext"/>, or <see langword="null"/> if the
    ///  UI thread has not yet started.
    /// </summary>
    internal IGuiContext? GuiContext => _guiContext;

    /// <summary>
    ///  Gets or sets the main form type to be shown by <see cref="WinFormsHostedService"/>.
    /// </summary>
    internal Type? MainFormType { get; set; }

    /// <summary>
    ///  Returns a <see cref="Task{TResult}"/> that completes when the UI thread registers
    ///  the <see cref="IGuiContext"/>.
    /// </summary>
    internal Task<IGuiContext> WaitForGuiContextAsync() => _readySource.Task;

    /// <summary>
    ///  Called by <see cref="WinFormsHostedService"/> on the STA thread to register the
    ///  live <see cref="IGuiContext"/> and signal readiness.
    /// </summary>
    internal void Register(GuiContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _guiContext = context;
        _readySource.TrySetResult(context);
    }
}
