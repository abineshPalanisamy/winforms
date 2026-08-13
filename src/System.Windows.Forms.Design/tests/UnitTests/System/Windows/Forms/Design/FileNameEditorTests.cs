// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.Drawing.Design;
using System.Reflection;
using System.Windows.Forms.TestUtilities;
using Moq;

namespace System.Windows.Forms.Design.Tests;

public class FileNameEditorTests
{
    [Fact]
    public void FileNameEditor_Ctor_Default()
    {
        FileNameEditor editor = new();
        Assert.False(editor.IsDropDownResizable);
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetEditValueInvalidProviderTestData))]
    public void FileNameEditor_EditValue_InvalidProvider_ReturnsValue(IServiceProvider provider, object value)
    {
        FileNameEditor editor = new();
        Assert.Same(value, editor.EditValue(null, provider, value));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void FileNameEditor_GetEditStyle_Invoke_ReturnsModal(ITypeDescriptorContext context)
    {
        FileNameEditor editor = new();
        Assert.Equal(UITypeEditorEditStyle.Modal, editor.GetEditStyle(context));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void FileNameEditor_GetPaintValueSupported_Invoke_ReturnsFalse(ITypeDescriptorContext context)
    {
        FileNameEditor editor = new();
        Assert.False(editor.GetPaintValueSupported(context));
    }

    [Fact]
    public void FileNameEditor_InitializeDialog_Invoke_Success()
    {
        SubFileNameEditor editor = new();
        using OpenFileDialog openFileDialog = new();
        editor.InitializeDialog(openFileDialog);
        Assert.Equal("All Files(*.*)|*.*", openFileDialog.Filter);
        Assert.Equal("Open File", openFileDialog.Title);
    }

    [Fact]
    public void FileNameEditor_InitializeDialog_NullOpenFileDialog_ThrowsArgumentNullException()
    {
        SubFileNameEditor editor = new();
        Assert.Throws<ArgumentNullException>("openFileDialog", () => editor.InitializeDialog(null));
    }

    [Fact]
    public void FileNameEditor_EditValue_ValidProvider_InitializesOpenFileDialog()
    {
        // Verifies the `is null` check semantics: a first EditValue call with a
        // valid provider initializes the _openFileDialog field.
        FileNameEditor editor = new();
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

        bool initialized = WaitForOpenFileDialogInitialization(editor, TimeSpan.FromSeconds(2));
        Assert.True(initialized, "_openFileDialog should be initialized after EditValue with a valid provider.");
    }

    [Fact]
    public void FileNameEditor_EditValue_ValidProvider_StringValue_ConfiguresOpenFileDialogFileName()
    {
        // Covers the value-is-string branch: when the value is a string, the
        // open file dialog's FileName is configured with that value.
        FileNameEditor editor = new();
        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        Thread thread = new(() =>
        {
            try
            {
                _ = editor.EditValue(null, mockServiceProvider.Object, "initialFile.txt");
            }
            catch
            {
                // The dialog is expected to be cancelled / aborted by the test runner.
            }
        })
        { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        bool initialized = WaitForOpenFileDialogInitialization(editor, TimeSpan.FromSeconds(2));
        Assert.True(initialized);

        OpenFileDialog dialog = GetOpenFileDialog(editor);
        Assert.NotNull(dialog);
        Assert.Equal("initialFile.txt", dialog.FileName);
    }

    [Fact]
    public void FileNameEditor_EditValue_ValidProvider_NonStringValue_DoesNotChangeFileName()
    {
        // Covers the value-is-not-string branch: when the value is not a string,
        // the open file dialog's FileName is not configured from it.
        FileNameEditor editor = new();
        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        Thread thread = new(() =>
        {
            try
            {
                _ = editor.EditValue(null, mockServiceProvider.Object, new object());
            }
            catch
            {
                // The dialog is expected to be cancelled / aborted by the test runner.
            }
        })
        { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        bool initialized = WaitForOpenFileDialogInitialization(editor, TimeSpan.FromSeconds(2));
        Assert.True(initialized);

        OpenFileDialog dialog = GetOpenFileDialog(editor);
        Assert.NotNull(dialog);
        Assert.Equal(string.Empty, dialog.FileName);
    }

    [Fact]
    public void FileNameEditor_EditValue_ValidProvider_ConfiguresOpenFileDialogFilterAndTitle()
    {
        // Verifies that the open file dialog has its Filter and Title configured
        // through the default InitializeDialog implementation.
        FileNameEditor editor = new();
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

        bool initialized = WaitForOpenFileDialogInitialization(editor, TimeSpan.FromSeconds(2));
        Assert.True(initialized);

        OpenFileDialog dialog = GetOpenFileDialog(editor);
        Assert.Equal("All Files(*.*)|*.*", dialog.Filter);
        Assert.Equal("Open File", dialog.Title);
    }

    [Fact]
    public void FileNameEditor_EditValue_ValidProvider_CalledTwice_ReusesOpenFileDialog()
    {
        // Verifies the `is null` check semantics: a second EditValue call with a
        // valid provider reuses the existing _openFileDialog rather than creating a new one.
        FileNameEditor editor = new();
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
                // The dialog is expected to be cancelled / aborted by the test runner.
            }
        }

        Thread thread1 = new(InvokeEdit) { IsBackground = true };
        thread1.SetApartmentState(ApartmentState.STA);
        thread1.Start();
        thread1.Join(TimeSpan.FromSeconds(2));

        OpenFileDialog firstDialog = GetOpenFileDialog(editor);
        Assert.NotNull(firstDialog);

        Thread thread2 = new(InvokeEdit) { IsBackground = true };
        thread2.SetApartmentState(ApartmentState.STA);
        thread2.Start();
        thread2.Join(TimeSpan.FromSeconds(2));

        OpenFileDialog secondDialog = GetOpenFileDialog(editor);
        Assert.NotNull(secondDialog);
        Assert.Same(firstDialog, secondDialog);
    }

    [Fact]
    public void FileNameEditor_EditValue_ValidProvider_StringValue_CalledTwice_ReusesAndConfiguresFileName()
    {
        // Verifies the `is null` check semantics with a string value: a second
        // EditValue call reuses the same _openFileDialog and configures its FileName
        // to the new value.
        FileNameEditor editor = new();
        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        void InvokeEdit(string value)
        {
            try
            {
                _ = editor.EditValue(null, mockServiceProvider.Object, value);
            }
            catch
            {
                // The dialog is expected to be cancelled / aborted by the test runner.
            }
        }

        Thread thread1 = new(() => InvokeEdit("first.txt")) { IsBackground = true };
        thread1.SetApartmentState(ApartmentState.STA);
        thread1.Start();
        thread1.Join(TimeSpan.FromSeconds(2));

        OpenFileDialog firstDialog = GetOpenFileDialog(editor);
        Assert.NotNull(firstDialog);
        Assert.Equal("first.txt", firstDialog.FileName);

        Thread thread2 = new(() => InvokeEdit("second.txt")) { IsBackground = true };
        thread2.SetApartmentState(ApartmentState.STA);
        thread2.Start();
        thread2.Join(TimeSpan.FromSeconds(2));

        OpenFileDialog secondDialog = GetOpenFileDialog(editor);
        Assert.NotNull(secondDialog);
        Assert.Same(firstDialog, secondDialog);
        Assert.Equal("second.txt", secondDialog.FileName);
    }

    [Fact]
    public void FileNameEditor_OpenFileDialogField_InitialValue_IsNull()
    {
        // Verifies that a freshly constructed FileNameEditor has _openFileDialog == null,
        // which is required for the lazy initialization in EditValue.
        FileNameEditor editor = new();
        FieldInfo field = typeof(FileNameEditor).GetField("_openFileDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        Assert.NotNull(field);
        Assert.Equal(typeof(OpenFileDialog), field.FieldType);

        object value = field.GetValue(editor);
        Assert.Null(value);
    }

    [Fact]
    public void FileNameEditor_OpenFileDialogField_AfterInvalidProviderEditValue_RemainsNull()
    {
        // Confirms that the invalid-provider early-return path in EditValue
        // does NOT initialize the _openFileDialog field.
        FileNameEditor editor = new();
        object value = new();
        _ = editor.EditValue(null, null, value);

        FieldInfo field = typeof(FileNameEditor).GetField("_openFileDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        object fieldValue = field.GetValue(editor);
        Assert.Null(fieldValue);
    }

    [Fact]
    public void FileNameEditor_InitializeDialog_OverriddenSubclass_AppliesCustomFilterAndTitle()
    {
        // Verifies that a derived class can override InitializeDialog to provide
        // a custom filter and title. The base virtual dispatch is exercised.
        CustomInitializeDialogFileNameEditor editor = new();
        using OpenFileDialog openFileDialog = new();
        editor.InvokeInitializeDialog(openFileDialog);
        Assert.Equal("Custom Filter|*.custom", openFileDialog.Filter);
        Assert.Equal("Custom Title", openFileDialog.Title);
    }

    private static bool WaitForOpenFileDialogInitialization(FileNameEditor editor, TimeSpan timeout)
    {
        FieldInfo field = typeof(FileNameEditor).GetField("_openFileDialog",
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

    private static OpenFileDialog GetOpenFileDialog(FileNameEditor editor)
    {
        FieldInfo field = typeof(FileNameEditor).GetField("_openFileDialog",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (OpenFileDialog)field.GetValue(editor)!;
    }

    private class SubFileNameEditor : FileNameEditor
    {
        public new void InitializeDialog(OpenFileDialog openFileDialog) => base.InitializeDialog(openFileDialog);
    }

    private class CustomInitializeDialogFileNameEditor : FileNameEditor
    {
        public void InvokeInitializeDialog(OpenFileDialog openFileDialog) => InitializeDialog(openFileDialog);

        protected override void InitializeDialog(OpenFileDialog openFileDialog)
        {
            openFileDialog.Filter = "Custom Filter|*.custom";
            openFileDialog.Title = "Custom Title";
        }
    }
}
