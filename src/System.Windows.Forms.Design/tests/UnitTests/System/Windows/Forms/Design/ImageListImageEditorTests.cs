// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Windows.Forms.TestUtilities;
using Moq;
using Windows.Win32.Foundation;

namespace System.Windows.Forms.Design.Tests;

public class ImageListImageEditorTests
{
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
    public void ImageListImageEditor_LoadImageFromStream_IconStream_ReturnsExpected()
    {
        ImageListImageEditor editor = new();
        var editor_LoadImageFromStream = editor.TestAccessor.CreateDelegate<Func<Stream, bool, ImageListImage>>("LoadImageFromStream");

        using Icon icon = new(SystemIcons.Application, 32, 32);
        using MemoryStream stream = new();
        icon.Save(stream);
        stream.Position = 0;

        var result = Assert.IsType<ImageListImage>(editor_LoadImageFromStream(stream, true));
        var resultImage = Assert.IsType<Bitmap>(result.Image);
        Assert.Equal(icon.Size, resultImage.Size);
    }

    [Fact]
    public void ImageListImageEditor_GetImageExtenders_Invoke_ReturnsExpected()
    {
        ImageListImageEditor editor = new();
        var editor_GetImageExtenders = editor.TestAccessor.CreateDelegate<Func<Type[]>>("GetImageExtenders");

        Type[] extenders = editor_GetImageExtenders();
        Assert.Equal([typeof(BitmapEditor)], extenders);
        Assert.Same(extenders, editor_GetImageExtenders());
    }

    [Fact]
    public void ImageListImageEditor_GetFileDialogDescription_Invoke_ReturnsExpected()
    {
        ImageListImageEditor editor = new();
        var editor_GetFileDialogDescription = editor.TestAccessor.CreateDelegate<Func<string>>("GetFileDialogDescription");

        Assert.Equal("All image files", editor_GetFileDialogDescription());
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void ImageListImageEditor_GetPaintValueSupported_Invoke_ReturnsTrue(ITypeDescriptorContext context)
    {
        ImageListImageEditor editor = new();
        Assert.True(editor.GetPaintValueSupported(context));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetEditValueInvalidProviderTestData))]
    public void ImageListImageEditor_EditValue_InvalidProvider_ReturnsValue(IServiceProvider provider, object value)
    {
        ImageListImageEditor editor = new();
        // When the provider is null, the editor returns the incoming value as-is.
        // When the provider is non-null but does not surface IWindowsFormsEditorService,
        // the editor returns an empty ArrayList.
        object result = editor.EditValue(null, provider, value);
        if (provider is null)
        {
            Assert.Same(value, result);
        }
        else
        {
            ArrayList list = Assert.IsType<ArrayList>(result);
            Assert.Empty(list);
        }
    }

    [Fact]
    public void ImageListImageEditor_EditValue_ValidProvider_PreInjectedDialog_RunsOnStaThread()
    {
        // Pre-inject a configured OpenFileDialog so the lazy initialization block is bypassed
        // and the editor immediately reaches the focus/dialog path. A DialogHostForm is
        // used as the active window to automatically dismiss the file dialog (returning
        // DialogResult.Cancel) when it becomes idle. The test runs on a dedicated STA
        // thread because FileDialog requires STA.
        object[] threadResult = new object[1];
        Exception[] threadException = new Exception[1];

        Thread thread = new(() =>
        {
            try
            {
                using DialogHostForm host = new();
                host.Show();
                host.Activate();

                ImageListImageEditor editor = new();
                using OpenFileDialog dialog = new()
                {
                    Multiselect = true,
                    FileName = string.Empty
                };
                editor.TestAccessor.Dynamic._fileDialog = dialog;

                Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
                Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
                mockServiceProvider
                    .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
                    .Returns(mockEditorService.Object)
                    .Verifiable();

                threadResult[0] = editor.EditValue(null, mockServiceProvider.Object, null);
                host.Close();
            }
            catch (Exception ex)
            {
                threadException[0] = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(threadException[0]);
        // The dialog is shown and then auto-dismissed (DialogResult.Cancel) by the host,
        // so the editor returns an empty ArrayList.
        ArrayList list = Assert.IsType<ArrayList>(threadResult[0]);
        Assert.Empty(list);
    }

    [Fact]
    public void ImageListImageEditor_EditValue_ValidProvider_LazyInitBuildsFilter_RunsOnStaThread()
    {
        // The first call to EditValue with a valid provider triggers the lazy
        // initialization: the OpenFileDialog is created, the filter is built using
        // CreateFilterEntry (and optionally extended via GetImageExtenders),
        // Multiselect is set to true, the current focus HWND is captured, and
        // finally the file dialog is shown. A DialogHostForm is used as the active
        // window to automatically dismiss the file dialog (DialogResult.Cancel).
        // The test runs on a dedicated STA thread because FileDialog requires STA.
        object[] threadResult = new object[1];
        Exception[] threadException = new Exception[1];

        Thread thread = new(() =>
        {
            try
            {
                using DialogHostForm host = new();
                host.Show();
                host.Activate();

                ImageListImageEditor editor = new();
                Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
                Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
                mockServiceProvider
                    .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
                    .Returns(mockEditorService.Object)
                    .Verifiable();

                threadResult[0] = editor.EditValue(null, mockServiceProvider.Object, null);
                host.Close();
            }
            catch (Exception ex)
            {
                threadException[0] = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(threadException[0]);
        ArrayList list = Assert.IsType<ArrayList>(threadResult[0]);
        Assert.Empty(list);
    }

    [Fact]
    public void ImageListImageEditor_PaintValue_ImageListImageValue_UsesImage()
    {
        ImageListImageEditor editor = new();
        using Bitmap image = new(10, 10);
        using Bitmap otherImage = new(3, 2);
        otherImage.SetPixel(0, 0, Color.Red);
        otherImage.SetPixel(1, 0, Color.Red);
        otherImage.SetPixel(2, 0, Color.Red);
        otherImage.SetPixel(0, 1, Color.Red);
        otherImage.SetPixel(1, 1, Color.Red);
        otherImage.SetPixel(2, 1, Color.Red);

        using Graphics graphics = Graphics.FromImage(image);
        ImageListImage listImage = new(otherImage);
        PaintValueEventArgs e = new(null, listImage, graphics, new Rectangle(1, 2, 3, 4));
        editor.PaintValue(e);
    }

    [Fact]
    public void ImageListImageEditor_PaintValue_NonImageListImageValue_UsesValueDirectly()
    {
        ImageListImageEditor editor = new();
        using Bitmap image = new(10, 10);
        using Bitmap otherImage = new(3, 2);
        otherImage.SetPixel(0, 0, Color.Red);
        otherImage.SetPixel(1, 0, Color.Red);
        otherImage.SetPixel(2, 0, Color.Red);
        otherImage.SetPixel(0, 1, Color.Red);
        otherImage.SetPixel(1, 1, Color.Red);
        otherImage.SetPixel(2, 1, Color.Red);

        using Graphics graphics = Graphics.FromImage(image);
        PaintValueEventArgs e = new(null, otherImage, graphics, new Rectangle(1, 2, 3, 4));
        editor.PaintValue(e);
    }

    [Fact]
    public void ImageListImageEditor_EditValue_ValidProvider_DialogAccepted_LoadsBitmapFiles()
    {
        // Pre-inject a configured OpenFileDialog whose FileName points to a real
        // bitmap file. An AcceptDialogHostForm is used as the active window to
        // automatically accept the file dialog (returning DialogResult.OK) when
        // it becomes idle. This exercises the OK branch of EditValue: the file is
        // read, loaded into an ImageListImage via LoadImageFromStream, named after
        // the file, and added to the returned list.
        string tempFile = Path.Combine(Path.GetTempPath(), $"ImageListImageEditorTest_{Guid.NewGuid():N}.bmp");
        using (Bitmap bmp = new(10, 10))
        {
            bmp.Save(tempFile, ImageFormat.Bmp);
        }

        try
        {
            object[] threadResult = new object[1];
            Exception[] threadException = new Exception[1];

            Thread thread = new(() =>
            {
                try
                {
                    using AcceptDialogHostForm host = new();
                    host.Show();
                    host.Activate();

                    ImageListImageEditor editor = new();
                    using OpenFileDialog dialog = new()
                    {
                        Multiselect = true,
                        InitialDirectory = Path.GetDirectoryName(tempFile),
                        FileName = tempFile
                    };
                    editor.TestAccessor.Dynamic._fileDialog = dialog;

                    Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
                    Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
                    mockServiceProvider
                        .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
                        .Returns(mockEditorService.Object)
                        .Verifiable();

                    threadResult[0] = editor.EditValue(null, mockServiceProvider.Object, null);
                    host.Close();
                }
                catch (Exception ex)
                {
                    threadException[0] = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.Null(threadException[0]);
            ArrayList list = Assert.IsType<ArrayList>(threadResult[0]);
            Assert.NotEmpty(list);
            ImageListImage first = Assert.IsType<ImageListImage>(list[0]);
            Assert.Equal(Path.GetFileName(tempFile), first.Name);
            Bitmap loaded = Assert.IsType<Bitmap>(first.Image);
            Assert.Equal(new Size(10, 10), loaded.Size);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    private class AcceptDialogHostForm : DialogHostForm
    {
        protected override void OnDialogIdle(HWND dialogHandle)
        {
            Accept(dialogHandle);
        }
    }
}
