// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.WinForms;

/// <summary>
///  Represents a Windows Forms application created and configured via
///  <see cref="WinFormsApplicationBuilder"/>.
/// </summary>
/// <remarks>
///  <para>
///   Use <see cref="CreateBuilder"/> to create a <see cref="WinFormsApplicationBuilder"/>,
///   configure the application's services and options, then call
///   <see cref="WinFormsApplicationBuilder.Build"/> to obtain a
///   <see cref="WinFormsApplication"/>. Finally, call <see cref="Run"/> (or
///   <see cref="RunAsync"/>) to start the application.
///  </para>
///  <example>
///   <para>
///    <code language="csharp">
///    var builder = WinFormsApplication.CreateBuilder(args);
///    builder.WinForms.AddWindowsFormsLifetime&lt;MainForm&gt;(opts =&gt;
///    {
///        opts.HighDpiMode = HighDpiMode.PerMonitorV2;
///    });
///    builder.Services.AddSingleton&lt;IMyService, MyService&gt;();
///
///    var app = builder.Build();
///    app.Run();
///    </code>
///   </para>
///  </example>
/// </remarks>
public sealed class WinFormsApplication : IHost, IAsyncDisposable
{
    private static WinFormsApplication? s_current;
    private readonly IHost _host;

    internal WinFormsApplication(IHost host)
    {
        _host = host;
        s_current = this;
    }

    /// <summary>
    ///  Gets the currently running <see cref="WinFormsApplication"/> instance, or
    ///  <see langword="null"/> if the application has not yet been started.
    /// </summary>
    public static WinFormsApplication? Current => s_current;

    /// <summary>
    ///  Gets the <see cref="IServiceProvider"/> for the application's DI container.
    /// </summary>
    public IServiceProvider Services => _host.Services;

    /// <summary>
    ///  Creates a new <see cref="WinFormsApplicationBuilder"/> with the provided command-line
    ///  arguments.
    /// </summary>
    /// <param name="args">
    ///  The command-line arguments passed to the application entry point, or
    ///  <see langword="null"/> if there are none.
    /// </param>
    /// <returns>A new, unconfigured <see cref="WinFormsApplicationBuilder"/>.</returns>
    public static WinFormsApplicationBuilder CreateBuilder(string[]? args = null)
        => new WinFormsApplicationBuilder(args);

    /// <summary>
    ///  Starts the application synchronously, running the Windows Forms message loop on a
    ///  dedicated STA thread until the main form is closed.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   This is a convenience wrapper around <see cref="RunAsync"/>. The calling thread
    ///   blocks until the application exits.
    ///  </para>
    /// </remarks>
    public void Run()
    {
        RunAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    ///  Starts the application asynchronously, running the Windows Forms message loop on a
    ///  dedicated STA thread until the main form is closed.
    /// </summary>
    /// <param name="cancellationToken">
    ///  A <see cref="CancellationToken"/> that can be used to request application
    ///  termination.
    /// </param>
    /// <returns>A <see cref="Task"/> that completes when the application exits.</returns>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await _host.RunAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///  Starts the host without blocking and returns once the WinForms UI thread is ready.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return _host.StartAsync(cancellationToken);
    }

    /// <summary>
    ///  Stops the host and shuts down the WinForms application.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return _host.StopAsync(cancellationToken);
    }

    /// <summary>
    ///  Releases the application's resources.
    /// </summary>
    public void Dispose()
    {
        _host.Dispose();

        if (s_current == this)
        {
            s_current = null;
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_host is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            _host.Dispose();
        }

        if (s_current == this)
        {
            s_current = null;
        }
    }
}
