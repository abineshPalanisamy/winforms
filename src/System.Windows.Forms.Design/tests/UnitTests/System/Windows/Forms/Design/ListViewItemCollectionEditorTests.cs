// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Windows.Forms.Design.Tests;

public class ListViewItemCollectionEditorTests
{
    [Fact]
    public void ListViewItemCollectionEditor_Constructor_SetsCollectionType()
    {
        Type expectedType = typeof(ListViewItem);

        SubListViewItemCollectionEditor editor = new(expectedType);

        Type actualType = editor.TestAccessor.Dynamic.CollectionType;
        actualType.Should().Be(expectedType);
    }

    [Fact]
    public void ListViewItemCollectionEditor_GetDisplayText_NullValue_ReturnsEmpty()
    {
        SubListViewItemCollectionEditor editor = new(typeof(ListViewItem));

        string result = editor.CallGetDisplayText(null);

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void ListViewItemCollectionEditor_GetDisplayText_ValueWithNonEmptyText_ReturnsText()
    {
        SubListViewItemCollectionEditor editor = new(typeof(ListViewItem));
        ListViewItem item = new("MyItem");

        string result = editor.CallGetDisplayText(item);

        result.Should().Be("MyItem");
    }

    [Fact]
    public void ListViewItemCollectionEditor_GetDisplayText_ValueWithEmptyText_UsesTypeConverter()
    {
        SubListViewItemCollectionEditor editor = new(typeof(ListViewItem));
        ListViewItem item = new(string.Empty);

        string result = editor.CallGetDisplayText(item);

        result.Should().Be(item.ToString());
    }

    [Fact]
    public void ListViewItemCollectionEditor_GetDisplayText_ObjectWithoutDefaultProperty_ReturnsConvertedString()
    {
        SubListViewItemCollectionEditor editor = new(typeof(object));
        object value = new();

        string result = editor.CallGetDisplayText(value);

        // TypeDescriptor.GetConverter(object).ConvertToString(object) returns the full type name.
        result.Should().Be(value.GetType().FullName);
    }

    private class SubListViewItemCollectionEditor : ListViewItemCollectionEditor
    {
        public SubListViewItemCollectionEditor(Type type)
            : base(type)
        {
        }

        public string CallGetDisplayText(object value) => GetDisplayText(value);
    }
}
