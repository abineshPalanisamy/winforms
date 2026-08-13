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

public class BitmapEditorTests
{
    [Fact]
    public void BitmapEditor_Ctor_Default()
    {
        BitmapEditor editor = new();
        Assert.False(editor.IsDropDownResizable);
    }

    [Fact]
    public void BitmapEditor_BitmapExtensions_Get_ReturnsExpected()
    {
        List<string> extensions = SubBitmapEditor.BitmapExtensions;
        Assert.Equal(new string[] { "bmp", "gif", "jpg", "jpeg", "png", "ico" }, extensions);
        Assert.Same(extensions, SubBitmapEditor.BitmapExtensions);
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void BitmapEditor_GetEditStyle_Invoke_ReturnsModal(ITypeDescriptorContext context)
    {
        BitmapEditor editor = new();
        Assert.Equal(UITypeEditorEditStyle.Modal, editor.GetEditStyle(context));
    }

    [Fact]
    public void BitmapEditor_GetExtensions_InvokeDefault_ReturnsExpected()
    {
        SubBitmapEditor editor = new();
        string[] extensions = editor.GetExtensions();
        Assert.Equal(new string[] { "bmp", "gif", "jpg", "jpeg", "png", "ico" }, extensions);
        Assert.NotSame(extensions, editor.GetExtensions());
    }

    [Fact]
    public void BitmapEditor_GetExtensions_InvokeCustomExtenders_ReturnsExpected()
    {
        CustomGetImageExtendersEditor editor = new();
        string[] extensions = editor.GetExtensions();
        Assert.Equal(new string[] { "bmp", "gif", "jpg", "jpeg", "png", "ico" }, extensions);
        Assert.NotSame(extensions, editor.GetExtensions());
    }

    [Fact]
    public void BitmapEditor_GetFileDialogDescription_Invoke_ReturnsExpected()
    {
        SubBitmapEditor editor = new();
        Assert.Equal("Bitmap files", editor.GetFileDialogDescription());
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void BitmapEditor_GetPaintValueSupported_Invoke_ReturnsTrue(ITypeDescriptorContext context)
    {
        BitmapEditor editor = new();
        Assert.True(editor.GetPaintValueSupported(context));
    }

    [Fact]
    public void BitmapEditor_LoadFromStream_BitmapStream_ReturnsExpected()
    {
        SubBitmapEditor editor = new();
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
    public void BitmapEditor_LoadFromStream_MetafileStream_ReturnsExpected()
    {
        SubBitmapEditor editor = new();
        using Stream stream = File.OpenRead("Resources/telescope_01.wmf");
        Bitmap result = Assert.IsType<Bitmap>(editor.LoadFromStream(stream));
        Assert.Equal(new Size(490, 654), result.Size);
    }

    [Fact]
    public void BitmapEditor_LoadFromStream_NullStream_ThrowsArgumentNullException()
    {
        SubBitmapEditor editor = new();
        Assert.Throws<ArgumentNullException>("stream", () => editor.LoadFromStream(null));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetEditValueInvalidProviderTestData))]
    public void BitmapEditor_EditValue_InvalidProvider_ReturnsValue(IServiceProvider provider, object value)
    {
        BitmapEditor editor = new();
        Assert.Same(value, editor.EditValue(null, provider, value));
    }

    [Fact]
    public void BitmapEditor_EditValue_ValidProvider_InitializesFileDialog()
    {
        BitmapEditor editor = new();
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
    public void BitmapEditor_EditValue_ValidProvider_ConfiguresFileDialogFilter()
    {
        // Validates the filter string constructed in EditValue combines this editor's
        // description/extensions and the extender editors' filters separated by '|'.
        BitmapEditor editor = new();
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
        // The default extension for BitmapEditor is "Bitmap files" with bmp/gif/jpg/jpeg/png/ico.
        Assert.StartsWith("Bitmap files", fileDialog.Filter);
        Assert.Contains("|", fileDialog.Filter);
    }

    [Fact]
    public void BitmapEditor_EditValue_ValidProvider_CalledTwice_ReusesFileDialog()
    {
        // Verifies the `is null` check semantics: a second EditValue call with a
        // valid provider reuses the existing _fileDialog rather than creating a new one.
        BitmapEditor editor = new();
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
    public void BitmapEditor_EditValue_ValidProvider_NonImageValue_InitializesFileDialog()
    {
        // Valid provider path with a non-Image value: covers the case where the value
        // passed in is not an Image, ensuring _fileDialog is still initialized.
        BitmapEditor editor = new();
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
    public void BitmapEditor_PaintValue_Invoke_Success()
    {
        BitmapEditor editor = new();
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

    [Fact]
    public void BitmapEditor_HasFileDialogField()
    {
        // The _fileDialog field is declared on the ImageEditor base class and inherited
        // by BitmapEditor. We look it up on the declaring type (ImageEditor) because
        // reflection does not walk up the inheritance chain by default.
        FieldInfo field = typeof(ImageEditor).GetField("_fileDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        Assert.NotNull(field);
        Assert.Equal(typeof(FileDialog), field.FieldType);
    }

    [Fact]
    public void BitmapEditor_FileDialogField_InitialValue_IsNull()
    {
        // Verifies that a freshly constructed BitmapEditor has _fileDialog == null,
        // which is required for the lazy initialization in EditValue.
        BitmapEditor editor = new();
        FieldInfo field = typeof(ImageEditor).GetField("_fileDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        object value = field.GetValue(editor);
        Assert.Null(value);
    }

    [Fact]
    public void BitmapEditor_FileDialogField_AfterInvalidProviderEditValue_RemainsNull()
    {
        // Confirms that the invalid-provider early-return path in EditValue
        // does NOT initialize the _fileDialog field.
        BitmapEditor editor = new();
        _ = editor.EditValue(null, null, "test");

        FieldInfo field = typeof(ImageEditor).GetField("_fileDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        object value = field.GetValue(editor);
        Assert.Null(value);
    }

    [Fact]
    public void BitmapEditor_LoadFromStream_PngStream_ReturnsExpected()
    {
        // Verify LoadFromStream works with a PNG stream (valid image path, not just BMP).
        SubBitmapEditor editor = new();
        using MemoryStream stream = new();
        using Bitmap image = new(16, 16);
        image.Save(stream, ImageFormat.Png);
        stream.Position = 0;
        Bitmap result = Assert.IsType<Bitmap>(editor.LoadFromStream(stream));
        Assert.Equal(new Size(16, 16), result.Size);
    }

    [Fact]
    public void BitmapEditor_LoadFromStream_GifStream_ReturnsExpected()
    {
        // Verify LoadFromStream works with a GIF stream (extension included in BitmapExtensions).
        SubBitmapEditor editor = new();
        using MemoryStream stream = new();
        using Bitmap image = new(8, 8);
        image.Save(stream, ImageFormat.Gif);
        stream.Position = 0;
        Bitmap result = Assert.IsType<Bitmap>(editor.LoadFromStream(stream));
        Assert.Equal(new Size(8, 8), result.Size);
    }

    [Fact]
    public void BitmapEditor_LoadFromStream_JpegStream_ReturnsExpected()
    {
        // Verify LoadFromStream works with a JPEG stream (extension included in BitmapExtensions).
        SubBitmapEditor editor = new();
        using MemoryStream stream = new();
        using Bitmap image = new(12, 12);
        image.Save(stream, ImageFormat.Jpeg);
        stream.Position = 0;
        Bitmap result = Assert.IsType<Bitmap>(editor.LoadFromStream(stream));
        Assert.Equal(new Size(12, 12), result.Size);
    }

    private static bool WaitForFileDialogInitialization(BitmapEditor editor, TimeSpan timeout)
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

    private static FileDialog GetFileDialog(BitmapEditor editor)
    {
        FieldInfo field = typeof(ImageEditor).GetField("_fileDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (FileDialog)field.GetValue(editor)!;
    }

    private class SubBitmapEditor : BitmapEditor
    {
#pragma warning disable IDE1006 // Naming Styles
        public static new List<string> BitmapExtensions = BitmapEditor.BitmapExtensions;
#pragma warning restore IDE1006

        public new string[] GetExtensions() => base.GetExtensions();

        public new string GetFileDialogDescription() => base.GetFileDialogDescription();

        public new Image LoadFromStream(Stream stream) => base.LoadFromStream(stream);
    }

    private class CustomGetImageExtendersEditor : BitmapEditor
    {
        public new string[] GetExtensions() => base.GetExtensions();

        protected override Type[] GetImageExtenders() => [typeof(CustomGetExtensionsEditor)];
    }

    private class CustomGetExtensionsEditor : ImageEditor
    {
        protected override string[] GetExtensions() => ["CustomGetExtensionsEditor"];
    }
}
