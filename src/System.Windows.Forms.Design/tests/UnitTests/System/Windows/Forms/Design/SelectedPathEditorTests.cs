// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.TestUtilities;

namespace System.Windows.Forms.Design.Tests;

public class SelectedPathEditorTests
{
    [Fact]
    public void SelectedPathEditor_Ctor_Default()
    {
        SelectedPathEditor editor = new();
        Assert.False(editor.IsDropDownResizable);
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void SelectedPathEditor_GetEditStyle_Invoke_ReturnsModal(ITypeDescriptorContext context)
    {
        SelectedPathEditor editor = new();
        Assert.Equal(UITypeEditorEditStyle.Modal, editor.GetEditStyle(context));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void SelectedPathEditor_GetPaintValueSupported_Invoke_ReturnsFalse(ITypeDescriptorContext context)
    {
        SelectedPathEditor editor = new();
        Assert.False(editor.GetPaintValueSupported(context));
    }

    [Fact]
    public void SelectedPathEditor_InitializeDialog_Invoke_SetsDescription()
    {
        SubSelectedPathEditor editor = new();
        Assert.Equal(SR.SelectedPathEditorLabel, editor.GetInitializedDescription());
    }

    private class SubSelectedPathEditor : SelectedPathEditor
    {
        public string GetInitializedDescription()
        {
            FolderBrowser folderBrowser = new();
            InitializeDialog(folderBrowser);
            return folderBrowser.Description;
        }
    }
}
