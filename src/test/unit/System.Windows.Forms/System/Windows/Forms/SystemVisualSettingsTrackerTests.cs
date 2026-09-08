// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;

namespace System.Windows.Forms.Tests;

public class SystemVisualSettingsTrackerTests
{
    [Fact]
    public void SystemVisualSettingsTracker_GetChangedCategories_SystemColorModeClassicToDark_ReturnsSystemColorMode()
    {
        SystemVisualSettings oldSettings = CreateSettings(
            systemColorMode: SystemColorMode.Classic);

        SystemVisualSettings newSettings = CreateSettings(
            systemColorMode: SystemColorMode.Dark);

        SystemVisualSettingsCategories actual =
            SystemVisualSettingsTracker.GetChangedCategories(
                oldSettings,
                newSettings);

        Assert.Equal(
            SystemVisualSettingsCategories.SystemColorMode,
            actual);
    }

    [Fact]
    public void SystemVisualSettingsTracker_GetChangedCategories_SystemColorModeDarkToClassic_ReturnsSystemColorMode()
    {
        SystemVisualSettings oldSettings = CreateSettings(
            systemColorMode: SystemColorMode.Dark);

        SystemVisualSettings newSettings = CreateSettings(
            systemColorMode: SystemColorMode.Classic);

        SystemVisualSettingsCategories actual =
            SystemVisualSettingsTracker.GetChangedCategories(
                oldSettings,
                newSettings);

        Assert.Equal(
            SystemVisualSettingsCategories.SystemColorMode,
            actual);
    }

    [Theory]
    [InlineData(SystemColorMode.Classic)]
    [InlineData(SystemColorMode.Dark)]
    public void SystemVisualSettingsTracker_GetChangedCategories_SystemColorModeUnchanged_ReturnsNone(
        SystemColorMode systemColorMode)
    {
        SystemVisualSettings oldSettings = CreateSettings(
            systemColorMode: systemColorMode);

        SystemVisualSettings newSettings = CreateSettings(
            systemColorMode: systemColorMode);

        SystemVisualSettingsCategories actual =
            SystemVisualSettingsTracker.GetChangedCategories(
                oldSettings,
                newSettings);

        Assert.Equal(
            SystemVisualSettingsCategories.None,
            actual);
    }

    [Fact]
    public void SystemVisualSettingsTracker_GetChangedCategories_AccentColorAndSystemColorModeChanged_ReturnsBoth()
    {
        SystemVisualSettings oldSettings = CreateSettings(
            accentColor: Color.Blue,
            systemColorMode: SystemColorMode.Classic);

        SystemVisualSettings newSettings = CreateSettings(
            accentColor: Color.Red,
            systemColorMode: SystemColorMode.Dark);

        SystemVisualSettingsCategories actual =
            SystemVisualSettingsTracker.GetChangedCategories(
                oldSettings,
                newSettings);

        Assert.Equal(
            SystemVisualSettingsCategories.AccentColor
                | SystemVisualSettingsCategories.SystemColorMode,
            actual);
    }

    [Fact]
    public void SystemVisualSettingsTracker_GetChangedCategories_HighContrastAndSystemColorModeChanged_ReturnsBoth()
    {
        SystemVisualSettings oldSettings = CreateSettings(
            highContrastEnabled: false,
            systemColorMode: SystemColorMode.Dark);

        SystemVisualSettings newSettings = CreateSettings(
            highContrastEnabled: true,
            systemColorMode: SystemColorMode.Classic);

        SystemVisualSettingsCategories actual =
            SystemVisualSettingsTracker.GetChangedCategories(
                oldSettings,
                newSettings);

        Assert.Equal(
            SystemVisualSettingsCategories.HighContrast
                | SystemVisualSettingsCategories.SystemColorMode,
            actual);
    }

    [Fact]
    public void SystemVisualSettingsTracker_GetChangedCategories_AllExistingValuesUnchangedAndSystemColorModeChanged_ReturnsSystemColorMode()
    {
        SystemVisualSettings oldSettings = CreateSettings(
            accentColor: Color.CornflowerBlue,
            textScaleFactor: 1.25f,
            highContrastEnabled: false,
            clientAreaAnimationEnabled: true,
            keyboardCuesVisible: true,
            focusBorderMetrics: new Size(2, 2),
            systemColorMode: SystemColorMode.Classic);

        SystemVisualSettings newSettings = CreateSettings(
            accentColor: Color.CornflowerBlue,
            textScaleFactor: 1.25f,
            highContrastEnabled: false,
            clientAreaAnimationEnabled: true,
            keyboardCuesVisible: true,
            focusBorderMetrics: new Size(2, 2),
            systemColorMode: SystemColorMode.Dark);

        SystemVisualSettingsCategories actual =
            SystemVisualSettingsTracker.GetChangedCategories(
                oldSettings,
                newSettings);

        Assert.Equal(
            SystemVisualSettingsCategories.SystemColorMode,
            actual);
    }

    [Fact]
    public void SystemVisualSettings_Constructor_SystemColorModeDark_PreservesValue()
    {
        SystemVisualSettings settings = CreateSettings(
            systemColorMode: SystemColorMode.Dark);

        Assert.Equal(
            SystemColorMode.Dark,
            settings.SystemColorMode);
    }

    [Fact]
    public void SystemVisualSettings_Constructor_SystemColorModeClassic_PreservesValue()
    {
        SystemVisualSettings settings = CreateSettings(
            systemColorMode: SystemColorMode.Classic);

        Assert.Equal(
            SystemColorMode.Classic,
            settings.SystemColorMode);
    }

    private static SystemVisualSettings CreateSettings(
        Color? accentColor = null,
        float textScaleFactor = 1.0f,
        bool highContrastEnabled = false,
        bool clientAreaAnimationEnabled = true,
        bool keyboardCuesVisible = false,
        Size? focusBorderMetrics = null,
        SystemColorMode systemColorMode = SystemColorMode.Classic)
    {
        return new SystemVisualSettings(
            accentColor ?? Color.Blue,
            textScaleFactor,
            highContrastEnabled,
            clientAreaAnimationEnabled,
            keyboardCuesVisible,
            focusBorderMetrics ?? new Size(1, 1),
            systemColorMode);
    }
}
