// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.Drawing.Design;
using Moq;

namespace System.Windows.Forms.Design.Tests;

public class DataMemberListEditorTests
{
    [Fact]
    public void DataMemberListEditor_GetEditStyle()
    {
        new DataMemberListEditor().GetEditStyle().Should().Be(UITypeEditorEditStyle.DropDown);
    }

    [Fact]
    public void DataMemberListEditor_IsDropDownResizable()
    {
        new DataMemberListEditor().IsDropDownResizable.Should().Be(true);
    }

    [Fact]
    public void DataMemberListEditor_EditValue()
    {
        DataMemberListEditor dataMemberListEditor = new();
        object value = "123";
        dataMemberListEditor.EditValue(null, null, value).Should().Be(value);

        Mock<ITypeDescriptorContext> mockTypeDescriptorContext = new(MockBehavior.Strict);
        dataMemberListEditor.EditValue(mockTypeDescriptorContext.Object, null, value).Should().Be(value);

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        dataMemberListEditor.EditValue(null, mockServiceProvider.Object, value).Should().Be(value);

        mockTypeDescriptorContext.Setup(x => x.Instance).Returns(null);
        dataMemberListEditor.EditValue(null, mockServiceProvider.Object, value).Should().Be(value);
    }

    [Fact]
    public void DataMemberListEditor_EditValue_InstanceHasNoDataSourceProperty_ReturnsValue()
    {
        // Exercises the branch where context.Instance is set, but the instance does not
        // expose a property named "DataSource". The editor must short-circuit and return
        // the original value unchanged.
        DataMemberListEditor editor = new();
        object value = "OriginalMember";

        using Control instance = new();
        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(instance);

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Loose);
        editor.EditValue(mockContext.Object, mockServiceProvider.Object, value).Should().Be(value);
    }

    [Fact]
    public void DataMemberListEditor_EditValue_InstanceWithNullDataSource_ReturnsValue()
    {
        // Exercises the branch where the instance has a "DataSource" property that returns
        // null. The editor enters the picker path, but Pick returns null because no
        // IWindowsFormsEditorService is available, so the value must come back unchanged.
        DataMemberListEditor editor = new();
        object value = "OriginalMember";

        using ComboBox comboBox = new();
        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(comboBox);

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Loose);
        editor.EditValue(mockContext.Object, mockServiceProvider.Object, value).Should().Be(value);
    }

    [Fact]
    public void DataMemberListEditor_EditValue_InstanceWithDataSource_NoEditorService_ReturnsValue()
    {
        // Exercises the branch where the instance has a "DataSource" property that returns
        // a non-null value. The editor initializes the picker and calls Pick, which returns
        // null because no IWindowsFormsEditorService is available. The value must come
        // back unchanged.
        DataMemberListEditor editor = new();
        object value = "OriginalMember";

        BindingList<BindingSourceTestItem> dataSource =
        [
            new BindingSourceTestItem { Name = "Alpha" },
        ];

        using ComboBox comboBox = new() { DataSource = dataSource };
        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(comboBox);

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Loose);
        editor.EditValue(mockContext.Object, mockServiceProvider.Object, value).Should().Be(value);
    }

    [Fact]
    public void DataMemberListEditor_EditValue_PickerReturnsNewBinding_ReplacesValue()
    {
        // Exercises the "value replacement" branch: when a non-null DesignBinding is
        // returned by the picker AND the instance has a DataSource, the editor must
        // replace the input value with newSelection.DataMember. The mock
        // IWindowsFormsEditorService seeds the picker's _selectedItem field via reflection
        // while DropDownControl is running, so Pick() returns the seeded selection.
        DataMemberListEditor editor = new();
        object value = "OldMember";

        BindingList<BindingSourceTestItem> dataSource =
        [
            new BindingSourceTestItem { Name = "Alpha" },
        ];

        using ComboBox comboBox = new() { DataSource = dataSource };
        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(comboBox);

        DesignBindingPicker picker = new();
        DesignBinding prebuiltSelection = new(dataSource, "NewMember");

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
        result.Should().Be("NewMember");
        mockEditorService.Verify(s => s.DropDownControl(It.IsAny<Control>()), Times.Once);
    }

    [Fact]
    public void DataMemberListEditor_EditValue_PickerReturnsNullSelection_LeavesValueUnchanged()
    {
        // Confirms the "no replacement" branch: even when the instance has a DataSource,
        // if the picker returns a null DesignBinding, the editor must not replace the value.
        DataMemberListEditor editor = new();
        object value = "OriginalMember";

        BindingList<BindingSourceTestItem> dataSource =
        [
            new BindingSourceTestItem { Name = "Alpha" },
        ];

        using ComboBox comboBox = new() { DataSource = dataSource };
        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(comboBox);

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

    private sealed class BindingSourceTestItem
    {
        public string Name { get; set; }
    }
}
