// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Windows.Forms;

namespace Microsoft.Extensions.WinForms;

/// <summary>
///  Configuration options that control the behavior of the Windows Forms hosting
///  infrastructure started via <see cref="WinFormsApplicationBuilder"/>.
/// </summary>
public sealed class WinFormsHostingOptions
{
    /// <summary>
    ///  Gets or sets the <see cref="HighDpiMode"/> that will be applied to the application
    ///  during startup.
    /// </summary>
    /// <value>
    ///  The default value is <see cref="HighDpiMode.SystemAware"/>.
    /// </value>
    public HighDpiMode HighDpiMode { get; set; } = HighDpiMode.SystemAware;

    /// <summary>
    ///  Gets or sets a value indicating whether Windows visual styles are enabled.
    /// </summary>
    /// <value>
    ///  The default value is <see langword="true"/>.
    /// </value>
    public bool EnableVisualStyles { get; set; } = true;

    /// <summary>
    ///  Gets or sets a value indicating whether the application uses the
    ///  compatible text-rendering path (<see langword="false"/>, GDI+) or the
    ///  modern GDI-based path (<see langword="true"/>).
    /// </summary>
    /// <value>
    ///  The default value is <see langword="false"/>, which enables GDI+ text
    ///  rendering (compatible mode). Set to <see langword="true"/> to opt in to GDI
    ///  text rendering for improved performance and fidelity.
    /// </value>
    public bool UseCompatibleTextRendering { get; set; }

    /// <summary>
    ///  Gets or sets the maximum number of forms that may be displayed simultaneously.
    /// </summary>
    /// <value>
    ///  The default value is <c>8</c>.  Setting this to a very large number is not
    ///  recommended because each visible form consumes a DI scope for the lifetime of the
    ///  window.
    /// </value>
    public int MaxConcurrentForms { get; set; } = 8;
}
