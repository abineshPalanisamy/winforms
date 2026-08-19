// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections;
using System.Drawing;

namespace System.Windows.Forms.Design.Tests;

public class ImageCollectionEditorTests
{
    [Fact]
    public void ImageCollectionEditor_Constructor_SetsCollectionType()
    {
        Type expectedType = typeof(ImageList.ImageCollection);

        ImageCollectionEditor editor = new(expectedType);

        Type actualType = editor.TestAccessor.Dynamic.CollectionType;
        actualType.Should().Be(expectedType);
    }

    [Fact]
    public void ImageCollectionEditor_GetDisplayText_NullValue_ReturnsEmpty()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));

        string result = editor.CallGetDisplayText(null);

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void ImageCollectionEditor_GetDisplayText_ValueWithNonEmptyName_ReturnsName()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));
        using Bitmap image = new(10, 10);
        ImageListImage imageListImage = new(image, "MyImage");

        string result = editor.CallGetDisplayText(imageListImage);

        result.Should().Be("MyImage");
    }

    [Fact]
    public void ImageCollectionEditor_GetDisplayText_ImageListImageWithoutName_UsesTypeConverter()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));
        using Bitmap image = new(10, 10);
        ImageListImage imageListImage = new(image);

        string result = editor.CallGetDisplayText(imageListImage);

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ImageCollectionEditor_GetDisplayText_ImageListImageWithEmptyName_UsesTypeConverter()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));
        using Bitmap image = new(10, 10);
        ImageListImage imageListImage = new(image) { Name = string.Empty };

        string result = editor.CallGetDisplayText(imageListImage);

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ImageCollectionEditor_GetDisplayText_RawImage_ReturnsConvertedString()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));
        using Bitmap image = new(10, 10);

        string result = editor.CallGetDisplayText(image);

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ImageCollectionEditor_GetDisplayText_ObjectWithoutNameProperty_ReturnsConvertedString()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));
        object value = new();

        string result = editor.CallGetDisplayText(value);

        // TypeDescriptor.GetConverter(object).ConvertToString(object) returns the full type name.
        result.Should().Be(value.GetType().FullName);
    }

    [Fact]
    public void ImageCollectionEditor_GetObjectsFromInstance_ArrayList_ReturnsSameArrayList()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));
        ArrayList list = new()
        {
            new Bitmap(1, 1),
            new Bitmap(2, 2)
        };

        IList result = editor.CallGetObjectsFromInstance(list);

        result.Should().BeSameAs(list);
    }

    [Fact]
    public void ImageCollectionEditor_GetObjectsFromInstance_NonArrayList_ReturnsNull()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));

        IList result = editor.CallGetObjectsFromInstance(new object());

        result.Should().BeNull();
    }

    [Fact]
    public void ImageCollectionEditor_GetObjectsFromInstance_Null_ReturnsNull()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));

        IList result = editor.CallGetObjectsFromInstance(null);

        result.Should().BeNull();
    }

    [Fact]
    public void ImageCollectionEditor_GetItems_ImageCollection_ReturnsImageListImages()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));
        using ImageList imageList = new();
        using Bitmap image1 = new(10, 10);
        using Bitmap image2 = new(20, 20);
        imageList.Images.Add("Key1", image1);
        imageList.Images.Add("Key2", image2);

        object[] result = editor.CallGetItems(imageList.Images);

        result.Should().HaveCount(2);
        result[0].Should().BeOfType<ImageListImage>();
        result[1].Should().BeOfType<ImageListImage>();
        ((ImageListImage)result[0]).Name.Should().Be("Key1");
        ((ImageListImage)result[1]).Name.Should().Be("Key2");
    }

    [Fact]
    public void ImageCollectionEditor_GetItems_EmptyImageCollection_ReturnsEmptyArray()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));
        using ImageList imageList = new();

        object[] result = editor.CallGetItems(imageList.Images);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ImageCollectionEditor_GetItems_NonImageCollection_CallsBase()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));
        object editValue = new();

        object[] result = editor.CallGetItems(editValue);

        // Base implementation returns items in an array, possibly empty for an object that is not a collection
        result.Should().NotBeNull();
    }

    [Fact]
    public void ImageCollectionEditor_SetItems_ImageCollectionWithImages_AddsImages()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));
        using ImageList imageList = new();
        using Bitmap image1 = new(10, 10);
        using Bitmap image2 = new(20, 20);

        object[] newItems = [image1, image2];
        object result = editor.CallSetItems(imageList.Images, newItems);

        result.Should().BeSameAs(imageList.Images);
        imageList.Images.Count.Should().Be(2);
    }

    [Fact]
    public void ImageCollectionEditor_SetItems_ImageCollectionWithImageListImages_AddsWithNames()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));
        using ImageList imageList = new();
        using Bitmap image1 = new(10, 10);
        using Bitmap image2 = new(20, 20);
        ImageListImage imageListImage1 = new(image1, "Key1");
        ImageListImage imageListImage2 = new(image2, "Key2");

        object[] newItems = [imageListImage1, imageListImage2];
        object result = editor.CallSetItems(imageList.Images, newItems);

        result.Should().BeSameAs(imageList.Images);
        imageList.Images.Count.Should().Be(2);
        // ImageList wraps images internally and scales them to its ImageSize, so we just verify
        // the count rather than the exact image references or sizes.
    }

    [Fact]
    public void ImageCollectionEditor_SetItems_ImageCollectionWithMixedItems_AddsValid()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));
        using ImageList imageList = new();
        using Bitmap image1 = new(10, 10);
        using Bitmap image2 = new(20, 20);
        ImageListImage imageListImage = new(image2, "Key2");

        object[] newItems = [image1, imageListImage];
        object result = editor.CallSetItems(imageList.Images, newItems);

        result.Should().BeSameAs(imageList.Images);
        imageList.Images.Count.Should().Be(2);
    }

    [Fact]
    public void ImageCollectionEditor_SetItems_ImageCollectionWithEmptyArray_ClearsCollection()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));
        using ImageList imageList = new();
        using Bitmap image = new(10, 10);
        imageList.Images.Add(image);

        object[] newItems = [];
        object result = editor.CallSetItems(imageList.Images, newItems);

        result.Should().BeSameAs(imageList.Images);
        imageList.Images.Count.Should().Be(0);
    }

    [Fact]
    public void ImageCollectionEditor_SetItems_ImageCollectionWithNullArray_ClearsCollection()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));
        using ImageList imageList = new();
        using Bitmap image = new(10, 10);
        imageList.Images.Add(image);

        object result = editor.CallSetItems(imageList.Images, null);

        result.Should().BeSameAs(imageList.Images);
        imageList.Images.Count.Should().Be(0);
    }

    [Fact]
    public void ImageCollectionEditor_SetItems_NonImageCollection_CallsBase()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));
        object editValue = new();
        object[] newItems = [new object()];

        object result = editor.CallSetItems(editValue, newItems);

        // Base implementation for an object that is not IList returns the editValue as-is
        result.Should().BeSameAs(editValue);
    }

    [Fact]
    public void ImageCollectionEditor_CreateCollectionForm_ReturnsFormWithExpectedText()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));

        using Form form = editor.CallCreateCollectionForm();

        form.Text.Should().Be(SR.ImageCollectionEditorFormText);
    }

    [Fact]
    public void ImageCollectionEditor_CreateInstance_ReturnsImageFromEditor()
    {
        SubImageCollectionEditor editor = new(typeof(ImageList.ImageCollection));

        // The CreateInstance method delegates to the ImageListImageEditor. Since no provider is supplied,
        // the editor's EditValue returns the value parameter (null) as-is.
        object result = editor.CallCreateInstance(typeof(ImageListImage));

        result.Should().BeNull();
    }

    private class SubImageCollectionEditor : ImageCollectionEditor
    {
        public SubImageCollectionEditor(Type type)
            : base(type)
        {
        }

        public string CallGetDisplayText(object value) => GetDisplayText(value);

        public IList CallGetObjectsFromInstance(object array) => GetObjectsFromInstance(array);

        public object[] CallGetItems(object editValue) => GetItems(editValue);

        public object CallSetItems(object editValue, object[] value) => SetItems(editValue, value);

        public Form CallCreateCollectionForm() => CreateCollectionForm();

        public object CallCreateInstance(Type type) => CreateInstance(type);
    }
}
