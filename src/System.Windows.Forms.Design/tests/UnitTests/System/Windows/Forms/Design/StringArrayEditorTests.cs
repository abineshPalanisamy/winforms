// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.TestUtilities;

namespace System.Windows.Forms.Design.Tests;

public class StringArrayEditorTests
{
    [Theory]
    [InlineData(typeof(string[]), typeof(string))]
    [InlineData(typeof(int[]), typeof(int))]
    public void StringArrayEditor_Ctor_Type(Type type, Type expectedItemType)
    {
        SubStringArrayEditor editor = new(type);
        Assert.Equal(expectedItemType, editor.CollectionItemType);
        Assert.Equal(type, editor.CollectionType);
        Assert.Null(editor.Context);
        Assert.Equal("net.ComponentModel.StringCollectionEditor", editor.HelpTopic);
        Assert.False(editor.IsDropDownResizable);
    }

    [Fact]
    public void StringArrayEditor_Ctor_NullType_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SubStringArrayEditor(null));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void StringArrayEditor_GetEditStyle_Invoke_ReturnsModal(ITypeDescriptorContext context)
    {
        StringArrayEditor editor = new(typeof(string[]));
        Assert.Equal(UITypeEditorEditStyle.Modal, editor.GetEditStyle(context));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void StringArrayEditor_GetPaintValueSupported_Invoke_ReturnsFalse(ITypeDescriptorContext context)
    {
        StringArrayEditor editor = new(typeof(string[]));
        Assert.False(editor.GetPaintValueSupported(context));
    }

    [Fact]
    public void StringArrayEditor_CreateCollectionForm_Invoke_Success()
    {
        SubStringArrayEditor editor = new(typeof(string[]));
        using Form form = editor.CreateCollectionForm();
        Assert.NotNull(form);
    }

    [Theory]
    [InlineData(typeof(string[]), typeof(string))]
    [InlineData(typeof(int[]), typeof(int))]
    public void StringArrayEditor_CreateCollectionItemType_Invoke_ReturnsExpected(Type type, Type expected)
    {
        SubStringArrayEditor editor = new(type);
        Assert.Equal(expected, editor.CreateCollectionItemType());
    }

    [Fact]
    public void StringArrayEditor_CreateCollectionItemType_TypeWithoutElementType_ReturnsStringArray()
    {
        // CollectionType.GetElementType() returns null for non-array types,
        // so the override should fall back to typeof(string[]).
        SubStringArrayEditor editor = new(typeof(string));
        Assert.Equal(typeof(string[]), editor.CreateCollectionItemType());
    }

    public static IEnumerable<object[]> GetItems_TestData()
    {
        yield return new object[] { null, Array.Empty<object>() };
        yield return new object[] { new object(), Array.Empty<object>() };
        yield return new object[] { new string[] { "a", "b", "c" }, new object[] { "a", "b", "c" } };
        yield return new object[] { Array.Empty<string>(), Array.Empty<object>() };
    }

    [Theory]
    [MemberData(nameof(GetItems_TestData))]
    public void StringArrayEditor_GetItems_Invoke_ReturnsExpected(object editValue, object[] expected)
    {
        SubStringArrayEditor editor = new(typeof(string[]));
        object[] result = editor.GetItems(editValue);
        Assert.Equal(expected, result);
    }

    public static IEnumerable<object[]> SetItems_Array_TestData()
    {
        yield return new object[] { null, Array.Empty<object>(), Array.Empty<string>() };
        yield return new object[] { null, new object[] { "a", "b", "c" }, new string[] { "a", "b", "c" } };
        yield return new object[] { new string[] { "x", "y" }, new object[] { "a", "b", "c" }, new string[] { "a", "b", "c" } };
    }

    [Theory]
    [MemberData(nameof(SetItems_Array_TestData))]
    public void StringArrayEditor_SetItems_InvokeArray_ReturnsCopy(object editValue, object[] value, string[] expected)
    {
        SubStringArrayEditor editor = new(typeof(string[]));
        string[] result = Assert.IsType<string[]>(editor.SetItems(editValue, value));
        Assert.Equal(expected, result);
    }

    [Fact]
    public void StringArrayEditor_SetItems_NonArrayEditValue_ReturnsEditValue()
    {
        SubStringArrayEditor editor = new(typeof(string[]));
        object editValue = new();
        object[] value = ["a", "b"];

        Assert.Same(editValue, editor.SetItems(editValue, value));
    }

    private class SubStringArrayEditor : StringArrayEditor
    {
        public SubStringArrayEditor(Type type) : base(type)
        {
        }

        public new Type CollectionItemType => base.CollectionItemType;

        public new Type CollectionType => base.CollectionType;

        public new ITypeDescriptorContext Context => base.Context;

        public new string HelpTopic => base.HelpTopic;

        public new Form CreateCollectionForm() => base.CreateCollectionForm();

        public new Type CreateCollectionItemType() => base.CreateCollectionItemType();

        public new object[] GetItems(object editValue) => base.GetItems(editValue);

        public new object SetItems(object editValue, object[] value) => base.SetItems(editValue, value);
    }
}
