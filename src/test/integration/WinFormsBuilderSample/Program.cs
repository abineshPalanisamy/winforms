// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ┌─────────────────────────────────────────────────────────────────────────┐
// │  WinForms Builder Sample                                                │
// │                                                                         │
// │  Demonstrates the three-phase WinForms hosting pattern:                 │
// │    1. CreateBuilder  — set up DI, config, logging                       │
// │    2. Build          — seal the DI container                            │
// │    3. Run            — start the STA message loop                       │
// │                                                                         │
// │  Mirrors the ASP.NET Core / MAUI builder experience for WinForms apps.  │
// └─────────────────────────────────────────────────────────────────────────┘

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WinForms;
using WinFormsBuilderSample;
using WinFormsBuilderSample.Configuration;
using WinFormsBuilderSample.Services;

// ── Phase 1: Configure ────────────────────────────────────────────────────
WinFormsApplicationBuilder builder = WinFormsApplication.CreateBuilder(args);

// Bind strongly-typed settings from appsettings.json  →  AppSettings
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("App"));

// Register application services
builder.Services.AddSingleton<IGreetingService, GreetingService>();

// Logging: console + debug output
builder.Logging
    .AddConsole()
    .AddDebug()
    .SetMinimumLevel(LogLevel.Debug);

// Register the main form and wire up WinForms hosting
builder.UseStartupForm<MainForm>()
    .UseHighDpiMode(HighDpiMode.PerMonitorV2)
    .UseVisualStyles(true)
    .UseTextRenderingV2(true);

// ── Phase 2: Build ────────────────────────────────────────────────────────
WinFormsApplication app = builder.Build();

// ── Phase 3: Run ─────────────────────────────────────────────────────────
app.Run();
