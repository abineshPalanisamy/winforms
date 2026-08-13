// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WinFormsBuilderSample.Configuration;

namespace WinFormsBuilderSample.Services;

/// <summary>
///  Default implementation of <see cref="IGreetingService"/>.
///  Reads the greeting prefix from <see cref="AppSettings"/> and logs each call.
/// </summary>
internal sealed class GreetingService : IGreetingService
{
    private readonly ILogger<GreetingService> _logger;
    private readonly AppSettings _settings;

    public GreetingService(ILogger<GreetingService> logger, IOptions<AppSettings> options)
    {
        _logger = logger;
        _settings = options.Value;
    }

    public string Greet(string name)
    {
        string message = $"{_settings.GreetingPrefix}, {name}! (App: {_settings.AppTitle})";
        _logger.LogInformation("Greeting generated: {Message}", message);
        return message;
    }
}
