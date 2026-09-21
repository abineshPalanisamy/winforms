// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;
using System.Windows.Forms.Rendering.CheckBox;

namespace System.Windows.Forms.Tests;

public class AnimatedToggleSwitchRendererTests
{
    [Fact]
    public void GetFocusBounds_ValidTextBounds_ReturnsTextBounds()
    {
        Rectangle textBounds = new(60, 4, 120, 28);

        Rectangle focusBounds =
            AnimatedToggleSwitchRenderer.GetFocusBounds(textBounds);

        Assert.Equal(textBounds, focusBounds);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(20, 0)]
    [InlineData(0, 0)]
    [InlineData(-1, 20)]
    [InlineData(20, -1)]
    public void GetFocusBounds_InvalidTextBounds_ReturnsEmpty(
        int width,
        int height)
    {
        Rectangle textBounds = new(10, 10, width, height);

        Rectangle focusBounds =
            AnimatedToggleSwitchRenderer.GetFocusBounds(textBounds);

        Assert.Equal(Rectangle.Empty, focusBounds);
    }

    [Fact]
    public void GetFocusBounds_CheckBoxSwitchAlignedLeft_ExcludesSwitchBounds()
    {
        using CheckBox control = new()
        {
            Appearance = Appearance.ToggleSwitch,
            AutoSize = false,
            CheckAlign = ContentAlignment.MiddleLeft,
            Size = new Size(240, 40),
            Text = "CheckBox"
        };

        ToggleSwitchMetrics metrics = ToggleSwitchMetrics.Create(control);

        Rectangle switchBounds =
            AnimatedToggleSwitchRenderer.GetSwitchBounds(
                control,
                control.CheckAlign,
                metrics);

        Rectangle textBounds =
            AnimatedToggleSwitchRenderer.GetTextBounds(
                control,
                control.CheckAlign,
                metrics);

        Rectangle focusBounds =
            AnimatedToggleSwitchRenderer.GetFocusBounds(textBounds);

        Assert.False(focusBounds.IsEmpty);
        Assert.Equal(textBounds, focusBounds);
        Assert.False(focusBounds.IntersectsWith(switchBounds));
        Assert.NotEqual(control.ClientRectangle, focusBounds);
    }

    [Fact]
    public void GetFocusBounds_RadioButtonSwitchAlignedLeft_ExcludesSwitchBounds()
    {
        using RadioButton control = new()
        {
            Appearance = Appearance.ToggleSwitch,
            AutoSize = false,
            CheckAlign = ContentAlignment.MiddleLeft,
            Size = new Size(260, 40),
            Text = "RadioButton"
        };

        ToggleSwitchMetrics metrics = ToggleSwitchMetrics.Create(control);

        Rectangle switchBounds =
            AnimatedToggleSwitchRenderer.GetSwitchBounds(
                control,
                control.CheckAlign,
                metrics);

        Rectangle textBounds =
            AnimatedToggleSwitchRenderer.GetTextBounds(
                control,
                control.CheckAlign,
                metrics);

        Rectangle focusBounds =
            AnimatedToggleSwitchRenderer.GetFocusBounds(textBounds);

        Assert.False(focusBounds.IsEmpty);
        Assert.Equal(textBounds, focusBounds);
        Assert.False(focusBounds.IntersectsWith(switchBounds));
        Assert.NotEqual(control.ClientRectangle, focusBounds);
    }

    [Fact]
    public void GetFocusBounds_CheckBoxSwitchAlignedRight_ExcludesSwitchBounds()
    {
        using CheckBox control = new()
        {
            Appearance = Appearance.ToggleSwitch,
            AutoSize = false,
            CheckAlign = ContentAlignment.MiddleRight,
            Size = new Size(240, 40),
            Text = "CheckBox"
        };

        ToggleSwitchMetrics metrics = ToggleSwitchMetrics.Create(control);

        Rectangle switchBounds =
            AnimatedToggleSwitchRenderer.GetSwitchBounds(
                control,
                control.CheckAlign,
                metrics);

        Rectangle textBounds =
            AnimatedToggleSwitchRenderer.GetTextBounds(
                control,
                control.CheckAlign,
                metrics);

        Rectangle focusBounds =
            AnimatedToggleSwitchRenderer.GetFocusBounds(textBounds);

        Assert.False(focusBounds.IsEmpty);
        Assert.Equal(textBounds, focusBounds);
        Assert.False(focusBounds.IntersectsWith(switchBounds));
        Assert.NotEqual(control.ClientRectangle, focusBounds);
    }

    [Fact]
    public void GetFocusBounds_RadioButtonSwitchAlignedRight_ExcludesSwitchBounds()
    {
        using RadioButton control = new()
        {
            Appearance = Appearance.ToggleSwitch,
            AutoSize = false,
            CheckAlign = ContentAlignment.MiddleRight,
            Size = new Size(260, 40),
            Text = "RadioButton"
        };

        ToggleSwitchMetrics metrics = ToggleSwitchMetrics.Create(control);

        Rectangle switchBounds =
            AnimatedToggleSwitchRenderer.GetSwitchBounds(
                control,
                control.CheckAlign,
                metrics);

        Rectangle textBounds =
            AnimatedToggleSwitchRenderer.GetTextBounds(
                control,
                control.CheckAlign,
                metrics);

        Rectangle focusBounds =
            AnimatedToggleSwitchRenderer.GetFocusBounds(textBounds);

        Assert.False(focusBounds.IsEmpty);
        Assert.Equal(textBounds, focusBounds);
        Assert.False(focusBounds.IntersectsWith(switchBounds));
        Assert.NotEqual(control.ClientRectangle, focusBounds);
    }
}
