// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Drawing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WinFormsBuilderSample.Configuration;
using WinFormsBuilderSample.Services;

namespace WinFormsBuilderSample;

/// <summary>
///  The main application form. Receives its dependencies from the DI container
///  — no <c>new</c> calls needed in <c>Program.cs</c>.
/// </summary>
[DesignerCategory("code")]
public sealed class MainForm : Form
{
    private readonly IGreetingService _greetingService;
    private readonly ILogger<MainForm> _logger;
    private readonly AppSettings _settings;

    // ── UI controls created in code (no Designer) ──────────────────────────
    private readonly TableLayoutPanel _layout = new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = 2,
        RowCount = 4,
        Padding = new Padding(12)
    };

    private readonly Label _lblName = new() { Text = "Your name:", AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly TextBox _txtName = new() { Dock = DockStyle.Fill, PlaceholderText = "Enter your name…" };
    private readonly Button _btnGreet = new() { Text = "Greet", Dock = DockStyle.Fill, UseVisualStyleBackColor = true };
    private readonly Label _lblGreeting = new() { AutoSize = true, Font = new Font("Segoe UI", 12f, FontStyle.Bold), Anchor = AnchorStyles.Left };

    private readonly GroupBox _grpInfo = new()
    {
        Text = "Runtime Info",
        Dock = DockStyle.Fill,
        Padding = new Padding(8)
    };

    private readonly FlowLayoutPanel _infoPanel = new()
    {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoScroll = true
    };

    private readonly StatusStrip _statusStrip = new();
    private readonly ToolStripStatusLabel _statusLabel = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };

    public MainForm(
        IGreetingService greetingService,
        ILogger<MainForm> logger,
        IOptions<AppSettings> options)
    {
        _greetingService = greetingService;
        _logger = logger;
        _settings = options.Value;

        BuildUI();
        PopulateRuntimeInfo();

        _logger.LogInformation("MainForm created via DI.");
    }

    // ── Construction helpers ────────────────────────────────────────────────

    private void BuildUI()
    {
        Text = _settings.AppTitle;
        ClientSize = new Size(520, 380);
        MinimumSize = new Size(480, 340);
        StartPosition = FormStartPosition.CenterScreen;

        // Column / row styles
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));    // name row
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));    // button row
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));    // greeting row
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // info group

        // Row 0 — name input
        _layout.Controls.Add(_lblName, 0, 0);
        _layout.Controls.Add(_txtName, 1, 0);

        // Row 1 — greet button (spans both columns)
        _layout.SetColumnSpan(_btnGreet, 2);
        _layout.Controls.Add(_btnGreet, 0, 1);

        // Row 2 — greeting output (spans both columns)
        _layout.SetColumnSpan(_lblGreeting, 2);
        _layout.Controls.Add(_lblGreeting, 0, 2);

        // Row 3 — runtime info group
        _layout.SetColumnSpan(_grpInfo, 2);
        _grpInfo.Controls.Add(_infoPanel);
        _layout.Controls.Add(_grpInfo, 0, 3);

        // Status bar
        _statusStrip.Items.Add(_statusLabel);
        _statusLabel.Text = "Ready";

        Controls.Add(_layout);
        Controls.Add(_statusStrip);

        _btnGreet.Click += OnGreetClicked;
        _txtName.KeyDown += (_, e) =>
        {
            if (e.KeyCode is Keys.Enter)
                _btnGreet.PerformClick();
        };
    }

    private void PopulateRuntimeInfo()
    {
        AddInfo("Framework", RuntimeInformation.FrameworkDescription);
        AddInfo("OS", RuntimeInformation.OSDescription);
        AddInfo("ProcessorCount", Environment.ProcessorCount.ToString());
        AddInfo("App Title (from config)", _settings.AppTitle);
        AddInfo("Greeting Prefix (from config)", _settings.GreetingPrefix);
    }

    private void AddInfo(string label, string value)
    {
        _infoPanel.Controls.Add(new Label
        {
            AutoSize = true,
            Text = $"• {label}: {value}",
            Font = new Font("Consolas", 9f)
        });
    }

    // ── Event handlers ──────────────────────────────────────────────────────

    private void OnGreetClicked(object? sender, EventArgs e)
    {
        string name = _txtName.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            _statusLabel.Text = "Please enter your name first.";
            _txtName.Focus();
            return;
        }

        string greeting = _greetingService.Greet(name);
        _lblGreeting.Text = greeting;
        _statusLabel.Text = $"Greeted at {DateTime.Now:HH:mm:ss}";
        _logger.LogDebug("Button clicked for name: {Name}", name);
    }
}
