// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using Moq;

namespace System.Windows.Forms.Design.Tests;

public class TabPageCollectionEditorTests
{
    [Fact]
    public void TabPageCollectionEditor_Constructor_SetsCollectionType()
    {
        SubTabPageCollectionEditor editor = new();

        Type actualType = editor.TestAccessor.Dynamic.CollectionType;
        actualType.Should().Be(typeof(TabControl.TabPageCollection));
    }

    [Fact]
    public void TabPageCollectionEditor_CreateInstance_Invoke_ReturnsTabPageWithUseVisualStyleBackColorTrue()
    {
        SubTabPageCollectionEditor editor = new();

        object instance = editor.CallCreateInstance(typeof(TabPage));

        TabPage tabPage = Assert.IsType<TabPage>(instance);
        tabPage.UseVisualStyleBackColor.Should().BeTrue();
    }

    [Fact]
    public void TabPageCollectionEditor_SetItems_NullEditValue_ReturnsNull()
    {
        SubTabPageCollectionEditor editor = new();
        using TabPage tabPage = new();

        object? result = editor.CallSetItems(null, [tabPage]);

        result.Should().BeNull();
    }

    [Fact]
    public void TabPageCollectionEditor_SetItems_NoContext_ReturnsSameEditValue()
    {
        SubTabPageCollectionEditor editor = new();
        using TabPage tabPage1 = new();
        using TabPage tabPage2 = new();
        ArrayList editValue = [tabPage1];

        object? result = editor.CallSetItems(editValue, [tabPage2]);

        result.Should().BeSameAs(editValue);
        editValue.Count.Should().Be(1);
        editValue[0].Should().Be(tabPage2);
    }

    [Fact]
    public void TabPageCollectionEditor_SetItems_ContextInstanceIsTabControl_SuspendsAndResumesLayout()
    {
        using TabControl tabControl = new();
        Mock<global::System.ComponentModel.ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(tabControl);

        SubTabPageCollectionEditor editor = new();
        editor.SetContext(mockContext.Object);

        using TabPage tabPage1 = new();
        using TabPage tabPage2 = new();
        ArrayList editValue = [tabPage1];

        object? result = editor.CallSetItems(editValue, [tabPage2]);

        result.Should().BeSameAs(editValue);
        editValue.Count.Should().Be(1);
        editValue[0].Should().Be(tabPage2);
    }

    [Fact]
    public void TabPageCollectionEditor_SetItems_ContextInstanceIsNotTabControl_ReturnsExpected()
    {
        Mock<global::System.ComponentModel.ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(new object());

        SubTabPageCollectionEditor editor = new();
        editor.SetContext(mockContext.Object);

        using TabPage tabPage1 = new();
        using TabPage tabPage2 = new();
        ArrayList editValue = [tabPage1];

        object? result = editor.CallSetItems(editValue, [tabPage2]);

        result.Should().BeSameAs(editValue);
        editValue.Count.Should().Be(1);
        editValue[0].Should().Be(tabPage2);
    }

    private class SubTabPageCollectionEditor : TabPageCollectionEditor
    {
        private static readonly PropertyInfo s_contextProperty =
            typeof(CollectionEditor).GetProperty("Context", BindingFlags.Instance | BindingFlags.NonPublic)!;

        public object? CallSetItems(object? editValue, object[]? value) => SetItems(editValue, value);

        public object CallCreateInstance(Type itemType) => CreateInstance(itemType);

        public void SetContext(ITypeDescriptorContext context) =>
            s_contextProperty.SetValue(this, context, index: null);
    }
}
