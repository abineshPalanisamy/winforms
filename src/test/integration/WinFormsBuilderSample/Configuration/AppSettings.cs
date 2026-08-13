// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace WinFormsBuilderSample.Configuration;

/// <summary>
///  Strongly-typed settings bound from <c>appsettings.json</c> section <c>App</c>.
/// </summary>
public sealed class AppSettings
{
    /// <summary>Gets or sets the title of the application window.</summary>
    public string AppTitle { get; set; } = "WinForms Builder Sample";

    /// <summary>Gets or sets the greeting prefix shown in the UI.</summary>
    public string GreetingPrefix { get; set; } = "Hello";
}
