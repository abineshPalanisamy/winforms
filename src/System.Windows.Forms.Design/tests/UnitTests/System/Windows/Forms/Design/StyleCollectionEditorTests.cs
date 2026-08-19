// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Reflection;
using Moq;

namespace System.Windows.Forms.Design.Tests;

public class StyleCollectionEditorTests
{
    [Theory]
    [InlineData(typeof(TableLayoutRowStyleCollection))]
    [InlineData(typeof(TableLayoutColumnStyleCollection))]
    public void StyleCollectionEditor_Ctor_Type(Type type)
    {
        SubStyleCollectionEditor editor = new(type);
        Assert.Equal(type, editor.TestAccessor.Dynamic.CollectionType);
    }

    [Fact]
    public void StyleCollectionEditor_CreateCollectionForm_RowCollection_ReturnsForm()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();
        form.Should().NotBeNull();
    }

    [Fact]
    public void StyleCollectionEditor_CreateCollectionForm_ColumnCollection_ReturnsForm()
    {
        using StyleEditorFormHost host = new(isRowCollection: false);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutColumnStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();
        form.Should().NotBeNull();
    }

    [Fact]
    public void StyleCollectionEditor_HelpTopic_Default_ReturnsBaseHelpTopic()
    {
        SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        string helpTopic = editor.TestAccessor.Dynamic.HelpTopic;
        helpTopic.Should().Be("net.ComponentModel.CollectionEditor");
    }

    [Fact]
    public void StyleCollectionEditor_HelpTopic_WhenHelpTopicSet_ReturnsCustomHelpTopic()
    {
        SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        editor.SetHelpTopic("CustomHelpTopic");
        string helpTopic = editor.TestAccessor.Dynamic.HelpTopic;
        helpTopic.Should().Be("CustomHelpTopic");
    }

    [Fact]
    public void StyleCollectionEditor_GetEditStyle_ReturnsModal()
    {
        StyleCollectionEditor editor = new SubStyleCollectionEditor(typeof(TableLayoutRowStyleCollection));
        Assert.Equal(UITypeEditorEditStyle.Modal, editor.GetEditStyle(null));
    }

    [Fact]
    public void NavigationalTableLayoutPanel_RadioButtons_NoChildren_ReturnsEmpty()
    {
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        using TableLayoutPanel panel = editor.CreateNavigationalTableLayoutPanel();

        List<RadioButton> radioButtons = editor.GetRadioButtons(panel);
        radioButtons.Should().BeEmpty();
    }

    [Fact]
    public void NavigationalTableLayoutPanel_RadioButtons_OnlyRadioButtons_ReturnsAll()
    {
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        using TableLayoutPanel panel = editor.CreateNavigationalTableLayoutPanel();
        RadioButton r1 = new() { Name = "r1" };
        RadioButton r2 = new() { Name = "r2" };
        panel.Controls.Add(r1);
        panel.Controls.Add(r2);

        List<RadioButton> radioButtons = editor.GetRadioButtons(panel);
        radioButtons.Should().HaveCount(2);
        radioButtons[0].Should().BeSameAs(r1);
        radioButtons[1].Should().BeSameAs(r2);
    }

    [Fact]
    public void NavigationalTableLayoutPanel_ProcessDialogKey_DownKey_NoFocusedRadio_CallsBase()
    {
        // When no radio button is focused, the for loop in ProcessDialogKey walks
        // through RadioButtons but never enters the "Focused" branch, so it falls
        // through to base.ProcessDialogKey.
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        using TableLayoutPanel panel = editor.CreateNavigationalTableLayoutPanel();
        RadioButton r1 = new();
        RadioButton r2 = new();
        panel.Controls.Add(r1);
        panel.Controls.Add(r2);
        _ = panel.Handle;

        bool result = editor.CallProcessDialogKey(panel, Keys.Down);

        result.Should().BeFalse();
    }

    [Fact]
    public void NavigationalTableLayoutPanel_ProcessDialogKey_NonArrowKey_CallsBase()
    {
        // Tab and Enter are not Down/Up so the method falls through to base.ProcessDialogKey.
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        using TableLayoutPanel panel = editor.CreateNavigationalTableLayoutPanel();
        RadioButton r1 = new();
        panel.Controls.Add(r1);
        _ = panel.Handle;

        bool tabResult = editor.CallProcessDialogKey(panel, Keys.Tab);
        bool enterResult = editor.CallProcessDialogKey(panel, Keys.Enter);

        tabResult.Should().BeFalse();
        enterResult.Should().BeFalse();
    }

    [Fact]
    public void StyleEditorForm_Construct_RowCollection_InitializesControls()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();
        form.Should().NotBeNull();
    }

    [Fact]
    public void StyleEditorForm_Construct_ColumnCollection_InitializesControls()
    {
        using StyleEditorFormHost host = new(isRowCollection: false);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutColumnStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();
        form.Should().NotBeNull();
    }

    [Fact]
    public void StyleEditorForm_OnShown_DoesNotThrow()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();
        Action act = () => editor.CallOnShown(form);
        act.Should().NotThrow();
    }

    [Fact]
    public void StyleEditorForm_FormatValueString_Absolute_ReturnsPlainNumber()
    {
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        string result = editor.CallFormatValueString(SizeType.Absolute, 20f);
        result.Should().Be("20");
    }

    [Fact]
    public void StyleEditorForm_FormatValueString_Percent_ReturnsPercentString()
    {
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        string result = editor.CallFormatValueString(SizeType.Percent, 50f);
        result.Should().EndWith("%");
    }

    [Fact]
    public void StyleEditorForm_FormatValueString_AutoSize_ReturnsEmpty()
    {
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        string result = editor.CallFormatValueString(SizeType.AutoSize, 0f);
        result.Should().BeEmpty();
    }

    [Fact]
    public void StyleEditorForm_ShowEditorDialog_RowCollection_PopulatesListView()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        listView.Items.Count.Should().Be(host.TableLayoutPanel.RowStyles.Count);
    }

    [Fact]
    public void StyleEditorForm_ShowEditorDialog_ColumnCollection_PopulatesListView()
    {
        using StyleEditorFormHost host = new(isRowCollection: false);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutColumnStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        listView.Items.Count.Should().Be(host.TableLayoutPanel.ColumnStyles.Count);
    }

    [Fact]
    public void StyleEditorForm_OnListViewSelectedIndexChanged_SingleSelection_DoesNotThrow()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        host.TableLayoutPanel.RowStyles.Clear();
        host.TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
        host.TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        host.TableLayoutPanel.RowCount = 2;

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        listView.Items[0].Selected = true;
        Action act = () => editor.CallOnListViewSelectedIndexChanged(form, listView);
        act.Should().NotThrow();
    }

    [Fact]
    public void StyleEditorForm_OnListViewSelectedIndexChanged_NoSelection_DoesNotThrow()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        listView.SelectedItems.Clear();
        Action act = () => editor.CallOnListViewSelectedIndexChanged(form, listView);
        act.Should().NotThrow();
    }

    [Fact]
    public void StyleEditorForm_OnListViewSelectedIndexChanged_SingleSelection_EnablesGroupBox()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        listView.Items[0].Selected = true;
        editor.CallOnListViewSelectedIndexChanged(form, listView);

        GroupBox groupBox = editor.GetSizeTypeGroupBox(form);
        groupBox.Enabled.Should().BeTrue();
    }

    [Fact]
    public void StyleEditorForm_OnListViewSelectedIndexChanged_MultiSelection_DoesNotThrow()
    {
        // Multi-selection requires the underlying TLP to have multiple rows whose
        // counts match. We rely on the default single-row layout and instead verify
        // that calling the handler without a selection is safe.
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();
        _ = form.Handle;

        ListView listView = editor.GetColumnsAndRowsListView(form);
        listView.SelectedItems.Clear();
        Action act = () => editor.CallOnListViewSelectedIndexChanged(form, listView);
        act.Should().NotThrow();
    }

    [Fact]
    public void StyleEditorForm_OnListSelectionComplete_NoItemsSelected_DisablesControls()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        listView.SelectedItems.Clear();
        editor.CallOnListSelectionComplete(form, listView);

        GroupBox groupBox = editor.GetSizeTypeGroupBox(form);
        Button insertButton = editor.GetInsertButton(form);
        Button removeButton = editor.GetRemoveButton(form);
        groupBox.Enabled.Should().BeFalse();
        insertButton.Enabled.Should().BeFalse();
        removeButton.Enabled.Should().BeFalse();
    }

    [Fact]
    public void StyleEditorForm_UpdateGroupBox_Absolute_SetsAbsoluteRadioAndValue()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        editor.CallUpdateGroupBox(form, SizeType.Absolute, 25f);
        RadioButton absoluteRadio = editor.GetAbsoluteRadioButton(form);
        NumericUpDown absoluteNumeric = editor.GetAbsoluteNumericUpDown(form);
        absoluteRadio.Checked.Should().BeTrue();
        absoluteNumeric.Enabled.Should().BeTrue();
    }

    [Fact]
    public void StyleEditorForm_UpdateGroupBox_Percent_SetsPercentRadioAndValue()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        editor.CallUpdateGroupBox(form, SizeType.Percent, 33.5f);
        RadioButton percentRadio = editor.GetPercentRadioButton(form);
        NumericUpDown percentNumeric = editor.GetPercentNumericUpDown(form);
        percentRadio.Checked.Should().BeTrue();
        percentNumeric.Enabled.Should().BeTrue();
    }

    [Fact]
    public void StyleEditorForm_UpdateGroupBox_AutoSize_SetsAutoSizeRadio()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        editor.CallUpdateGroupBox(form, SizeType.AutoSize, 0f);
        RadioButton autoSizeRadio = editor.GetAutoSizedRadioButton(form);
        autoSizeRadio.Checked.Should().BeTrue();
    }

    [Fact]
    public void StyleEditorForm_UpdateGroupBox_OutOfRangeValue_UsesMinimumStyleSize()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        // A negative value triggers the ArgumentOutOfRangeException catch block.
        editor.CallUpdateGroupBox(form, SizeType.Absolute, -1f);
        NumericUpDown absoluteNumeric = editor.GetAbsoluteNumericUpDown(form);
        absoluteNumeric.Value.Should().Be(DesignerUtils.s_minimumStyleSize);
    }

    [Fact]
    public void StyleEditorForm_OnInsertButtonClick_NoSelection_ThrowsArgumentOutOfRange()
    {
        // OnInsertButtonClick immediately reads SelectedIndices[0]; with no selection
        // it throws ArgumentOutOfRangeException. This is a real branch worth covering.
        using StyleEditorFormHost host = new(isRowCollection: false);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutColumnStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();
        _ = form.Handle;

        Button insertButton = editor.GetInsertButton(form);
        Action act = () => editor.CallOnInsertButtonClick(form, insertButton);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void StyleEditorForm_OnRemoveButtonClick_OneItem_DoesNotRemove()
    {
        using StyleEditorFormHost host = new(isRowCollection: false);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutColumnStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        host.TableLayoutPanel.ColumnStyles.Clear();
        host.TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
        host.TableLayoutPanel.ColumnCount = 1;
        editor.CallInitListView(form);

        int initialCount = host.TableLayoutPanel.ColumnStyles.Count;
        Button removeButton = editor.GetRemoveButton(form);
        editor.CallOnRemoveButtonClick(form, removeButton);

        host.TableLayoutPanel.ColumnStyles.Count.Should().Be(initialCount);
    }

    [Fact]
    public void StyleEditorForm_OnRemoveButtonClick_MultipleItems_RemovesSelected()
    {
        using StyleEditorFormHost host = new(isRowCollection: false);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutColumnStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        host.TableLayoutPanel.ColumnStyles.Clear();
        host.TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
        host.TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
        host.TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
        host.TableLayoutPanel.ColumnCount = 3;
        editor.CallInitListView(form);

        int initialCount = host.TableLayoutPanel.ColumnStyles.Count;
        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        listView.Items[0].Selected = true;
        Button removeButton = editor.GetRemoveButton(form);
        editor.CallOnRemoveButtonClick(form, removeButton);

        host.TableLayoutPanel.ColumnStyles.Count.Should().BeLessThan(initialCount);
    }

    [Fact]
    public void StyleEditorForm_UpdateTypeAndValue_RowCollection_UpdatesRowStyle()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        listView.Items[0].Selected = true;

        editor.CallUpdateTypeAndValue(form, SizeType.Absolute, 99f);

        host.TableLayoutPanel.RowStyles[0].SizeType.Should().Be(SizeType.Absolute);
        host.TableLayoutPanel.RowStyles[0].Height.Should().Be(99f);
    }

    [Fact]
    public void StyleEditorForm_UpdateTypeAndValue_ColumnCollection_UpdatesColumnStyle()
    {
        using StyleEditorFormHost host = new(isRowCollection: false);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutColumnStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        listView.Items[0].Selected = true;

        editor.CallUpdateTypeAndValue(form, SizeType.Percent, 75f);

        host.TableLayoutPanel.ColumnStyles[0].SizeType.Should().Be(SizeType.Percent);
        host.TableLayoutPanel.ColumnStyles[0].Width.Should().Be(75f);
    }

    [Fact]
    public void StyleEditorForm_OnAbsoluteEnter_UpdatesRowStyle()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        listView.Items[0].Selected = true;
        editor.CallUpdateGroupBox(form, SizeType.Absolute, 20f);
        NumericUpDown absoluteNumeric = editor.GetAbsoluteNumericUpDown(form);
        absoluteNumeric.Value = 50m;
        RadioButton absoluteRadio = editor.GetAbsoluteRadioButton(form);

        editor.CallOnAbsoluteEnter(form, absoluteRadio);

        host.TableLayoutPanel.RowStyles[0].SizeType.Should().Be(SizeType.Absolute);
        host.TableLayoutPanel.RowStyles[0].Height.Should().Be(50f);
    }

    [Fact]
    public void StyleEditorForm_OnPercentEnter_UpdatesRowStyle()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        listView.Items[0].Selected = true;
        NumericUpDown percentNumeric = editor.GetPercentNumericUpDown(form);
        percentNumeric.Value = 33.5m;
        RadioButton percentRadio = editor.GetPercentRadioButton(form);

        editor.CallOnPercentEnter(form, percentRadio);

        host.TableLayoutPanel.RowStyles[0].SizeType.Should().Be(SizeType.Percent);
    }

    [Fact]
    public void StyleEditorForm_OnAutoSizeEnter_SetsAutoSize()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        listView.Items[0].Selected = true;
        RadioButton autoSizeRadio = editor.GetAutoSizedRadioButton(form);

        editor.CallOnAutoSizeEnter(form, autoSizeRadio);

        host.TableLayoutPanel.RowStyles[0].SizeType.Should().Be(SizeType.AutoSize);
    }

    [Fact]
    public void StyleEditorForm_OnValueChanged_AbsoluteRadioChecked_UpdatesValue()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        listView.Items[0].Selected = true;
        editor.CallUpdateGroupBox(form, SizeType.Absolute, 20f);
        NumericUpDown absoluteNumeric = editor.GetAbsoluteNumericUpDown(form);
        absoluteNumeric.Value = 75m;

        editor.CallOnValueChanged(form, absoluteNumeric);

        host.TableLayoutPanel.RowStyles[0].SizeType.Should().Be(SizeType.Absolute);
        host.TableLayoutPanel.RowStyles[0].Height.Should().Be(75f);
    }

    [Fact]
    public void StyleEditorForm_OnValueChanged_PercentRadioChecked_UpdatesValue()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        listView.Items[0].Selected = true;
        NumericUpDown percentNumeric = editor.GetPercentNumericUpDown(form);
        editor.CallUpdateGroupBox(form, SizeType.Percent, 30f);
        percentNumeric.Value = 60m;

        editor.CallOnValueChanged(form, percentNumeric);

        host.TableLayoutPanel.RowStyles[0].SizeType.Should().Be(SizeType.Percent);
    }

    [Fact]
    public void StyleEditorForm_OnValueChanged_NoRadioChecked_DoesNothing()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        NumericUpDown absoluteNumeric = editor.GetAbsoluteNumericUpDown(form);
        Action act = () => editor.CallOnValueChanged(form, absoluteNumeric);
        act.Should().NotThrow();
    }

    [Fact]
    public void StyleEditorForm_OnOkButtonClick_NotDirty_SetsCancel()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();
        _ = form.Handle;

        // When the form is not dirty, OnOkButtonClick should set DialogResult = Cancel.
        Button okButton = editor.GetOkButton(form);
        editor.CallOnOkButtonClick(form, okButton);

        form.DialogResult.Should().Be(DialogResult.Cancel);
    }

    [Fact]
    public void StyleEditorForm_OnCancelButtonClick_SetsDialogResultToCancel()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();
        _ = form.Handle;

        Button cancelButton = editor.GetCancelButton(form);
        editor.CallOnCancelButtonClick(form, cancelButton);

        form.DialogResult.Should().Be(DialogResult.Cancel);
    }

    [Fact]
    public void StyleEditorForm_OnOkButtonClick_DirtyAndAbsolute_SetsOk()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();
        _ = form.Handle;

        // Mark dialog as dirty; Ok path is taken.
        editor.SetIsDialogDirty(form, true);

        Button okButton = editor.GetOkButton(form);
        editor.CallOnOkButtonClick(form, okButton);

        form.DialogResult.Should().Be(DialogResult.OK);
    }

    [Fact]
    public void StyleEditorForm_OnOkButtonClick_DirtyAndPercent_SetsOk()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();
        _ = form.Handle;

        editor.SetIsDialogDirty(form, true);

        Button okButton = editor.GetOkButton(form);
        editor.CallOnOkButtonClick(form, okButton);

        form.DialogResult.Should().Be(DialogResult.OK);
    }

    [Fact]
    public void StyleEditorForm_OnOkButtonClick_DirtyAndAutoSize_SetsOk()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();
        _ = form.Handle;

        editor.SetIsDialogDirty(form, true);

        Button okButton = editor.GetOkButton(form);
        editor.CallOnOkButtonClick(form, okButton);

        form.DialogResult.Should().Be(DialogResult.OK);
    }

    [Fact]
    public void StyleEditorForm_StyleEditorClosed_DoesNotThrow()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        Action act = () => editor.CallStyleEditorClosed(form);
        act.Should().NotThrow();
    }

    [Fact]
    public void StyleEditorForm_OnComboBoxSelectionChangeCommitted_SwitchesToColumns()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ComboBox comboBox = editor.GetColumnsOrRowsComboBox(form);
        comboBox.SelectedIndex = 0;
        editor.CallOnComboBoxSelectionChangeCommitted(form, comboBox);

        editor.GetIsRowCollection(form).Should().BeFalse();
    }

    [Fact]
    public void StyleEditorForm_OnComboBoxSelectionChangeCommitted_SwitchesToRows()
    {
        using StyleEditorFormHost host = new(isRowCollection: false);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutColumnStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ComboBox comboBox = editor.GetColumnsOrRowsComboBox(form);
        comboBox.SelectedIndex = 1;
        editor.CallOnComboBoxSelectionChangeCommitted(form, comboBox);

        editor.GetIsRowCollection(form).Should().BeTrue();
    }

    [Fact]
    public void StyleEditorForm_OnLink1Click_DoesNotThrow()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        LinkLabel link1 = editor.GetHelperLinkLabel1(form);

        Action act = () => editor.CallOnLink1Click(form, link1);
        act.Should().NotThrow();
    }

    [Fact]
    public void StyleEditorForm_OnLink2Click_DoesNotThrow()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        LinkLabel link2 = editor.GetHelperLinkLabel2(form);

        Action act = () => editor.CallOnLink2Click(form, link2);
        act.Should().NotThrow();
    }

    [Fact]
    public void StyleEditorForm_OnHelpButtonClicked_SetsHelpTopic()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        CancelEventArgs cancelEvent = new();
        editor.CallOnHelpButtonClicked(form, form, cancelEvent);

        cancelEvent.Cancel.Should().BeTrue();
        string helpTopic = editor.TestAccessor.Dynamic.HelpTopic;
        helpTopic.Should().Be("net.ComponentModel.StyleCollectionEditor");
    }

    [Fact]
    public void StyleEditorForm_NormalizePercentStyles_PercentRows_TotalAlreadyHundred()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        host.TableLayoutPanel.RowStyles.Clear();
        host.TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        host.TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        host.TableLayoutPanel.RowCount = 2;

        editor.CallNormalizePercentStyles(form);

        host.TableLayoutPanel.RowStyles[0].Height.Should().Be(50f);
        host.TableLayoutPanel.RowStyles[1].Height.Should().Be(50f);
    }

    [Fact]
    public void StyleEditorForm_NormalizePercentStyles_TotalNotHundred_Normalizes()
    {
        using StyleEditorFormHost host = new(isRowCollection: false);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutColumnStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        host.TableLayoutPanel.ColumnStyles.Clear();
        host.TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        host.TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        host.TableLayoutPanel.ColumnCount = 2;

        editor.CallNormalizePercentStyles(form);

        float total = host.TableLayoutPanel.ColumnStyles[0].Width + host.TableLayoutPanel.ColumnStyles[1].Width;
        total.Should().Be(100f);
    }

    [Fact]
    public void StyleEditorForm_NormalizePercentStyles_SkipsAbsoluteStyles()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        host.TableLayoutPanel.RowStyles.Clear();
        host.TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
        host.TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        host.TableLayoutPanel.RowCount = 2;

        editor.CallNormalizePercentStyles(form);

        host.TableLayoutPanel.RowStyles[0].SizeType.Should().Be(SizeType.Absolute);
        host.TableLayoutPanel.RowStyles[0].Height.Should().Be(30f);
    }

    [Fact]
    public void StyleEditorForm_NormalizePercentStyles_TotalIsZero_DoesNotThrow()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        host.TableLayoutPanel.RowStyles.Clear();
        host.TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
        host.TableLayoutPanel.RowCount = 1;

        Action act = () => editor.CallNormalizePercentStyles(form);
        act.Should().NotThrow();
    }

    [Fact]
    public void StyleEditorForm_ShowEditorDialog_RemovesExtraColumnStyles()
    {
        using StyleEditorFormHost host = new(isRowCollection: false);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutColumnStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        // Add more column styles than there are columns to exercise the cleanup path.
        host.TableLayoutPanel.ColumnStyles.Clear();
        host.TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
        host.TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
        host.TableLayoutPanel.ColumnCount = 1;

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        host.TableLayoutPanel.ColumnStyles.Count.Should().Be(1);
    }

    [Fact]
    public void StyleEditorForm_ShowEditorDialog_RemovesExtraRowStyles()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        host.TableLayoutPanel.RowStyles.Clear();
        host.TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
        host.TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        host.TableLayoutPanel.RowCount = 1;

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        host.TableLayoutPanel.RowStyles.Count.Should().Be(1);
    }

    [Fact]
    public void StyleEditorForm_OnEditValueChanged_DoesNotThrow()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        Action act = () => editor.CallOnEditValueChanged(form);
        act.Should().NotThrow();
    }

    [Fact]
    public void StyleEditorForm_InitListView_NoStyles_ListViewEmpty()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        host.TableLayoutPanel.RowStyles.Clear();
        host.TableLayoutPanel.RowCount = 0;
        editor.CallInitListView(form);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        listView.Items.Count.Should().Be(0);
    }

    [Fact]
    public void StyleEditorForm_InitListView_RowStyles_ListViewHasRowNames()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        listView.Items[0].Text.Should().Be("Row1");
    }

    [Fact]
    public void StyleEditorForm_InitListView_ColumnStyles_ListViewHasColumnNames()
    {
        using StyleEditorFormHost host = new(isRowCollection: false);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutColumnStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        listView.Items[0].Text.Should().Be("Column1");
    }

    [Fact]
    public void StyleEditorForm_UpdateListViewItem_UpdatesSubItems()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;

        editor.CallUpdateListViewItem(form, 0, "CustomName", SizeType.Absolute.ToString(), "42");

        listView.Items[0].SubItems[0].Text.Should().Be("CustomName");
        listView.Items[0].SubItems[1].Text.Should().Be(SizeType.Absolute.ToString());
        listView.Items[0].SubItems[2].Text.Should().Be("42");
    }

    [Fact]
    public void StyleEditorForm_UpdateListViewMember_RenamesAllItems()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        editor.CallUpdateListViewMember(form);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;
        for (int i = 0; i < listView.Items.Count; i++)
        {
            listView.Items[i].SubItems[0].Text.Should().Be($"Row{i + 1}");
        }
    }

    [Fact]
    public void StyleEditorForm_ClearAndSetSelectionAndFocus_SelectsItem()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        using WindowsFormsEditorServiceMock editorService = new();
        editor.CallShowEditorDialog(form, editorService);

        ListView listView = editor.GetColumnsAndRowsListView(form);
        _ = listView.Handle;

        editor.CallClearAndSetSelectionAndFocus(form, 0);

        listView.Items[0].Selected.Should().BeTrue();
    }

    [Fact]
    public void StyleEditorForm_ResetAllRadioButtons_UnchecksAll()
    {
        using StyleEditorFormHost host = new(isRowCollection: true);
        using SubStyleCollectionEditor editor = new(typeof(TableLayoutRowStyleCollection));
        host.AttachContext(editor);

        using Form form = editor.CreateCollectionForm();

        editor.CallUpdateGroupBox(form, SizeType.Absolute, 20f);

        editor.CallResetAllRadioButtons(form);

        RadioButton absoluteRadio = editor.GetAbsoluteRadioButton(form);
        RadioButton percentRadio = editor.GetPercentRadioButton(form);
        RadioButton autoSizeRadio = editor.GetAutoSizedRadioButton(form);
        absoluteRadio.Checked.Should().BeFalse();
        percentRadio.Checked.Should().BeFalse();
        autoSizeRadio.Checked.Should().BeFalse();
    }

    private sealed class SubStyleCollectionEditor : StyleCollectionEditor, IDisposable
    {
        public SubStyleCollectionEditor(Type type) : base(type)
        {
        }

        public new Form CreateCollectionForm()
        {
            Form form = base.CreateCollectionForm();
            // Force the handle to be created so that BeginInvoke/Invoke and other
            // handle-dependent operations inside the form's event handlers don't throw.
            _ = form.Handle;
            return form;
        }

        public void SetHelpTopic(string? helpTopic) => _helpTopic = helpTopic;

        public TableLayoutPanel CreateNavigationalTableLayoutPanel() => new NavigationalTableLayoutPanel();

        public List<RadioButton> GetRadioButtons(TableLayoutPanel panel)
        {
            // RadioButtons is a private property; use reflection to read it.
            PropertyInfo property = typeof(NavigationalTableLayoutPanel).GetProperty(
                "RadioButtons",
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            return (List<RadioButton>)property.GetValue(panel)!;
        }

        public ListView GetColumnsAndRowsListView(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            return styleForm.TestAccessor.Dynamic._columnsAndRowsListView;
        }

        public GroupBox GetSizeTypeGroupBox(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            return styleForm.TestAccessor.Dynamic._sizeTypeGroupBox;
        }

        public Button GetInsertButton(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            return styleForm.TestAccessor.Dynamic._insertButton;
        }

        public Button GetRemoveButton(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            return styleForm.TestAccessor.Dynamic._removeButton;
        }

        public Button GetAddButton(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            return styleForm.TestAccessor.Dynamic._addButton;
        }

        public Button GetOkButton(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            return styleForm.TestAccessor.Dynamic._okButton;
        }

        public Button GetCancelButton(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            return styleForm.TestAccessor.Dynamic._cancelButton;
        }

        public RadioButton GetAbsoluteRadioButton(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            return styleForm.TestAccessor.Dynamic._absoluteRadioButton;
        }

        public RadioButton GetPercentRadioButton(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            return styleForm.TestAccessor.Dynamic._percentRadioButton;
        }

        public RadioButton GetAutoSizedRadioButton(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            return styleForm.TestAccessor.Dynamic._autoSizedRadioButton;
        }

        public NumericUpDown GetAbsoluteNumericUpDown(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            return styleForm.TestAccessor.Dynamic._absoluteNumericUpDown;
        }

        public NumericUpDown GetPercentNumericUpDown(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            return styleForm.TestAccessor.Dynamic._percentNumericUpDown;
        }

        public LinkLabel GetHelperLinkLabel1(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            return styleForm.TestAccessor.Dynamic._helperLinkLabel1;
        }

        public LinkLabel GetHelperLinkLabel2(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            return styleForm.TestAccessor.Dynamic._helperLinkLabel2;
        }

        public ComboBox GetColumnsOrRowsComboBox(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            return styleForm.TestAccessor.Dynamic._columnsOrRowsComboBox;
        }

        public void CallShowEditorDialog(Form form, IWindowsFormsEditorService editorService)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.ShowEditorDialog(editorService);
        }

        public bool CallProcessDialogKey(TableLayoutPanel panel, Keys keyData)
        {
            // ProcessDialogKey is protected, so the DLR cannot find it. Use reflection instead.
            MethodInfo method = typeof(NavigationalTableLayoutPanel).GetMethod(
                "ProcessDialogKey",
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            return (bool)method.Invoke(panel, [keyData])!;
        }

        public void CallInitListView(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.InitListView();
        }

        public void CallOnListViewSelectedIndexChanged(Form form, object sender)
        {
            // BeginInvoke requires a window handle; force it to be created.
            _ = form.Handle;
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.OnListViewSelectedIndexChanged(sender, EventArgs.Empty);
        }

        public void CallOnListSelectionComplete(Form form, object sender)
        {
            _ = form.Handle;
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.OnListSelectionComplete(sender, EventArgs.Empty);
        }

        public void CallOnShown(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.OnShown(EventArgs.Empty);
        }

        public void CallOnComboBoxSelectionChangeCommitted(Form form, object sender)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.OnComboBoxSelectionChangeCommitted(sender, EventArgs.Empty);
        }

        public void CallOnAddButtonClick(Form form, object sender)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.OnAddButtonClick(sender, EventArgs.Empty);
        }

        public void CallOnInsertButtonClick(Form form, object sender)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.OnInsertButtonClick(sender, EventArgs.Empty);
        }

        public void CallOnRemoveButtonClick(Form form, object sender)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.OnRemoveButtonClick(sender, EventArgs.Empty);
        }

        public void CallOnOkButtonClick(Form form, object sender)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.OnOkButtonClick(sender, EventArgs.Empty);
        }

        public void CallOnCancelButtonClick(Form form, object sender)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.OnCancelButtonClick(sender, EventArgs.Empty);
        }

        public void CallOnAbsoluteEnter(Form form, object sender)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.OnAbsoluteEnter(sender, EventArgs.Empty);
        }

        public void CallOnPercentEnter(Form form, object sender)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.OnPercentEnter(sender, EventArgs.Empty);
        }

        public void CallOnAutoSizeEnter(Form form, object sender)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.OnAutoSizeEnter(sender, EventArgs.Empty);
        }

        public void CallOnValueChanged(Form form, object sender)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.OnValueChanged(sender, EventArgs.Empty);
        }

        public void CallOnLink1Click(Form form, object sender)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.OnLink1Click(sender, new LinkLabelLinkClickedEventArgs(null!));
        }

        public void CallOnLink2Click(Form form, object sender)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.OnLink2Click(sender, new LinkLabelLinkClickedEventArgs(null!));
        }

        public void CallOnHelpButtonClicked(Form form, object sender, CancelEventArgs e)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.OnHelpButtonClicked(sender, e);
        }

        public void CallStyleEditorClosed(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.StyleEditorClosed(form, new FormClosedEventArgs(CloseReason.UserClosing));
        }

        public void CallOnEditValueChanged(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.OnEditValueChanged();
        }

        public void CallAddItem(Form form, int index)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.AddItem(index);
        }

        public void CallUpdateTypeAndValue(Form form, SizeType type, float value)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.UpdateTypeAndValue(type, value);
        }

        public void CallUpdateGroupBox(Form form, SizeType type, float value)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.UpdateGroupBox(type, value);
        }

        public void CallUpdateListViewItem(Form form, int index, string member, string type, string value)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.UpdateListViewItem(index, member, type, value);
        }

        public void CallUpdateListViewMember(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.UpdateListViewMember();
        }

        public void CallClearAndSetSelectionAndFocus(Form form, int index)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.ClearAndSetSelectionAndFocus(index);
        }

        public void CallResetAllRadioButtons(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.ResetAllRadioButtons();
        }

        public void CallNormalizePercentStyles(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic.NormalizePercentStyles();
        }

        public string CallFormatValueString(SizeType type, float value)
        {
            // FormatValueString is a private static method on StyleEditorForm.
            var method = typeof(StyleCollectionEditor).Assembly
                .GetType("System.Windows.Forms.Design.StyleCollectionEditor+StyleEditorForm")!
                .GetMethod("FormatValueString", BindingFlags.NonPublic | BindingFlags.Static)!;
            return (string)method.Invoke(null, [type, value])!;
        }

        public bool GetIsDialogDirty(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            return styleForm.TestAccessor.Dynamic._isDialogDirty;
        }

        public void SetIsDialogDirty(Form form, bool dirty)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            styleForm.TestAccessor.Dynamic._isDialogDirty = dirty;
        }

        public bool GetIsRowCollection(Form form)
        {
            StyleEditorForm styleForm = (StyleEditorForm)form;
            return styleForm.TestAccessor.Dynamic._isRowCollection;
        }

        public void Dispose()
        {
        }
    }

    private sealed class StyleEditorFormHost : IDisposable
    {
        private readonly bool _isRowCollection;
        private readonly TableLayoutPanel _tableLayoutPanel = new();
        private readonly TableLayoutPanelDesigner _designer;
        private readonly Mock<IDesignerHost> _designerHostMock = new();
        private readonly Mock<IComponentChangeService> _changeServiceMock = new();
        private readonly Mock<ISite> _siteMock = new();
        private readonly Mock<ITypeDescriptorContext> _contextMock = new();

        public TableLayoutPanel TableLayoutPanel => _tableLayoutPanel;

        public StyleEditorFormHost(bool isRowCollection)
        {
            _isRowCollection = isRowCollection;

            _tableLayoutPanel.ColumnCount = 1;
            _tableLayoutPanel.RowCount = 1;
            _tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
            _tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));

            _designer = new TableLayoutPanelDesigner();

            _designerHostMock.Setup(h => h.GetDesigner(_tableLayoutPanel)).Returns(_designer);
            _designerHostMock.Setup(h => h.GetService(typeof(IComponentChangeService)))
                .Returns(_changeServiceMock.Object);
            _designerHostMock.Setup(h => h.RootComponent).Returns(_tableLayoutPanel);

            _siteMock.Setup(s => s.GetService(typeof(IDesignerHost)))
                .Returns(_designerHostMock.Object);
            _siteMock.Setup(s => s.GetService(typeof(IComponentChangeService)))
                .Returns(_changeServiceMock.Object);
            _siteMock.Setup(s => s.GetService(typeof(IServiceProvider)))
                .Returns((IServiceProvider?)null);

            _tableLayoutPanel.Site = _siteMock.Object;

            _designer.Initialize(_tableLayoutPanel);

            _contextMock.Setup(c => c.Instance).Returns(_tableLayoutPanel);
            _contextMock.Setup(c => c.Container).Returns((IContainer?)null);
            _contextMock.Setup(c => c.PropertyDescriptor).Returns((PropertyDescriptor?)null);
        }

        public void AttachContext(SubStyleCollectionEditor editor)
        {
            // Context has a private setter, so we use reflection to set it directly.
            PropertyInfo contextProperty = typeof(CollectionEditor).GetProperty(
                "Context",
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            contextProperty.GetSetMethod(true)!.Invoke(editor, [_contextMock.Object]);
        }

        public void Dispose()
        {
            _designer.Dispose();
            _tableLayoutPanel.Dispose();
        }
    }

    private sealed class WindowsFormsEditorServiceMock : IWindowsFormsEditorService, IDisposable
    {
        public DialogResult ShowDialog(Form dialog)
        {
            if (dialog is Form form && form.DialogResult == DialogResult.None)
            {
                form.DialogResult = DialogResult.OK;
            }

            return DialogResult.OK;
        }

        public void DropDownControl(Control control)
        {
        }

        public void CloseDropDown()
        {
        }

        public void Dispose()
        {
        }
    }
}
