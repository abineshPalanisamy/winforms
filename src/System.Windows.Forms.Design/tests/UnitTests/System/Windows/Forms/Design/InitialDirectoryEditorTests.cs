// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.TestUtilities;

namespace System.Windows.Forms.Design.Tests;

public class InitialDirectoryEditorTests
{
    [Fact]
    public void InitialDirectoryEditor_Ctor_Default()
    {
        InitialDirectoryEditor editor = new();
        Assert.False(editor.IsDropDownResizable);
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void InitialDirectoryEditor_GetEditStyle_Invoke_ReturnsModal(ITypeDescriptorContext context)
    {
        InitialDirectoryEditor editor = new();
        Assert.Equal(UITypeEditorEditStyle.Modal, editor.GetEditStyle(context));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void InitialDirectoryEditor_GetPaintValueSupported_Invoke_ReturnsFalse(ITypeDescriptorContext context)
    {
        InitialDirectoryEditor editor = new();
        Assert.False(editor.GetPaintValueSupported(context));
    }

    [Fact]
    public void InitialDirectoryEditor_InitializeDialog_Invoke_SetsDescription()
    {
        SubInitialDirectoryEditor editor = new();
        Assert.Equal(SR.InitialDirectoryEditorLabel, editor.GetInitializedDescription());
    }

    private class SubInitialDirectoryEditor : InitialDirectoryEditor
    {
        public string GetInitializedDescription()
        {
            FolderBrowser folderBrowser = new();
            InitializeDialog(folderBrowser);
            return folderBrowser.Description;
        }
    }
}
