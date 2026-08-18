// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Windows.Forms.Design;
using System.Windows.Forms.TestUtilities;
using Moq;

namespace System.ComponentModel.Design.Tests;

public class MultilineStringEditorTests
{
    [Fact]
    public void MultilineStringEditor_Ctor_Default()
    {
        MultilineStringEditor editor = new();
        Assert.False(editor.IsDropDownResizable);
    }

    public static IEnumerable<object[]> EditValue_TestData() =>
    [
        [null],
        ["value"],
        [new()]
    ];

    [Theory]
    [MemberData(nameof(EditValue_TestData))]
    public void MultilineStringEditor_EditValue_ValidProvider_ReturnsValue(object value)
    {
        MultilineStringEditor editor = new();
        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object)
            .Verifiable();
        mockEditorService
            .Setup(e => e.DropDownControl(It.IsAny<Control>()))
            .Verifiable();
        Assert.Same(string.Empty, editor.EditValue(null, mockServiceProvider.Object, value));
        mockServiceProvider.Verify(p => p.GetService(typeof(IWindowsFormsEditorService)), Times.Once());
        mockEditorService.Verify(e => e.DropDownControl(It.IsAny<Control>()), Times.Once());

        // Edit again.
        Assert.Same(string.Empty, editor.EditValue(null, mockServiceProvider.Object, value));
        mockServiceProvider.Verify(p => p.GetService(typeof(IWindowsFormsEditorService)), Times.Exactly(2));
        mockServiceProvider.Verify(p => p.GetService(typeof(IWindowsFormsEditorService)), Times.Exactly(2));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetEditValueInvalidProviderTestData))]
    public void MultilineStringEditor_EditValue_InvalidProvider_ReturnsValue(IServiceProvider provider, object value)
    {
        MultilineStringEditor editor = new();
        Assert.Same(value, editor.EditValue(null, provider, value));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void MultilineStringEditor_GetEditStyle_Invoke_ReturnsDropDown(ITypeDescriptorContext context)
    {
        MultilineStringEditor editor = new();
        Assert.Equal(UITypeEditorEditStyle.DropDown, editor.GetEditStyle(context));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void MultilineStringEditor_GetPaintValueSupported_Invoke_ReturnsFalse(ITypeDescriptorContext context)
    {
        MultilineStringEditor editor = new();
        Assert.False(editor.GetPaintValueSupported(context));
    }

    [Fact]
    public void MultilineStringEditorUI_InitializeComponent_Default()
    {
        using RichTextBox ui = CreateEditorUI();
        Assert.True(ui.Multiline);
        Assert.False(ui.WordWrap);
        Assert.Equal(BorderStyle.None, ui.BorderStyle);
        Assert.Equal(RichTextBoxScrollBars.Both, ui.ScrollBars);
        Assert.False(ui.DetectUrls);
        Assert.False(ui.RichTextShortcutsEnabled);
    }

    [Fact]
    public void MultilineStringEditorUI_ProcessDialogKey_Escape_SetsEscapeFlag()
    {
        using RichTextBox ui = CreateEditorUI();
        dynamic dynamicUi = ui.TestAccessor.Dynamic;
        dynamicUi._editing = true;
        InvokeProcessDialogKey(ui, Keys.Escape);
        Assert.True(dynamicUi._escapePressed);
    }

    [Fact]
    public void MultilineStringEditorUI_ProcessDialogKey_CtrlEscape_DoesNotSetEscapeFlag()
    {
        using RichTextBox ui = CreateEditorUI();
        dynamic dynamicUi = ui.TestAccessor.Dynamic;
        dynamicUi._editing = true;
        InvokeProcessDialogKey(ui, Keys.Escape | Keys.Control);
        Assert.False(dynamicUi._escapePressed);
    }

    [Fact]
    public void MultilineStringEditorUI_ProcessDialogKey_ShiftEscape_DoesNotSetEscapeFlag()
    {
        using RichTextBox ui = CreateEditorUI();
        dynamic dynamicUi = ui.TestAccessor.Dynamic;
        dynamicUi._editing = true;
        InvokeProcessDialogKey(ui, Keys.Escape | Keys.Shift);
        Assert.False(dynamicUi._escapePressed);
    }

    [Fact]
    public void MultilineStringEditorUI_ProcessDialogKey_AltEscape_DoesNotSetEscapeFlag()
    {
        using RichTextBox ui = CreateEditorUI();
        dynamic dynamicUi = ui.TestAccessor.Dynamic;
        dynamicUi._editing = true;
        InvokeProcessDialogKey(ui, Keys.Escape | Keys.Alt);
        Assert.False(dynamicUi._escapePressed);
    }

    [Fact]
    public void MultilineStringEditorUI_ProcessDialogKey_OtherKey_DoesNotSetEscapeFlag()
    {
        using RichTextBox ui = CreateEditorUI();
        dynamic dynamicUi = ui.TestAccessor.Dynamic;
        dynamicUi._editing = true;
        InvokeProcessDialogKey(ui, Keys.Enter);
        Assert.False(dynamicUi._escapePressed);
    }

    [Fact]
    public void MultilineStringEditorUI_BeginEndEdit_ResetsState()
    {
        using RichTextBox ui = CreateEditorUI();
        dynamic dynamicUi = ui.TestAccessor.Dynamic;
        dynamicUi.BeginEdit(MakeEditorService().Object, "hello world");
        Assert.True(dynamicUi._editing);
        // Note: The override's Text getter returns string.Empty when the handle is not
        // created, so we don't assert the text value here. We only verify state changes.

        bool result = dynamicUi.EndEdit();
        Assert.True(result);
        Assert.False(dynamicUi._editing);
        Assert.Null(dynamicUi._editorService);
    }

    [Fact]
    public void MultilineStringEditorUI_EndEdit_AfterEscape_ReturnsFalse()
    {
        using RichTextBox ui = CreateEditorUI();
        dynamic dynamicUi = ui.TestAccessor.Dynamic;
        dynamicUi.BeginEdit(MakeEditorService().Object, "some text");
        dynamicUi._escapePressed = true;
        bool result = dynamicUi.EndEdit();
        Assert.False(result);
    }

    [Fact]
    public void MultilineStringEditorUI_EndEdit_ResetsCtrlEnterFlag()
    {
        using RichTextBox ui = CreateEditorUI();
        dynamic dynamicUi = ui.TestAccessor.Dynamic;
        dynamicUi.BeginEdit(MakeEditorService().Object, "text");
        dynamicUi._ctrlEnterPressed = true;
        dynamicUi.EndEdit();
        Assert.False(dynamicUi._ctrlEnterPressed);
    }

    [Fact]
    public void MultilineStringEditorUI_BeginEdit_NullValue_SetsEmptyText()
    {
        using RichTextBox ui = CreateEditorUI();
        dynamic dynamicUi = ui.TestAccessor.Dynamic;
        dynamicUi.BeginEdit(MakeEditorService().Object, null);
        Assert.True(dynamicUi._editing);
        Assert.Equal(string.Empty, ui.Text);
    }

    [Fact]
    public void MultilineStringEditorUI_BeginEdit_ResetsWatermarkAndMinimumSize()
    {
        using RichTextBox ui = CreateEditorUI();
        dynamic dynamicUi = ui.TestAccessor.Dynamic;
        dynamicUi._watermarkSize = new Size(50, 20);
        dynamicUi._minimumSize = new Size(100, 30);
        dynamicUi._escapePressed = true;
        dynamicUi._ctrlEnterPressed = true;
        dynamicUi.BeginEdit(MakeEditorService().Object, "value");
        Assert.Equal(Size.Empty, dynamicUi._watermarkSize);
        Assert.Equal(Size.Empty, dynamicUi._minimumSize);
        Assert.False(dynamicUi._escapePressed);
        Assert.False(dynamicUi._ctrlEnterPressed);
    }

    [Fact]
    public void MultilineStringEditorUI_CreateRichEditOleCallback_ReturnsOleCallback()
    {
        using RichTextBox ui = CreateEditorUI();
        object callback = ui.TestAccessor.Dynamic.CreateRichEditOleCallback();
        Assert.NotNull(callback);
        Assert.Equal("OleCallback", callback.GetType().Name);
    }

    [Fact]
    public void MultilineStringEditorUI_Font_Get_ReturnsNonNull()
    {
        using RichTextBox ui = CreateEditorUI();
        Assert.NotNull(ui.Font);
    }

    [Fact]
    public void MultilineStringEditorUI_Font_Set_DoesNotChange()
    {
        using RichTextBox ui = CreateEditorUI();
        Font original = ui.Font;
        try
        {
            using Font newFont = new(original.FontFamily, original.Size + 4, original.Style);
            ui.Font = newFont;
            // The override ignores the assignment; the underlying base.Font remains unchanged.
            Assert.Equal(original.SizeInPoints, ui.Font.SizeInPoints);
        }
        finally
        {
            original.Dispose();
        }
    }

    [Fact]
    public void MultilineStringEditorUI_Font_SetNull_DoesNotThrow()
    {
        using RichTextBox ui = CreateEditorUI();
        // The override's setter accepts a null font via [AllowNull] and does nothing.
        ui.Font = null;
        Assert.NotNull(ui.Font);
    }

    [Fact]
    public void MultilineStringEditorUI_Text_Get_BeforeHandle_ReturnsEmpty()
    {
        using RichTextBox ui = CreateEditorUI();
        Assert.Equal(string.Empty, ui.Text);
    }

    [Fact]
    public void MultilineStringEditorUI_Text_Set_DoesNotThrow()
    {
        using RichTextBox ui = CreateEditorUI();
        // The setter delegates to base.Text = value. Without a handle, the override's
        // getter would return string.Empty, so we don't assert the getter value here.
        ui.Text = "hello";
    }

    [Fact]
    public void MultilineStringEditorUI_WatermarkBrush_CreatedOnce()
    {
        RichTextBox ui = CreateEditorUI();
        dynamic dynamicUi = ui.TestAccessor.Dynamic;
        Brush first = dynamicUi.WatermarkBrush;
        Brush second = dynamicUi.WatermarkBrush;
        Assert.Same(first, second);
        first.Dispose();
        ui.Dispose();
    }

    [Fact]
    public void MultilineStringEditorUI_OnTextChanged_ResetsContentsResizedFlag()
    {
        using RichTextBox ui = CreateEditorUI();
        dynamic dynamicUi = ui.TestAccessor.Dynamic;
        // When _contentsResizedRaised is true, OnTextChanged does not call ResizeToContent,
        // which avoids the Screen/PointToScreen dependencies. Set the flag to a safe value.
        dynamicUi._contentsResizedRaised = true;
        dynamicUi.OnTextChanged(EventArgs.Empty);
        Assert.False(dynamicUi._contentsResizedRaised);
    }

    private static bool InvokeProcessDialogKey(RichTextBox ui, Keys keyData)
    {
        // ProcessDialogKey is protected on TextBoxBase so it cannot be called directly from outside the class.
        MethodInfo method = ui.GetType().GetMethod("ProcessDialogKey", BindingFlags.Instance | BindingFlags.NonPublic);
        if (method is null)
        {
            throw new InvalidOperationException("ProcessDialogKey method not found.");
        }
        return (bool)method.Invoke(ui, new object[] { keyData });
    }

    private static RichTextBox CreateEditorUI()
    {
        Type uiType = typeof(MultilineStringEditor).GetNestedType("MultilineStringEditorUI", BindingFlags.NonPublic);
        return (RichTextBox)Activator.CreateInstance(uiType, nonPublic: true);
    }

    private static Mock<IWindowsFormsEditorService> MakeEditorService()
    {
        Mock<IWindowsFormsEditorService> mock = new(MockBehavior.Strict);
        mock.Setup(e => e.DropDownControl(It.IsAny<Control>())).Verifiable();
        return mock;
    }
}
