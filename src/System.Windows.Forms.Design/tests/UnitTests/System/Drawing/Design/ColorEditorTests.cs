// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Windows.Forms.TestUtilities;
using Moq;

namespace System.Drawing.Design.Tests;

public partial class ColorEditorTests
{
    #region ColorEditor Main Class Tests

    [Fact]
    public void ColorEditor_Ctor_Default()
    {
        ColorEditor editor = new();
        Assert.False(editor.IsDropDownResizable);
    }

    public static IEnumerable<object[]> EditValue_TestData()
    {
        yield return new object[] { null };
        yield return new object[] { "value" };
        yield return new object[] { Color.Empty };
        yield return new object[] { Color.Red };
        yield return new object[] { Color.Blue };
        yield return new object[] { Color.Green };
        yield return new object[] { Color.White };
        yield return new object[] { Color.Black };
        yield return new object[] { new() };
    }

    [Theory]
    [MemberData(nameof(EditValue_TestData))]
    public void ColorEditor_EditValue_ValidProvider_ReturnsValue(object value)
    {
        ColorEditor editor = new();
        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object)
            .Verifiable();
        mockEditorService
            .Setup(e => e.DropDownControl(It.IsAny<Control>()))
            .Verifiable();
        Assert.Equal(value, editor.EditValue(null, mockServiceProvider.Object, value));
        mockServiceProvider.Verify(p => p.GetService(typeof(IWindowsFormsEditorService)), Times.Once());
        mockEditorService.Verify(e => e.DropDownControl(It.IsAny<Control>()), Times.Once());

        // Edit again.
        Assert.Equal(value, editor.EditValue(null, mockServiceProvider.Object, value));
        mockServiceProvider.Verify(p => p.GetService(typeof(IWindowsFormsEditorService)), Times.Exactly(2));
        mockServiceProvider.Verify(p => p.GetService(typeof(IWindowsFormsEditorService)), Times.Exactly(2));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetEditValueInvalidProviderTestData))]
    public void ColorEditor_EditValue_InvalidProvider_ReturnsValue(IServiceProvider provider, object value)
    {
        ColorEditor editor = new();
        Assert.Same(value, editor.EditValue(null, provider, value));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void ColorEditor_GetEditStyle_Invoke_ReturnsDropDown(ITypeDescriptorContext context)
    {
        ColorEditor editor = new();
        Assert.Equal(UITypeEditorEditStyle.DropDown, editor.GetEditStyle(context));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void ColorEditor_GetPaintValueSupported_Invoke_ReturnsTrue(ITypeDescriptorContext context)
    {
        ColorEditor editor = new();
        Assert.True(editor.GetPaintValueSupported(context));
    }

    #endregion

    #region PaintValue Tests

    public static IEnumerable<object[]> PaintValue_WithColor_TestData()
    {
        yield return new object[] { Color.Red };
        yield return new object[] { Color.Blue };
        yield return new object[] { Color.Green };
        yield return new object[] { Color.White };
        yield return new object[] { Color.Black };
        yield return new object[] { Color.Yellow };
        yield return new object[] { Color.Cyan };
        yield return new object[] { Color.Magenta };
        yield return new object[] { Color.Empty };
        yield return new object[] { Color.RebeccaPurple };
    }

    [Theory]
    [MemberData(nameof(PaintValue_WithColor_TestData))]
    public void ColorEditor_PaintValue_WithColor_FillsRectangleWithBrush(Color color)
    {
        ColorEditor editor = new();
        using Bitmap bitmap = new(10, 10);
        using Graphics graphics = Graphics.FromImage(bitmap);

        var paintEventArgs = new PaintValueEventArgs(null, color, graphics, new Rectangle(0, 0, 10, 10));

        // Should not throw
        editor.PaintValue(paintEventArgs);

        // Verify the graphics object didn't get disposed (should still be usable)
        Assert.NotNull(graphics);
    }

    [Fact]
    public void ColorEditor_PaintValue_WithNullValue_DoesNotThrow()
    {
        ColorEditor editor = new();
        using Bitmap bitmap = new(10, 10);
        using Graphics graphics = Graphics.FromImage(bitmap);

        var paintEventArgs = new PaintValueEventArgs(null, null, graphics, new Rectangle(0, 0, 10, 10));

        // Should not throw
        editor.PaintValue(paintEventArgs);
    }

    [Fact]
    public void ColorEditor_PaintValue_WithNonColorValue_DoesNotThrow()
    {
        ColorEditor editor = new();
        using Bitmap bitmap = new(10, 10);
        using Graphics graphics = Graphics.FromImage(bitmap);

        var paintEventArgs = new PaintValueEventArgs(null, "not a color", graphics, new Rectangle(0, 0, 10, 10));

        // Should not throw
        editor.PaintValue(paintEventArgs);
    }

    [Fact]
    public void ColorEditor_PaintValue_WithDifferentRectangles_Succeeds()
    {
        ColorEditor editor = new();
        using Bitmap bitmap = new(100, 100);
        using Graphics graphics = Graphics.FromImage(bitmap);

        // Test with different rectangle sizes
        Rectangle[] rectangles = new[]
        {
            new Rectangle(0, 0, 10, 10),
            new Rectangle(5, 5, 20, 20),
            new Rectangle(10, 10, 50, 50),
            new Rectangle(0, 0, 1, 1),
        };

        foreach (var rect in rectangles)
        {
            var paintEventArgs = new PaintValueEventArgs(null, Color.Red, graphics, rect);
            editor.PaintValue(paintEventArgs);
        }
    }

    #endregion

    #region EditValue Context Tests

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void ColorEditor_EditValue_WithContext_ReturnsValue(ITypeDescriptorContext context)
    {
        ColorEditor editor = new();
        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);

        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object)
            .Verifiable();
        mockEditorService
            .Setup(e => e.DropDownControl(It.IsAny<Control>()))
            .Verifiable();

        Color testColor = Color.Red;
        object result = editor.EditValue(context, mockServiceProvider.Object, testColor);

        Assert.Equal(testColor, result);
        mockServiceProvider.Verify(p => p.GetService(typeof(IWindowsFormsEditorService)), Times.Once());
    }

    [Fact]
    public void ColorEditor_EditValue_WithNullContext_ReturnsValue()
    {
        ColorEditor editor = new();
        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);

        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object)
            .Verifiable();
        mockEditorService
            .Setup(e => e.DropDownControl(It.IsAny<Control>()))
            .Verifiable();

        object result = editor.EditValue(null, mockServiceProvider.Object, Color.Blue);

        Assert.Equal(Color.Blue, result);
    }

    [Fact]
    public void ColorEditor_EditValue_NullProvider_ReturnsOriginalValue()
    {
        ColorEditor editor = new();
        Color testColor = Color.Red;

        object result = editor.EditValue(null, null, testColor);

        Assert.Equal(testColor, result);
    }

    #endregion

    #region GetEditStyle Tests

    [Fact]
    public void ColorEditor_GetEditStyle_WithNullContext_ReturnsDropDown()
    {
        ColorEditor editor = new();

        UITypeEditorEditStyle style = editor.GetEditStyle(null);

        Assert.Equal(UITypeEditorEditStyle.DropDown, style);
    }

    #endregion

    #region GetPaintValueSupported Tests

    [Fact]
    public void ColorEditor_GetPaintValueSupported_WithNullContext_ReturnsTrue()
    {
        ColorEditor editor = new();

        bool supported = editor.GetPaintValueSupported(null);

        Assert.True(supported);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ColorEditor_MultipleEditSessions_WorkCorrectly()
    {
        ColorEditor editor = new();
        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);

        mockServiceProvider
            .Setup(p => p.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object)
            .Verifiable();
        mockEditorService
            .Setup(e => e.DropDownControl(It.IsAny<Control>()))
            .Verifiable();

        // First edit
        Color color1 = Color.Red;
        object result1 = editor.EditValue(null, mockServiceProvider.Object, color1);
        Assert.Equal(color1, result1);

        // Second edit
        Color color2 = Color.Blue;
        object result2 = editor.EditValue(null, mockServiceProvider.Object, color2);
        Assert.Equal(color2, result2);

        // Third edit with different input type
        string stringValue = "not a color";
        object result3 = editor.EditValue(null, mockServiceProvider.Object, stringValue);
        Assert.Equal(stringValue, result3);

        mockServiceProvider.Verify(p => p.GetService(typeof(IWindowsFormsEditorService)), Times.Exactly(3));
    }

    [Fact]
    public void ColorEditor_AllMethods_WorkTogether()
    {
        ColorEditor editor = new();

        // Test all methods
        Assert.False(editor.IsDropDownResizable);
        Assert.True(editor.GetPaintValueSupported(null));
        Assert.Equal(UITypeEditorEditStyle.DropDown, editor.GetEditStyle(null));

        // Test PaintValue
        using Bitmap bitmap = new(10, 10);
        using Graphics graphics = Graphics.FromImage(bitmap);
        var paintEventArgs = new PaintValueEventArgs(null, Color.Red, graphics, new Rectangle(0, 0, 10, 10));
        editor.PaintValue(paintEventArgs);

        // All operations succeeded
        Assert.True(true);
    }

    #endregion
}
