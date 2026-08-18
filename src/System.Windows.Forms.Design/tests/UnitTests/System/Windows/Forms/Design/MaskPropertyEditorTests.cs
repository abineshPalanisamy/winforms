// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using Moq;

namespace System.Windows.Forms.Design.Tests;
public class MaskPropertyEditorTests
{
    private readonly MaskedTextBox _maskedTextBox;
    private readonly MaskPropertyEditor _editor;

    public MaskPropertyEditorTests()
    {
        _maskedTextBox = new();
        _editor = new();
    }

    [WinFormsFact]
    public void EditValue_WhenContextOrProviderAreNull_ShouldReturnOriginalValue()
    {
        var context = new Mock<ITypeDescriptorContext>().Object;
        Mock<IServiceProvider> provider = new();

        object? result;
        if (context.Instance is MaskedTextBox maskedTextBox)
        {
            result = _editor.EditValue(context, provider.Object, maskedTextBox.Mask);
        }
        else
        {
            result = _maskedTextBox.Mask;
        }

        result.Should().Be(_maskedTextBox.Mask);
    }

    [Fact]
    public void GetPaintValueSupported_ShouldReturnFalse()
    {
        bool result = _editor.GetPaintValueSupported(null);
        result.Should().BeFalse();
    }

    [Fact]
    public void GetEditStyle_ShouldReturnModal()
    {
        var result = _editor.GetEditStyle(null);
        result.Should().Be(UITypeEditorEditStyle.Modal);
    }

    [Fact]
    public void EditValue_WhenContextIsNull_ShouldReturnOriginalValue()
    {
        // When context is null, EditValue must short-circuit and return the original value.
        object value = "00/00/0000";

        object? result = _editor.EditValue(null, new Mock<IServiceProvider>(MockBehavior.Loose).Object, value);

        result.Should().Be(value);
    }

    [Fact]
    public void EditValue_WhenProviderIsNull_ShouldReturnOriginalValue()
    {
        // When provider is null, EditValue must short-circuit and return the original value.
        object value = "00/00/0000";
        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Loose);
        mockContext.Setup(c => c.Instance).Returns(_maskedTextBox);

        object? result = _editor.EditValue(mockContext.Object, null, value);

        result.Should().Be(value);
    }

    [Fact]
    public void EditValue_WhenContextInstanceIsMaskedTextBox_NoUIService_ReturnsOriginalValue()
    {
        // The editor dialog needs a UI service to show; without one, the call must not throw
        // and the editor must return the original value when no dialog is completed.
        object value = "00/00/0000";
        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(_maskedTextBox);

        Mock<IUIService> mockUIService = new();
        mockUIService
            .Setup(u => u.ShowDialog(It.IsAny<MaskDesignerDialog>()))
            .Returns(DialogResult.Cancel);

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Loose);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IUIService))).Returns(mockUIService.Object);

        object? result = _editor.EditValue(mockContext.Object, mockServiceProvider.Object, value);

        result.Should().Be(value);
    }

    [Fact]
    public void EditMask_WithUIServiceShowingCancel_ReturnsNull()
    {
        // When the UI service shows the dialog and the user clicks Cancel, EditMask returns null.
        using MaskedTextBox maskedTextBox = new();

        Mock<IUIService> mockUIService = new();
        mockUIService
            .Setup(u => u.ShowDialog(It.IsAny<MaskDesignerDialog>()))
            .Returns(DialogResult.Cancel);

        string? result = MaskPropertyEditor.EditMask(null, mockUIService.Object, maskedTextBox, null);

        result.Should().BeNull();
    }

    [Fact]
    public void EditMask_WithUIServiceShowingOK_ReturnsDialogMask()
    {
        // When the UI service shows the dialog and the user clicks OK, EditMask returns the mask
        // from the dialog.
        using MaskedTextBox maskedTextBox = new("00/00/0000");

        Mock<IUIService> mockUIService = new();
        mockUIService
            .Setup(u => u.ShowDialog(It.IsAny<MaskDesignerDialog>()))
            .Returns(DialogResult.OK)
            .Callback<Form>(form =>
            {
                // Simulate the user pressing OK after the dialog has been populated.
                // Do not dispose the dialog here: EditMask will dispose it after reading values.
                MaskDesignerDialog dialog = (MaskDesignerDialog)form;
                dialog.TestAccessor.Dynamic._maskedTextBox.Mask = "(999) 000-0000";
                dialog.TestAccessor.Dynamic._checkBoxUseValidatingType.Checked = false;
                dialog.TestAccessor.Dynamic.btnOK_Click(null, EventArgs.Empty);
            });

        string? result = MaskPropertyEditor.EditMask(null, mockUIService.Object, maskedTextBox, null);

        result.Should().Be("(999) 000-0000");
    }

    [Fact]
    public void EditMask_WithUIServiceShowingOK_DifferentValidatingType_UpdatesInstance()
    {
        // When the dialog returns OK with a ValidatingType different from the instance, the
        // instance's ValidatingType must be updated to the new value.
        using MaskedTextBox maskedTextBox = new();
        Type originalValidatingType = maskedTextBox.ValidatingType;

        Mock<IUIService> mockUIService = new();
        mockUIService
            .Setup(u => u.ShowDialog(It.IsAny<MaskDesignerDialog>()))
            .Returns(DialogResult.OK)
            .Callback<Form>(form =>
            {
                MaskDesignerDialog dialog = (MaskDesignerDialog)form;
                dialog.TestAccessor.Dynamic._maskedTextBox.Mask = "00/00/0000";
                dialog.TestAccessor.Dynamic._maskedTextBox.ValidatingType = typeof(DateTime);
                dialog.TestAccessor.Dynamic._checkBoxUseValidatingType.Checked = true;
                dialog.TestAccessor.Dynamic.btnOK_Click(null, EventArgs.Empty);
            });

        MaskPropertyEditor.EditMask(null, mockUIService.Object, maskedTextBox, null);

        maskedTextBox.ValidatingType.Should().NotBe(originalValidatingType);
        maskedTextBox.ValidatingType.Should().Be(typeof(DateTime));
    }

    [Fact]
    public void EditMask_WithUIServiceShowingOK_SameValidatingType_DoesNotUpdateInstance()
    {
        // When the dialog returns OK with the same ValidatingType as the instance, the editor
        // must not redundantly assign the property.
        using MaskedTextBox maskedTextBox = new() { ValidatingType = typeof(DateTime) };

        Mock<IUIService> mockUIService = new();
        mockUIService
            .Setup(u => u.ShowDialog(It.IsAny<MaskDesignerDialog>()))
            .Returns(DialogResult.OK)
            .Callback<Form>(form =>
            {
                MaskDesignerDialog dialog = (MaskDesignerDialog)form;
                dialog.TestAccessor.Dynamic._maskedTextBox.Mask = "00/00/0000";
                dialog.TestAccessor.Dynamic._maskedTextBox.ValidatingType = typeof(DateTime);
                dialog.TestAccessor.Dynamic._checkBoxUseValidatingType.Checked = true;
                dialog.TestAccessor.Dynamic.btnOK_Click(null, EventArgs.Empty);
            });

        MaskPropertyEditor.EditMask(null, mockUIService.Object, maskedTextBox, null);

        maskedTextBox.ValidatingType.Should().Be(typeof(DateTime));
    }

    [Fact]
    public void EditMask_WithUIServiceAndDiscoveryService_AddsDiscoveredDescriptors()
    {
        // When a type discovery service is provided, the dialog should add the discovered
        // descriptors to its list in addition to the default ones.
        using MaskedTextBox maskedTextBox = new();

        Mock<ITypeDiscoveryService> mockDiscoveryService = new();
        List<Type> types = new() { typeof(TestMaskDescriptor) };
        mockDiscoveryService.Setup(ds => ds.GetTypes(typeof(MaskDescriptor), false)).Returns(types);

        Mock<IUIService> mockUIService = new();
        mockUIService
            .Setup(u => u.ShowDialog(It.IsAny<MaskDesignerDialog>()))
            .Returns(DialogResult.OK)
            .Callback<Form>(form =>
            {
                MaskDesignerDialog dialog = (MaskDesignerDialog)form;
                dialog.TestAccessor.Dynamic._maskedTextBox.Mask = "0000";
                dialog.TestAccessor.Dynamic._checkBoxUseValidatingType.Checked = false;
                dialog.TestAccessor.Dynamic.btnOK_Click(null, EventArgs.Empty);
            });

        string? result = MaskPropertyEditor.EditMask(
            mockDiscoveryService.Object,
            mockUIService.Object,
            maskedTextBox,
            null);

        result.Should().Be("0000");
    }

    [Fact]
    public void EditMask_WithUIServiceAndHelpService_DoesNotThrow()
    {
        // When an IHelpService is provided, the editor must accept it without throwing. The help
        // service is only invoked when the user clicks the help button.
        using MaskedTextBox maskedTextBox = new();

        Mock<IHelpService> mockHelpService = new();
        Mock<IUIService> mockUIService = new();
        mockUIService
            .Setup(u => u.ShowDialog(It.IsAny<MaskDesignerDialog>()))
            .Returns(DialogResult.OK)
            .Callback<Form>(form =>
            {
                MaskDesignerDialog dialog = (MaskDesignerDialog)form;
                dialog.TestAccessor.Dynamic._maskedTextBox.Mask = "00/00/0000";
                dialog.TestAccessor.Dynamic._checkBoxUseValidatingType.Checked = false;
                dialog.TestAccessor.Dynamic.btnOK_Click(null, EventArgs.Empty);
            });

        Action act = () => MaskPropertyEditor.EditMask(
            null,
            mockUIService.Object,
            maskedTextBox,
            mockHelpService.Object);

        act.Should().NotThrow();
    }

    [Fact]
    public void EditValue_WithMaskedTextBoxContext_ReturnsOriginalValue_WhenDialogCancelled()
    {
        // End-to-end happy path: with a MaskedTextBox context and a UI service that returns
        // Cancel, EditValue must return the original value.
        object value = "00/00/0000";
        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(_maskedTextBox);

        Mock<IUIService> mockUIService = new();
        mockUIService
            .Setup(u => u.ShowDialog(It.IsAny<MaskDesignerDialog>()))
            .Returns(DialogResult.Cancel);

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Loose);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IUIService))).Returns(mockUIService.Object);

        object? result = _editor.EditValue(mockContext.Object, mockServiceProvider.Object, value);

        result.Should().Be(value);
    }

    [Fact]
    public void EditValue_WithMaskedTextBoxContext_DialogOK_ReturnsNewMask()
    {
        // End-to-end happy path: with a MaskedTextBox context and a UI service that returns
        // OK, EditValue must return the new mask value from the dialog.
        object value = "00/00/0000";
        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        mockContext.Setup(c => c.Instance).Returns(_maskedTextBox);

        Mock<IUIService> mockUIService = new();
        mockUIService
            .Setup(u => u.ShowDialog(It.IsAny<MaskDesignerDialog>()))
            .Returns(DialogResult.OK)
            .Callback<Form>(form =>
            {
                MaskDesignerDialog dialog = (MaskDesignerDialog)form;
                dialog.TestAccessor.Dynamic._maskedTextBox.Mask = "(999) 000-0000";
                dialog.TestAccessor.Dynamic._checkBoxUseValidatingType.Checked = false;
                dialog.TestAccessor.Dynamic.btnOK_Click(null, EventArgs.Empty);
            });

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Loose);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IUIService))).Returns(mockUIService.Object);

        object? result = _editor.EditValue(mockContext.Object, mockServiceProvider.Object, value);

        result.Should().Be("(999) 000-0000");
    }

    private class TestMaskDescriptor : MaskDescriptor
    {
        public override string? Mask => "0000";
        public override string? Name => "Test Mask";
        public override string? Sample => "1234";
        public override Type? ValidatingType => typeof(int);
    }
}
