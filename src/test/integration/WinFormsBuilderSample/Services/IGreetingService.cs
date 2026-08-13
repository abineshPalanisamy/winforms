// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace WinFormsBuilderSample.Services;

/// <summary>
///  Provides a greeting message for the application.
/// </summary>
public interface IGreetingService
{
    /// <summary>Gets a greeting string for the given name.</summary>
    string Greet(string name);
}
