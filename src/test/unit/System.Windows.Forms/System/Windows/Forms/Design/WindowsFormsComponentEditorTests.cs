// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using Moq;

namespace System.Windows.Forms.Design.Tests;

// NB: doesn't require thread affinity
public class WindowsFormsComponentEditorTests
{
    public static IEnumerable<object[]> EditComponent_ObjectIWin32Window_TestData()
    {
        yield return new object[] { null, null, null };
        yield return new object[] { Array.Empty<Type>(), null, null };

        var mockWindow = new Mock<IWin32Window>(MockBehavior.Strict);
        yield return new object[] { null, new(), mockWindow.Object };
        yield return new object[] { Array.Empty<Type>(), new(), mockWindow.Object };
    }

    [Theory]
    [MemberData(nameof(EditComponent_ObjectIWin32Window_TestData))]
    public void WindowsFormsComponentEditor_EditComponent_InvokeObjectIWin32Window_ReturnsFalse(Type[] pages, object component, IWin32Window owner)
    {
        CustomWindowsFormsComponentEditor editor = new()
        {
            GetComponentEditorPagesResult = pages
        };
        Assert.False(editor.EditComponent(component, owner));
    }

    public static IEnumerable<object[]> EditComponent_ITypeDescriptorContextObject_TestData()
    {
        yield return new object[] { null, null, null };
        yield return new object[] { Array.Empty<Type>(), null, null };

        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        yield return new object[] { null, mockContext.Object, new() };
        yield return new object[] { Array.Empty<Type>(), mockContext.Object, new() };
    }

    [Theory]
    [MemberData(nameof(EditComponent_ITypeDescriptorContextObject_TestData))]
    public void WindowsFormsComponentEditor_EditComponent_InvokeITypeDescriptorContextObject_ReturnsFalse(Type[] pages, ITypeDescriptorContext context, object component)
    {
        CustomWindowsFormsComponentEditor editor = new()
        {
            GetComponentEditorPagesResult = pages
        };
        Assert.False(editor.EditComponent(context, component));
    }

    public static IEnumerable<object[]> EditComponent_ITypeDescriptorContextObjectIWin32Window_TestData()
    {
        yield return new object[] { null, null, null, null };
        yield return new object[] { Array.Empty<Type>(), null, null, null };

        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        var mockWindow = new Mock<IWin32Window>(MockBehavior.Strict);
        yield return new object[] { null, mockContext.Object, new(), mockWindow.Object };
        yield return new object[] { Array.Empty<Type>(), mockContext.Object, new(), mockWindow.Object };
    }

    [Theory]
    [MemberData(nameof(EditComponent_ITypeDescriptorContextObjectIWin32Window_TestData))]
    public void WindowsFormsComponentEditor_EditComponent_InvokeITypeDescriptorContextObjectIWin32Window_ReturnsFalse(Type[] pages, ITypeDescriptorContext context, object component, IWin32Window owner)
    {
        CustomWindowsFormsComponentEditor editor = new()
        {
            GetComponentEditorPagesResult = pages
        };
        Assert.False(editor.EditComponent(context, component, owner));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("component")]
    public void EditComponent_NonComponentWithPages_ThrowsArgumentException(object component)
    {
        CustomWindowsFormsComponentEditor editor = new()
        {
            GetComponentEditorPagesResult = [typeof(int)]
        };
        Assert.Throws<ArgumentException>("component", () => editor.EditComponent(component, null));
        Assert.Throws<ArgumentException>("component", () => editor.EditComponent(null, component));
        Assert.Throws<ArgumentException>("component", () => editor.EditComponent(null, component, null));
    }

    [Fact]
    public void WindowsFormsComponentEditor_GetComponentEditorPages_Invoke_ReturnsNull()
    {
        SubWindowsFormsComponentEditor editor = new();
        Assert.Null(editor.GetComponentEditorPages());
    }

    [Fact]
    public void WindowsFormsComponentEditor_GetInitialComponentEditorPageIndex_Invoke_ReturnsZero()
    {
        SubWindowsFormsComponentEditor editor = new();
        Assert.Equal(0, editor.GetInitialComponentEditorPageIndex());
    }

    [WinFormsFact]
    public void WindowsFormsComponentEditor_EditComponent_InvokeITypeDescriptorContextObject_WithValidPages_ReturnsExpected()
    {
        using Component component = new();
        CustomWindowsFormsComponentEditor editor = new()
        {
            GetComponentEditorPagesResult = [typeof(OkComponentEditorPage)]
        };
        Assert.True(editor.EditComponent((ITypeDescriptorContext)null, component));
    }

    [WinFormsFact]
    public void WindowsFormsComponentEditor_EditComponent_InvokeITypeDescriptorContextObject_WithValidPagesCancel_ReturnsFalse()
    {
        using Component component = new();
        CustomWindowsFormsComponentEditor editor = new()
        {
            GetComponentEditorPagesResult = [typeof(CancelComponentEditorPage)]
        };
        Assert.False(editor.EditComponent((ITypeDescriptorContext)null, component));
    }

    [WinFormsFact]
    public void WindowsFormsComponentEditor_EditComponent_InvokeITypeDescriptorContextObject_WithMultipleValidPages_ReturnsExpected()
    {
        using Component component = new();
        CustomWindowsFormsComponentEditor editor = new()
        {
            GetComponentEditorPagesResult =
            [
                typeof(OkComponentEditorPage),
                typeof(OkComponentEditorPage),
                typeof(OkComponentEditorPage)
            ]
        };
        Assert.True(editor.EditComponent((ITypeDescriptorContext)null, component));
    }

    [WinFormsFact]
    public void WindowsFormsComponentEditor_EditComponent_InvokeObjectIWin32Window_WithValidPages_ReturnsExpected()
    {
        using Component component = new();
        CustomWindowsFormsComponentEditor editor = new()
        {
            GetComponentEditorPagesResult = [typeof(OkComponentEditorPage)]
        };
        Assert.True(editor.EditComponent(component, null));
    }

    [WinFormsFact]
    public void WindowsFormsComponentEditor_EditComponent_InvokeObjectIWin32Window_WithValidPagesCancel_ReturnsFalse()
    {
        using Component component = new();
        CustomWindowsFormsComponentEditor editor = new()
        {
            GetComponentEditorPagesResult = [typeof(CancelComponentEditorPage)]
        };
        Assert.False(editor.EditComponent(component, null));
    }

    [WinFormsFact]
    public void WindowsFormsComponentEditor_EditComponent_InvokeObjectIWin32Window_WithValidPagesAndOwner_ReturnsExpected()
    {
        using Component component = new();
        using Control owner = new();
        CustomWindowsFormsComponentEditor editor = new()
        {
            GetComponentEditorPagesResult = [typeof(OkComponentEditorPage)]
        };
        Assert.True(editor.EditComponent(component, owner));
    }

    [WinFormsFact]
    public void WindowsFormsComponentEditor_EditComponent_InvokeITypeDescriptorContextObjectIWin32Window_WithValidPages_ReturnsExpected()
    {
        using Component component = new();
        CustomWindowsFormsComponentEditor editor = new()
        {
            GetComponentEditorPagesResult = [typeof(OkComponentEditorPage)]
        };
        Assert.True(editor.EditComponent((ITypeDescriptorContext)null, component, null));
    }

    [WinFormsFact]
    public void WindowsFormsComponentEditor_EditComponent_InvokeITypeDescriptorContextObjectIWin32Window_WithValidPagesCancel_ReturnsFalse()
    {
        using Component component = new();
        CustomWindowsFormsComponentEditor editor = new()
        {
            GetComponentEditorPagesResult = [typeof(CancelComponentEditorPage)]
        };
        Assert.False(editor.EditComponent((ITypeDescriptorContext)null, component, null));
    }

    [WinFormsFact]
    public void WindowsFormsComponentEditor_EditComponent_InvokeITypeDescriptorContextObjectIWin32Window_WithValidPagesAndOwner_ReturnsExpected()
    {
        using Component component = new();
        using Control owner = new();
        CustomWindowsFormsComponentEditor editor = new()
        {
            GetComponentEditorPagesResult = [typeof(OkComponentEditorPage)]
        };
        Assert.True(editor.EditComponent((ITypeDescriptorContext)null, component, owner));
    }

    [WinFormsFact]
    public void WindowsFormsComponentEditor_EditComponent_InvokeITypeDescriptorContextObjectIWin32Window_WithContext_ReturnsExpected()
    {
        using Component component = new();
        Mock<ITypeDescriptorContext> mockContext = new(MockBehavior.Strict);
        CustomWindowsFormsComponentEditor editor = new()
        {
            GetComponentEditorPagesResult = [typeof(OkComponentEditorPage)]
        };
        Assert.True(editor.EditComponent(mockContext.Object, component, null));
    }

    [WinFormsFact]
    public void WindowsFormsComponentEditor_EditComponent_InvokeITypeDescriptorContextObjectIWin32Window_WithNonZeroInitialPageIndex_ReturnsExpected()
    {
        using Component component = new();
        CustomIndexWindowsFormsComponentEditor editor = new()
        {
            GetComponentEditorPagesResult =
            [
                typeof(OkComponentEditorPage),
                typeof(OkComponentEditorPage),
                typeof(OkComponentEditorPage)
            ],
            GetInitialComponentEditorPageIndexResult = 1
        };
        Assert.True(editor.EditComponent((ITypeDescriptorContext)null, component, null));
    }

    [Fact]
    public void WindowsFormsComponentEditor_GetComponentEditorPages_Override_ReturnsExpected()
    {
        Type[] expected = [typeof(OkComponentEditorPage)];
        CustomWindowsFormsComponentEditor editor = new()
        {
            GetComponentEditorPagesResult = expected
        };
        Assert.Same(expected, editor.PublicGetComponentEditorPages());
    }

    [Fact]
    public void WindowsFormsComponentEditor_GetInitialComponentEditorPageIndex_Override_ReturnsExpected()
    {
        CustomIndexWindowsFormsComponentEditor editor = new()
        {
            GetInitialComponentEditorPageIndexResult = 2
        };
        Assert.Equal(2, editor.PublicGetInitialComponentEditorPageIndex());
    }

    private class SubWindowsFormsComponentEditor : WindowsFormsComponentEditor
    {
        public new Type[] GetComponentEditorPages() => base.GetComponentEditorPages();

        public new int GetInitialComponentEditorPageIndex() => base.GetInitialComponentEditorPageIndex();
    }

    private class CustomWindowsFormsComponentEditor : WindowsFormsComponentEditor
    {
        public Type[] GetComponentEditorPagesResult { get; set; }

        public Type[] PublicGetComponentEditorPages() => GetComponentEditorPages();

        protected override Type[] GetComponentEditorPages() => GetComponentEditorPagesResult;
    }

    private class CustomIndexWindowsFormsComponentEditor : WindowsFormsComponentEditor
    {
        public Type[] GetComponentEditorPagesResult { get; set; }

        public int GetInitialComponentEditorPageIndexResult { get; set; }

        public Type[] PublicGetComponentEditorPages() => GetComponentEditorPages();

        public int PublicGetInitialComponentEditorPageIndex() => GetInitialComponentEditorPageIndex();

        protected override Type[] GetComponentEditorPages() => GetComponentEditorPagesResult;

        protected override int GetInitialComponentEditorPageIndex() => GetInitialComponentEditorPageIndexResult;
    }

    private class OkComponentEditorPage : ComponentEditorPage
    {
        public override void Activate()
        {
            base.Activate();
            Form form = FindForm();
            if (form is not null)
            {
                form.DialogResult = DialogResult.OK;
                form.Close();
            }
        }

        public override string Title => nameof(OkComponentEditorPage);

        protected override void LoadComponent()
        {
        }

        protected override void ReloadComponent()
        {
        }

        protected override void SaveComponent()
        {
        }
    }

    private class CancelComponentEditorPage : ComponentEditorPage
    {
        public override void Activate()
        {
            base.Activate();
            Form form = FindForm();
            if (form is not null)
            {
                form.DialogResult = DialogResult.Cancel;
                form.Close();
            }
        }

        public override string Title => nameof(CancelComponentEditorPage);

        protected override void LoadComponent()
        {
        }

        protected override void ReloadComponent()
        {
        }

        protected override void SaveComponent()
        {
        }
    }
}
