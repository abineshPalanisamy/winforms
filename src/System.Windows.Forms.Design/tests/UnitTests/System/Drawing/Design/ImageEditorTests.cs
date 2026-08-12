// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms.Design;
using System.Windows.Forms.TestUtilities;
using Moq;

namespace System.Drawing.Design.Tests;

public class ImageEditorTests
{
    [Fact]
    public void ImageEditor_Ctor_Default()
    {
        ImageEditor editor = new();
        Assert.False(editor.IsDropDownResizable);
    }

    public static IEnumerable<object[]> CreateExtensionsString_TestData()
    {
        yield return new object[] { null, ",", null };
        yield return new object[] { Array.Empty<string>(), ",", null };
        yield return new object[] { new string[] { "a", "b", "c" }, ",", "*.a,*.b,*.c" };
        yield return new object[] { new string[] { "a", "b", "c" }, "", "*.a*.b*.c" };
        yield return new object[] { new string[] { "a", "b", "c" }, null, "*.a*.b*.c" };
        yield return new object[] { new string[] { null, null, null }, ",", "" };
        yield return new object[] { new string[] { string.Empty, string.Empty, string.Empty }, ",", "" };
    }

    [Theory]
    [MemberData(nameof(CreateExtensionsString_TestData))]
    public void ImageEditor_CreateExtensionsString_Invoke_ReturnsExpected(string[] extensions, string sep, string expected)
    {
        Assert.Equal(expected, SubImageEditor.CreateExtensionsString(extensions, sep));
    }

    [Fact]
    public void ImageEditor_CreateFilterEntry_Invoke_CallsGetExtensionsOnce()
    {
        CustomGetImageExtendersEditor editor = new()
        {
            GetImageExtendersResult = [typeof(PublicImageEditor), typeof(PrivateImageEditor)]
        };
        Assert.Equal("CustomGetImageExtendersEditor(*.PublicImageEditor,*.PrivateImageEditor)|*.PublicImageEditor;*.PrivateImageEditor", SubImageEditor.CreateFilterEntry(editor));
        Assert.Equal(1, editor.GetImageExtendersCallCount);
    }

    [Fact]
    public void ImageEditor_CreateFilterEntry_NullE_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("e", () => SubImageEditor.CreateFilterEntry(null));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetEditValueInvalidProviderTestData))]
    public void ImageEditor_EditValue_InvalidProvider_ReturnsValue(IServiceProvider provider, object value)
    {
        ImageEditor editor = new();
        Assert.Same(value, editor.EditValue(null, provider, value));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void ImageEditor_GetEditStyle_Invoke_ReturnsModal(ITypeDescriptorContext context)
    {
        ImageEditor editor = new();
        Assert.Equal(UITypeEditorEditStyle.Modal, editor.GetEditStyle(context));
    }

    [Fact]
    public void ImageEditor_GetExtensions_InvokeDefault_ReturnsExpected()
    {
        SubImageEditor editor = new();
        string[] extensions = editor.GetExtensions();
        Assert.Equal(new string[] { "bmp", "gif", "jpg", "jpeg", "png", "ico", "emf", "wmf" }, extensions);
        Assert.NotSame(extensions, editor.GetExtensions());
    }

    [Fact]
    public void ImageEditor_GetExtensions_InvokeCustom_CallsGetImageExtendersOnce()
    {
        CustomGetImageExtendersEditor editor = new()
        {
            GetImageExtendersResult = [typeof(PublicImageEditor), typeof(PrivateImageEditor), typeof(ImageEditor), typeof(NullExtensionsImageEditor)]
        };
        Assert.Equal(new string[] { "PublicImageEditor", "PrivateImageEditor" }, editor.GetExtensions());
        Assert.Equal(1, editor.GetImageExtendersCallCount);
    }

    [Fact]
    public void ImageEditor_GetExtensions_InvokeInvalid_ReturnsExpected()
    {
        CustomGetImageExtendersEditor editor = new()
        {
            GetImageExtendersResult = [typeof(object), null]
        };
        Assert.Empty(editor.GetExtensions());
        Assert.Equal(1, editor.GetImageExtendersCallCount);
    }

    [Fact]
    public void ImageEditor_GetFileDialogDescription_Invoke_ReturnsExpected()
    {
        SubImageEditor editor = new();
        Assert.Equal("All image files", editor.GetFileDialogDescription());
    }

    [Fact]
    public void ImageEditor_GetImageExtenders_Invoke_ReturnsExpected()
    {
        SubImageEditor editor = new();
        Type[] extenders = editor.GetImageExtenders();
        Assert.Equal([typeof(BitmapEditor), typeof(MetafileEditor)], extenders);
        Assert.Same(extenders, editor.GetImageExtenders());
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void ImageEditor_GetPaintValueSupported_Invoke_ReturnsTrue(ITypeDescriptorContext context)
    {
        ImageEditor editor = new();
        Assert.True(editor.GetPaintValueSupported(context));
    }

    [Fact]
    public void ImageEditor_LoadFromStream_BitmapStream_ReturnsExpected()
    {
        SubImageEditor editor = new();
        using MemoryStream stream = new();
        using Bitmap image = new(10, 10);
        image.Save(stream, ImageFormat.Bmp);
        stream.Position = 0;
        Bitmap result = Assert.IsType<Bitmap>(editor.LoadFromStream(stream));
        Assert.Equal(new Size(10, 10), result.Size);

        using MemoryStream resultStream = new();
        result.Save(resultStream, ImageFormat.Bmp);
        Assert.Equal(stream.Length, resultStream.Length);
    }

    [Fact]
    public void ImageEditor_LoadFromStream_MetafileStream_ThrowsArgumentException()
    {
        SubImageEditor editor = new();
        using Stream stream = File.OpenRead("Resources/telescope_01.wmf");
        Assert.Throws<ArgumentException>(() => editor.LoadFromStream(stream));
    }

    [Fact]
    public void ImageEditor_LoadFromStream_NullStream_ThrowsArgumentNullException()
    {
        SubImageEditor editor = new();
        Assert.Throws<ArgumentNullException>("stream", () => editor.LoadFromStream(null));
    }

    [Fact]
    public void ImageEditor_PaintValue_Invoke_Success()
    {
        ImageEditor editor = new();
        using Bitmap image = new(10, 10);
        using Bitmap otherImage = new(3, 2);
        using Graphics graphics = Graphics.FromImage(image);
        otherImage.SetPixel(0, 0, Color.Red);
        otherImage.SetPixel(1, 0, Color.Red);
        otherImage.SetPixel(2, 0, Color.Red);
        otherImage.SetPixel(0, 1, Color.Red);
        otherImage.SetPixel(1, 1, Color.Red);
        otherImage.SetPixel(2, 1, Color.Red);

        PaintValueEventArgs e = new(null, otherImage, graphics, new Rectangle(1, 2, 3, 4));
        editor.PaintValue(e);
    }

    public static IEnumerable<object[]> PaintValue_InvalidArgsValue_TestData()
    {
        yield return new object[] { null };
        yield return new object[] { new() };
    }

    [Theory]
    [MemberData(nameof(PaintValue_InvalidArgsValue_TestData))]
    public void ImageEditor_PaintValue_InvalidArgsValue_Nop(object value)
    {
        ImageEditor editor = new();
        using Bitmap image = new(10, 10);
        using Graphics graphics = Graphics.FromImage(image);
        PaintValueEventArgs e = new(null, value, graphics, new Rectangle(1, 2, 3, 4));
        editor.PaintValue(e);
    }

    [Fact]
    public void ImageEditor_PaintValue_NullE_Nop()
    {
        ImageEditor editor = new();
        editor.PaintValue(null);
    }

    [Fact]
    public void ImageEditor_EditValue_ValidProvider_InitializesFileDialog()
    {
        ImageEditor editor = new();
        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        Thread thread = new(() =>
        {
            try
            {
                _ = editor.EditValue(null, mockServiceProvider.Object, null);
            }
            catch
            {
                // The dialog is expected to be cancelled / aborted by the test runner.
            }
        })
        { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        bool initialized = WaitForFileDialogInitialization(editor, TimeSpan.FromSeconds(2));
        Assert.True(initialized, "_fileDialog should be initialized after EditValue with a valid provider.");
    }

    [Fact]
    public void ImageEditor_EditValue_ValidProvider_ConfiguresFileDialogFilter()
    {
        // Validates the filter string constructed in EditValue combines this editor's
        // description/extensions and the extender editors' filters separated by '|'.
        ImageEditor editor = new();
        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        Thread thread = new(() =>
        {
            try
            {
                _ = editor.EditValue(null, mockServiceProvider.Object, null);
            }
            catch
            {
            }
        })
        { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        bool initialized = WaitForFileDialogInitialization(editor, TimeSpan.FromSeconds(2));
        Assert.True(initialized);

        OpenFileDialog fileDialog = (OpenFileDialog)GetFileDialog(editor);
        Assert.NotNull(fileDialog);
        // The default extenders are BitmapEditor (bmp, gif, jpg, jpeg, png, ico) and
        // MetafileEditor (emf, wmf). The filter should start with this editor's
        // description and contain entries for the extender editors.
        Assert.StartsWith("All image files", fileDialog.Filter);
        Assert.Contains("|", fileDialog.Filter);
    }

    [Fact]
    public void ImageEditor_EditValue_ValidProvider_CalledTwice_ReusesFileDialog()
    {
        // Verifies the `is null` check semantics: a second EditValue call with a
        // valid provider reuses the existing _fileDialog rather than creating a new one.
        ImageEditor editor = new();
        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        void InvokeEdit()
        {
            try
            {
                _ = editor.EditValue(null, mockServiceProvider.Object, null);
            }
            catch
            {
            }
        }

        Thread thread1 = new(InvokeEdit) { IsBackground = true };
        thread1.SetApartmentState(ApartmentState.STA);
        thread1.Start();
        thread1.Join(TimeSpan.FromSeconds(2));

        OpenFileDialog firstDialog = (OpenFileDialog)GetFileDialog(editor);
        Assert.NotNull(firstDialog);

        Thread thread2 = new(InvokeEdit) { IsBackground = true };
        thread2.SetApartmentState(ApartmentState.STA);
        thread2.Start();
        thread2.Join(TimeSpan.FromSeconds(2));

        OpenFileDialog secondDialog = (OpenFileDialog)GetFileDialog(editor);
        Assert.NotNull(secondDialog);
        Assert.Same(firstDialog, secondDialog);
    }

    [Fact]
    public void ImageEditor_EditValue_ValidProvider_NonImageValue_InitializesFileDialog()
    {
        // Valid provider path with a non-Image value: covers the case where the value
        // passed in is not an Image, ensuring _fileDialog is still initialized.
        ImageEditor editor = new();
        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        Thread thread = new(() =>
        {
            try
            {
                _ = editor.EditValue(null, mockServiceProvider.Object, "not-an-image");
            }
            catch
            {
            }
        })
        { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        bool initialized = WaitForFileDialogInitialization(editor, TimeSpan.FromSeconds(2));
        Assert.True(initialized, "_fileDialog should be initialized after EditValue with a valid provider, even for a non-Image value.");
    }

    [Fact]
    public void ImageEditor_HasFileDialogField()
    {
        FieldInfo field = typeof(ImageEditor).GetField("_fileDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        Assert.NotNull(field);
        Assert.Equal(typeof(FileDialog), field.FieldType);
    }

    [Fact]
    public void ImageEditor_FileDialogField_InitialValue_IsNull()
    {
        // Verifies that a freshly constructed ImageEditor has _fileDialog == null,
        // which is required for the lazy initialization in EditValue.
        ImageEditor editor = new();
        FieldInfo field = typeof(ImageEditor).GetField("_fileDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        object value = field.GetValue(editor);
        Assert.Null(value);
    }

    [Fact]
    public void ImageEditor_FileDialogField_AfterInvalidProviderEditValue_RemainsNull()
    {
        // Confirms that the invalid-provider early-return path in EditValue
        // does NOT initialize the _fileDialog field.
        ImageEditor editor = new();
        _ = editor.EditValue(null, null, "test");

        FieldInfo field = typeof(ImageEditor).GetField("_fileDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        object value = field.GetValue(editor);
        Assert.Null(value);
    }

    [Fact]
    public void ImageEditor_GetExtensions_InvokeWithEmptyExtenders_ReturnsEmpty()
    {
        // Verifies that when GetImageExtenders returns an empty array, GetExtensions
        // returns an empty string array (no extenders to iterate).
        EmptyExtendersImageEditor editor = new();
        Assert.Empty(editor.GetExtensions());
    }

    [Fact]
    public void ImageEditor_GetExtensions_InvokeWithNullEntryInExtenders_SkipsNull()
    {
        // Verifies that GetExtensions skips a null entry in the extenders list
        // (the `extender is null` check inside the loop).
        NullEntryExtendersImageEditor editor = new();
        string[] extensions = editor.GetExtensions();
        Assert.Equal(new string[] { "PublicImageEditor" }, extensions);
    }

    [Fact]
    public void ImageEditor_GetExtensions_InvokeWithNonImageEditorExtender_SkipsIt()
    {
        // Verifies that GetExtensions skips an entry that is not assignable to
        // ImageEditor (the `!typeof(ImageEditor).IsAssignableFrom(extender)` check).
        NonImageEditorExtendersImageEditor editor = new();
        Assert.Empty(editor.GetExtensions());
    }

    [Fact]
    public void ImageEditor_GetExtensions_InvokeWithExtenderReturningNullExtensions_SkipsIt()
    {
        // Verifies that GetExtensions skips an extender whose GetExtensions()
        // returns null (the `if (extensions is not null)` check).
        NullExtensionsExtendersImageEditor editor = new();
        Assert.Empty(editor.GetExtensions());
    }

    [Fact]
    public void ImageEditor_LoadFromStream_PngStream_ReturnsExpected()
    {
        // Verify LoadFromStream works with a PNG stream (valid image path, not just BMP).
        SubImageEditor editor = new();
        using MemoryStream stream = new();
        using Bitmap image = new(16, 16);
        image.Save(stream, ImageFormat.Png);
        stream.Position = 0;
        Bitmap result = Assert.IsType<Bitmap>(editor.LoadFromStream(stream));
        Assert.Equal(new Size(16, 16), result.Size);
    }

    private static bool WaitForFileDialogInitialization(ImageEditor editor, TimeSpan timeout)
    {
        FieldInfo field = typeof(ImageEditor).GetField("_fileDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        DateTime deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (field.GetValue(editor) is not null)
            {
                return true;
            }

            Thread.Sleep(20);
        }

        return field.GetValue(editor) is not null;
    }

    private static FileDialog GetFileDialog(ImageEditor editor)
    {
        FieldInfo field = typeof(ImageEditor).GetField("_fileDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (FileDialog)field.GetValue(editor)!;
    }

    private class SubImageEditor : ImageEditor
    {
        public static new string CreateExtensionsString(string[] extensions, string sep)
        {
            return ImageEditor.CreateExtensionsString(extensions, sep);
        }

        public static new string CreateFilterEntry(ImageEditor e)
        {
            return ImageEditor.CreateFilterEntry(e);
        }

        public new string[] GetExtensions() => base.GetExtensions();

        public new string GetFileDialogDescription() => base.GetFileDialogDescription();

        public new Type[] GetImageExtenders() => base.GetImageExtenders();

        public new Image LoadFromStream(Stream stream) => base.LoadFromStream(stream);
    }

    private class CustomGetImageExtendersEditor : ImageEditor
    {
        public int GetImageExtendersCallCount { get; set; }

        public Type[] GetImageExtendersResult { get; set; }

        public new string[] GetExtensions() => base.GetExtensions();

        protected override string GetFileDialogDescription() => "CustomGetImageExtendersEditor";

        protected override Type[] GetImageExtenders()
        {
            GetImageExtendersCallCount++;
            return GetImageExtendersResult;
        }
    }

    private class PublicImageEditor : ImageEditor
    {
        public PublicImageEditor()
        {
        }

        protected override string[] GetExtensions() => ["PublicImageEditor"];
    }

    private class PrivateImageEditor : ImageEditor
    {
        private PrivateImageEditor()
        {
        }

        protected override string[] GetExtensions() => ["PrivateImageEditor"];
    }

    private class NullExtensionsImageEditor : ImageEditor
    {
        public NullExtensionsImageEditor()
        {
        }

        protected override string[] GetExtensions() => null;
    }

    private class EmptyExtendersImageEditor : ImageEditor
    {
        public new string[] GetExtensions() => base.GetExtensions();

        protected override Type[] GetImageExtenders() => [];
    }

    private class NullEntryExtendersImageEditor : ImageEditor
    {
        public new string[] GetExtensions() => base.GetExtensions();

        protected override Type[] GetImageExtenders() => [null, typeof(PublicImageEditor)];
    }

    private class NonImageEditorExtendersImageEditor : ImageEditor
    {
        public new string[] GetExtensions() => base.GetExtensions();

        protected override Type[] GetImageExtenders() => [typeof(object)];
    }

    private class NullExtensionsExtendersImageEditor : ImageEditor
    {
        public new string[] GetExtensions() => base.GetExtensions();

        protected override Type[] GetImageExtenders() => [typeof(NullExtensionsImageEditor)];
    }
}
