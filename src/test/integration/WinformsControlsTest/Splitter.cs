// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;

namespace WinFormsControlsTest;

// Reproduction for dotnet/winforms issue #14931. Use the payload and orientation
// selectors to exercise continuous SplitContainer layout and painting.
[DesignerCategory("Default")]
public partial class Splitter : Form, IMessageFilter
{
    private readonly ComboBox _payloadSelector = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 160
    };

    private readonly ComboBox _orientationSelector = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 120
    };

    private readonly ComboBox _visualStylesModeSelector = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 100
    };

    private readonly ComboBox _colorModeSelector = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 80
    };

    private readonly ComboBox _rightToLeftSelector = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 60
    };

    private readonly ComboBox _splitterIncrementSelector = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 50
    };

    private readonly ToolStripButton _cancelSplitterMovingButton = new("Cancel movement")
    {
        CheckOnClick = true
    };

    private readonly ToolStripButton _splitterFixedButton = new("Fixed")
    {
        CheckOnClick = true
    };

    private readonly ToolStripButton _resetMetricsButton = new("Reset metrics");
    private readonly ToolStripButton _startPerformanceButton = new("Start performance");
    private readonly ToolStripButton _stopPerformanceButton = new("Stop performance")
    {
        Enabled = false
    };

    private readonly ToolStripStatusLabel _metricsLabel = new()
    {
        Spring = true,
        TextAlign = ContentAlignment.MiddleLeft
    };

    private readonly ToolStripStatusLabel _performanceLabel = new()
    {
        Spring = true,
        TextAlign = ContentAlignment.MiddleLeft,
        Text = "Performance: select Start performance, drag continuously, then select Stop performance."
    };

    private readonly System.Windows.Forms.Timer _metricsTimer = new()
    {
        Interval = 250
    };

    private readonly Stopwatch _performanceStopwatch = new();

    private int _splitterMovingCount;
    private int _splitterMovedCount;
    private int _panel1ResizeCount;
    private int _panel2ResizeCount;
    private int _panel1PaintCount;
    private int _panel2PaintCount;
    private int _descendantPaintCount;
    private int _splitterDistanceAtReset;
    private int _escapeKeyCount;
    private int _escapeDuringCaptureCount;
    private int _splitterDistanceAtEscape;
    private bool _performanceMeasurementActive;
    private long _lastHeartbeatTimestamp;
    private double _maximumHeartbeatDelayMilliseconds;
    private long _managedMemoryAtStart;
    private long _privateMemoryAtStart;
    private long _workingSetAtStart;
    private int _handleCountAtStart;
    private uint _gdiObjectCountAtStart;
    private uint _userObjectCountAtStart;
    private TimeSpan _processorTimeAtStart;
    private int _gen0CollectionsAtStart;
    private int _gen1CollectionsAtStart;
    private int _gen2CollectionsAtStart;

    public Splitter()
    {
        InitializeComponent();
        BuildReproUI();
        RebuildPayload();
    }

    private void BuildReproUI()
    {
        splitContainer1.SplitterWidth = 8;
        splitContainer1.Panel1MinSize = 50;
        splitContainer1.Panel2MinSize = 50;

        _payloadSelector.Items.AddRange(["Light", "Medium", "Heavy (nested TLP)", "Nested SplitContainer", "Owner Draw Controls", "Month Calendar", "TrackBar", "All Controls"]);
        _payloadSelector.SelectedIndex = 0;
        _payloadSelector.SelectedIndexChanged += (_, _) => RebuildPayload();

        _visualStylesModeSelector.Items.AddRange(
            [
                VisualStylesMode.Inherit,
                VisualStylesMode.Classic,
                VisualStylesMode.Disabled,
                VisualStylesMode.Net11,
                VisualStylesMode.Latest
            ]);
        _visualStylesModeSelector.SelectedIndexChanged += (_, _) =>
        {
            if (_visualStylesModeSelector.SelectedItem is VisualStylesMode mode)
            {
                splitContainer1.VisualStylesMode = mode;
            }
        };
        _visualStylesModeSelector.SelectedItem = VisualStylesMode.Net11;

        _colorModeSelector.Items.AddRange(
            [
                SystemColorMode.Classic,
                SystemColorMode.System,
                SystemColorMode.Dark
            ]);
        _colorModeSelector.SelectedIndexChanged += (_, _) =>
        {
            if (_colorModeSelector.SelectedItem is SystemColorMode mode)
            {
                Application.SetColorMode(mode);
                ResetMetrics();
            }
        };
        _colorModeSelector.SelectedItem = SystemColorMode.Classic;

        _orientationSelector.Items.AddRange(["Vertical", "Horizontal"]);
        _orientationSelector.SelectedIndex = 0;
        _orientationSelector.SelectedIndexChanged += (_, _) =>
            splitContainer1.Orientation = _orientationSelector.SelectedIndex == 0
                ? Orientation.Vertical
                : Orientation.Horizontal;

        _rightToLeftSelector.Items.AddRange(["LTR", "RTL"]);
        _rightToLeftSelector.SelectedIndex = 0;
        _rightToLeftSelector.SelectedIndexChanged += (_, _) =>
        {
            splitContainer1.RightToLeft = _rightToLeftSelector.SelectedIndex == 0
                ? RightToLeft.No
                : RightToLeft.Yes;
            ResetMetrics();
        };

        _splitterIncrementSelector.Items.AddRange([1, 5, 10]);
        _splitterIncrementSelector.SelectedIndexChanged += (_, _) =>
        {
            if (_splitterIncrementSelector.SelectedItem is int increment)
            {
                splitContainer1.SplitterIncrement = increment;
                ResetMetrics();
            }
        };
        _splitterIncrementSelector.SelectedItem = 1;

        splitContainer1.SplitterMoving += (_, e) =>
        {
            _splitterMovingCount++;
            e.Cancel = _cancelSplitterMovingButton.Checked;
        };
        splitContainer1.SplitterMoved += (_, _) => _splitterMovedCount++;
        splitContainer1.Panel1.Resize += (_, _) => _panel1ResizeCount++;
        splitContainer1.Panel2.Resize += (_, _) => _panel2ResizeCount++;
        splitContainer1.Panel1.Paint += (_, _) => _panel1PaintCount++;
        splitContainer1.Panel2.Paint += (_, _) => _panel2PaintCount++;

        _resetMetricsButton.Click += (_, _) => ResetMetrics();
        _startPerformanceButton.Click += (_, _) => StartPerformanceMeasurement();
        _stopPerformanceButton.Click += (_, _) => StopPerformanceMeasurement();
        _splitterFixedButton.CheckedChanged += (_, _) =>
        {
            splitContainer1.IsSplitterFixed = _splitterFixedButton.Checked;
            ResetMetrics();
        };

        ToolStrip toolBar = new();
        toolBar.Items.Add(new ToolStripLabel("Payload:"));
        toolBar.Items.Add(new ToolStripControlHost(_payloadSelector));
        toolBar.Items.Add(new ToolStripLabel("  Mode:"));
        toolBar.Items.Add(new ToolStripControlHost(_visualStylesModeSelector));
        toolBar.Items.Add(new ToolStripLabel("  Color:"));
        toolBar.Items.Add(new ToolStripControlHost(_colorModeSelector));
        toolBar.Items.Add(new ToolStripLabel("  Orientation:"));
        toolBar.Items.Add(new ToolStripControlHost(_orientationSelector));
        toolBar.Items.Add(new ToolStripLabel("  Direction:"));
        toolBar.Items.Add(new ToolStripControlHost(_rightToLeftSelector));
        toolBar.Items.Add(new ToolStripLabel("  Increment:"));
        toolBar.Items.Add(new ToolStripControlHost(_splitterIncrementSelector));
        toolBar.Items.Add(new ToolStripSeparator());
        toolBar.Items.Add(_cancelSplitterMovingButton);
        toolBar.Items.Add(_splitterFixedButton);
        toolBar.Items.Add(_resetMetricsButton);
        toolBar.Items.Add(_startPerformanceButton);
        toolBar.Items.Add(_stopPerformanceButton);

        StatusStrip metricsBar = new()
        {
            SizingGrip = false
        };
        metricsBar.Items.Add(_metricsLabel);

        StatusStrip performanceBar = new()
        {
            SizingGrip = false
        };
        performanceBar.Items.Add(_performanceLabel);

        Controls.Add(performanceBar);
        Controls.Add(metricsBar);
        Controls.Add(toolBar);

        Application.AddMessageFilter(this);
        _metricsTimer.Tick += (_, _) =>
        {
            RecordUiHeartbeat();
            UpdateMetricsLabel();
            UpdatePerformanceLabel();
        };
        _metricsTimer.Start();
        FormClosed += (_, _) =>
        {
            Application.SetColorMode(SystemColorMode.Classic);
            Application.RemoveMessageFilter(this);
            _metricsTimer.Dispose();
        };
    }

    private void RebuildPayload()
    {
        DisposeChildControls(splitContainer1.Panel1);
        DisposeChildControls(splitContainer1.Panel2);

        switch (_payloadSelector.SelectedIndex)
        {
            case 0:
                splitContainer1.Panel1.Controls.Add(MakeLabel("Panel1 (light)"));
                splitContainer1.Panel2.Controls.Add(MakeLabel("Panel2 (light)"));
                break;

            case 1:
                splitContainer1.Panel1.Controls.Add(MakeMediumPayload("Panel1"));
                splitContainer1.Panel2.Controls.Add(MakeMediumPayload("Panel2"));
                break;

            case 2:
                splitContainer1.Panel1.Controls.Add(MakeComplexPayload("Panel1", primaryLayout: true));
                splitContainer1.Panel2.Controls.Add(MakeComplexPayload("Panel2", primaryLayout: false));
                break;

            case 3:
                splitContainer1.Panel1.Controls.Add(
                MakeNestedSplitContainerPayload("Panel1"));
                splitContainer1.Panel2.Controls.Add(
                MakeNestedSplitContainerPayload("Panel2"));
                break;

            case 4:
                splitContainer1.Panel1.Controls.Add(MakeOwnerDrawPayload("Panel1"));
                splitContainer1.Panel2.Controls.Add(MakeOwnerDrawPayload("Panel2"));
                break;

            case 5:
                splitContainer1.Panel1.Controls.Add(MakeMonthCalendar());
                splitContainer1.Panel2.Controls.Add(MakeMonthCalendar());
                break;

            case 6: // TrackBar
                splitContainer1.Panel1.Controls.Add(
                    new TrackBar
                    {
                        Dock = DockStyle.Fill,
                        Minimum = 0,
                        Maximum = 10,
                        Value = 5
                    });

                splitContainer1.Panel2.Controls.Add(
                    new TrackBar
                    {
                        Dock = DockStyle.Fill,
                        Minimum = 0,
                        Maximum = 10,
                        Value = 5
                    });
                break;

            default:
                splitContainer1.Panel1.Controls.Add(MakeAllControlsPayload("Panel1"));
                splitContainer1.Panel2.Controls.Add(MakeAllControlsPayload("Panel2"));
                break;
        }

        TrackDescendantPainting(splitContainer1.Panel1);
        TrackDescendantPainting(splitContainer1.Panel2);
        ResetMetrics();
    }

    private static MonthCalendar MakeMonthCalendar()
        => new()
        {
            Dock = DockStyle.Fill,
            MaxSelectionCount = 1
        };

    private void TrackDescendantPainting(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            child.Paint += (_, _) => _descendantPaintCount++;
            TrackDescendantPainting(child);
        }
    }

    private void ResetMetrics()
    {
        _splitterDistanceAtReset = splitContainer1.SplitterDistance;
        _splitterMovingCount = 0;
        _splitterMovedCount = 0;
        _panel1ResizeCount = 0;
        _panel2ResizeCount = 0;
        _panel1PaintCount = 0;
        _panel2PaintCount = 0;
        _descendantPaintCount = 0;
        _escapeKeyCount = 0;
        _escapeDuringCaptureCount = 0;
        _splitterDistanceAtEscape = 0;
        UpdateMetricsLabel();
    }

    private void StartPerformanceMeasurement()
    {
        ResetMetrics();

        using Process process = Process.GetCurrentProcess();
        process.Refresh();

        _managedMemoryAtStart = GC.GetTotalMemory(forceFullCollection: false);
        _privateMemoryAtStart = process.PrivateMemorySize64;
        _workingSetAtStart = process.WorkingSet64;
        _handleCountAtStart = process.HandleCount;
        _gdiObjectCountAtStart = GetGuiResourceCount(process, GET_GUI_RESOURCES_FLAGS.GR_GDIOBJECTS);
        _userObjectCountAtStart = GetGuiResourceCount(process, GET_GUI_RESOURCES_FLAGS.GR_USEROBJECTS);
        _processorTimeAtStart = process.TotalProcessorTime;
        _gen0CollectionsAtStart = GC.CollectionCount(0);
        _gen1CollectionsAtStart = GC.CollectionCount(1);
        _gen2CollectionsAtStart = GC.CollectionCount(2);
        _maximumHeartbeatDelayMilliseconds = 0;
        _lastHeartbeatTimestamp = Stopwatch.GetTimestamp();
        _performanceMeasurementActive = true;
        _performanceStopwatch.Restart();
        SetPerformanceConfigurationEnabled(enabled: false);
        UpdatePerformanceLabel();
    }

    private void StopPerformanceMeasurement()
    {
        _performanceStopwatch.Stop();
        _performanceMeasurementActive = false;

        using Process process = Process.GetCurrentProcess();
        process.Refresh();

        TimeSpan elapsed = _performanceStopwatch.Elapsed;
        TimeSpan processorTime = process.TotalProcessorTime - _processorTimeAtStart;
        double cpuUsage = elapsed.TotalMilliseconds > 0
            ? processorTime.TotalMilliseconds / elapsed.TotalMilliseconds / Environment.ProcessorCount * 100
            : 0;
        double movingRate = elapsed.TotalSeconds > 0
            ? _splitterMovingCount / elapsed.TotalSeconds
            : 0;
        double childPaintRate = elapsed.TotalSeconds > 0
            ? _descendantPaintCount / elapsed.TotalSeconds
            : 0;

        long managedMemoryDelta = GC.GetTotalMemory(forceFullCollection: false) - _managedMemoryAtStart;
        long privateMemoryDelta = process.PrivateMemorySize64 - _privateMemoryAtStart;
        long workingSetDelta = process.WorkingSet64 - _workingSetAtStart;
        int handleCountDelta = process.HandleCount - _handleCountAtStart;
        long gdiObjectDelta = (long)GetGuiResourceCount(process, GET_GUI_RESOURCES_FLAGS.GR_GDIOBJECTS)
            - _gdiObjectCountAtStart;
        long userObjectDelta = (long)GetGuiResourceCount(process, GET_GUI_RESOURCES_FLAGS.GR_USEROBJECTS)
            - _userObjectCountAtStart;

        _performanceLabel.Text =
            $"Performance: {elapsed.TotalSeconds:F1}s | CPU: {cpuUsage:F1}%"
            + $" | Moving/s: {movingRate:F1} | Child paint/s: {childPaintRate:F1}"
            + $" | Max UI delay: {_maximumHeartbeatDelayMilliseconds:F0} ms"
            + $" | Memory Δ managed/private/working: {FormatByteDelta(managedMemoryDelta)}"
            + $"/{FormatByteDelta(privateMemoryDelta)}/{FormatByteDelta(workingSetDelta)}"
            + $" | Handles Δ total/GDI/USER: {handleCountDelta:+#;-#;0}"
            + $"/{gdiObjectDelta:+#;-#;0}/{userObjectDelta:+#;-#;0}"
            + $" | GC Δ 0/1/2: {GC.CollectionCount(0) - _gen0CollectionsAtStart}"
            + $"/{GC.CollectionCount(1) - _gen1CollectionsAtStart}"
            + $"/{GC.CollectionCount(2) - _gen2CollectionsAtStart}";

        SetPerformanceConfigurationEnabled(enabled: true);
    }

    private void RecordUiHeartbeat()
    {
        if (!_performanceMeasurementActive)
        {
            return;
        }

        long currentTimestamp = Stopwatch.GetTimestamp();
        double heartbeatDelay = Math.Max(
            0,
            Stopwatch.GetElapsedTime(_lastHeartbeatTimestamp, currentTimestamp).TotalMilliseconds
                - _metricsTimer.Interval);
        _maximumHeartbeatDelayMilliseconds = Math.Max(
            _maximumHeartbeatDelayMilliseconds,
            heartbeatDelay);
        _lastHeartbeatTimestamp = currentTimestamp;
    }

    private void UpdatePerformanceLabel()
    {
        if (!_performanceMeasurementActive)
        {
            return;
        }

        double elapsedSeconds = _performanceStopwatch.Elapsed.TotalSeconds;
        double movingRate = elapsedSeconds > 0 ? _splitterMovingCount / elapsedSeconds : 0;
        double childPaintRate = elapsedSeconds > 0 ? _descendantPaintCount / elapsedSeconds : 0;

        _performanceLabel.Text =
            $"Performance running: {elapsedSeconds:F1}s"
            + $" | Moving/s: {movingRate:F1} | Child paint/s: {childPaintRate:F1}"
            + $" | Max UI delay: {_maximumHeartbeatDelayMilliseconds:F0} ms";
    }

    private void SetPerformanceConfigurationEnabled(bool enabled)
    {
        _payloadSelector.Enabled = enabled;
        _orientationSelector.Enabled = enabled;
        _visualStylesModeSelector.Enabled = enabled;
        _colorModeSelector.Enabled = enabled;
        _rightToLeftSelector.Enabled = enabled;
        _splitterIncrementSelector.Enabled = enabled;
        _cancelSplitterMovingButton.Enabled = enabled;
        _splitterFixedButton.Enabled = enabled;
        _resetMetricsButton.Enabled = enabled;
        _startPerformanceButton.Enabled = enabled;
        _stopPerformanceButton.Enabled = !enabled;
    }

    private static uint GetGuiResourceCount(Process process, GET_GUI_RESOURCES_FLAGS resource)
        => PInvokeCore.GetGuiResources((HANDLE)process.Handle, resource);

    private static string FormatByteDelta(long bytes)
        => $"{bytes / (1024d * 1024d):+0.0;-0.0;0.0} MB";

    private void UpdateMetricsLabel()
    {
        _metricsLabel.Text =
            $" Distance: {_splitterDistanceAtReset}->{splitContainer1.SplitterDistance}"
            + $" | Moving/Moved: {_splitterMovingCount}/{_splitterMovedCount}"
            + $" | Resize P1/P2: {_panel1ResizeCount}/{_panel2ResizeCount}"
            + $" | Paint P1/P2: {_panel1PaintCount}/{_panel2PaintCount}"
            + $" | Child paint: {_descendantPaintCount}"
            + $" | Escape total/during drag: {_escapeKeyCount}/{_escapeDuringCaptureCount}"
            + $" | At Escape: {_splitterDistanceAtEscape} | Capture: {splitContainer1.Capture}";
    }

    bool IMessageFilter.PreFilterMessage(ref Message m)
    {
        const int WmKeyDown = 0x0100;

        if (m.Msg == WmKeyDown && (Keys)(nint)m.WParam == Keys.Escape)
        {
            _escapeKeyCount++;
            if (splitContainer1.Capture)
            {
                _escapeDuringCaptureCount++;
                _splitterDistanceAtEscape = splitContainer1.SplitterDistance;
            }
        }

        return false;
    }

    private static void DisposeChildControls(Control parent)
    {
        while (parent.Controls.Count > 0)
        {
            parent.Controls[0].Dispose();
        }
    }

    private static Label MakeLabel(string text) => new()
    {
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleCenter,
        Text = text
    };

    private static TableLayoutPanel MakeMediumPayload(string title)
    {
        TableLayoutPanel layout = new()
        {
            ColumnCount = 3,
            Dock = DockStyle.Fill,
            Padding = new Padding(4),
            RowCount = 4
        };

        for (int column = 0; column < layout.ColumnCount; column++)
        {
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / layout.ColumnCount));
        }

        for (int row = 0; row < layout.RowCount; row++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / layout.RowCount));
        }

        ComboBox comboBox = new()
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        comboBox.Items.AddRange(["General", "Layout", "Rendering"]);
        comboBox.SelectedIndex = 0;

        ListBox listBox = new() { Dock = DockStyle.Fill };
        listBox.Items.AddRange(["Alpha", "Beta", "Gamma"]);
        listBox.SelectedIndex = 0;

        FlowLayoutPanel optionsPanel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        optionsPanel.Controls.Add(new RadioButton { AutoSize = true, Checked = true, Text = "Option A" });
        optionsPanel.Controls.Add(new RadioButton { AutoSize = true, Text = "Option B" });

        layout.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = $"{title} medium",
            TextAlign = ContentAlignment.MiddleCenter
        }, 0, 0);
        layout.Controls.Add(new TextBox
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Text = title
        }, 1, 0);
        layout.Controls.Add(new Button { Dock = DockStyle.Fill, Text = "Apply" }, 2, 0);
        layout.Controls.Add(new CheckBox
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Checked = true,
            Text = "Enabled"
        }, 0, 1);
        layout.Controls.Add(comboBox, 1, 1);
        layout.Controls.Add(new NumericUpDown
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Maximum = 100,
            Value = 50
        }, 2, 1);
        layout.Controls.Add(listBox, 0, 2);
        layout.Controls.Add(new ProgressBar
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Value = 65
        }, 1, 2);
        layout.Controls.Add(optionsPanel, 2, 2);
        layout.Controls.Add(new LinkLabel
        {
            Dock = DockStyle.Fill,
            Text = "More details",
            TextAlign = ContentAlignment.MiddleCenter
        }, 0, 3);
        layout.Controls.Add(new DomainUpDown
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Items = { "Low", "Medium", "High" },
            SelectedIndex = 1
        }, 1, 3);
        layout.Controls.Add(new Panel
        {
            BackColor = Color.SteelBlue,
            BorderStyle = BorderStyle.FixedSingle,
            Dock = DockStyle.Fill
        }, 2, 3);

        return layout;
    }

    private static Control MakeAllControlsPayload(string title)
    {
        string[] controlNames =
        [
            "Button", "CheckBox", "RadioButton", "Label", "LinkLabel",
            "TextBox", "MaskedTextBox", "RichTextBox", "ComboBox", "ListBox",
            "CheckedListBox", "NumericUpDown", "DomainUpDown", "DateTimePicker",
            "MonthCalendar", "ProgressBar", "TrackBar", "HScrollBar", "VScrollBar",
            "PictureBox", "TreeView", "ListView", "DataGridView", "TabControl",
            "Panel", "FlowLayoutPanel", "TableLayoutPanel", "GroupBox",
            "SplitContainer", "UserControl", "MenuStrip", "ToolStrip", "StatusStrip"
        ];

        ListBox selector = new()
        {
            Dock = DockStyle.Left,
            IntegralHeight = false,
            Width = 180
        };
        selector.Items.AddRange(controlNames.Cast<object>().ToArray());

        Panel preview = new()
        {
            BorderStyle = BorderStyle.FixedSingle,
            Dock = DockStyle.Fill,
            Padding = new Padding(8)
        };

        Label header = new()
        {
            Dock = DockStyle.Top,
            Height = 28,
            Text = $"{title}: select a control",
            TextAlign = ContentAlignment.MiddleLeft
        };

        Panel root = new() { Dock = DockStyle.Fill };
        root.Controls.Add(preview);
        root.Controls.Add(selector);
        root.Controls.Add(header);

        selector.SelectedIndexChanged += (_, _) =>
        {
            if (selector.SelectedItem is not string controlName)
            {
                return;
            }

            DisposeChildControls(preview);

            Control control = CreateAllControlsSample(controlName, title);
            control.Dock = DockStyle.Fill;
            preview.Controls.Add(control);
        };

        selector.SelectedIndex = 0;
        return root;
    }

    private static Control CreateAllControlsSample(string controlName, string title)
    {
        switch (controlName)
        {
            case "Button":
                return new Button { Text = "Button" };
            case "CheckBox":
                return new CheckBox { Checked = true, Text = "CheckBox" };
            case "RadioButton":
                return new RadioButton { Checked = true, Text = "RadioButton" };
            case "Label":
                return new Label { Text = $"{title} Label", TextAlign = ContentAlignment.MiddleCenter };
            case "LinkLabel":
                return new LinkLabel { Text = "LinkLabel", TextAlign = ContentAlignment.MiddleCenter };
            case "TextBox":
                return new TextBox { Multiline = true, Text = "TextBox" };
            case "MaskedTextBox":
                return new MaskedTextBox("00/00/0000") { Text = "01012026" };
            case "RichTextBox":
                return new RichTextBox { Text = "RichTextBox" };
            case "ComboBox":
                {
                    ComboBox control = new() { DropDownStyle = ComboBoxStyle.DropDownList };
                    control.Items.AddRange(new object[] { "One", "Two", "Three" });
                    control.SelectedIndex = 0;
                    return control;
                }

            case "ListBox":
                {
                    ListBox control = new();
                    control.Items.AddRange(new object[] { "One", "Two", "Three" });
                    return control;
                }

            case "CheckedListBox":
                {
                    CheckedListBox control = new() { CheckOnClick = true };
                    control.Items.AddRange(new object[] { "One", "Two", "Three" });
                    for (int index = 0; index < control.Items.Count; index++)
                    {
                        control.SetItemChecked(index, true);
                    }

                    return control;
                }

            case "NumericUpDown":
                return new NumericUpDown { Value = 42 };
            case "DomainUpDown":
                return new DomainUpDown
                {
                    Items = { "Low", "Medium", "High" },
                    SelectedIndex = 1
                };
            case "DateTimePicker":
                return new DateTimePicker();
            case "MonthCalendar":
                return new MonthCalendar { MaxSelectionCount = 1 };
            case "ProgressBar":
                return new ProgressBar { Value = 65 };
            case "TrackBar":
                return new TrackBar { Value = 5 };
            case "HScrollBar":
                return new HScrollBar { Value = 25 };
            case "VScrollBar":
                return new VScrollBar { Value = 25 };
            case "PictureBox":
                return new PictureBox
                {
                    BackColor = Color.SteelBlue,
                    BorderStyle = BorderStyle.FixedSingle
                };
            case "TreeView":
                {
                    TreeView control = new();
                    TreeNode root = control.Nodes.Add("Root");
                    root.Nodes.Add("Child 1");
                    root.Nodes.Add("Child 2");
                    control.ExpandAll();
                    return control;
                }

            case "ListView":
                {
                    ListView control = new()
                    {
                        FullRowSelect = true,
                        GridLines = true,
                        View = View.Details
                    };
                    control.Columns.Add("Name", 120);
                    control.Columns.Add("State", 90);
                    control.Items.Add(new ListViewItem(new[] { "Item 1", "Ready" }));
                    control.Items.Add(new ListViewItem(new[] { "Item 2", "Active" }));
                    return control;
                }

            case "DataGridView":
                {
                    DataGridView control = new()
                    {
                        AllowUserToAddRows = false,
                        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                        RowHeadersVisible = false
                    };
                    control.Columns.Add("Name", "Name");
                    control.Columns.Add("Value", "Value");
                    control.Rows.Add("Row 1", 10);
                    control.Rows.Add("Row 2", 20);
                    return control;
                }

            case "TabControl":
                {
                    TabControl control = new();
                    control.TabPages.Add("One");
                    control.TabPages.Add("Two");
                    return control;
                }

            case "Panel":
                return new Panel
                {
                    BackColor = Color.LightSteelBlue,
                    BorderStyle = BorderStyle.FixedSingle
                };
            case "FlowLayoutPanel":
                {
                    FlowLayoutPanel control = new();
                    control.Controls.Add(new Button { Text = "A" });
                    control.Controls.Add(new Button { Text = "B" });
                    return control;
                }

            case "TableLayoutPanel":
                {
                    TableLayoutPanel control = new() { ColumnCount = 2, RowCount = 2 };
                    control.Controls.Add(new Label { Text = "A" }, 0, 0);
                    control.Controls.Add(new Label { Text = "B" }, 1, 1);
                    return control;
                }

            case "GroupBox":
                {
                    GroupBox control = new() { Text = "GroupBox" };
                    control.Controls.Add(new CheckBox
                    {
                        AutoSize = true,
                        Location = new Point(10, 28),
                        Text = "Option"
                    });
                    return control;
                }

            case "SplitContainer":
                {
                    SplitContainer control = new()
                    {
                        IsSplitterFixed = true,
                        SplitterDistance = 100
                    };
                    control.Panel1.Controls.Add(new Label
                    {
                        Dock = DockStyle.Fill,
                        Text = "Panel 1",
                        TextAlign = ContentAlignment.MiddleCenter
                    });
                    control.Panel2.Controls.Add(new Label
                    {
                        Dock = DockStyle.Fill,
                        Text = "Panel 2",
                        TextAlign = ContentAlignment.MiddleCenter
                    });
                    return control;
                }

            case "UserControl":
                return new UserControl
                {
                    BackColor = Color.Lavender,
                    BorderStyle = BorderStyle.FixedSingle
                };
            case "MenuStrip":
                {
                    MenuStrip control = new();
                    control.Items.Add("File");
                    control.Items.Add("Edit");
                    return control;
                }

            case "ToolStrip":
                {
                    ToolStrip control = new() { GripStyle = ToolStripGripStyle.Hidden };
                    control.Items.Add(new ToolStripButton("New"));
                    control.Items.Add(new ToolStripButton("Save"));
                    return control;
                }

            case "StatusStrip":
                {
                    StatusStrip control = new() { SizingGrip = false };
                    control.Items.Add(new ToolStripStatusLabel("Ready"));
                    return control;
                }

            default:
                return new Label
                {
                    Text = $"{controlName} is unavailable.",
                    TextAlign = ContentAlignment.MiddleCenter
                };
        }
    }

    private static Control MakeOwnerDrawPayload(string title)
    {
        TableLayoutPanel content = new()
        {
            ColumnCount = 3,
            Dock = DockStyle.Fill,
            Padding = new Padding(4),
            RowCount = 2
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        ComboBox comboBox = new() { Dock = DockStyle.Top, DrawMode = DrawMode.OwnerDrawVariable, DropDownStyle = ComboBoxStyle.DropDownList };
        comboBox.Items.AddRange(["General", "Layout", "Rendering", "Accessibility", "Performance"]);
        comboBox.MeasureItem += OwnerDrawComboBox_MeasureItem;
        comboBox.DrawItem += OwnerDrawItem;
        comboBox.SelectedIndex = 0;

        ListBox listBox = new() { Dock = DockStyle.Fill, DrawMode = DrawMode.OwnerDrawVariable };
        listBox.Items.AddRange(Enumerable.Range(1, 24).Select(index => $"{title} item {index}").ToArray());
        listBox.MeasureItem += OwnerDrawListBox_MeasureItem;
        listBox.DrawItem += OwnerDrawItem;

        TabControl tabControl = new() { Dock = DockStyle.Fill, DrawMode = TabDrawMode.OwnerDrawFixed };
        tabControl.TabPages.Add(new TabPage("Summary") { BackColor = SystemColors.Window });
        tabControl.TabPages.Add(new TabPage("Metrics") { BackColor = SystemColors.Window });
        tabControl.TabPages.Add(new TabPage("Details") { BackColor = SystemColors.Window });
        tabControl.DrawItem += OwnerDrawTabControl_DrawItem;

        TreeView treeView = new() { Dock = DockStyle.Fill, DrawMode = TreeViewDrawMode.OwnerDrawText };
        for (int section = 1; section <= 8; section++)
        {
            TreeNode parent = treeView.Nodes.Add($"{title} section {section}");
            for (int item = 1; item <= 5; item++)
                parent.Nodes.Add($"Item {section}.{item}");
        }

        treeView.DrawNode += OwnerDrawTreeView_DrawNode;
        treeView.ExpandAll();

        ListView listView = new() { Dock = DockStyle.Fill, FullRowSelect = true, GridLines = true, OwnerDraw = true, View = View.Details };
        listView.Columns.Add("Name", 120);
        listView.Columns.Add("State", 80);
        listView.Columns.Add("Value", 70);
        for (int item = 1; item <= 30; item++)
            listView.Items.Add(new ListViewItem([$"{title} row {item}", item % 2 == 0 ? "Ready" : "Active", $"{item * 11}"]));
        listView.DrawColumnHeader += OwnerDrawListView_DrawColumnHeader;
        listView.DrawItem += OwnerDrawListView_DrawItem;
        listView.DrawSubItem += OwnerDrawListView_DrawSubItem;

        DataGridView grid = new()
        {
            AllowUserToAddRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Dock = DockStyle.Fill,
            RowHeadersVisible = false
        };
        grid.Columns.Add("Control", "Control");
        grid.Columns.Add("Status", "Status");
        grid.Columns.Add("Count", "Count");
        for (int row = 1; row <= 24; row++)
            grid.Rows.Add($"{title} control {row}", row % 3 == 0 ? "Running" : "Ready", row * 17);
        grid.CellPainting += OwnerDrawDataGridView_CellPainting;

        content.Controls.Add(MakeOwnerDrawGroup("ComboBox", comboBox), 0, 0);
        content.Controls.Add(MakeOwnerDrawGroup("ListBox", listBox), 1, 0);
        content.Controls.Add(MakeOwnerDrawGroup("TabControl", tabControl), 2, 0);
        content.Controls.Add(MakeOwnerDrawGroup("TreeView", treeView), 0, 1);
        content.Controls.Add(MakeOwnerDrawGroup("ListView", listView), 1, 1);
        content.Controls.Add(MakeOwnerDrawGroup("DataGridView", grid), 2, 1);

        ToolStrip toolStrip = new() { Dock = DockStyle.Top, GripStyle = ToolStripGripStyle.Hidden, Renderer = new ToolStripProfessionalRenderer() };
        toolStrip.Items.Add(new ToolStripLabel($"{title} drawing controls"));
        toolStrip.Items.Add(new ToolStripButton("Refresh"));
        toolStrip.Items.Add(new ToolStripButton("Export"));

        Panel root = new() { Dock = DockStyle.Fill };
        root.Controls.Add(content);
        root.Controls.Add(toolStrip);

        ContextMenuStrip contextMenu = new();
        contextMenu.Items.Add("Drawing controls");
        contextMenu.Items.Add("Reset view");
        root.ContextMenuStrip = contextMenu;

        ToolTip toolTip = new() { OwnerDraw = true };
        toolTip.Popup += (_, e) => e.ToolTipSize = new Size(180, 30);
        toolTip.Draw += OwnerDrawToolTip_Draw;
        toolTip.SetToolTip(comboBox, "Owner-drawn ComboBox");
        toolTip.SetToolTip(listBox, "Owner-drawn ListBox");
        toolTip.SetToolTip(tabControl, "Owner-drawn TabControl");
        toolTip.SetToolTip(treeView, "Owner-drawn TreeView");
        toolTip.SetToolTip(listView, "Owner-drawn ListView");
        toolTip.SetToolTip(grid, "Cell-painted DataGridView");
        root.Disposed += (_, _) => toolTip.Dispose();
        return root;
    }

    private static GroupBox MakeOwnerDrawGroup(string text, Control control)
    {
        GroupBox group = new() { Dock = DockStyle.Fill, Padding = new Padding(6), Text = text };
        group.Controls.Add(control);
        return group;
    }

    private static void OwnerDrawComboBox_MeasureItem(object? sender, MeasureItemEventArgs e) => e.ItemHeight = 24 + ((e.Index >= 0 ? e.Index : 0) % 2 * 4);
    private static void OwnerDrawListBox_MeasureItem(object? sender, MeasureItemEventArgs e) => e.ItemHeight = 22 + (e.Index % 3 * 3);

    private static void OwnerDrawItem(object? sender, DrawItemEventArgs e)
    {
        if (sender is not ListControl control || e.Index < 0)
            return;
        string text = control switch
        {
            ComboBox comboBox => comboBox.Items[e.Index]?.ToString() ?? string.Empty,
            ListBox listBox => listBox.Items[e.Index]?.ToString() ?? string.Empty,
            _ => string.Empty
        };
        e.DrawBackground();
        using SolidBrush accentBrush = new(e.Index % 2 == 0 ? Color.SteelBlue : Color.DarkOrange);
        e.Graphics.FillRectangle(accentBrush, e.Bounds.X + 3, e.Bounds.Y + 3, 5, Math.Max(1, e.Bounds.Height - 6));
        TextRenderer.DrawText(e.Graphics, text, e.Font, new Rectangle(e.Bounds.X + 12, e.Bounds.Y, Math.Max(0, e.Bounds.Width - 12), e.Bounds.Height), e.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        e.DrawFocusRectangle();
    }

    private static void OwnerDrawTabControl_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (sender is not TabControl tabs)
            return;
        Rectangle bounds = tabs.GetTabRect(e.Index);
        bool selected = e.Index == tabs.SelectedIndex;
        using SolidBrush brush = new(selected ? Color.SteelBlue : SystemColors.Control);
        e.Graphics.FillRectangle(brush, bounds);
        TextRenderer.DrawText(e.Graphics, tabs.TabPages[e.Index].Text, tabs.Font, bounds, selected ? Color.White : SystemColors.ControlText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private static void OwnerDrawTreeView_DrawNode(object? sender, DrawTreeNodeEventArgs e)
    {
        Color back = e.Node.IsSelected ? SystemColors.Highlight : e.Node.TreeView.BackColor;
        Color fore = e.Node.IsSelected ? SystemColors.HighlightText : e.Node.TreeView.ForeColor;
        using SolidBrush brush = new(back);
        e.Graphics.FillRectangle(brush, e.Bounds);
        TextRenderer.DrawText(e.Graphics, e.Node.Text, e.Node.TreeView.Font, e.Bounds, fore, TextFormatFlags.VerticalCenter);
    }

    private static void OwnerDrawListView_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
    {
        e.DrawBackground();
        TextRenderer.DrawText(e.Graphics, e.Header?.Text ?? string.Empty, SystemFonts.DefaultFont, e.Bounds, SystemColors.ControlText, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void OwnerDrawListView_DrawItem(object? sender, DrawListViewItemEventArgs e)
    {
        if (e.Item.ListView?.View != View.Details)
            e.DrawDefault = true;
    }

    private static void OwnerDrawListView_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
    {
        bool selected = e.Item.Selected;
        Color back = selected ? SystemColors.Highlight : e.Item.ListView.BackColor;
        Color fore = selected ? SystemColors.HighlightText : e.Item.ListView.ForeColor;
        using SolidBrush brush = new(back);
        e.Graphics.FillRectangle(brush, e.Bounds);
        TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.Item.Font, Rectangle.Inflate(e.Bounds, -3, 0), fore, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void OwnerDrawDataGridView_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            e.Paint(e.ClipBounds, DataGridViewPaintParts.All);
            e.Handled = true;
            return;
        }

        e.PaintBackground(e.ClipBounds, true);
        Color fore = e.State.HasFlag(DataGridViewElementStates.Selected) ? SystemColors.HighlightText : SystemColors.ControlText;
        TextRenderer.DrawText(e.Graphics, e.FormattedValue?.ToString() ?? string.Empty, e.CellStyle.Font ?? SystemFonts.DefaultFont, Rectangle.Inflate(e.CellBounds, -4, -2), fore, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        e.Paint(e.ClipBounds, DataGridViewPaintParts.Border);
        e.Handled = true;
    }

    private static void OwnerDrawToolTip_Draw(object? sender, DrawToolTipEventArgs e)
    {
        e.DrawBackground();
        e.DrawBorder();
        e.DrawText(TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    }

    private static TableLayoutPanel MakeComplexPayload(string title, bool primaryLayout)
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 2,
            Padding = new Padding(4)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, primaryLayout ? 38 : 55));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, primaryLayout ? 62 : 45));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, primaryLayout ? 45 : 58));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, primaryLayout ? 55 : 42));

        if (primaryLayout)
        {
            root.Controls.Add(MakeTreeView(title), 0, 0);
            root.Controls.Add(MakeTabbedDataView(title), 1, 0);
            root.Controls.Add(MakeInputForm(title), 0, 1);
            root.Controls.Add(MakeMixedNestedLayout(depth: 2, seed: 0), 1, 1);
        }
        else
        {
            root.Controls.Add(MakeInputForm(title), 0, 0);
            root.Controls.Add(MakeMixedNestedLayout(depth: 2, seed: 1), 1, 0);
            root.Controls.Add(MakeTabbedDataView(title), 0, 1);
            root.Controls.Add(MakeTreeView(title), 1, 1);
        }

        return root;
    }

    private static Control MakeNestedSplitContainerPayload(string title)
    {
        SplitContainer outerSplit = new()
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 300
        };

        SplitContainer leftSplit = new()
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 180
        };

        SplitContainer rightSplit = new()
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 180
        };

        SplitContainer thirdLevel = new()
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 220
        };

        leftSplit.Panel1.Controls.Add(MakeTreeView($"{title} Tree"));
        leftSplit.Panel2.Controls.Add(MakeListView($"{title} List"));

        thirdLevel.Panel1.Controls.Add(MakeDataGrid($"{title} Grid"));
        thirdLevel.Panel2.Controls.Add(MakeInputForm($"{title} Form"));

        rightSplit.Panel1.Controls.Add(thirdLevel);
        rightSplit.Panel2.Controls.Add(MakeTabbedDataView($"{title} Tabs"));

        outerSplit.Panel1.Controls.Add(leftSplit);
        outerSplit.Panel2.Controls.Add(rightSplit);

        return outerSplit;
    }

    private static TreeView MakeTreeView(string title)
    {
        TreeView treeView = new()
        {
            Dock = DockStyle.Fill,
            CheckBoxes = true
        };

        for (int section = 0; section < 8; section++)
        {
            TreeNode parent = treeView.Nodes.Add($"{title} section {section + 1}");
            for (int item = 0; item < 6; item++)
            {
                parent.Nodes.Add($"Item {section + 1}.{item + 1}");
            }
        }

        treeView.ExpandAll();
        return treeView;
    }

    private static TabControl MakeTabbedDataView(string title)
    {
        TabControl tabs = new()
        {
            Dock = DockStyle.Fill
        };

        TabPage gridPage = new("Grid");
        gridPage.Controls.Add(MakeDataGrid(title));
        tabs.TabPages.Add(gridPage);

        TabPage listPage = new("Details");
        listPage.Controls.Add(MakeListView(title));
        tabs.TabPages.Add(listPage);

        TabPage textPage = new("Text");
        textPage.Controls.Add(
            new RichTextBox
            {
                Dock = DockStyle.Fill,
                Text = string.Join(
                    Environment.NewLine,
                    Enumerable.Range(1, 30).Select(index => $"{title} diagnostic line {index}"))
            });
        tabs.TabPages.Add(textPage);

        return tabs;
    }

    private static DataGridView MakeDataGrid(string title)
    {
        DataGridView grid = new()
        {
            AllowUserToAddRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Dock = DockStyle.Fill,
            RowHeadersVisible = false
        };
        grid.Columns.Add("Name", "Name");
        grid.Columns.Add("State", "State");
        grid.Columns.Add("Value", "Value");

        for (int row = 0; row < 24; row++)
        {
            grid.Rows.Add($"{title} row {row + 1}", row % 3 == 0 ? "Active" : "Ready", row * 17);
        }

        return grid;
    }

    private static ListView MakeListView(string title)
    {
        ListView listView = new()
        {
            Dock = DockStyle.Fill,
            FullRowSelect = true,
            GridLines = true,
            View = View.Details
        };
        listView.Columns.Add("Control", 140);
        listView.Columns.Add("Status", 90);
        listView.Columns.Add("Count", 70);

        for (int item = 0; item < 40; item++)
        {
            listView.Items.Add(
                new ListViewItem(
                    [$"{title} item {item + 1}", item % 2 == 0 ? "Visible" : "Pending", $"{item * 11}"]));
        }

        return listView;
    }

    private static TableLayoutPanel MakeInputForm(string title)
    {
        TableLayoutPanel form = new()
        {
            AutoScroll = true,
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            RowCount = 9
        };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddInputRow(form, 0, "Name", new TextBox { Dock = DockStyle.Fill, Text = title });
        AddInputRow(
            form,
            1,
            "Category",
            new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = new[] { "General", "Layout", "Rendering", "Interop" }
            });
        AddInputRow(form, 2, "Date", new System.Windows.Forms.DateTimePicker { Dock = DockStyle.Fill });
        AddInputRow(form, 3, "Amount", new NumericUpDown { Dock = DockStyle.Fill, Value = 42 });
        AddInputRow(form, 4, "Enabled", new CheckBox { AutoSize = true, Checked = true });
        AddInputRow(form, 5, "Progress", new ProgressBar { Dock = DockStyle.Fill, Value = 65 });
        AddInputRow(
            form,
            6,
            "Choice",
            new FlowLayoutPanel
            {
                AutoSize = true,
                Controls =
                {
                    new RadioButton { AutoSize = true, Checked = true, Text = "A" },
                    new RadioButton { AutoSize = true, Text = "B" },
                    new RadioButton { AutoSize = true, Text = "C" }
                },
                Dock = DockStyle.Fill
            });
        AddInputRow(
            form,
            7,
            "Notes",
            new TextBox { AcceptsReturn = true, Dock = DockStyle.Fill, Multiline = true, Text = "Resize payload" });
        AddInputRow(
            form,
            8,
            "Actions",
            new FlowLayoutPanel
            {
                AutoSize = true,
                Controls =
                {
                    new Button { AutoSize = true, Text = "Apply" },
                    new Button { AutoSize = true, Text = "Reset" }
                },
                Dock = DockStyle.Fill
            });

        return form;
    }

    private static void AddInputRow(TableLayoutPanel form, int row, string label, Control control)
    {
        form.RowStyles.Add(new RowStyle(row == 7 ? SizeType.Percent : SizeType.AutoSize, row == 7 ? 100 : 0));
        form.Controls.Add(new Label { Anchor = AnchorStyles.Left, AutoSize = true, Text = label }, 0, row);
        form.Controls.Add(control, 1, row);
    }

    private static TableLayoutPanel MakeMixedNestedLayout(int depth, int seed)
    {
        TableLayoutPanel layout = new()
        {
            BackColor = depth % 2 == 0 ? Color.Gainsboro : Color.LightYellow,
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            RowCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        for (int index = 0; index < 4; index++)
        {
            Control child = depth > 0
                ? MakeMixedNestedLayout(depth - 1, seed + index + 1)
                : MakeMixedLeaf(seed + index);
            layout.Controls.Add(child, index % 2, index / 2);
        }

        return layout;
    }

    private static Control MakeMixedLeaf(int index)
        => (index % 6) switch
        {
            0 => new Button { Dock = DockStyle.Fill, Text = $"Action {index}" },
            1 => new TextBox { Dock = DockStyle.Fill, Multiline = true, Text = $"Editor {index}" },
            2 => new ListBox { Dock = DockStyle.Fill, DataSource = new[] { "Alpha", "Beta", "Gamma", "Delta" } },
            3 => new CheckedListBox
            {
                Dock = DockStyle.Fill,
                Items = { "Layout", "Paint", "Input", "Interop" }
            },
            4 => new TrackBar { Dock = DockStyle.Fill, TickFrequency = 2, Value = 5 },
            _ => new MonthCalendar { MaxSelectionCount = 1 }
        };
}
