// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Moq;

namespace System.Windows.Forms.Design.Tests;

public class DataGridViewCellStyleEditorTests
{
    [Fact]
    public void DataGridViewCellStyleEditor_Ctor_Default()
    {
        DataGridViewCellStyleEditor editor = new();
        Assert.False(editor.IsDropDownResizable);
    }

    [Fact]
    public void DataGridViewCellStyleEditor_GetEditStyle_ReturnsModal()
    {
        DataGridViewCellStyleEditor editor = new();
        Assert.Equal(UITypeEditorEditStyle.Modal, editor.GetEditStyle(null));
    }

    [Fact]
    public void DataGridViewCellStyleEditor_GetEditStyle_WithContext_ReturnsModal()
    {
        DataGridViewCellStyleEditor editor = new();
        Mock<ITypeDescriptorContext> mockContext = new();
        Assert.Equal(UITypeEditorEditStyle.Modal, editor.GetEditStyle(mockContext.Object));
    }

    [Fact]
    public void DataGridViewCellStyleEditor_EditValue_NullProvider_ThrowsArgumentNullException()
    {
        DataGridViewCellStyleEditor editor = new();
        Assert.Throws<ArgumentNullException>(
            () => editor.EditValue(null, null, new DataGridViewCellStyle()));
    }

    [Fact]
    public void DataGridViewCellStyleEditor_EditValue_NullProviderWithNullValue_ThrowsArgumentNullException()
    {
        DataGridViewCellStyleEditor editor = new();
        Assert.Throws<ArgumentNullException>(
            () => editor.EditValue(null, null, null));
    }

    [Fact]
    public void DataGridViewCellStyleEditor_EditValue_ProviderWithoutEditorService_ThrowsInvalidOperationException()
    {
        DataGridViewCellStyleEditor editor = new();
        // Use Loose (default) behavior so the mock returns null for any unconfigured GetService call
        // instead of throwing MockException. This lets the editor's own InvalidOperationException
        // (raised when IWindowsFormsEditorService is missing) be the one that propagates.
        Mock<IServiceProvider> mockServiceProvider = new();
        object value = new DataGridViewCellStyle();

        Assert.Throws<InvalidOperationException>(
            () => editor.EditValue(null, mockServiceProvider.Object, value));
    }

    [Fact]
    public void DataGridViewCellStyleEditor_EditValue_ValidProvider_WithoutUIService_ReturnsValue()
    {
        DataGridViewCellStyleEditor editor = new();
        DataGridViewCellStyle value = new() { BackColor = Color.Red };

        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Loose);

        Mock<IServiceProvider> mockServiceProvider = new();
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        using (new NoAssertContext())
        {
            InstallAutoClosingBuilder(editor, DialogResult.Cancel);
            object result = editor.EditValue(null, mockServiceProvider.Object, value);
            Assert.Same(value, result);
        }
    }

    [Fact]
    public void DataGridViewCellStyleEditor_EditValue_ValidProvider_WithUIService_AppliesDialogFont()
    {
        DataGridViewCellStyleEditor editor = new();
        DataGridViewCellStyle value = new() { BackColor = Color.Red };
        using Font expectedFont = new("Arial", 12);

        Mock<IUIService> mockUIService = new();
        mockUIService
            .Setup(u => u.Styles["DialogFont"])
            .Returns(expectedFont);

        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Loose);

        Mock<IServiceProvider> mockServiceProvider = new();
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IUIService)))
            .Returns(mockUIService.Object);

        using (new NoAssertContext())
        {
            InstallAutoClosingBuilder(editor, DialogResult.Cancel);
            object result = editor.EditValue(null, mockServiceProvider.Object, value);
            Assert.Same(value, result);
        }

        mockUIService.Verify(u => u.Styles["DialogFont"], Times.AtLeastOnce());
    }

    [Fact]
    public void DataGridViewCellStyleEditor_EditValue_ShowDialogReturnsOK_ReplacesValue()
    {
        DataGridViewCellStyleEditor editor = new();
        DataGridViewCellStyle originalValue = new() { BackColor = Color.Red };

        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Loose);

        Mock<IServiceProvider> mockServiceProvider = new();
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        using (new NoAssertContext())
        {
            InstallAutoClosingBuilder(editor, DialogResult.OK);
            object result = editor.EditValue(null, mockServiceProvider.Object, originalValue);

            Assert.NotNull(result);
            Assert.IsType<DataGridViewCellStyle>(result);
        }
    }

    [Fact]
    public void DataGridViewCellStyleEditor_EditValue_NullValue_DialogReturnsCancel_ReturnsNull()
    {
        DataGridViewCellStyleEditor editor = new();

        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Loose);

        Mock<IServiceProvider> mockServiceProvider = new();
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        using (new NoAssertContext())
        {
            InstallAutoClosingBuilder(editor, DialogResult.Cancel);
            object result = editor.EditValue(null, mockServiceProvider.Object, null);
            Assert.Null(result);
        }
    }

    [Fact]
    public void DataGridViewCellStyleEditor_EditValue_NonCellStyleValue_DoesNotAssignStyle()
    {
        DataGridViewCellStyleEditor editor = new();
        object nonStyleValue = "not a cell style";

        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Loose);

        Mock<IServiceProvider> mockServiceProvider = new();
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        using (new NoAssertContext())
        {
            InstallAutoClosingBuilder(editor, DialogResult.Cancel);
            object result = editor.EditValue(null, mockServiceProvider.Object, nonStyleValue);
            Assert.Same(nonStyleValue, result);
        }
    }

    [Fact]
    public void DataGridViewCellStyleEditor_EditValue_ContextWithIComponent_PassesComponentToBuilder()
    {
        DataGridViewCellStyleEditor editor = new();
        DataGridViewCellStyle value = new() { BackColor = Color.Green };
        Mock<IComponent> mockComponent = new();
        Mock<ITypeDescriptorContext> mockContext = new();
        mockContext.Setup(c => c.Instance).Returns(mockComponent.Object);

        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Loose);

        Mock<IServiceProvider> mockServiceProvider = new();
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        using (new NoAssertContext())
        {
            InstallAutoClosingBuilder(editor, DialogResult.Cancel);
            object result = editor.EditValue(mockContext.Object, mockServiceProvider.Object, value);
            Assert.Same(value, result);
        }

        mockContext.Verify(c => c.Instance, Times.AtLeastOnce());
    }

    [Fact]
    public void DataGridViewCellStyleEditor_EditValue_ContextInstanceIsNotIComponent_StillWorks()
    {
        DataGridViewCellStyleEditor editor = new();
        DataGridViewCellStyle value = new() { BackColor = Color.Yellow };
        object nonComponentInstance = new();
        Mock<ITypeDescriptorContext> mockContext = new();
        mockContext.Setup(c => c.Instance).Returns(nonComponentInstance);

        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Loose);

        Mock<IServiceProvider> mockServiceProvider = new();
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        using (new NoAssertContext())
        {
            InstallAutoClosingBuilder(editor, DialogResult.Cancel);
            object result = editor.EditValue(mockContext.Object, mockServiceProvider.Object, value);
            Assert.Same(value, result);
        }
    }

    [Fact]
    public void DataGridViewCellStyleEditor_EditValue_CalledTwice_ReusesBuilderDialog()
    {
        DataGridViewCellStyleEditor editor = new();
        DataGridViewCellStyle firstValue = new() { BackColor = Color.Red };
        DataGridViewCellStyle secondValue = new() { BackColor = Color.Blue };

        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Loose);

        Mock<IServiceProvider> mockServiceProvider = new();
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);

        using (new NoAssertContext())
        {
            InstallAutoClosingBuilder(editor, DialogResult.Cancel);
            object firstResult = editor.EditValue(null, mockServiceProvider.Object, firstValue);
            object secondResult = editor.EditValue(null, mockServiceProvider.Object, secondValue);

            Assert.Same(firstValue, firstResult);
            Assert.Same(secondValue, secondResult);
        }
    }

    /// <summary>
    ///  Replaces the editor's private <c>_builderDialog</c> field with a stub that closes itself
    ///  with the supplied <see cref="DialogResult"/> as soon as the form is shown. This lets the
    ///  editor's call to <c>Form.ShowDialog()</c> return a deterministic result without requiring
    ///  the test thread to interact with a real modal window.
    /// </summary>
    private static void InstallAutoClosingBuilder(DataGridViewCellStyleEditor editor, DialogResult result)
    {
        editor.TestAccessor.Dynamic._builderDialog =
            new AutoClosingDataGridViewCellStyleBuilder(result);
    }

    private sealed class AutoClosingDataGridViewCellStyleBuilder : DataGridViewCellStyleBuilder
    {
        private readonly DialogResult _result;

        public AutoClosingDataGridViewCellStyleBuilder(DialogResult result)
            : base(new EmptyServiceProvider(), new EmptyComponent())
        {
            _result = result;
        }

        protected override void OnShown(EventArgs e)
        {
            // Set the dialog result and close the form immediately so ShowDialog() returns
            // without requiring user interaction. OnShown runs once the form is about to be
            // displayed; setting DialogResult here terminates the modal message loop.
            DialogResult = _result;
            base.OnShown(e);
            Close();
        }
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType) => null;
    }

    private sealed class EmptyComponent : IComponent
    {
        public ISite Site { get; set; }

        public event EventHandler Disposed;

        public void Dispose() => Disposed?.Invoke(this, EventArgs.Empty);
    }
}
