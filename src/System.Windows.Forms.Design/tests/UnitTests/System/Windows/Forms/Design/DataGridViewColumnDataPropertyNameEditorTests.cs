// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.CompilerServices;
using Moq;

namespace System.Windows.Forms.Design.Tests;

public class DataGridViewColumnDataPropertyNameEditorTests
{
    [Fact]
    public void DataGridViewColumnDataPropertyNameEditor_GetEditStyle()
    {
        new DataGridViewColumnDataPropertyNameEditor().GetEditStyle().Should().Be(UITypeEditorEditStyle.DropDown);
    }

    [Fact]
    public void DataGridViewColumnDataPropertyNameEditor_IsDropDownResizable()
    {
        new DataGridViewColumnDataPropertyNameEditor().IsDropDownResizable.Should().Be(true);
    }

    [Fact]
    public void DataGridViewColumnDataPropertyNameEditor_Ctor_Default()
    {
        DataGridViewColumnDataPropertyNameEditor editor = new();
        editor.Should().NotBeNull();
        editor.IsDropDownResizable.Should().BeTrue();
    }

    [Fact]
    public void DataGridViewColumnDataPropertyNameEditor_EditValue()
    {
        DataGridViewColumnDataPropertyNameEditor dataGridViewColumnEditor = new();
        object value = "123";
        dataGridViewColumnEditor.EditValue(null, null, value).Should().Be(value);

        Mock<ITypeDescriptorContext> mockTypeDescriptorContext = new(MockBehavior.Strict);
        dataGridViewColumnEditor.EditValue(mockTypeDescriptorContext.Object, null, value).Should().Be(value);

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        dataGridViewColumnEditor.EditValue(null, mockServiceProvider.Object, value).Should().Be(value);

        mockTypeDescriptorContext.Setup(x => x.Instance).Returns(null);
        dataGridViewColumnEditor.EditValue(null, mockServiceProvider.Object, value).Should().Be(value);
    }

    [Fact]
    public void DataGridViewColumnDataPropertyNameEditor_EditValue_DataGridViewColumnInstance_NotAttachedToDataGridView_ReturnsValue()
    {
        // Exercises the branch where context.Instance is a DataGridViewColumn whose DataGridView
        // is null: the editor must short-circuit and return the original value unchanged.
        DataGridViewColumnDataPropertyNameEditor editor = new();
        object value = "FieldName";

        using DataGridViewColumn detachedColumn = new DataGridViewTextBoxColumn();
        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(detachedColumn);

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Loose);
        editor.EditValue(mockContext.Object, mockServiceProvider.Object, value).Should().Be(value);
    }

    [Fact]
    public void DataGridViewColumnDataPropertyNameEditor_EditValue_ListBoxItemInstance_NotAttachedToDataGridView_ReturnsValue()
    {
        // Exercises the ListBoxItem branch of the type check: when the context instance is a
        // ListBoxItem whose DataGridViewColumn is not attached to a DataGridView, the editor
        // must short-circuit and return the original value unchanged.
        DataGridViewColumnDataPropertyNameEditor editor = new();
        object value = "FieldName";

        using DataGridViewColumn detachedColumn = new DataGridViewTextBoxColumn();
        DataGridViewColumnCollectionDialog.ListBoxItem listBoxItem =
            CreateListBoxItemWithoutRunningConstructor(detachedColumn);

        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(listBoxItem);

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Loose);
        editor.EditValue(mockContext.Object, mockServiceProvider.Object, value).Should().Be(value);
    }

    [Fact]
    public void DataGridViewColumnDataPropertyNameEditor_EditValue_DataGridViewColumnInstance_NoDataSource_ReturnsValue()
    {
        // Exercises the branch where the column is attached to a DataGridView, but the
        // DataGridView has no DataSource. The editor resets dataMember to string.Empty and
        // selectedMember to valueString, then opens the picker (which has no editor service
        // and therefore returns null). The original value must come back unchanged.
        DataGridViewColumnDataPropertyNameEditor editor = new();
        object value = "FieldName";

        using DataGridView dataGridView = new();
        using DataGridViewColumn column = new DataGridViewTextBoxColumn();
        dataGridView.Columns.Add(column);

        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(column);

        // Service provider has no IWindowsFormsEditorService, so DesignBindingPicker.Pick
        // returns null immediately. This still drives the "dataSource is null" code path
        // and exercises the _designBindingPicker ??= new() initialization. Loose behavior
        // ensures any auxiliary service lookups (e.g. ITypeResolutionService) return null
        // instead of throwing MockException.
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Loose);
        editor.EditValue(mockContext.Object, mockServiceProvider.Object, value).Should().Be(value);
    }

    [Fact]
    public void DataGridViewColumnDataPropertyNameEditor_EditValue_ListBoxItemInstance_NoDataSource_ReturnsValue()
    {
        // Combines the ListBoxItem branch with the "dataSource is null" branch: the editor
        // must navigate through the ListBoxItem to its DataGridViewColumn, observe that
        // the underlying DataGridView has no DataSource, reset dataMember accordingly, and
        // return the original value when the picker is unable to produce a selection.
        DataGridViewColumnDataPropertyNameEditor editor = new();
        object value = "FieldName";

        using DataGridView dataGridView = new();
        using DataGridViewColumn column = new DataGridViewTextBoxColumn();
        dataGridView.Columns.Add(column);

        DataGridViewColumnCollectionDialog.ListBoxItem listBoxItem =
            CreateListBoxItemWithoutRunningConstructor(column);

        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(listBoxItem);

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Loose);
        editor.EditValue(mockContext.Object, mockServiceProvider.Object, value).Should().Be(value);
    }

    [Fact]
    public void DataGridViewColumnDataPropertyNameEditor_EditValue_DataGridViewColumnInstance_WithDataSource_NoEditorService_ReturnsValue()
    {
        // Exercises the path where the DataGridView has a DataSource: the editor composes
        // selectedMember as "{dataMember}.{valueString}", initializes the picker, and
        // because no IWindowsFormsEditorService is available, the picker returns null and
        // the value is left unchanged.
        DataGridViewColumnDataPropertyNameEditor editor = new();
        object value = "Name";

        BindingList<BindingSourceTestItem> dataSource =
        [
            new BindingSourceTestItem { Name = "Alpha" },
        ];

        using DataGridView dataGridView = new() { DataSource = dataSource };
        using DataGridViewColumn column = new DataGridViewTextBoxColumn();
        dataGridView.Columns.Add(column);

        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(column);

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Loose);
        editor.EditValue(mockContext.Object, mockServiceProvider.Object, value).Should().Be(value);
    }

    [Fact]
    public void DataGridViewColumnDataPropertyNameEditor_EditValue_ListBoxItemInstance_WithDataSource_NoEditorService_ReturnsValue()
    {
        // Same as above but the context instance is a ListBoxItem: confirms the ListBoxItem
        // path is taken even when the DataGridView has a DataSource set.
        DataGridViewColumnDataPropertyNameEditor editor = new();
        object value = "Name";

        BindingList<BindingSourceTestItem> dataSource =
        [
            new BindingSourceTestItem { Name = "Alpha" },
        ];

        using DataGridView dataGridView = new() { DataSource = dataSource };
        using DataGridViewColumn column = new DataGridViewTextBoxColumn();
        dataGridView.Columns.Add(column);

        DataGridViewColumnCollectionDialog.ListBoxItem listBoxItem =
            CreateListBoxItemWithoutRunningConstructor(column);

        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(listBoxItem);

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Loose);
        editor.EditValue(mockContext.Object, mockServiceProvider.Object, value).Should().Be(value);
    }

    [Fact]
    public void DataGridViewColumnDataPropertyNameEditor_EditValue_PickerReturnsNewBinding_ReplacesValue()
    {
        // Exercises the "value replacement" branch: when a non-null DesignBinding is
        // returned by the picker AND the DataGridView has a DataSource, the editor must
        // replace the input value with newSelection.DataField. The mock IWindowsFormsEditorService
        // seeds the picker's _selectedItem field via reflection while DropDownControl is
        // running, so Pick() returns the seeded selection.
        DataGridViewColumnDataPropertyNameEditor editor = new();
        object value = "OldField";

        BindingList<BindingSourceTestItem> dataSource =
        [
            new BindingSourceTestItem { Name = "Alpha" },
        ];

        using DataGridView dataGridView = new() { DataSource = dataSource };
        using DataGridViewColumn column = new DataGridViewTextBoxColumn();
        dataGridView.Columns.Add(column);

        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(column);

        DesignBindingPicker picker = new();
        DesignBinding prebuiltSelection = new(dataSource, "NewField");

        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        mockEditorService
            .Setup(s => s.DropDownControl(It.IsAny<Control>()))
            .Callback<Control>(ctrl =>
            {
                // The picker passes itself to DropDownControl. While the modal loop is
                // running, seed the picker's _selectedItem with a non-null DesignBinding;
                // Pick() reads that field immediately after DropDownControl returns.
                DesignBindingPicker actualPicker = (DesignBindingPicker)ctrl;
                actualPicker.TestAccessor.Dynamic._selectedItem = prebuiltSelection;
            });

        // Loose so unconfigured GetService lookups (DataSourceProviderService,
        // ITypeResolutionService, IDesignerHost) return null instead of throwing.
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Loose);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        editor.TestAccessor.Dynamic._designBindingPicker = picker;

        object result = editor.EditValue(mockContext.Object, mockServiceProvider.Object, value);
        result.Should().Be("NewField");
        mockEditorService.Verify(s => s.DropDownControl(It.IsAny<Control>()), Times.Once);
    }

    [Fact]
    public void DataGridViewColumnDataPropertyNameEditor_EditValue_PickerReturnsNullSelection_LeavesValueUnchanged()
    {
        // Confirms the "no replacement" branch: even when a DataSource is present, if the
        // picker returns a null DesignBinding, the editor must not replace the value.
        DataGridViewColumnDataPropertyNameEditor editor = new();
        object value = "OriginalField";

        BindingList<BindingSourceTestItem> dataSource =
        [
            new BindingSourceTestItem { Name = "Alpha" },
        ];

        using DataGridView dataGridView = new() { DataSource = dataSource };
        using DataGridViewColumn column = new DataGridViewTextBoxColumn();
        dataGridView.Columns.Add(column);

        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(column);

        DesignBindingPicker picker = new();
        // Leave _selectedItem as null; Pick() will return null because no selection
        // was made during the (mocked) DropDownControl callback.

        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        mockEditorService
            .Setup(s => s.DropDownControl(It.IsAny<Control>()))
            .Callback<Control>(ctrl => { /* no-op: picker keeps _selectedItem as null */ });

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Loose);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        editor.TestAccessor.Dynamic._designBindingPicker = picker;

        object result = editor.EditValue(mockContext.Object, mockServiceProvider.Object, value);
        result.Should().Be(value);
        mockEditorService.Verify(s => s.DropDownControl(It.IsAny<Control>()), Times.Once);
    }

    /// <summary>
    ///  Builds a <see cref="DataGridViewColumnCollectionDialog.ListBoxItem"/> instance without
    ///  invoking its constructor. The editor under test only reads the
    ///  <c>DataGridViewColumn</c> auto-property from the ListBoxItem, so populating the
    ///  backing field is sufficient. This avoids the need to construct a full
    ///  <c>DataGridViewColumnCollectionDialog</c> (a WinForms Form), which would require
    ///  additional resources and runtime support.
    /// </summary>
    private static DataGridViewColumnCollectionDialog.ListBoxItem CreateListBoxItemWithoutRunningConstructor(
        DataGridViewColumn column)
    {
        DataGridViewColumnCollectionDialog.ListBoxItem item =
            (DataGridViewColumnCollectionDialog.ListBoxItem)RuntimeHelpers.GetUninitializedObject(
                typeof(DataGridViewColumnCollectionDialog.ListBoxItem));

        FieldInfo backingField = typeof(DataGridViewColumnCollectionDialog.ListBoxItem)
            .GetField(
                $"<{nameof(DataGridViewColumnCollectionDialog.ListBoxItem.DataGridViewColumn)}>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException(
                "Could not locate backing field for ListBoxItem.DataGridViewColumn.");

        backingField.SetValue(item, column);
        return item;
    }

    private sealed class BindingSourceTestItem
    {
        public string Name { get; set; }
    }
}
