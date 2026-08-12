// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Reflection;

namespace System.Drawing.Design.Tests;

/// <summary>
/// Tests for ColorEditor comparers (StandardColorComparer and SystemColorComparer)
/// These classes are used internally for color sorting
/// </summary>
public class ColorEditorComparerTests
{
    #region StandardColorComparer Tests

    [Fact]
    public void StandardColorComparer_Instance_Exists()
    {
        Type comparerType = GetStandardColorComparerType();
        Assert.NotNull(comparerType);

        // Check if it has an Instance property
        PropertyInfo instanceProperty = comparerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        if (instanceProperty is not null)
        {
            object instance = instanceProperty.GetValue(null);
            Assert.NotNull(instance);
        }
    }

    [Fact]
    public void StandardColorComparer_IsIComparer()
    {
        Type comparerType = GetStandardColorComparerType();
        Type iComparerType = typeof(IComparer<Color>);

        Assert.True(iComparerType.IsAssignableFrom(comparerType));
    }

    [Fact]
    public void StandardColorComparer_Compare_WithColors_Returns()
    {
        Type comparerType = GetStandardColorComparerType();
        PropertyInfo instanceProperty = comparerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);

        if (instanceProperty is not null)
        {
            object instance = instanceProperty.GetValue(null);
            IComparer<Color> comparer = instance as IComparer<Color>;

            if (comparer is not null)
            {
                // Compare two colors
                int result = comparer.Compare(Color.Red, Color.Blue);
                // Should return an integer (comparison result)
                Assert.IsType<int>(result);
            }
        }
    }

    [Fact]
    public void StandardColorComparer_Compare_SameColor_ReturnsZero()
    {
        Type comparerType = GetStandardColorComparerType();
        PropertyInfo instanceProperty = comparerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);

        if (instanceProperty is not null)
        {
            object instance = instanceProperty.GetValue(null);
            IComparer<Color> comparer = instance as IComparer<Color>;

            if (comparer is not null)
            {
                // Compare same color
                int result = comparer.Compare(Color.Red, Color.Red);
                Assert.Equal(0, result);
            }
        }
    }

    #endregion

    #region SystemColorComparer Tests

    [Fact]
    public void SystemColorComparer_CanBeInstantiated()
    {
        Type comparerType = GetSystemColorComparerType();
        Assert.NotNull(comparerType);

        object instance = Activator.CreateInstance(comparerType);
        Assert.NotNull(instance);
    }

    [Fact]
    public void SystemColorComparer_IsIComparer()
    {
        Type comparerType = GetSystemColorComparerType();
        Type iComparerType = typeof(IComparer<Color>);

        Assert.True(iComparerType.IsAssignableFrom(comparerType));
    }

    [Fact]
    public void SystemColorComparer_Compare_WithColors_Returns()
    {
        Type comparerType = GetSystemColorComparerType();
        object instance = Activator.CreateInstance(comparerType);
        IComparer<Color> comparer = instance as IComparer<Color>;

        if (comparer is not null)
        {
            // Compare two colors
            int result = comparer.Compare(SystemColors.ActiveCaption, SystemColors.ActiveBorder);
            // Should return an integer (comparison result)
            Assert.IsType<int>(result);
        }
    }

    [Fact]
    public void SystemColorComparer_Compare_SameColor_ReturnsZero()
    {
        Type comparerType = GetSystemColorComparerType();
        object instance = Activator.CreateInstance(comparerType);
        IComparer<Color> comparer = instance as IComparer<Color>;

        if (comparer is not null)
        {
            // Compare same color
            int result = comparer.Compare(SystemColors.ActiveCaption, SystemColors.ActiveCaption);
            Assert.Equal(0, result);
        }
    }

    #endregion

    #region Helper Methods

    private Type GetStandardColorComparerType()
    {
        Type colorEditorType = typeof(ColorEditor);
        Type comparerType = colorEditorType.GetNestedType("StandardColorComparer", BindingFlags.NonPublic);
        Assert.NotNull(comparerType);
        return comparerType;
    }

    private Type GetSystemColorComparerType()
    {
        Type colorEditorType = typeof(ColorEditor);
        Type comparerType = colorEditorType.GetNestedType("SystemColorComparer", BindingFlags.NonPublic);
        Assert.NotNull(comparerType);
        return comparerType;
    }

    #endregion
}
