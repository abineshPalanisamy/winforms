// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Moq;

namespace System.Windows.Forms.Design.Editors.Tests;

public class TreeNodeCollectionEditorTests
{
    [WinFormsFact]
    public void TreeNodeCollectionEditor_Constructor_InitializesProperties()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor editor = new(type);
        editor.Should().NotBeNull();
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_Property_HelpTopic()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        string helpTopic = collectionEditor.TestAccessor.Dynamic.HelpTopic;
        helpTopic.Should().Be("net.ComponentModel.TreeNodeCollectionEditor");
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_CreateCollectionForm_returnExpectedValue()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form colletionForm;
        using (new NoAssertContext())
        {
            colletionForm = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        colletionForm.Should().NotBeNull();
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_CollectionType_ReturnsExpected()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor editor = new(type);
        Type collectionType = editor.TestAccessor.Dynamic.CollectionType;
        collectionType.Should().Be(type);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_GetEditStyle_ReturnsModal()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor editor = new(type);
        editor.GetEditStyle().Should().Be(UITypeEditorEditStyle.Modal);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_CreateCollectionForm_ReturnsExpectedType()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor editor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = editor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        form.Should().NotBeNull();
        form.GetType().Name.Should().Be("TreeNodeCollectionForm");
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_EditValue_NullProvider_ReturnsValue()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor editor = new(type);
        object value = Array.Empty<TreeNode>();

#nullable enable
        object? result = editor.EditValue(context: null, provider: null!, value: value);
#nullable disable

        result.Should().BeSameAs(value);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_EditValue_ServiceProviderWithoutEditorService_ReturnsValue()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor editor = new(type);
        Mock<IServiceProvider> mockServiceProvider = new();
        object value = Array.Empty<TreeNode>();

#nullable enable
        object? result = editor.EditValue(context: null, provider: mockServiceProvider.Object, value: value);
#nullable disable

        result.Should().BeSameAs(value);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_CollectionForm_ButtonsInitialized()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }
        dynamic formAccessor = form.TestAccessor.Dynamic;

        object okButton = formAccessor._okButton;
        object btnCancel = formAccessor._btnCancel;
        object btnAddChild = formAccessor._btnAddChild;
        object btnAddRoot = formAccessor._btnAddRoot;
        object btnDelete = formAccessor._btnDelete;
        object moveDownButton = formAccessor._moveDownButton;
        object moveUpButton = formAccessor._moveUpButton;
        object label1 = formAccessor._label1;
        object treeView1 = formAccessor._treeView1;
        object label2 = formAccessor._label2;
        object propertyGrid1 = formAccessor._propertyGrid1;
        object okCancelPanel = formAccessor._okCancelPanel;
        object nodeControlPanel = formAccessor._nodeControlPanel;
        object overarchingTableLayoutPanel = formAccessor._overarchingTableLayoutPanel;
        object navigationButtonsTableLayoutPanel = formAccessor._navigationButtonsTableLayoutPanel;

        okButton.Should().NotBeNull();
        btnCancel.Should().NotBeNull();
        btnAddChild.Should().NotBeNull();
        btnAddRoot.Should().NotBeNull();
        btnDelete.Should().NotBeNull();
        moveDownButton.Should().NotBeNull();
        moveUpButton.Should().NotBeNull();
        label1.Should().NotBeNull();
        treeView1.Should().NotBeNull();
        label2.Should().NotBeNull();
        propertyGrid1.Should().NotBeNull();
        okCancelPanel.Should().NotBeNull();
        nodeControlPanel.Should().NotBeNull();
        overarchingTableLayoutPanel.Should().NotBeNull();
        navigationButtonsTableLayoutPanel.Should().NotBeNull();
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_CollectionForm_AcceptButton_OK_CancelButton_Cancel()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }
        dynamic formAccessor = form.TestAccessor.Dynamic;

        IButtonControl okButtonControl = formAccessor._okButton;
        IButtonControl btnCancelControl = formAccessor._btnCancel;

        form.AcceptButton.Should().Be(okButtonControl);
        form.CancelButton.Should().Be(btnCancelControl);
        form.HelpButton.Should().BeTrue();
        form.MaximizeBox.Should().BeFalse();
        form.MinimizeBox.Should().BeFalse();
        form.ShowInTaskbar.Should().BeFalse();
        form.ShowIcon.Should().BeFalse();
        form.AutoScaleMode.Should().Be(AutoScaleMode.Font);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_DragEnter_WithTreeNodeData_SetsMoveEffect()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }
        dynamic formAccessor = form.TestAccessor.Dynamic;

        Mock<IDataObject> mockDataObject = new();
        mockDataObject.Setup(d => d.GetDataPresent(typeof(TreeNode))).Returns(true);

        DragEventArgs args = new(mockDataObject.Object, 0, 0, 0, DragDropEffects.None, DragDropEffects.None);
        formAccessor.treeView1_DragEnter(formAccessor._treeView1, args);

        args.Effect.Should().Be(DragDropEffects.Move);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_DragEnter_WithoutTreeNodeData_SetsNoneEffect()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }
        dynamic formAccessor = form.TestAccessor.Dynamic;

        Mock<IDataObject> mockDataObject = new();
        mockDataObject.Setup(d => d.GetDataPresent(typeof(TreeNode))).Returns(false);

        DragEventArgs args = new(mockDataObject.Object, 0, 0, 0, DragDropEffects.None, DragDropEffects.None);
        formAccessor.treeView1_DragEnter(formAccessor._treeView1, args);

        args.Effect.Should().Be(DragDropEffects.None);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_SetImageProps_WithImageList_CopiesProperties()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }
        dynamic formAccessor = form.TestAccessor.Dynamic;

        using ImageList sourceList = new();
        sourceList.Images.Add(new Bitmap(16, 16));
        sourceList.Images.Add(new Bitmap(16, 16));

        using TreeView actualTreeView = new() { ImageList = sourceList, ImageIndex = 1, SelectedImageIndex = 0 };
        formAccessor.SetImageProps(actualTreeView);

        ImageList resultImageList = formAccessor._treeView1.ImageList;
        int resultImageIndex = formAccessor._treeView1.ImageIndex;
        int resultSelectedImageIndex = formAccessor._treeView1.SelectedImageIndex;

        resultImageList.Should().BeSameAs(sourceList);
        resultImageIndex.Should().Be(1);
        resultSelectedImageIndex.Should().Be(0);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_SetImageProps_NoImageList_ResetsToDefaults()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }
        dynamic formAccessor = form.TestAccessor.Dynamic;

        using TreeView actualTreeView = new();
        formAccessor.SetImageProps(actualTreeView);

        ImageList resultImageList = formAccessor._treeView1.ImageList;
        int resultImageIndex = formAccessor._treeView1.ImageIndex;
        int resultSelectedImageIndex = formAccessor._treeView1.SelectedImageIndex;

        resultImageList.Should().BeNull();
        resultImageIndex.Should().Be(-1);
        resultSelectedImageIndex.Should().Be(-1);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_SetImageProps_WithStateImageList_CopiesProperties()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }
        dynamic formAccessor = form.TestAccessor.Dynamic;

        using ImageList stateList = new();
        stateList.Images.Add(new Bitmap(16, 16));

        using TreeView actualTreeView = new() { StateImageList = stateList };
        formAccessor.SetImageProps(actualTreeView);

        ImageList resultStateImageList = formAccessor._treeView1.StateImageList;
        resultStateImageList.Should().BeSameAs(stateList);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_SetImageProps_NoStateImageList_NullStateImageList()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }
        dynamic formAccessor = form.TestAccessor.Dynamic;

        using TreeView actualTreeView = new();
        formAccessor.SetImageProps(actualTreeView);

        ImageList resultStateImageList = formAccessor._treeView1.StateImageList;
        resultStateImageList.Should().BeNull();
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_SetImageProps_CopiesCheckBoxes()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }
        dynamic formAccessor = form.TestAccessor.Dynamic;

        using TreeView actualTreeView = new() { CheckBoxes = true };
        formAccessor.SetImageProps(actualTreeView);

        bool resultCheckBoxes = formAccessor._treeView1.CheckBoxes;
        resultCheckBoxes.Should().BeTrue();
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_HelpButtonClicked_CancelsAndShowsHelp()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }
        dynamic formAccessor = form.TestAccessor.Dynamic;

        CancelEventArgs cancelArgs = new();
        formAccessor.TreeNodeCollectionEditor_HelpButtonClicked(form, cancelArgs);

        cancelArgs.Cancel.Should().BeTrue();
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_ResetMarginsOnTheForm_ExecutesSuccessfully()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }
        dynamic formAccessor = form.TestAccessor.Dynamic;

        // The constructor calls ResetMarginsOnTheForm(true, 1) which sets margins
        // on the controls. Just verify the method runs successfully via the constructor.
        // (Margin values are DPI-scaled, so we don't assert exact pixel values here.)
        int okRight = formAccessor._okButton.Margin.Right;
        int cancelLeft = formAccessor._btnCancel.Margin.Left;
        int addRootRight = formAccessor._btnAddRoot.Margin.Right;
        int addChildLeft = formAccessor._btnAddChild.Margin.Left;
        int deleteTop = formAccessor._btnDelete.Margin.Top;
        int moveDownBottom = formAccessor._moveDownButton.Margin.Bottom;
        int moveUpBottom = formAccessor._moveUpButton.Margin.Bottom;

        okRight.Should().BeGreaterThan(0);
        cancelLeft.Should().BeGreaterThan(0);
        addRootRight.Should().BeGreaterThan(0);
        addChildLeft.Should().BeGreaterThan(0);
        deleteTop.Should().BeGreaterThan(0);
        moveDownBottom.Should().BeGreaterThan(0);
        moveUpBottom.Should().BeGreaterThan(0);
    }
}
