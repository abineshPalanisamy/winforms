// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using Moq;

namespace System.Windows.Forms.Design.Tests;

public class ColumnHeaderCollectionEditorTests
{
    [Fact]
    public void ColumnHeaderCollectionEditor_Ctor_Default()
    {
        ColumnHeaderCollectionEditor editor = new(typeof(string));
        Assert.False(editor.IsDropDownResizable);
    }

    [Fact]
    public void ColumnHeaderCollectionEditor_EditValue_ReturnsValue()
    {
        ColumnHeaderCollectionEditor editor = new(typeof(string));
        string[] value = ["asdf", "qwer", "zxcv"];

        Assert.Same(value, editor.EditValue(null, value));
    }

    [Fact]
    public void ColumnHeaderCollectionEditor_HelpTopic_ReturnsExpected()
    {
        ColumnHeaderCollectionEditor editor = new(typeof(string));

        string helpTopic = editor.TestAccessor.Dynamic.HelpTopic;

        helpTopic.Should().Be("net.ComponentModel.ColumnHeaderCollectionEditor");
    }

    [Fact]
    public void ColumnHeaderCollectionEditor_SetItems_WithListViewCollection_ReplacesColumns()
    {
        ColumnHeaderCollectionEditor editor = new(typeof(string));
        using ListView listView = new();
        listView.Columns.Add("Original1");
        listView.Columns.Add("Original2");

        ColumnHeader newHeader1 = new() { Text = "New1" };
        ColumnHeader newHeader2 = new() { Text = "New2" };
        object[] newValue = [newHeader1, newHeader2];

        ((object)editor.TestAccessor.Dynamic.SetItems(listView.Columns, newValue)).Should().BeSameAs(listView.Columns);
        listView.Columns.Count.Should().Be(2);
        listView.Columns[0].Should().BeSameAs(newHeader1);
        listView.Columns[1].Should().BeSameAs(newHeader2);
    }

    [Fact]
    public void ColumnHeaderCollectionEditor_SetItems_WithListViewCollectionAndEmptyArray_ClearsColumns()
    {
        ColumnHeaderCollectionEditor editor = new(typeof(string));
        using ListView listView = new();
        listView.Columns.Add("Col1");
        listView.Columns.Add("Col2");

        ((object)editor.TestAccessor.Dynamic.SetItems(listView.Columns, Array.Empty<object>())).Should().BeSameAs(listView.Columns);
        listView.Columns.Count.Should().Be(0);
    }

    [Fact]
    public void ColumnHeaderCollectionEditor_SetItems_WithListViewCollectionAndNullValue_ClearsColumns()
    {
        ColumnHeaderCollectionEditor editor = new(typeof(string));
        using ListView listView = new();
        listView.Columns.Add("Col1");

        ((object)editor.TestAccessor.Dynamic.SetItems(listView.Columns, null)).Should().BeSameAs(listView.Columns);
        listView.Columns.Count.Should().Be(0);
    }

    [Fact]
    public void ColumnHeaderCollectionEditor_SetItems_WithNonListViewCollection_ReturnsEditValueUnchanged()
    {
        ColumnHeaderCollectionEditor editor = new(typeof(string));
        object editValue = new();
        object[] value = ["a", "b"];

        ((object)editor.TestAccessor.Dynamic.SetItems(editValue, value)).Should().BeSameAs(editValue);
    }

    [Fact]
    public void ColumnHeaderCollectionEditor_OnItemRemoving_WithNullContext_DoesNotThrow()
    {
        ColumnHeaderCollectionEditor editor = new(typeof(string));

        Action action = () => editor.TestAccessor.Dynamic.OnItemRemoving(new ColumnHeader());

        action.Should().NotThrow();
    }

    [Fact]
    public void ColumnHeaderCollectionEditor_OnItemRemoving_WithContextInstanceNotListView_ReturnsEarly()
    {
        ColumnHeaderCollectionEditor editor = new(typeof(string));
        Mock<ITypeDescriptorContext> mockContext = new();
        object nonListViewInstance = new();
        mockContext.Setup(c => c.Instance).Returns(nonListViewInstance);

        SetContext(editor, mockContext.Object);

        Action action = () => editor.TestAccessor.Dynamic.OnItemRemoving(new ColumnHeader());

        action.Should().NotThrow();
    }

    [Fact]
    public void ColumnHeaderCollectionEditor_OnItemRemoving_WithListViewContextAndNoChangeService_RemovesColumn()
    {
        ColumnHeaderCollectionEditor editor = new(typeof(string));
        using ListView listView = new();
        ColumnHeader column = new() { Text = "Removable" };
        listView.Columns.Add(column);

        Mock<ITypeDescriptorContext> mockContext = new();
        mockContext.Setup(c => c.Instance).Returns(listView);
        mockContext.Setup(c => c.GetService(typeof(IComponentChangeService))).Returns((object)null);

        SetContext(editor, mockContext.Object);

        editor.TestAccessor.Dynamic.OnItemRemoving(column);

        listView.Columns.Count.Should().Be(0);
    }

    [Fact]
    public void ColumnHeaderCollectionEditor_OnItemRemoving_WithListViewContextAndChangeService_NotifiesChangeAndRemovesColumn()
    {
        ColumnHeaderCollectionEditor editor = new(typeof(string));
        using ListView listView = new();
        ColumnHeader column1 = new() { Text = "Keep" };
        ColumnHeader column2 = new() { Text = "Remove" };
        listView.Columns.Add(column1);
        listView.Columns.Add(column2);

        Mock<IComponentChangeService> mockChangeService = new(MockBehavior.Loose);

        Mock<ITypeDescriptorContext> mockContext = new();
        mockContext.Setup(c => c.Instance).Returns(listView);
        mockContext.Setup(c => c.GetService(typeof(IComponentChangeService))).Returns(mockChangeService.Object);

        SetContext(editor, mockContext.Object);

        editor.TestAccessor.Dynamic.OnItemRemoving(column2);

        listView.Columns.Count.Should().Be(1);
        listView.Columns[0].Should().BeSameAs(column1);

        // Inspect invocations directly to avoid Moq's expression-tree matcher resolution
        // issues with It.IsAny<PropertyDescriptor>() on two-argument setups.
        PropertyDescriptor columnsProperty = TypeDescriptor.GetProperties(listView)["Columns"]!;
        AssertChangeServiceInvoked(mockChangeService, nameof(IComponentChangeService.OnComponentChanging), listView, columnsProperty);
        AssertChangeServiceInvoked(mockChangeService, nameof(IComponentChangeService.OnComponentChanged), listView, columnsProperty);
    }

    [Fact]
    public void ColumnHeaderCollectionEditor_OnItemRemoving_WithNonColumnHeaderItem_DoesNotRemove()
    {
        ColumnHeaderCollectionEditor editor = new(typeof(string));
        using ListView listView = new();
        listView.Columns.Add("Existing");

        // item is not a ColumnHeader, so the editor should not even ask the change service
        // for notifications. Use a non-strict mock that allows GetService but never touches
        // OnComponentChanging / OnComponentChanged.
        Mock<IComponentChangeService> mockChangeService = new(MockBehavior.Loose);

        Mock<ITypeDescriptorContext> mockContext = new();
        mockContext.Setup(c => c.Instance).Returns(listView);
        mockContext.Setup(c => c.GetService(typeof(IComponentChangeService))).Returns(mockChangeService.Object);

        SetContext(editor, mockContext.Object);

        editor.TestAccessor.Dynamic.OnItemRemoving("not a column header");

        listView.Columns.Count.Should().Be(1);
    }

    // The CollectionEditor.Context property has a private setter, so the dynamically
    // generated TestAccessor cannot expose it as a public property. Use reflection
    // to set it directly on the editor for tests that need to drive OnItemRemoving.
    private static void SetContext(ColumnHeaderCollectionEditor editor, ITypeDescriptorContext context)
    {
        PropertyInfo contextProperty = typeof(CollectionEditor).GetProperty(
            "Context",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(contextProperty);
        contextProperty.SetValue(editor, context);
    }

    // Asserts that the supplied mocked IComponentChangeService received exactly one call
    // to the named method with (listView, "Columns" PropertyDescriptor). Iterates the
    // recorded invocations directly so we avoid Moq's expression-tree matcher resolution
    // (which can't bind It.IsAny<PropertyDescriptor>()) and expression-tree lambda
    // restrictions (no 'is' patterns, no '?.' null-propagating operator).
    private static void AssertChangeServiceInvoked(
        Mock<IComponentChangeService> mockChangeService,
        string methodName,
        ListView listView,
        PropertyDescriptor columnsProperty)
    {
        int matchCount = 0;
        foreach (var invocation in mockChangeService.Invocations.Select(i => i.Method))
        {
            if (invocation.Name != methodName)
            {
                continue;
            }

            var arguments = mockChangeService.Invocations
                .First(i => i.Method == invocation)
                .Arguments;
            if (arguments.Count >= 2
                && ReferenceEquals(arguments[0], listView)
                && ReferenceEquals(arguments[1], columnsProperty))
            {
                matchCount++;
            }
        }

        Assert.Equal(1, matchCount);
    }
}
