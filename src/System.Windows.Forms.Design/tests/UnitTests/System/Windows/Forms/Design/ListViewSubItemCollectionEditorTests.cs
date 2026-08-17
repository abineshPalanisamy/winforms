// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Windows.Forms.Design.Tests;

public class ListViewSubItemCollectionEditorTests
{
    [Fact]
    public void ListViewSubItemCollectionEditor_Ctor_Default()
    {
        ListViewSubItemCollectionEditor editor = new(typeof(ListViewItem.ListViewSubItem));

        editor.IsDropDownResizable.Should().Be(false);
    }

    [Fact]
    public void ListViewSubItemCollectionEditor_Ctor_CollectionTypeIsListViewSubItem()
    {
        Type expectedType = typeof(ListViewItem.ListViewSubItem);

        ListViewSubItemCollectionEditor editor = new(expectedType);

        Type actualType = editor.TestAccessor.Dynamic.CollectionType;

        actualType.Should().Be(expectedType);
    }

    [Fact]
    public void ListViewSubItemCollectionEditor_CreateInstance_SetsUniqueName()
    {
        // CreateInstance is called without a designer host / context, so the base
        // implementation falls through to TypeDescriptor.CreateInstance, which uses
        // the parameterless constructor. The override then sets the new sub-item's
        // Name to "ListViewSubItem" + s_count.
        ListViewSubItemCollectionEditor editor = new(typeof(ListViewItem.ListViewSubItem));

        ListViewItem.ListViewSubItem? result =
            editor.TestAccessor.Dynamic.CreateInstance(typeof(ListViewItem.ListViewSubItem))
            as ListViewItem.ListViewSubItem;

        result.Should().NotBeNull();
        result.Name.Should().StartWith("ListViewSubItem");
    }

    [Fact]
    public void ListViewSubItemCollectionEditor_CreateInstance_MultipleInvokesAppendIncrementingSuffix()
    {
        // The static s_count counter increments on every CreateInstance call, so the
        // suffix on the generated Name must keep growing across invocations.
        ListViewSubItemCollectionEditor editor = new(typeof(ListViewItem.ListViewSubItem));

        ListViewItem.ListViewSubItem first =
            (ListViewItem.ListViewSubItem)editor.TestAccessor.Dynamic.CreateInstance(
                typeof(ListViewItem.ListViewSubItem));
        ListViewItem.ListViewSubItem second =
            (ListViewItem.ListViewSubItem)editor.TestAccessor.Dynamic.CreateInstance(
                typeof(ListViewItem.ListViewSubItem));

        first.Name.Should().NotBe(second.Name);
        first.Name.Should().StartWith("ListViewSubItem");
        second.Name.Should().StartWith("ListViewSubItem");
    }

    [Fact]
    public void ListViewSubItemCollectionEditor_GetDisplayText_NullValue_ReturnsEmpty()
    {
        ListViewSubItemCollectionEditor editor = new(typeof(ListViewItem.ListViewSubItem));

        string displayText = editor.TestAccessor.Dynamic.GetDisplayText(null);

        displayText.Should().Be(string.Empty);
    }

    [Fact]
    public void ListViewSubItemCollectionEditor_GetDisplayText_WithText_ReturnsText()
    {
        // The DefaultProperty on ListViewSubItem is "Text", so the override returns
        // the Text property value when it is non-empty.
        ListViewSubItemCollectionEditor editor = new(typeof(ListViewItem.ListViewSubItem));
        ListViewItem.ListViewSubItem subItem = new(null, "Hello");

        string displayText = editor.TestAccessor.Dynamic.GetDisplayText(subItem);

        displayText.Should().Be("Hello");
    }

    [Fact]
    public void ListViewSubItemCollectionEditor_GetDisplayText_WithEmptyText_ReturnsConverterToStringResult()
    {
        // When the default-property text is null/empty, the override falls back to
        // the type's converter. For a ListViewSubItem, the inherited
        // ExpandableObjectConverter.ConvertToString returns the sub-item's
        // ToString() value, which is "ListViewSubItem: {<text>}".
        ListViewSubItemCollectionEditor editor = new(typeof(ListViewItem.ListViewSubItem));
        ListViewItem.ListViewSubItem subItem = new(null, string.Empty);

        string displayText = editor.TestAccessor.Dynamic.GetDisplayText(subItem);

        displayText.Should().Be("ListViewSubItem: {}");
    }

    [Fact]
    public void ListViewSubItemCollectionEditor_GetDisplayText_WithNullText_ReturnsConverterToStringResult()
    {
        // Same fallback path as the empty-text case, but exercised with a literal
        // null Text. The Text getter normalizes null to string.Empty.
        ListViewSubItemCollectionEditor editor = new(typeof(ListViewItem.ListViewSubItem));
        ListViewItem.ListViewSubItem subItem = new(null, null);

        string displayText = editor.TestAccessor.Dynamic.GetDisplayText(subItem);

        displayText.Should().Be("ListViewSubItem: {}");
    }

    [Fact]
    public void ListViewSubItemCollectionEditor_GetItems_SingleSubItem_ReturnsEmptyArray()
    {
        // When the collection has exactly one sub-item, the editor caches it as the
        // "first" sub-item but returns an empty array, because the first sub-item is
        // the column-0 text and is never user-editable in this editor.
        ListViewSubItemCollectionEditor editor = new(typeof(ListViewItem.ListViewSubItem));
        using ListView listView = new();
        ListViewItem item = new("Header");
        listView.Items.Add(item);

        object[] result = editor.TestAccessor.Dynamic.GetItems(item.SubItems);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ListViewSubItemCollectionEditor_GetItems_MultipleSubItems_ExcludesFirst()
    {
        // When the collection has more than one sub-item, GetItems returns all
        // sub-items except the first (which is the implicit "column 0" header).
        ListViewSubItemCollectionEditor editor = new(typeof(ListViewItem.ListViewSubItem));
        using ListView listView = new();
        ListViewItem item = new("Header");
        listView.Items.Add(item);
        item.SubItems.Add("Column1");
        item.SubItems.Add("Column2");
        item.SubItems.Add("Column3");

        object[] result = editor.TestAccessor.Dynamic.GetItems(item.SubItems);

        result.Should().HaveCount(3);
        result[0].Should().BeSameAs(item.SubItems[1]);
        result[1].Should().BeSameAs(item.SubItems[2]);
        result[2].Should().BeSameAs(item.SubItems[3]);
    }

    [Fact]
    public void ListViewSubItemCollectionEditor_SetItems_ClearsAndReAddsFirstThenNewValues()
    {
        // SetItems clears the collection, re-adds the previously-cached first
        // sub-item, then appends every value from the supplied array. The first
        // sub-item is obtained from a prior GetItems call.
        ListViewSubItemCollectionEditor editor = new(typeof(ListViewItem.ListViewSubItem));
        using ListView listView = new();
        ListViewItem item = new("Header");
        listView.Items.Add(item);
        item.SubItems.Add("Old1");
        item.SubItems.Add("Old2");

        // Drive GetItems so the editor caches the first sub-item internally.
        editor.TestAccessor.Dynamic.GetItems(item.SubItems);

        ListViewItem.ListViewSubItem newSubItem1 = new(item, "New1");
        ListViewItem.ListViewSubItem newSubItem2 = new(item, "New2");
        object[] newValues = [newSubItem1, newSubItem2];

        object? result = editor.TestAccessor.Dynamic.SetItems(item.SubItems, newValues);

        result.Should().BeSameAs(item.SubItems);
        item.SubItems.Count.Should().Be(3);
        item.SubItems[0].Text.Should().Be("Header");
        item.SubItems[1].Should().BeSameAs(newSubItem1);
        item.SubItems[2].Should().BeSameAs(newSubItem2);
    }

    [Fact]
    public void ListViewSubItemCollectionEditor_SetItems_EmptyArray_LeavesOnlyFirstSubItem()
    {
        // SetItems with an empty values array must clear the collection and then
        // re-add only the cached first sub-item, leaving the collection size at 1.
        ListViewSubItemCollectionEditor editor = new(typeof(ListViewItem.ListViewSubItem));
        using ListView listView = new();
        ListViewItem item = new("Header");
        listView.Items.Add(item);
        item.SubItems.Add("Extra1");
        item.SubItems.Add("Extra2");

        // Drive GetItems so the editor caches the first sub-item internally.
        editor.TestAccessor.Dynamic.GetItems(item.SubItems);

        object? result = editor.TestAccessor.Dynamic.SetItems(item.SubItems, Array.Empty<object>());

        result.Should().BeSameAs(item.SubItems);
        item.SubItems.Count.Should().Be(1);
        item.SubItems[0].Text.Should().Be("Header");
    }
}
