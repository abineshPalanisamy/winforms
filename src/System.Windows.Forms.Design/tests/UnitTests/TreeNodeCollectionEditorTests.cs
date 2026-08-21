// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using Moq;

namespace System.Windows.Forms.Design.Editors.Tests;

public class TreeNodeCollectionEditorTests
{
    // CollectionEditor.Context has a private setter, which the dynamic TestAccessor cannot
    // reliably invoke. Set it directly via reflection against the declaring base type instead.
    private static void SetContext(TreeNodeCollectionEditor editor, ITypeDescriptorContext context)
    {
        PropertyInfo contextProperty = typeof(CollectionEditor).GetProperty("Context", BindingFlags.NonPublic | BindingFlags.Instance);
        contextProperty.SetValue(editor, context);
    }

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

    [WinFormsFact]
    public void TreeNodeCollectionEditor_TreeView_Property_ReturnsTreeViewFromContext()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        using TreeView actualTreeView = new();
        Mock<ITypeDescriptorContext> mockContext = new();
        mockContext.Setup(c => c.Instance).Returns(actualTreeView);
        SetContext(collectionEditor, mockContext.Object);

        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        TreeView result = formAccessor.TreeView;

        result.Should().BeSameAs(actualTreeView);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_TreeView_Property_NoContext_ReturnsNull()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);

        using (new NoAssertContext())
        {
            Form form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
            dynamic formAccessor = form.TestAccessor.Dynamic;

            TreeView result = formAccessor.TreeView;

            result.Should().BeNull();
        }
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_NextNode_WithDictionaryService_GetsAndSetsValue()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        using TreeView actualTreeView = new();

        object storedValue = null;
        Mock<IDictionaryService> mockDictionaryService = new();
        mockDictionaryService.Setup(d => d.GetValue(It.IsAny<object>())).Returns(() => storedValue);
        mockDictionaryService.Setup(d => d.SetValue(It.IsAny<object>(), It.IsAny<object>()))
            .Callback<object, object>((key, value) => storedValue = value);

        Mock<ISite> mockSite = new();
        mockSite.Setup(s => s.GetService(typeof(IDictionaryService))).Returns(mockDictionaryService.Object);
        actualTreeView.Site = mockSite.Object;

        Mock<ITypeDescriptorContext> mockContext = new();
        mockContext.Setup(c => c.Instance).Returns(actualTreeView);
        SetContext(collectionEditor, mockContext.Object);

        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        PropertyInfo nextNodeProperty = form.GetType().GetProperty("NextNode", BindingFlags.NonPublic | BindingFlags.Instance);

        int firstRead = (int)nextNodeProperty.GetValue(form);
        firstRead.Should().Be(0);

        nextNodeProperty.SetValue(form, 5);
        int secondRead = (int)nextNodeProperty.GetValue(form);

        secondRead.Should().Be(5);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_Add_RootNode_AddsAndSelectsNode()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        using (new NoAssertContext())
        {
            formAccessor.Add(null);
        }

        TreeView treeView = formAccessor._treeView1;

        treeView.Nodes.Count.Should().Be(1);
        treeView.SelectedNode.Should().Be(treeView.Nodes[0]);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_Add_ChildNode_AddsNodeToParentAndExpands()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        TreeView treeView = formAccessor._treeView1;
        TreeNode parent = treeView.Nodes.Add("Parent");

        using (new NoAssertContext())
        {
            formAccessor.Add(parent);
        }

        parent.Nodes.Count.Should().Be(1);
        treeView.SelectedNode.Should().Be(parent);
        parent.IsExpanded.Should().BeTrue();
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_LastNode_ReturnsDeepestLastNode()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        TreeView treeView = formAccessor._treeView1;
        treeView.Nodes.Add("Root0");
        TreeNode root1 = treeView.Nodes.Add("Root1");
        TreeNode child = root1.Nodes.Add("Child");

        TreeNode result = formAccessor.LastNode;

        result.Should().Be(child);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_PropertyGrid_PropertyValueChanged_UpdatesLabel()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        TreeView treeView = formAccessor._treeView1;
        TreeNode node = treeView.Nodes.Add("NodeName");
        treeView.SelectedNode = node;

        formAccessor.PropertyGrid_propertyValueChanged(formAccessor._propertyGrid1, new PropertyValueChangedEventArgs(null, null));

        string label = formAccessor._label2.Text;

        label.Should().Contain("NodeName");
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_SetNodeProps_WithNode_UpdatesLabelAndPropertyGrid()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        TreeView treeView = formAccessor._treeView1;
        TreeNode node = treeView.Nodes.Add("MyNode");
        node.Name = "MyNode";

        formAccessor.SetNodeProps(node);

        object selectedObject = formAccessor._propertyGrid1.SelectedObject;
        string label = formAccessor._label2.Text;

        selectedObject.Should().Be(node);
        label.Should().Contain("MyNode");
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_SetNodeProps_WithNullNode_ResetsPropertyGrid()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        formAccessor.SetNodeProps(null);

        object selectedObject = formAccessor._propertyGrid1.SelectedObject;

        selectedObject.Should().BeNull();
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_TreeView1AfterSelect_SetsCurNodeAndButtonsState()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        TreeView treeView = formAccessor._treeView1;
        TreeNode node = treeView.Nodes.Add("Node");

        formAccessor.treeView1_afterSelect(treeView, new TreeViewEventArgs(node));

        object curNode = formAccessor._curNode;
        bool deleteEnabled = formAccessor._btnDelete.Enabled;

        curNode.Should().Be(node);
        deleteEnabled.Should().BeTrue();
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_BtnAddRootClick_AddsRootNodeAndEnablesButtons()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        using (new NoAssertContext())
        {
            formAccessor.BtnAddRoot_click(formAccessor._btnAddRoot, EventArgs.Empty);
        }

        TreeView treeView = formAccessor._treeView1;
        bool deleteEnabled = formAccessor._btnDelete.Enabled;

        treeView.Nodes.Count.Should().Be(1);
        deleteEnabled.Should().BeTrue();
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_BtnAddChildClick_AddsChildToCurrentNode()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        TreeView treeView = formAccessor._treeView1;
        TreeNode parent = treeView.Nodes.Add("Parent");
        formAccessor._curNode = parent;

        using (new NoAssertContext())
        {
            formAccessor.BtnAddChild_click(formAccessor._btnAddChild, EventArgs.Empty);
        }

        parent.Nodes.Count.Should().Be(1);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_BtnDeleteClick_RemovesNodeAndResetsWhenEmpty()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        TreeView treeView = formAccessor._treeView1;
        TreeNode node = treeView.Nodes.Add("Node");
        formAccessor._curNode = node;

        formAccessor.BtnDelete_click(formAccessor._btnDelete, EventArgs.Empty);

        object curNode = formAccessor._curNode;

        treeView.Nodes.Count.Should().Be(0);
        curNode.Should().BeNull();
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_BtnOKClick_SetsTreeViewFieldToNull()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        TreeView treeView = formAccessor._treeView1;
        treeView.Nodes.Add("NodeA");
        treeView.Nodes.Add("NodeB");

        formAccessor.BtnOK_click(formAccessor._okButton, EventArgs.Empty);

        object treeViewAfter = formAccessor._treeView1;

        treeViewAfter.Should().BeNull();
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_MoveDownButtonClick_MovesRootNodeUnderNextSibling()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        TreeView treeView = formAccessor._treeView1;
        TreeNode root0 = treeView.Nodes.Add("Root0");
        TreeNode root1 = treeView.Nodes.Add("Root1");
        formAccessor._curNode = root0;

        formAccessor.moveDownButton_Click(formAccessor._moveDownButton, EventArgs.Empty);

        treeView.Nodes.Count.Should().Be(1);
        treeView.Nodes[0].Should().Be(root1);
        root1.Nodes.Count.Should().Be(1);
        root1.Nodes[0].Should().Be(root0);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_MoveUpButtonClick_MovesRootNodeUnderPreviousSibling()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        TreeView treeView = formAccessor._treeView1;
        TreeNode root0 = treeView.Nodes.Add("Root0");
        TreeNode root1 = treeView.Nodes.Add("Root1");
        formAccessor._curNode = root1;

        formAccessor.moveUpButton_Click(formAccessor._moveUpButton, EventArgs.Empty);

        treeView.Nodes.Count.Should().Be(1);
        treeView.Nodes[0].Should().Be(root0);
        root0.Nodes.Count.Should().Be(1);
        root0.Nodes[0].Should().Be(root1);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_SetButtonsState_TogglesButtonsBasedOnNodes()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        formAccessor.SetButtonsState();

        bool addChildEnabledEmpty = formAccessor._btnAddChild.Enabled;
        addChildEnabledEmpty.Should().BeFalse();

        TreeView treeView = formAccessor._treeView1;
        TreeNode node = treeView.Nodes.Add("Node");
        formAccessor._curNode = node;

        formAccessor.SetButtonsState();

        bool addChildEnabled = formAccessor._btnAddChild.Enabled;
        bool deleteEnabled = formAccessor._btnDelete.Enabled;
        bool moveUpEnabled = formAccessor._moveUpButton.Enabled;

        addChildEnabled.Should().BeTrue();
        deleteEnabled.Should().BeTrue();
        moveUpEnabled.Should().BeFalse();
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_BtnCancelClick_RestoresInitialNextNode()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        PropertyInfo nextNodeProperty = form.GetType().GetProperty("NextNode", BindingFlags.NonPublic | BindingFlags.Instance);

        int nextNode;
        using (new NoAssertContext())
        {
            nextNodeProperty.SetValue(form, 10);

            dynamic formAccessor = form.TestAccessor.Dynamic;
            formAccessor.BtnCancel_click(formAccessor._btnCancel, EventArgs.Empty);

            nextNode = (int)nextNodeProperty.GetValue(form);
        }

        nextNode.Should().Be(0);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_OnEditValueChanged_PopulatesTreeViewWithClonedNodes()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        TreeNode sourceNode1 = new("Source1");
        TreeNode sourceNode2 = new("Source2");

        PropertyInfo editValueProperty = form.GetType().GetProperty("EditValue", BindingFlags.Public | BindingFlags.Instance);
        using (new NoAssertContext())
        {
            editValueProperty.SetValue(form, new object[] { sourceNode1, sourceNode2 });
        }

        TreeView treeView = formAccessor._treeView1;

        treeView.Nodes.Count.Should().Be(2);
        treeView.Nodes[0].Text.Should().Be("Source1");
        treeView.Nodes[0].Should().NotBeSameAs(sourceNode1);
        treeView.SelectedNode.Should().Be(treeView.Nodes[0]);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_NextNode_TreeViewWithoutSite_ReturnsDefaultZero()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        using TreeView actualTreeView = new();

        Mock<ITypeDescriptorContext> mockContext = new();
        mockContext.Setup(c => c.Instance).Returns(actualTreeView);
        SetContext(collectionEditor, mockContext.Object);

        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        PropertyInfo nextNodeProperty = form.GetType().GetProperty("NextNode", BindingFlags.NonPublic | BindingFlags.Instance);

        int nextNode = (int)nextNodeProperty.GetValue(form);

        nextNode.Should().Be(0);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_BtnCancelClick_NoChange_WhenNextNodeAlreadyMatchesInitial()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        PropertyInfo nextNodeProperty = form.GetType().GetProperty("NextNode", BindingFlags.NonPublic | BindingFlags.Instance);

        int nextNode;
        using (new NoAssertContext())
        {
            dynamic formAccessor = form.TestAccessor.Dynamic;
            formAccessor.BtnCancel_click(formAccessor._btnCancel, EventArgs.Empty);

            nextNode = (int)nextNodeProperty.GetValue(form);
        }

        nextNode.Should().Be(0);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_SetButtonsState_MoveDownEnabled_WhenCurNodeIsNotLastNode()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        TreeView treeView = formAccessor._treeView1;
        TreeNode root0 = treeView.Nodes.Add("Root0");
        treeView.Nodes.Add("Root1");
        formAccessor._curNode = root0;

        formAccessor.SetButtonsState();

        bool moveDownEnabled = formAccessor._moveDownButton.Enabled;
        bool moveUpEnabled = formAccessor._moveUpButton.Enabled;

        moveDownEnabled.Should().BeTrue();
        moveUpEnabled.Should().BeFalse();
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_TreeView1DragOver_NoNodeAtPosition_SetsSelectedNodeNull()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        TreeView treeView = formAccessor._treeView1;
        TreeNode node = treeView.Nodes.Add("Node");
        treeView.SelectedNode = node;

        Point screenPoint = treeView.PointToScreen(new Point(-1, -2));

        Mock<IDataObject> mockDataObject = new();
        DragEventArgs args = new(mockDataObject.Object, 0, screenPoint.X, screenPoint.Y, DragDropEffects.None, DragDropEffects.None);

        formAccessor.treeView1_DragOver(treeView, args);

        object selectedNode = treeView.SelectedNode;

        selectedNode.Should().BeNull();
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_TreeView1DragOver_NodeAtPosition_SetsSelectedNode()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        TreeView treeView = formAccessor._treeView1;
        TreeNode node = treeView.Nodes.Add("Node");

        Point screenPoint = treeView.PointToScreen(new Point(0, 0));

        Mock<IDataObject> mockDataObject = new();
        DragEventArgs args = new(mockDataObject.Object, 0, screenPoint.X, screenPoint.Y, DragDropEffects.None, DragDropEffects.None);

        formAccessor.treeView1_DragOver(treeView, args);

        TreeNode selectedNode = treeView.SelectedNode;

        selectedNode.Should().Be(node);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_TreeView1DragDrop_NoDropTarget_MovesDraggedNodeToRoot()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        TreeView treeView = formAccessor._treeView1;
        TreeNode root0 = treeView.Nodes.Add("Root0");
        TreeNode childNode = root0.Nodes.Add("Child0");

        Point screenPoint = treeView.PointToScreen(new Point(-1, -2));

        Mock<IDataObject> mockDataObject = new();
        mockDataObject.Setup(d => d.GetData(typeof(TreeNode))).Returns(childNode);

        DragEventArgs args = new(mockDataObject.Object, 0, screenPoint.X, screenPoint.Y, DragDropEffects.None, DragDropEffects.Move);

        formAccessor.treeView1_DragDrop(treeView, args);

        root0.Nodes.Count.Should().Be(0);
        treeView.Nodes.Count.Should().Be(2);
        treeView.Nodes[1].Should().Be(childNode);
    }

    [WinFormsFact]
    public void TreeNodeCollectionEditor_TreeView1DragDrop_ValidDropTarget_AddsDraggedNodeAsChild()
    {
        Type type = typeof(TreeNode);
        TreeNodeCollectionEditor collectionEditor = new(type);
        Form form;
        using (new NoAssertContext())
        {
            form = collectionEditor.TestAccessor.Dynamic.CreateCollectionForm();
        }

        dynamic formAccessor = form.TestAccessor.Dynamic;

        TreeView treeView = formAccessor._treeView1;
        TreeNode root0 = treeView.Nodes.Add("Root0");
        TreeNode root1 = treeView.Nodes.Add("Root1");

        Point screenPoint = treeView.PointToScreen(new Point(0, 0));

        Mock<IDataObject> mockDataObject = new();
        mockDataObject.Setup(d => d.GetData(typeof(TreeNode))).Returns(root1);

        DragEventArgs args = new(mockDataObject.Object, 0, screenPoint.X, screenPoint.Y, DragDropEffects.None, DragDropEffects.Move);

        formAccessor.treeView1_DragDrop(treeView, args);

        treeView.Nodes.Count.Should().Be(1);
        treeView.Nodes[0].Should().Be(root0);
        root0.Nodes.Count.Should().Be(1);
        root0.Nodes[0].Should().Be(root1);
    }
}
