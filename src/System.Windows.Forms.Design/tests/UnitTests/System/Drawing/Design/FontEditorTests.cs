// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms.Design;
using System.Windows.Forms.TestUtilities;
using Moq;

namespace System.Drawing.Design.Tests;

public class FontEditorTests
{
    [Fact]
    public void FontEditor_Ctor_Default()
    {
        FontEditor editor = new();
        Assert.False(editor.IsDropDownResizable);
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetEditValueInvalidProviderTestData))]
    public void FontEditor_EditValue_InvalidProvider_ReturnsValue(IServiceProvider provider, object value)
    {
        FontEditor editor = new();
        Assert.Same(value, editor.EditValue(null, provider, value));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void FontEditor_GetEditStyle_Invoke_ReturnsModal(ITypeDescriptorContext context)
    {
        FontEditor editor = new();
        Assert.Equal(UITypeEditorEditStyle.Modal, editor.GetEditStyle(context));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void FontEditor_GetPaintValueSupported_Invoke_ReturnsFalse(ITypeDescriptorContext context)
    {
        FontEditor editor = new();
        Assert.False(editor.GetPaintValueSupported(context));
    }

    [Fact]
    public void FontEditor_HasFontDialogField()
    {
        FieldInfo field = typeof(FontEditor).GetField("_fontDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        Assert.NotNull(field);
        Assert.Equal(typeof(FontDialog), field.FieldType);
    }

    [Fact]
    public void FontEditor_FontDialogField_InitialValue_IsNull()
    {
        // Verifies that a freshly constructed FontEditor has _fontDialog == null,
        // which is required for the lazy initialization in EditValue.
        FontEditor editor = new();
        FieldInfo field = typeof(FontEditor).GetField("_fontDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        object value = field.GetValue(editor);
        Assert.Null(value);
    }

    [Fact]
    public void FontEditor_FontDialogField_AfterInvalidProviderEditValue_RemainsNull()
    {
        // Confirms that the invalid-provider early-return path in EditValue
        // does NOT initialize the _fontDialog field.
        FontEditor editor = new();
        _ = editor.EditValue(null, null, "test");

        FieldInfo field = typeof(FontEditor).GetField("_fontDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        object value = field.GetValue(editor);
        Assert.Null(value);
    }

    [Fact]
    public void FontEditor_EditValue_ValidProvider_FontValue_InitializesFontDialog()
    {
        FontEditor editor = new();
        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        Font fontValue = new("Arial", 12);

        Thread thread = new(() =>
        {
            try
            {
                _ = editor.EditValue(null, mockServiceProvider.Object, fontValue);
            }
            catch
            {
                // The dialog is expected to be cancelled / aborted by the test runner.
            }
        })
        {
            IsBackground = true
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        bool initialized = WaitForFieldInitialization(editor, TimeSpan.FromSeconds(2));
        Assert.True(initialized, "_fontDialog should be initialized after EditValue with a valid provider.");
    }

    [Fact]
    public void FontEditor_EditValue_ValidProvider_NonFontValue_InitializesFontDialog()
    {
        // Valid provider path with a non-Font value: covers the
        // `if (value is Font fontValue)` false branch.
        FontEditor editor = new();
        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        Thread thread = new(() =>
        {
            try
            {
                _ = editor.EditValue(null, mockServiceProvider.Object, "not-a-font");
            }
            catch
            {
            }
        })
        {
            IsBackground = true
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        bool initialized = WaitForFieldInitialization(editor, TimeSpan.FromSeconds(2));
        Assert.True(initialized, "_fontDialog should be initialized after EditValue with a valid provider, even for a non-Font value.");
    }

    [Fact]
    public void FontEditor_EditValue_ValidProvider_NullValue_InitializesFontDialog()
    {
        // Valid provider path with a null value.
        FontEditor editor = new();
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
        {
            IsBackground = true
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        bool initialized = WaitForFieldInitialization(editor, TimeSpan.FromSeconds(2));
        Assert.True(initialized, "_fontDialog should be initialized after EditValue with a valid provider, even for a null value.");
    }

    [Fact]
    public void FontEditor_EditValue_ValidProvider_FontDialogConfiguredCorrectly()
    {
        // Verifies that the lazily-created FontDialog has the expected property values
        // (ShowApply=false, ShowColor=false, AllowVerticalFonts=false).
        FontEditor editor = new();
        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        Thread thread = new(() =>
        {
            try
            {
                _ = editor.EditValue(null, mockServiceProvider.Object, "value");
            }
            catch
            {
            }
        })
        {
            IsBackground = true
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        bool initialized = WaitForFieldInitialization(editor, TimeSpan.FromSeconds(2));
        Assert.True(initialized);

        FieldInfo field = typeof(FontEditor).GetField("_fontDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        FontDialog fontDialog = (FontDialog)field.GetValue(editor)!;
        Assert.NotNull(fontDialog);
        Assert.False(fontDialog.ShowApply);
        Assert.False(fontDialog.ShowColor);
        Assert.False(fontDialog.AllowVerticalFonts);
    }

    [Fact]
    public void FontEditor_EditValue_ValidProvider_CalledTwice_ReusesFontDialog()
    {
        // Verifies the `??=` semantics: a second EditValue call with a valid provider
        // reuses the existing _fontDialog rather than creating a new one.
        FontEditor editor = new();
        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        void InvokeEdit()
        {
            try
            {
                _ = editor.EditValue(null, mockServiceProvider.Object, "value");
            }
            catch
            {
            }
        }

        Thread thread1 = new(InvokeEdit)
        {
            IsBackground = true
        };
        thread1.SetApartmentState(ApartmentState.STA);
        thread1.Start();
        thread1.Join(TimeSpan.FromSeconds(2));

        FieldInfo field = typeof(FontEditor).GetField("_fontDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        object firstDialog = field.GetValue(editor);
        Assert.NotNull(firstDialog);

        Thread thread2 = new(InvokeEdit)
        {
            IsBackground = true
        };
        thread2.SetApartmentState(ApartmentState.STA);
        thread2.Start();
        thread2.Join(TimeSpan.FromSeconds(2));

        object secondDialog = field.GetValue(editor);
        Assert.NotNull(secondDialog);
        Assert.Same(firstDialog, secondDialog);
    }

    private static bool WaitForFieldInitialization(FontEditor editor, TimeSpan timeout)
    {
        FieldInfo field = typeof(FontEditor).GetField("_fontDialog",
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
}
