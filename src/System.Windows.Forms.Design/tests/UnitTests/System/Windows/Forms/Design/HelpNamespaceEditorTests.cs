// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.TestUtilities;

namespace System.Windows.Forms.Design.Tests;

public class HelpNamespaceEditorTests
{
    [Fact]
    public void HelpNamespaceEditor_Ctor_Default()
    {
        HelpNamespaceEditor editor = new();
        Assert.False(editor.IsDropDownResizable);
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetEditValueInvalidProviderTestData))]
    public void HelpNamespaceEditor_EditValue_InvalidProvider_ReturnsValue(IServiceProvider provider, object value)
    {
        HelpNamespaceEditor editor = new();
        Assert.Same(value, editor.EditValue(null, provider, value));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void HelpNamespaceEditor_GetEditStyle_Invoke_ReturnsModal(ITypeDescriptorContext context)
    {
        HelpNamespaceEditor editor = new();
        Assert.Equal(UITypeEditorEditStyle.Modal, editor.GetEditStyle(context));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void HelpNamespaceEditor_GetPaintValueSupported_Invoke_ReturnsFalse(ITypeDescriptorContext context)
    {
        HelpNamespaceEditor editor = new();
        Assert.False(editor.GetPaintValueSupported(context));
    }

    [Fact]
    public void HelpNamespaceEditor_InitializeDialog_Invoke_Success()
    {
        SubHelpNamespaceEditor editor = new();
        using OpenFileDialog openFileDialog = new();
        editor.InitializeDialog(openFileDialog);
        Assert.Equal("All Help Files(*.chm,*.col,*.htm,*.html)|*.chm;*.col;*.htm;*.html|Compressed HTML Files(*.chm)|*.chm|Help Collection Files(*.col)|*.col|HTML Files(*.htm,*.html)|*.htm;*.html|All Files(*.*)|*.*", openFileDialog.Filter);
        Assert.Equal("Open Help File", openFileDialog.Title);
    }

    private class SubHelpNamespaceEditor : HelpNamespaceEditor
    {
        public new void InitializeDialog(OpenFileDialog openFileDialog) => base.InitializeDialog(openFileDialog);
    }
}
