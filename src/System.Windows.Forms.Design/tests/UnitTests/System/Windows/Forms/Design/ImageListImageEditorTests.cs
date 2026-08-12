// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms.TestUtilities;
using Moq;

namespace System.Windows.Forms.Design.Tests;

public class ImageListImageEditorTests
{
    [Fact]
    public void ImageListImageEditor_Ctor_Default()
    {
        ImageListImageEditor editor = new();
        Assert.False(editor.IsDropDownResizable);
    }

    [Fact]
    public void ImageListImageEditor_LoadImageFromStream_BitmapStream_ReturnsExpected()
    {
        ImageListImageEditor editor = new();
        var editor_LoadImageFromStream = editor.TestAccessor.CreateDelegate<Func<Stream, bool, ImageListImage>>("LoadImageFromStream");

        using MemoryStream stream = new();
        using Bitmap image = new(10, 10);
        image.Save(stream, ImageFormat.Bmp);
        stream.Position = 0;

        var result = Assert.IsType<ImageListImage>(editor_LoadImageFromStream(stream, false));
        var resultImage = Assert.IsType<Bitmap>(result.Image);
        Assert.Equal(new Size(10, 10), result.Size);
        Assert.Equal(new Size(10, 10), resultImage.Size);

        using MemoryStream resultStream = new();
        result.Image.Save(resultStream, ImageFormat.Bmp);
        Assert.Equal(stream.Length, resultStream.Length);
    }

    [Fact]
    public void ImageListImageEditor_GetImageExtenders_Invoke_ReturnsExpected()
    {
        // The default extenders should be only [typeof(BitmapEditor)] because metafiles
        // are not supported in ImageListImageEditor.
        SubImageListImageEditor editor = new();
        Type[] extenders = editor.GetImageExtenders();
        Assert.Equal([typeof(BitmapEditor)], extenders);
        Assert.Same(extenders, editor.GetImageExtenders());
    }

    [Fact]
    public void ImageListImageEditor_GetFileDialogDescription_Invoke_ReturnsExpected()
    {
        SubImageListImageEditor editor = new();
        Assert.Equal("All image files", editor.GetFileDialogDescription());
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void ImageListImageEditor_GetPaintValueSupported_Invoke_ReturnsTrue(ITypeDescriptorContext context)
    {
        ImageListImageEditor editor = new();
        Assert.True(editor.GetPaintValueSupported(context));
    }

    [Fact]
    public void ImageListImageEditor_PaintValue_Invoke_Success()
    {
        // PaintValue with an Image value (base-class path).
        ImageListImageEditor editor = new();
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
    public void ImageListImageEditor_PaintValue_WithImageListImageValue_DelegatesToBase()
    {
        // PaintValue with an ImageListImage value: covers the `is ImageListImage image`
        // branch which extracts the inner Image and forwards to the base implementation.
        ImageListImageEditor editor = new();
        using Bitmap innerImage = new(3, 2);
        using Bitmap surface = new(10, 10);
        using Graphics graphics = Graphics.FromImage(surface);

        ImageListImage imageListImage = new(innerImage);

        PaintValueEventArgs e = new(null, imageListImage, graphics, new Rectangle(1, 2, 3, 4));
        editor.PaintValue(e);
    }

    [Fact]
    public void ImageListImageEditor_EditValue_NullProvider_ReturnsValue()
    {
        // Valid: when provider is null, EditValue returns the passed-in value directly.
        ImageListImageEditor editor = new();
        object value = new();
        Assert.Same(value, editor.EditValue(null, null, value));
    }

    [Fact]
    public void ImageListImageEditor_EditValue_ProviderWithoutEditorService_ReturnsEmptyList()
    {
        // Valid: when the provider does not provide IWindowsFormsEditorService,
        // EditValue returns an empty ArrayList.
        ImageListImageEditor editor = new();
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns((object)null);

        object result = editor.EditValue(null, mockServiceProvider.Object, null);
        ArrayList images = Assert.IsType<ArrayList>(result);
        Assert.Empty(images);
    }

    [Fact]
    public void ImageListImageEditor_EditValue_InvalidProviderReturnsObject_ReturnsEmptyList()
    {
        // Valid: when the provider returns a non-null object that is not
        // IWindowsFormsEditorService, EditValue returns an empty ArrayList.
        ImageListImageEditor editor = new();
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(new object());

        object result = editor.EditValue(null, mockServiceProvider.Object, null);
        ArrayList images = Assert.IsType<ArrayList>(result);
        Assert.Empty(images);
    }

    [Fact]
    public void ImageListImageEditor_EditValue_ValidProvider_InitializesFileDialog()
    {
        // Valid: with a valid IWindowsFormsEditorService provider, EditValue
        // initializes the _fileDialog.
        ImageListImageEditor editor = new();
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
    public void ImageListImageEditor_EditValue_ValidProvider_FileDialogIsMultiselect()
    {
        // Valid: confirm the lazily created file dialog has Multiselect = true
        // (ImageListImageEditor supports selecting multiple images).
        ImageListImageEditor editor = new();
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
        Assert.True(fileDialog.Multiselect);
    }

    [Fact]
    public void ImageListImageEditor_EditValue_ValidProvider_FileDialogFilterIsSet()
    {
        // Valid: confirm the lazily created file dialog has its Filter property
        // populated with the constructed filter string.
        ImageListImageEditor editor = new();
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
        Assert.False(string.IsNullOrEmpty(fileDialog.Filter));
    }

    [Fact]
    public void ImageListImageEditor_EditValue_ValidProvider_CalledTwice_ReusesFileDialog()
    {
        // Valid: a second EditValue call with a valid provider reuses the
        // existing _fileDialog rather than creating a new one.
        ImageListImageEditor editor = new();
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
    public void ImageListImageEditor_EditValue_ValidProvider_NullValue_InitializesFileDialog()
    {
        // Valid: confirm initialization works with a null value passed in.
        ImageListImageEditor editor = new();
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
    }

    [Fact]
    public void ImageListImageEditor_HasFileDialogField()
    {
        // The ImageListImageEditor stores its dialog in a private field of type OpenFileDialog.
        FieldInfo field = typeof(ImageListImageEditor).GetField("_fileDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        Assert.NotNull(field);
        Assert.Equal(typeof(OpenFileDialog), field.FieldType);
    }

    [Fact]
    public void ImageListImageEditor_FileDialogField_InitialValue_IsNull()
    {
        // Verifies that a freshly constructed ImageListImageEditor has _fileDialog == null,
        // which is required for the lazy initialization in EditValue.
        ImageListImageEditor editor = new();
        FieldInfo field = typeof(ImageListImageEditor).GetField("_fileDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        object value = field.GetValue(editor);
        Assert.Null(value);
    }

    [Fact]
    public void ImageListImageEditor_FileDialogField_AfterNullProviderEditValue_RemainsNull()
    {
        // Confirms the null-provider early-return path does not initialize _fileDialog.
        ImageListImageEditor editor = new();
        _ = editor.EditValue(null, null, null);

        FieldInfo field = typeof(ImageListImageEditor).GetField("_fileDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        object value = field.GetValue(editor);
        Assert.Null(value);
    }

    private static bool WaitForFileDialogInitialization(ImageListImageEditor editor, TimeSpan timeout)
    {
        FieldInfo field = typeof(ImageListImageEditor).GetField("_fileDialog",
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

    private static OpenFileDialog GetFileDialog(ImageListImageEditor editor)
    {
        FieldInfo field = typeof(ImageListImageEditor).GetField("_fileDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (OpenFileDialog)field.GetValue(editor)!;
    }

    private class SubImageListImageEditor : ImageListImageEditor
    {
        public new string GetFileDialogDescription() => base.GetFileDialogDescription();

        public new Type[] GetImageExtenders() => base.GetImageExtenders();
    }
}
