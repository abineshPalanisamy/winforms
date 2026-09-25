// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;

namespace WinFormsControlsTest;

[DesignerCategory("Default")]
public partial class Buttons : Form
{
    private readonly ListBox _testResultList;
    private readonly Label _instructionLabel;
    private readonly Button _runAllTestsButton;
    private readonly Button _runOriginalIssueButton;
    private readonly Button _clearResultsButton;

    private int _passedCount;
    private int _failedCount;

    public Buttons()
    {
        Point cursorPosition = Cursor.Position;
        StartPosition = FormStartPosition.Manual;
        // Position the DataGridView top-left header under the mouse.
       SetDesktopLocation(
            cursorPosition.X - 30,
            cursorPosition.Y - 40);
        InitializeComponent();

        Text = "DataGridView Fill Column Regression Tests";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(850, 560);

        _instructionLabel = new Label
        {
            AutoSize = false,
            Location = new Point(20, 20),
            Size = new Size(810, 90),
            Text =
                "This sample validates the DataGridView Fill column and AutoSize header issue.\r\n" +
                "Run Original Issue opens the exact reported scenario under the mouse pointer.\r\n" +
                "Run All Tests validates the related scenarios and prints PASS or FAIL results.\r\n" +
                "Detailed results are also available in the Visual Studio Debug Output window."
        };

        _runOriginalIssueButton = new Button
        {
            Location = new Point(20, 125),
            Size = new Size(200, 38),
            Text = "Run Original Issue"
        };

        _runOriginalIssueButton.Click += RunOriginalIssueButton_Click;

        _runAllTestsButton = new Button
        {
            Location = new Point(235, 125),
            Size = new Size(200, 38),
            Text = "Run All Tests"
        };

        _runAllTestsButton.Click += RunAllTestsButton_Click;

        _clearResultsButton = new Button
        {
            Location = new Point(450, 125),
            Size = new Size(160, 38),
            Text = "Clear Results"
        };

        _clearResultsButton.Click += ClearResultsButton_Click;

        _testResultList = new ListBox
        {
            Location = new Point(20, 180),
            Size = new Size(810, 350),
            HorizontalScrollbar = true
        };

        Controls.Add(_instructionLabel);
        Controls.Add(_runOriginalIssueButton);
        Controls.Add(_runAllTestsButton);
        Controls.Add(_clearResultsButton);
        Controls.Add(_testResultList);

        WriteSection("DataGridView regression test sample started");
    }

    private void RunOriginalIssueButton_Click(object sender, EventArgs e)
    {
        WriteSection("Original issue reproduction");

        Point originalCursorPosition = Cursor.Position;

        try
        {
            Point targetLocation = new(
                originalCursorPosition.X - 30,
                originalCursorPosition.Y - 40);

            using DataGridViewTestForm testForm = new(
                DataGridViewTestConfiguration.OriginalIssue,
                targetLocation);

            WriteInformation(
                "Opening a DataGridView with a Fill column and AutoSize column headers.");

            WriteInformation(
                "Keep the mouse stationary over the top-left header area.");

            testForm.ShowDialog(this);

            WriteResult(
                "Original issue form loaded without InvalidOperationException",
                passed: true,
                "No exception occurred during DataGridView handle creation.");
        }
        catch (InvalidOperationException exception)
        {
            bool expectedException =
                exception.Message.Contains(
                    "auto-filled column",
                    StringComparison.OrdinalIgnoreCase);

            WriteResult(
                "Original issue form loaded without InvalidOperationException",
                passed: false,
                $"Exception: {exception.Message}");

            if (expectedException)
            {
                WriteInformation(
                    "The original DataGridView fill-column issue was reproduced.");
            }
        }
        catch (Exception exception)
        {
            WriteResult(
                "Original issue form loaded without an unexpected exception",
                passed: false,
                $"{exception.GetType().Name}: {exception.Message}");
        }
        finally
        {
            Cursor.Position = originalCursorPosition;
        }
    }

    private void RunAllTestsButton_Click(object sender, EventArgs e)
    {
        _passedCount = 0;
        _failedCount = 0;

        WriteSection("Running all DataGridView regression tests");

        RunOriginalConfigurationTest();
        RunFillOnlyTest();
        RunHeaderAutoSizeOnlyTest();
        RunNeitherFeatureEnabledTest();
        RunPreCreatedTopLeftHeaderCellTest();
        RunCustomTopLeftHeaderCellTest();
        RunMultipleFillColumnsTest();
        RunHiddenRowHeadersTest();
        RunHiddenColumnHeadersTest();
        RunFormResizeTest();
        RunHeaderTextChangeTest();
        RunFontChangeTest();
        RunHandleRecreationTest();

        WriteSection(
            $"Completed. Passed: {_passedCount}, Failed: {_failedCount}");
    }

    private void RunOriginalConfigurationTest()
    {
        RunFormTest(
            "Fill column and AutoSize headers",
            DataGridViewTestConfiguration.OriginalIssue,
            grid =>
            {
                bool passed =
                    grid.IsHandleCreated
                    && grid.Columns.Count == 1
                    && grid.Columns[0].Width > 0
                    && grid.ColumnHeadersHeight > 0;

                return new TestResult(
                    passed,
                    $"Handle: {grid.IsHandleCreated}, " +
                    $"Column width: {grid.Columns[0].Width}, " +
                    $"Header height: {grid.ColumnHeadersHeight}");
            });
    }

    private void RunFillOnlyTest()
    {
        RunFormTest(
            "Fill column with fixed header height",
            DataGridViewTestConfiguration.FillOnly,
            grid =>
            {
                bool passed =
                    grid.IsHandleCreated
                    && grid.Columns[0].AutoSizeMode
                        == DataGridViewAutoSizeColumnMode.Fill
                    && grid.Columns[0].Width > 0;

                return new TestResult(
                    passed,
                    $"Column width: {grid.Columns[0].Width}");
            });
    }

    private void RunHeaderAutoSizeOnlyTest()
    {
        RunFormTest(
            "AutoSize headers without a Fill column",
            DataGridViewTestConfiguration.HeaderAutoSizeOnly,
            grid =>
            {
                bool passed =
                    grid.IsHandleCreated
                    && grid.ColumnHeadersHeightSizeMode
                        == DataGridViewColumnHeadersHeightSizeMode.AutoSize
                    && grid.ColumnHeadersHeight > 0;

                return new TestResult(
                    passed,
                    $"Header height: {grid.ColumnHeadersHeight}");
            });
    }

    private void RunNeitherFeatureEnabledTest()
    {
        RunFormTest(
            "Fixed column width and fixed header height",
            DataGridViewTestConfiguration.Neither,
            grid =>
            {
                bool passed =
                    grid.IsHandleCreated
                    && grid.Columns[0].Width > 0
                    && grid.ColumnHeadersHeight > 0;

                return new TestResult(
                    passed,
                    $"Column width: {grid.Columns[0].Width}, " +
                    $"Header height: {grid.ColumnHeadersHeight}");
            });
    }

    private void RunPreCreatedTopLeftHeaderCellTest()
    {
        RunFormTest(
            "Pre-created TopLeftHeaderCell workaround",
            DataGridViewTestConfiguration.PreCreateTopLeftHeaderCell,
            grid =>
            {
                bool passed =
                    grid.IsHandleCreated
                    && grid.TopLeftHeaderCell is not null
                    && grid.Columns[0].Width > 0;

                return new TestResult(
                    passed,
                    $"Cell type: {grid.TopLeftHeaderCell.GetType().Name}, " +
                    $"Column width: {grid.Columns[0].Width}");
            });
    }

    private void RunCustomTopLeftHeaderCellTest()
    {
        using DataGridViewTopLeftHeaderCell customCell = new()
        {
            Value = "Custom"
        };

        RunFormTest(
            "Custom TopLeftHeaderCell is preserved",
            DataGridViewTestConfiguration.OriginalIssue,
            grid =>
            {
                grid.TopLeftHeaderCell = customCell;

                return new TestResult(
                    Passed: true,
                    "Custom cell assigned before handle creation.");
            },
            grid =>
            {
                bool passed =
                    grid.IsHandleCreated
                    && ReferenceEquals(
                        customCell,
                        grid.TopLeftHeaderCell)
                    && Equals(
                        "Custom",
                        grid.TopLeftHeaderCell.Value);

                return new TestResult(
                    passed,
                    $"Same instance: " +
                    $"{ReferenceEquals(customCell, grid.TopLeftHeaderCell)}, " +
                    $"Value: {grid.TopLeftHeaderCell.Value}");
            });
    }

    private void RunMultipleFillColumnsTest()
    {
        RunFormTest(
            "Multiple Fill columns",
            DataGridViewTestConfiguration.MultipleFillColumns,
            grid =>
            {
                bool allColumnsHaveWidth = true;

                foreach (DataGridViewColumn column in grid.Columns)
                {
                    if (column.Width <= 0)
                    {
                        allColumnsHaveWidth = false;
                        break;
                    }
                }

                bool passed =
                    grid.IsHandleCreated
                    && grid.Columns.Count == 3
                    && allColumnsHaveWidth;

                return new TestResult(
                    passed,
                    $"Widths: {grid.Columns[0].Width}, " +
                    $"{grid.Columns[1].Width}, " +
                    $"{grid.Columns[2].Width}");
            });
    }

    private void RunHiddenRowHeadersTest()
    {
        RunFormTest(
            "Fill column with hidden row headers",
            DataGridViewTestConfiguration.HiddenRowHeaders,
            grid =>
            {
                bool passed =
                    grid.IsHandleCreated
                    && !grid.RowHeadersVisible
                    && grid.Columns[0].Width > 0;

                return new TestResult(
                    passed,
                    $"RowHeadersVisible: {grid.RowHeadersVisible}, " +
                    $"Column width: {grid.Columns[0].Width}");
            });
    }

    private void RunHiddenColumnHeadersTest()
    {
        RunFormTest(
            "Fill column with hidden column headers",
            DataGridViewTestConfiguration.HiddenColumnHeaders,
            grid =>
            {
                bool passed =
                    grid.IsHandleCreated
                    && !grid.ColumnHeadersVisible
                    && grid.Columns[0].Width > 0;

                return new TestResult(
                    passed,
                    $"ColumnHeadersVisible: {grid.ColumnHeadersVisible}, " +
                    $"Column width: {grid.Columns[0].Width}");
            });
    }

    private void RunFormResizeTest()
    {
        RunFormTest(
            "Fill column adjusts after form resize",
            DataGridViewTestConfiguration.OriginalIssue,
            grid =>
            {
                int originalWidth = grid.Columns[0].Width;

                grid.FindForm()!.ClientSize =
                    new Size(800, 450);

                Application.DoEvents();

                int resizedWidth = grid.Columns[0].Width;

                bool passed =
                    originalWidth > 0
                    && resizedWidth > originalWidth;

                return new TestResult(
                    passed,
                    $"Original width: {originalWidth}, " +
                    $"Resized width: {resizedWidth}");
            });
    }

    private void RunHeaderTextChangeTest()
    {
        RunFormTest(
            "AutoSize header remains valid after text change",
            DataGridViewTestConfiguration.OriginalIssue,
            grid =>
            {
                int originalHeight = grid.ColumnHeadersHeight;

                grid.Columns[0].HeaderText =
                    "Long header text used to validate " +
                    "automatic header measurement";

                grid.AutoResizeColumnHeadersHeight();
                Application.DoEvents();

                int updatedHeight = grid.ColumnHeadersHeight;

                bool passed =
                    originalHeight > 0
                    && updatedHeight > 0;

                return new TestResult(
                    passed,
                    $"Original height: {originalHeight}, " +
                    $"Updated height: {updatedHeight}");
            });
    }

    private void RunFontChangeTest()
    {
        RunFormTest(
            "AutoSize header remains valid after font change",
            DataGridViewTestConfiguration.OriginalIssue,
            grid =>
            {
                int originalHeight = grid.ColumnHeadersHeight;

                using Font largerFont = new(
                    grid.Font.FontFamily,
                    grid.Font.Size + 4,
                    grid.Font.Style);

                grid.Font = largerFont;
                grid.AutoResizeColumnHeadersHeight();
                Application.DoEvents();

                int updatedHeight = grid.ColumnHeadersHeight;

                bool passed =
                    originalHeight > 0
                    && updatedHeight >= originalHeight;

                return new TestResult(
                    passed,
                    $"Original height: {originalHeight}, " +
                    $"Updated height: {updatedHeight}");
            });
    }

    private void RunHandleRecreationTest()
    {
        RunFormTest(
            "Handle recreation preserves TopLeftHeaderCell",
            DataGridViewTestConfiguration.OriginalIssue,
            grid =>
            {
                DataGridViewHeaderCell originalCell =
                    grid.TopLeftHeaderCell;

                if (grid is TestDataGridView testGrid)
                {
                    testGrid.RecreateControlHandle();
                }

                bool passed =
                    grid.IsHandleCreated
                    && ReferenceEquals(
                        originalCell,
                        grid.TopLeftHeaderCell)
                    && grid.Columns[0].Width > 0;

                return new TestResult(
                    passed,
                    $"Handle created: {grid.IsHandleCreated}, " +
                    $"Same cell: " +
                    $"{ReferenceEquals(originalCell, grid.TopLeftHeaderCell)}");
            });
    }

    private void RunFormTest(
        string testName,
        DataGridViewTestConfiguration configuration,
        Func<DataGridView, TestResult> test)
    {
        RunFormTest(
            testName,
            configuration,
            beforeShow: null,
            afterShow: test);
    }

    private void RunFormTest(
        string testName,
        DataGridViewTestConfiguration configuration,
        Func<DataGridView, TestResult>? beforeShow,
        Func<DataGridView, TestResult> afterShow)
    {
        Point originalCursorPosition = Cursor.Position;

        try
        {
            Point formLocation = GetSafeFormLocation();

            using DataGridViewTestForm testForm = new(
                configuration,
                formLocation,
                createHandleImmediately: false);

            if (beforeShow is not null)
            {
                TestResult beforeShowResult =
                    beforeShow(testForm.Grid);

                if (!beforeShowResult.Passed)
                {
                    WriteResult(
                        testName,
                        passed: false,
                        $"Precondition failed. {beforeShowResult.Details}");

                    return;
                }
            }

            PositionCursorOverTopLeftHeader(
                testForm,
                testForm.Grid);

            testForm.Show();
            Application.DoEvents();

            TestResult result =
                afterShow(testForm.Grid);

            WriteResult(
                testName,
                result.Passed,
                result.Details);

            testForm.Close();
        }
        catch (Exception exception)
        {
            WriteResult(
                testName,
                passed: false,
                $"{exception.GetType().Name}: {exception.Message}");
        }
        finally
        {
            Cursor.Position = originalCursorPosition;
        }
    }

    private static Point GetSafeFormLocation()
    {
        Rectangle workingArea =
            Screen.PrimaryScreen!.WorkingArea;

        int x = workingArea.Left + 100;
        int y = workingArea.Top + 100;

        return new Point(x, y);
    }

    private static void PositionCursorOverTopLeftHeader(
        Form form,
        DataGridView grid)
    {
        Point targetPoint = new(
            form.Location.X + grid.RowHeadersWidth / 2,
            form.Location.Y + grid.ColumnHeadersHeight / 2);

        Cursor.Position = targetPoint;
    }

    private void WriteSection(string text)
    {
        string message =
            $"{Environment.NewLine}" +
            "==============================================" +
            $"{Environment.NewLine}{text}" +
            $"{Environment.NewLine}" +
            "==============================================";

        Debug.WriteLine(message);
        _testResultList.Items.Add(text);
        ScrollToLastResult();
    }

    private void WriteInformation(string text)
    {
        Debug.WriteLine($"[INFO] {text}");
        _testResultList.Items.Add($"INFO: {text}");
        ScrollToLastResult();
    }

    private void WriteResult(
        string testName,
        bool passed,
        string details)
    {
        string result = passed ? "PASS" : "FAIL";

        if (passed)
        {
            _passedCount++;
        }
        else
        {
            _failedCount++;
        }

        string message =
            $"[{result}] {testName}. {details}";

        Debug.WriteLine(message);
        _testResultList.Items.Add(message);
        ScrollToLastResult();
    }

    private void ScrollToLastResult()
    {
        if (_testResultList.Items.Count > 0)
        {
            _testResultList.TopIndex =
                _testResultList.Items.Count - 1;
        }
    }

    private void ClearResultsButton_Click(
        object sender,
        EventArgs e)
    {
        _testResultList.Items.Clear();
        _passedCount = 0;
        _failedCount = 0;

        Debug.WriteLine(string.Empty);
        Debug.WriteLine("Test results cleared.");
    }

    private readonly record struct TestResult(
        bool Passed,
        string Details);

    private enum DataGridViewTestConfiguration
    {
        OriginalIssue,
        FillOnly,
        HeaderAutoSizeOnly,
        Neither,
        PreCreateTopLeftHeaderCell,
        MultipleFillColumns,
        HiddenRowHeaders,
        HiddenColumnHeaders
    }

    private sealed class DataGridViewTestForm : Form
    {
        public DataGridViewTestForm(
            DataGridViewTestConfiguration configuration,
            Point location,
            bool createHandleImmediately = true)
        {
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.None;
            Location = location;
            ClientSize = new Size(600, 350);
            ShowInTaskbar = false;

            Grid = new TestDataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false
            };

            ConfigureGrid(configuration);

            if (createHandleImmediately)
            {
                Cursor.Position = new Point(
                    location.X + Grid.RowHeadersWidth / 2,
                    location.Y + Grid.ColumnHeadersHeight / 2);
            }

            Controls.Add(Grid);
        }

        public TestDataGridView Grid { get; }

        private void ConfigureGrid(
            DataGridViewTestConfiguration configuration)
        {
            switch (configuration)
            {
                case DataGridViewTestConfiguration.OriginalIssue:
                    AddFillColumn();
                    EnableAutoSizedHeaders();
                    break;

                case DataGridViewTestConfiguration.FillOnly:
                    AddFillColumn();
                    break;

                case DataGridViewTestConfiguration.HeaderAutoSizeOnly:
                    AddFixedColumn();
                    EnableAutoSizedHeaders();
                    break;

                case DataGridViewTestConfiguration.Neither:
                    AddFixedColumn();
                    break;

                case DataGridViewTestConfiguration.PreCreateTopLeftHeaderCell:
                    AddFillColumn();
                    EnableAutoSizedHeaders();
                    _ = Grid.TopLeftHeaderCell;
                    break;

                case DataGridViewTestConfiguration.MultipleFillColumns:
                    AddMultipleFillColumns();
                    EnableAutoSizedHeaders();
                    break;

                case DataGridViewTestConfiguration.HiddenRowHeaders:
                    AddFillColumn();
                    EnableAutoSizedHeaders();
                    Grid.RowHeadersVisible = false;
                    break;

                case DataGridViewTestConfiguration.HiddenColumnHeaders:
                    AddFillColumn();
                    EnableAutoSizedHeaders();
                    Grid.ColumnHeadersVisible = false;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(configuration),
                        configuration,
                        message: null);
            }
        }

        private void AddFillColumn()
        {
            int columnIndex = Grid.Columns.Add(
                "ColumnName",
                "HeaderText");

            Grid.Columns[columnIndex].AutoSizeMode =
                DataGridViewAutoSizeColumnMode.Fill;

            Grid.Rows.Add("Test value");
        }

        private void AddFixedColumn()
        {
            int columnIndex = Grid.Columns.Add(
                "ColumnName",
                "HeaderText");

            Grid.Columns[columnIndex].AutoSizeMode =
                DataGridViewAutoSizeColumnMode.None;

            Grid.Columns[columnIndex].Width = 150;

            Grid.Rows.Add("Test value");
        }

        private void AddMultipleFillColumns()
        {
            for (int index = 0; index < 3; index++)
            {
                int columnIndex = Grid.Columns.Add(
                    $"Column{index}",
                    $"Header {index}");

                Grid.Columns[columnIndex].AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.Fill;

                Grid.Columns[columnIndex].FillWeight =
                    index + 1;
            }

            Grid.Rows.Add(
                "First",
                "Second",
                "Third");
        }

        private void EnableAutoSizedHeaders()
        {
            Grid.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        }
    }

    private sealed class TestDataGridView : DataGridView
    {
        public void RecreateControlHandle()
        {
            RecreateHandle();
        }
    }

}
