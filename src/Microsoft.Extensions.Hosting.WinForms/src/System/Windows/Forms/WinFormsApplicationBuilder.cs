// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows.Forms;

namespace Microsoft.Extensions.WinForms;

/// <summary>
///  A builder for configuring and creating a Windows Forms application using the
///  .NET generic host infrastructure.
/// </summary>
/// <remarks>
///  <para>
///   <see cref="WinFormsApplicationBuilder"/> follows the same three-phase pattern used
///   by ASP.NET Core (<c>WebApplicationBuilder</c>) and .NET MAUI
///   (<c>MauiAppBuilder</c>):
///  </para>
///  <list type="number">
///   <item>
///    <description>
///     <b>Configure</b> — add services, logging, configuration, and Windows Forms options
///     via the properties on this builder.
///    </description>
///   </item>
///   <item>
///    <description>
///     <b>Build</b> — call <see cref="Build"/> to create the <see cref="WinFormsApplication"/>
///     and build the DI container.
///    </description>
///   </item>
///   <item>
///    <description>
///     <b>Run</b> — call <see cref="WinFormsApplication.Run"/> or
///     <see cref="WinFormsApplication.RunAsync"/> on the returned application to start the
///     Windows Forms message loop.
///    </description>
///   </item>
///  </list>
///  <example>
///   <para>
///    <code language="csharp">
///    var builder = WinFormsApplication.CreateBuilder(args);
///    builder.Services.AddSingleton&lt;IMyService, MyService&gt;();
///    builder.WinForms.AddWindowsFormsLifetime&lt;MainForm&gt;(opts =&gt;
///    {
///        opts.HighDpiMode = HighDpiMode.PerMonitorV2;
///        opts.EnableVisualStyles = true;
///    });
///
///    var app = builder.Build();
///    app.Run();
///    </code>
///   </para>
///  </example>
/// </remarks>
public sealed class WinFormsApplicationBuilder : IHostApplicationBuilder
{
    private readonly IHostApplicationBuilder _hostBuilder;

    internal WinFormsApplicationBuilder(string[]? args)
    {
        HostApplicationBuilderSettings settings = new()
        {
            Args = args,
            ContentRootPath = System.Environment.CurrentDirectory,
        };

        _hostBuilder = new HostApplicationBuilder(settings);
    }

    /// <summary>
    ///  Gets the service collection used to register DI services for the application.
    /// </summary>
    public IServiceCollection Services => _hostBuilder.Services;

    /// <summary>
    ///  Gets the stable collection of properties shared across the host builder.
    /// </summary>
    public IDictionary<object, object> Properties => _hostBuilder.Properties;

    /// <summary>
    ///  Gets the metrics produced by the host builder.
    /// </summary>
    public IMetricsBuilder Metrics => _hostBuilder.Metrics;

    /// <summary>
    ///  Gets the configuration manager that allows reading and overriding application
    ///  configuration from multiple sources (JSON files, environment variables, etc.).
    /// </summary>
    public IConfigurationManager Configuration => _hostBuilder.Configuration;

    /// <summary>
    ///  Gets the logging builder that allows configuring logging providers and log filters.
    /// </summary>
    public ILoggingBuilder Logging => _hostBuilder.Logging;

    /// <summary>
    ///  Gets the host environment for the application.
    /// </summary>
    public IHostEnvironment Environment => _hostBuilder.Environment;

    /// <summary>
    ///  Gets the <see cref="IServiceCollection"/> pre-scoped to Windows Forms hosting
    ///  extension methods.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Calling extension methods directly on <see cref="WinForms"/> (e.g.,
    ///   <c>builder.WinForms.AddWindowsFormsLifetime&lt;MainForm&gt;(...)</c>) is equivalent to
    ///   calling them on <see cref="Services"/>.
    ///  </para>
    /// </remarks>
    public IServiceCollection WinForms => _hostBuilder.Services;

    /// <summary>
    ///  Registers a startup form type and configures WinForms hosting with defaults.
    /// </summary>
    /// <typeparam name="TForm">The <see cref="Form"/>-derived type to use as the startup form.</typeparam>
    /// <returns>The current builder instance.</returns>
    public WinFormsApplicationBuilder UseStartupForm<TForm>() where TForm : Form
    {
        Services.AddWindowsFormsLifetime<TForm>();
        return this;
    }

    /// <summary>
    ///  Configures the application's high DPI mode.
    /// </summary>
    /// <param name="highDpiMode">The high DPI mode to apply.</param>
    /// <returns>The current builder instance.</returns>
    public WinFormsApplicationBuilder UseHighDpiMode(HighDpiMode highDpiMode)
    {
        Services.Configure<WinFormsHostingOptions>(options => options.HighDpiMode = highDpiMode);
        return this;
    }

    /// <summary>
    ///  Configures whether Windows visual styles are enabled.
    /// </summary>
    /// <param name="enable">Whether to enable visual styles.</param>
    /// <returns>The current builder instance.</returns>
    public WinFormsApplicationBuilder UseVisualStyles(bool enable = true)
    {
        Services.Configure<WinFormsHostingOptions>(options => options.EnableVisualStyles = enable);
        return this;
    }

    /// <summary>
    ///  Configures whether the application uses the modern GDI-based text rendering path.
    /// </summary>
    /// <param name="enable">Whether to use text rendering v2.</param>
    /// <returns>The current builder instance.</returns>
    public WinFormsApplicationBuilder UseTextRenderingV2(bool enable = true)
    {
        Services.Configure<WinFormsHostingOptions>(options => options.UseCompatibleTextRendering = !enable);
        return this;
    }

    /// <summary>
    ///  Configures a custom container builder.
    /// </summary>
    public void ConfigureContainer<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure)
        where TContainerBuilder : notnull
    {
        _hostBuilder.ConfigureContainer(factory, configure);
    }

    /// <summary>
    ///  Builds the application and composes the DI container.
    /// </summary>
    /// <returns>A configured <see cref="WinFormsApplication"/> ready to run.</returns>
    public WinFormsApplication Build()
    {
        IHost host = ((HostApplicationBuilder)_hostBuilder).Build();
        return new WinFormsApplication(host);
    }
}
