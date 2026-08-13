// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Windows.Forms;

namespace Microsoft.Extensions.WinForms;

/// <summary>
///  An <see cref="IHostedService"/> that manages the Windows Forms message loop on a
///  dedicated Single-Threaded Apartment (STA) thread.
/// </summary>
/// <remarks>
///  <para>
///   On <see cref="StartAsync"/>, a new STA thread is created that:
///   <list type="number">
///    <item>
///     <description>
///      Applies high-DPI mode, enables visual styles, and sets the compatible
///      text-rendering default from <see cref="WinFormsHostingOptions"/>.
///     </description>
///    </item>
///    <item>
///     <description>
///      Installs a <see cref="WindowsFormsSynchronizationContext"/> and exposes it via
///      <see cref="IGuiContext"/> so any service can dispatch work back to the UI thread.
///     </description>
///    </item>
///    <item>
///     <description>
///      Resolves and shows the main <see cref="Form"/> (the type registered via
///      <see cref="WinFormsHostingExtensions.AddWinFormsMainForm{TForm}"/>).
///     </description>
///    </item>
///    <item>
///     <description>
///      Runs <see cref="Application.Run(Form)"/> and, when the message loop exits,
///      requests <see cref="IHostApplicationLifetime.StopApplication"/> so the
///      generic host shuts down gracefully.
///     </description>
///    </item>
///   </list>
///  </para>
/// </remarks>
internal sealed partial class WinFormsHostedService : IHostedService, IDisposable
{
    [LoggerMessage(Level = LogLevel.Critical, Message = "Unhandled exception on the Windows Forms UI thread.")]
    private static partial void LogUiThreadCritical(ILogger logger, Exception exception);
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IOptions<WinFormsHostingOptions> _options;
    private readonly ILogger<WinFormsHostedService> _logger;
    private readonly GuiContextRegistration _guiContextRegistration;
    private Thread? _uiThread;
    private bool _disposed;

    /// <summary>
    ///  Initializes a new instance of <see cref="WinFormsHostedService"/>.
    /// </summary>
    public WinFormsHostedService(
        IServiceProvider serviceProvider,
        IHostApplicationLifetime lifetime,
        IOptions<WinFormsHostingOptions> options,
        ILogger<WinFormsHostedService> logger,
        GuiContextRegistration guiContextRegistration)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(lifetime);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(guiContextRegistration);

        _serviceProvider = serviceProvider;
        _lifetime = lifetime;
        _options = options;
        _logger = logger;
        _guiContextRegistration = guiContextRegistration;
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // The UI thread readiness signal is used to ensure StartAsync completes only
        // after the Windows Forms message loop is ready to accept messages.
        TaskCompletionSource uiThreadReady = new(TaskCreationOptions.RunContinuationsAsynchronously);

        _uiThread = new Thread(() => RunMessageLoop(uiThreadReady))
        {
            IsBackground = false,
            Name = "Windows Forms UI Thread"
        };
        _uiThread.SetApartmentState(ApartmentState.STA);
        _uiThread.Start();

        return uiThreadReady.Task.WaitAsync(cancellationToken);
    }

    private void RunMessageLoop(TaskCompletionSource uiThreadReady)
    {
        try
        {
            // ---- Phase 1: configure application settings ----
            WinFormsHostingOptions opts = _options.Value;

            Application.SetHighDpiMode(opts.HighDpiMode);

            if (opts.EnableVisualStyles)
            {
                Application.EnableVisualStyles();
            }

            Application.SetCompatibleTextRenderingDefault(opts.UseCompatibleTextRendering);

            // ---- Phase 2: install synchronization context ----
            WindowsFormsSynchronizationContext.AutoInstall = false;
            WindowsFormsSynchronizationContext syncContext = new();
            SynchronizationContext.SetSynchronizationContext(syncContext);

            GuiContext guiContext = new(syncContext);
            _guiContextRegistration.Register(guiContext);

            // Signal that the UI thread is ready before entering the message loop.
            uiThreadReady.TrySetResult();

            // ---- Phase 3: resolve and show the main form ----
            Type mainFormType = _guiContextRegistration.MainFormType
                ?? throw new InvalidOperationException(SR.WinFormsHostedService_MainFormRequired);

            Form mainForm = (Form)_serviceProvider.GetRequiredService(mainFormType);

            if (mainForm is IServiceProviderAssignable assignable)
            {
                assignable.SetServiceProvider(_serviceProvider);
            }

            // ---- Phase 4: run the Windows Forms message loop ----
            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            LogUiThreadCritical(_logger, ex);
            uiThreadReady.TrySetException(ex);
        }
        finally
        {
            // When the message loop exits, stop the generic host.
            _lifetime.StopApplication();
        }
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _guiContextRegistration.GuiContext?.RequestClose();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}
