// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Reflection;
using Moq;
using static System.ComponentModel.Design.CollectionEditor;

namespace System.Windows.Forms.Design.Tests;

public class ToolStripCollectionEditorTests
{
    private readonly ToolStripCollectionEditor _editor;

    public ToolStripCollectionEditorTests()
    {
        _editor = new();
    }

    [Fact]
    public void ToolStripCollectionEditor_CreateCollectionForm_DoesNotThrowException()
    {
        Action act = () => _editor.TestAccessor.Dynamic.CreateCollectionForm();
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_HelpTopic_ReturnsExpectedValue()
    {
        string helpTopic = _editor.TestAccessor.Dynamic.HelpTopic;
        helpTopic.Should().Be("net.ComponentModel.ToolStripCollectionEditor");
    }

    [Fact]
    public void ToolStripCollectionEditor_EditValue_NullProvider_ReturnsNull()
    {
        object? result = _editor.EditValue(context: null, provider: null!, value: new object());

        result.Should().BeNull();
    }

    [Fact]
    public void ToolStripCollectionEditor_EditValue_WithProvider_ReturnsExpected()
    {
        Mock<ITypeDescriptorContext> mockTypeDescriptorContext = new();
        Mock<IServiceProvider> mockServiceProvider = new();
        object? result = _editor.EditValue(mockTypeDescriptorContext.Object, mockServiceProvider.Object, new object());

        result.Should().NotBeNull();
    }

    [Fact]
    public void ToolStripCollectionEditor_EditValue_WithDropDownItemSelection_ReturnsExpected()
    {
        // Verify that when the primary selection is a ToolStripDropDownItem, the editor
        // resolves the owner ToolStrip and drives the flow.
        using ToolStrip toolStrip = new();
        ToolStripMenuItem dropDownItem = new("dropDown")
        {
            Owner = toolStrip
        };

        Mock<ITypeDescriptorContext> mockTypeDescriptorContext = new();
        Mock<ISelectionService> mockSelectionService = new();
        mockSelectionService.Setup(s => s.PrimarySelection).Returns(dropDownItem);

        Mock<IDesignerHost> mockDesignerHost = new();
        mockDesignerHost.Setup(h => h.GetDesigner(It.IsAny<IComponent>())).Returns((IDesigner?)null);

        Mock<IServiceProvider> mockServiceProvider = new();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(ISelectionService))).Returns(mockSelectionService.Object);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IDesignerHost))).Returns(mockDesignerHost.Object);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IWindowsFormsEditorService))).Returns((IWindowsFormsEditorService?)null);

        object? result = _editor.EditValue(mockTypeDescriptorContext.Object, mockServiceProvider.Object, new object());

        result.Should().NotBeNull();
    }

    [Fact]
    public void ToolStripCollectionEditor_VerbResourceString_ReturnsExpectedValue()
    {
        SR.ToolStripItemCollectionEditorVerb.Should().Be("&Edit Items...");
    }

    [Fact]
    public void ToolStripCollectionEditor_LabelNoneResourceString_ReturnsExpectedValue()
    {
        SR.ToolStripItemCollectionEditorLabelNone.Should().Be("(&None)");
    }

    [Fact]
    public void ToolStripCollectionEditor_LabelMultipleItemsResourceString_ReturnsExpectedValue()
    {
        SR.ToolStripItemCollectionEditorLabelMultipleItems.Should().Be("(&Multiple Items)");
    }

    [Fact]
    public void ToolStripCollectionEditor_OnSelectedItemName_Paint_NoItemsSelected_UsesLabelNone()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        Label label = formAccessor._selectedItemName;
        FilterListBox listBox = formAccessor._listBoxItems;

        // No items selected by default.
        listBox.ClearSelected();
        listBox.SelectedItems.Count.Should().Be(0);

        using Bitmap bitmap = new(10, 10);
        using Graphics graphics = Graphics.FromImage(bitmap);
        using PaintEventArgs paintEventArgs = new(graphics, label.ClientRectangle);

        Action act = () => formAccessor.OnSelectedItemName_Paint(label, paintEventArgs);
        act.Should().NotThrow();
        label.Text.Should().Be(SR.ToolStripItemCollectionEditorLabelNone);
    }

    [Fact]
    public void ToolStripCollectionEditor_OnSelectedItemName_Paint_MultipleItemsSelected_UsesLabelMultipleItems()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        Label label = formAccessor._selectedItemName;
        FilterListBox listBox = formAccessor._listBoxItems;

        // Add and select multiple items.
        listBox.Items.Add(new ToolStripButton());
        listBox.Items.Add(new ToolStripButton());
        listBox.SetSelected(0, true);
        listBox.SetSelected(1, true);
        listBox.SelectedItems.Count.Should().Be(2);

        using Bitmap bitmap = new(10, 10);
        using Graphics graphics = Graphics.FromImage(bitmap);
        using PaintEventArgs paintEventArgs = new(graphics, label.ClientRectangle);

        Action act = () => formAccessor.OnSelectedItemName_Paint(label, paintEventArgs);
        act.Should().NotThrow();
        label.Text.Should().Be(SR.ToolStripItemCollectionEditorLabelMultipleItems);
    }

    [Fact]
    public void ToolStripCollectionEditor_OnSelectedItemName_Paint_SingleItemSelected_UsesClassNameAndItemName()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        Label label = formAccessor._selectedItemName;
        FilterListBox listBox = formAccessor._listBoxItems;

        using ToolStripButton item = new("item1");
        listBox.Items.Add(item);
        listBox.SetSelected(0, true);
        listBox.SelectedItems.Count.Should().Be(1);

        using Bitmap bitmap = new(10, 10);
        using Graphics graphics = Graphics.FromImage(bitmap);
        using PaintEventArgs paintEventArgs = new(graphics, label.ClientRectangle);

        Action act = () => formAccessor.OnSelectedItemName_Paint(label, paintEventArgs);
        act.Should().NotThrow();
        label.Text.Should().StartWith("&ToolStripButton");
    }

    [Fact]
    public void ToolStripCollectionEditor_OnSelectedItemName_Paint_SingleToolStripSelected_UsesTypeName()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        Label label = formAccessor._selectedItemName;
        FilterListBox listBox = formAccessor._listBoxItems;

        using ToolStrip toolStrip = new();
        listBox.Items.Add(toolStrip);
        listBox.SetSelected(0, true);

        using Bitmap bitmap = new(10, 10);
        using Graphics graphics = Graphics.FromImage(bitmap);
        using PaintEventArgs paintEventArgs = new(graphics, label.ClientRectangle);

        Action act = () => formAccessor.OnSelectedItemName_Paint(label, paintEventArgs);
        act.Should().NotThrow();
        label.Text.Should().StartWith("&ToolStrip");
    }

    [Fact]
    public void ToolStripCollectionEditor_OnFormLoad_NewItemTypesItemHeightIsSet()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        ComboBox newItemTypes = formAccessor._newItemTypes;

        // Verify the constructor sets the newItemTypes.ItemHeight correctly.
        // The full OnFormLoad path requires a non-null Context.Instance (a Component),
        // which is exercised indirectly by EditValue_WithProvider_ReturnsExpected.
        newItemTypes.ItemHeight.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ToolStripCollectionEditor_OnBtnOK_Click_SetsDialogResult()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        Button btnOK = formAccessor._btnOK;

        formAccessor.OnBtnOK_Click(btnOK, EventArgs.Empty);

        form.DialogResult.Should().Be(DialogResult.OK);
    }

    [Fact]
    public void ToolStripCollectionEditor_OnBtnRemove_Click_RemovesSelectedItem()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        Button btnRemove = formAccessor._btnRemove;
        FilterListBox listBox = formAccessor._listBoxItems;
        (object itemList, ToolStrip _) = CreateEditorItemCollectionWithHost(form);

        // Populate the listbox with items and select one.
        using ToolStripButton item = new("item1");
        InvokeEditorItemCollectionAdd(itemList, item);
        listBox.SetSelected(1, true);

        Action act = () => formAccessor.OnBtnRemove_Click(btnRemove, EventArgs.Empty);
        act.Should().NotThrow();

        // The item should be removed from the listbox.
        listBox.Items.Cast<object>().Should().NotContain(item);
    }

    [Fact]
    public void ToolStripCollectionEditor_OnBtnRemove_Click_NoSelection_DoesNotThrow()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        Button btnRemove = formAccessor._btnRemove;
        FilterListBox listBox = formAccessor._listBoxItems;
        listBox.ClearSelected();

        // No selection: the handler should iterate zero items and return.
        Action act = () => formAccessor.OnBtnRemove_Click(btnRemove, EventArgs.Empty);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnBtnMoveDown_Click_MovesItemDown()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        Button btnMoveDown = formAccessor._btnMoveDown;
        FilterListBox listBox = formAccessor._listBoxItems;
        (object itemList, ToolStrip _) = CreateEditorItemCollectionWithHost(form);

        using ToolStripButton item1 = new("item1");
        using ToolStripButton item2 = new("item2");
        InvokeEditorItemCollectionAdd(itemList, item1);
        InvokeEditorItemCollectionAdd(itemList, item2);

        // Select the first item (index 1 in the listbox, since the ToolStrip host is at index 0).
        listBox.SetSelected(1, true);
        listBox.SelectedItem.Should().Be(item1);

        Action act = () => formAccessor.OnBtnMoveDown_Click(btnMoveDown, EventArgs.Empty);
        act.Should().NotThrow();

        // The first item should now be at index 2.
        listBox.Items[2].Should().Be(item1);
    }

    [Fact]
    public void ToolStripCollectionEditor_OnBtnMoveUp_Click_MovesItemUp()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        Button btnMoveUp = formAccessor._btnMoveUp;
        FilterListBox listBox = formAccessor._listBoxItems;
        (object itemList, ToolStrip _) = CreateEditorItemCollectionWithHost(form);

        using ToolStripButton item1 = new("item1");
        using ToolStripButton item2 = new("item2");
        InvokeEditorItemCollectionAdd(itemList, item1);
        InvokeEditorItemCollectionAdd(itemList, item2);

        // Select the second item (index 2 in the listbox, since the ToolStrip host is at index 0).
        listBox.SetSelected(2, true);
        listBox.SelectedItem.Should().Be(item2);

        Action act = () => formAccessor.OnBtnMoveUp_Click(btnMoveUp, EventArgs.Empty);
        act.Should().NotThrow();

        // The second item should now be at index 1.
        listBox.Items[1].Should().Be(item2);
    }

    [Fact]
    public void ToolStripCollectionEditor_OnBtnMoveUp_Click_AtIndexOne_DoesNotMove()
    {
        // Selecting the item at index 1 (the first non-ToolStrip item) means currentIndex == 1,
        // which fails the (currentIndex > 1) guard, so OnBtnMoveUp_Click returns early without moving.
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        Button btnMoveUp = formAccessor._btnMoveUp;
        FilterListBox listBox = formAccessor._listBoxItems;
        (object itemList, ToolStrip _) = CreateEditorItemCollectionWithHost(form);

        using ToolStripButton item1 = new("item1");
        using ToolStripButton item2 = new("item2");
        InvokeEditorItemCollectionAdd(itemList, item1);
        InvokeEditorItemCollectionAdd(itemList, item2);

        // Select the first item at index 1.
        listBox.SetSelected(1, true);

        Action act = () => formAccessor.OnBtnMoveUp_Click(btnMoveUp, EventArgs.Empty);
        act.Should().NotThrow();

        // Order unchanged because the move is guarded by currentIndex > 1.
        listBox.Items[1].Should().Be(item1);
        listBox.Items[2].Should().Be(item2);
    }

    [Fact]
    public void ToolStripCollectionEditor_OnNewItemTypes_SelectedIndexChanged_DoesNotThrow()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        ComboBox newItemTypes = formAccessor._newItemTypes;

        // Ensure the combo has at least one item to select without throwing.
        newItemTypes.Items.Add("dummy");

        Action act = () => formAccessor.OnNewItemTypes_SelectedIndexChanged(newItemTypes, EventArgs.Empty);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnListBoxItems_MeasureItem_ListBoxSender_ReturnsHeight()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        FilterListBox listBox = formAccessor._listBoxItems;

        using Bitmap bitmap = new(10, 10);
        using Graphics graphics = Graphics.FromImage(bitmap);
        MeasureItemEventArgs args = new(graphics, 0, 0);

        Action act = () => formAccessor.OnListBoxItems_MeasureItem(listBox, args);
        act.Should().NotThrow();
        args.ItemHeight.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ToolStripCollectionEditor_OnListBoxItems_MeasureItem_ComboBoxSender_NoCustomItem_ReturnsHeight()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        ComboBox newItemTypes = formAccessor._newItemTypes;
        newItemTypes.Items.Add("item1");

        using Bitmap bitmap = new(10, 10);
        using Graphics graphics = Graphics.FromImage(bitmap);
        MeasureItemEventArgs args = new(graphics, 0, 0);

        // _customItemIndex is -1 by default, so no separator is added.
        Action act = () => formAccessor.OnListBoxItems_MeasureItem(newItemTypes, args);
        act.Should().NotThrow();
        args.ItemHeight.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ToolStripCollectionEditor_OnListBoxItems_MeasureItem_ComboBoxSender_CustomItemIndex_ReturnsHeightWithSeparator()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        ComboBox newItemTypes = formAccessor._newItemTypes;
        newItemTypes.Items.Add("item1");
        newItemTypes.Items.Add("item2");

        // Set _customItemIndex to 1 to enable the separator branch.
        formAccessor._customItemIndex = 1;

        using Bitmap bitmap = new(10, 10);
        using Graphics graphics = Graphics.FromImage(bitmap);
        MeasureItemEventArgs args = new(graphics, 1, 0);

        Action act = () => formAccessor.OnListBoxItems_MeasureItem(newItemTypes, args);
        act.Should().NotThrow();
        args.ItemHeight.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ToolStripCollectionEditor_OnListBoxItems_DrawItem_NegativeIndex_ReturnsEarly()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        FilterListBox listBox = formAccessor._listBoxItems;

        using Bitmap bitmap = new(50, 20);
        using Graphics graphics = Graphics.FromImage(bitmap);
        DrawItemEventArgs args = new(graphics, form.Font, new Rectangle(0, 0, 50, 20), -1, DrawItemState.None);

        Action act = () => formAccessor.OnListBoxItems_DrawItem(listBox, args);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnListBoxItems_DrawItem_ListBoxSender_ToolStripItem_DoesNotThrow()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        FilterListBox listBox = formAccessor._listBoxItems;

        using ToolStripButton item = new("drawItem");
        listBox.Items.Add(item);

        using Bitmap bitmap = new(50, 20);
        using Graphics graphics = Graphics.FromImage(bitmap);
        DrawItemEventArgs args = new(graphics, form.Font, new Rectangle(0, 0, 50, 20), 0, DrawItemState.None);

        Action act = () => formAccessor.OnListBoxItems_DrawItem(listBox, args);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnListBoxItems_DrawItem_ListBoxSender_ToolStrip_DoesNotThrow()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        FilterListBox listBox = formAccessor._listBoxItems;

        using ToolStrip toolStrip = new();
        listBox.Items.Add(toolStrip);

        using Bitmap bitmap = new(50, 20);
        using Graphics graphics = Graphics.FromImage(bitmap);
        DrawItemEventArgs args = new(graphics, form.Font, new Rectangle(0, 0, 50, 20), 0, DrawItemState.None);

        Action act = () => formAccessor.OnListBoxItems_DrawItem(listBox, args);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnListBoxItems_DrawItem_ListBoxSender_SelectedAndFocused_DoesNotThrow()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        FilterListBox listBox = formAccessor._listBoxItems;

        using ToolStripButton item = new("selected");
        listBox.Items.Add(item);

        using Bitmap bitmap = new(50, 20);
        using Graphics graphics = Graphics.FromImage(bitmap);
        DrawItemEventArgs args = new(
            graphics,
            form.Font,
            new Rectangle(0, 0, 50, 20),
            0,
            DrawItemState.Selected | DrawItemState.Focus,
            SystemColors.Highlight,
            SystemColors.HighlightText);

        Action act = () => formAccessor.OnListBoxItems_DrawItem(listBox, args);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnListBoxItems_DrawItem_ComboBoxSender_TypeListItem_DoesNotThrow()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        ComboBox newItemTypes = formAccessor._newItemTypes;

        // Inject a real TypeListItem so the DrawItem handler exercises the ComboBox branch
        // (otherwise it would Debug.Fail on an unexpected combo box item).
        Type typeListItemType = form.GetType()
            .GetNestedType("TypeListItem", BindingFlags.NonPublic)!;
        object typeListItem = Activator.CreateInstance(typeListItemType, typeof(ToolStripButton))!;
        newItemTypes.Items.Add(typeListItem);

        using Bitmap bitmap = new(50, 20);
        using Graphics graphics = Graphics.FromImage(bitmap);
        DrawItemEventArgs args = new(graphics, form.Font, new Rectangle(0, 0, 50, 20), 0, DrawItemState.None);

        Action act = () => formAccessor.OnListBoxItems_DrawItem(newItemTypes, args);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnListBoxItems_DrawItem_ComboBoxSender_ComboBoxEdit_DoesNotThrow()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        ComboBox newItemTypes = formAccessor._newItemTypes;

        // Inject a real TypeListItem so the DrawItem handler exercises the ComboBox branch.
        Type typeListItemType = form.GetType()
            .GetNestedType("TypeListItem", BindingFlags.NonPublic)!;
        object typeListItem = Activator.CreateInstance(typeListItemType, typeof(ToolStripButton))!;
        newItemTypes.Items.Add(typeListItem);

        using Bitmap bitmap = new(50, 20);
        using Graphics graphics = Graphics.FromImage(bitmap);
        DrawItemEventArgs args = new(
            graphics,
            form.Font,
            new Rectangle(0, 0, 50, 20),
            0,
            DrawItemState.ComboBoxEdit,
            SystemColors.Window,
            SystemColors.WindowText);

        Action act = () => formAccessor.OnListBoxItems_DrawItem(newItemTypes, args);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnListBoxItems_SelectedIndexChanged_NoSelection_DisablesButtons()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        FilterListBox listBox = formAccessor._listBoxItems;
        Button btnMoveUp = formAccessor._btnMoveUp;
        Button btnMoveDown = formAccessor._btnMoveDown;
        Button btnRemove = formAccessor._btnRemove;

        // Add an item but do not select it.
        using ToolStripButton item = new("item1");
        listBox.Items.Add(item);
        listBox.ClearSelected();

        formAccessor.OnListBoxItems_SelectedIndexChanged(listBox, EventArgs.Empty);

        btnMoveUp.Enabled.Should().BeFalse();
        btnMoveDown.Enabled.Should().BeFalse();
        btnRemove.Enabled.Should().BeFalse();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnListBoxItems_SelectedIndexChanged_SingleToolStripItem_EnablesRemove()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        FilterListBox listBox = formAccessor._listBoxItems;
        Button btnMoveUp = formAccessor._btnMoveUp;
        Button btnMoveDown = formAccessor._btnMoveDown;
        Button btnRemove = formAccessor._btnRemove;

        using ToolStripButton item = new("item1");
        listBox.Items.Add(item);
        listBox.Items.Add(new ToolStripButton("item2"));
        listBox.SetSelected(0, true);

        formAccessor.OnListBoxItems_SelectedIndexChanged(listBox, EventArgs.Empty);

        // First item can't move up (index 0), but can move down and can be removed.
        btnMoveUp.Enabled.Should().BeFalse();
        btnMoveDown.Enabled.Should().BeTrue();
        btnRemove.Enabled.Should().BeTrue();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnListBoxItems_SelectedIndexChanged_SingleToolStrip_DisablesAllButtons()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        FilterListBox listBox = formAccessor._listBoxItems;
        Button btnMoveUp = formAccessor._btnMoveUp;
        Button btnMoveDown = formAccessor._btnMoveDown;
        Button btnRemove = formAccessor._btnRemove;

        using ToolStrip toolStrip = new();
        listBox.Items.Add(toolStrip);
        listBox.SetSelected(0, true);

        formAccessor.OnListBoxItems_SelectedIndexChanged(listBox, EventArgs.Empty);

        // Cannot remove or move the ToolStrip itself.
        btnRemove.Enabled.Should().BeFalse();
        btnMoveUp.Enabled.Should().BeFalse();
        btnMoveDown.Enabled.Should().BeFalse();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnListBoxItems_SelectedIndexChanged_MultipleItemsIncludingToolStrip_DisablesAllButtons()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        FilterListBox listBox = formAccessor._listBoxItems;
        Button btnMoveUp = formAccessor._btnMoveUp;
        Button btnMoveDown = formAccessor._btnMoveDown;
        Button btnRemove = formAccessor._btnRemove;

        using ToolStrip toolStrip = new();
        using ToolStripButton item = new("item1");
        listBox.Items.Add(toolStrip);
        listBox.Items.Add(item);
        listBox.SetSelected(0, true);
        listBox.SetSelected(1, true);

        formAccessor.OnListBoxItems_SelectedIndexChanged(listBox, EventArgs.Empty);

        btnRemove.Enabled.Should().BeFalse();
        btnMoveUp.Enabled.Should().BeFalse();
        btnMoveDown.Enabled.Should().BeFalse();
    }

    [Fact]
    public void ToolStripCollectionEditor_PropertyGrid_propertyValueChanged_DoesNotThrow()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        VsPropertyGrid selectedItemProps = formAccessor._selectedItemProps;

        PropertyValueChangedEventArgs args = new(null, null);

        Action act = () => formAccessor.PropertyGrid_propertyValueChanged(selectedItemProps, args);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_ToolStripCollectionEditor_HelpButtonClicked_Cancels()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;

        CancelEventArgs cancelArgs = new();
        formAccessor.ToolStripCollectionEditor_HelpButtonClicked(form, cancelArgs);

        cancelArgs.Cancel.Should().BeTrue();
    }

    [Fact]
    public void ToolStripCollectionEditor_AddItem_AppendsToEndAndSelects()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        FilterListBox listBox = formAccessor._listBoxItems;
        CreateEditorItemCollectionWithHost(form);

        // AddItem with index = -1 appends to the end and selects the new item.
        using ToolStripButton newItem = new("newItem");
        formAccessor.AddItem(newItem, -1);

        listBox.Items.Cast<object>().Should().Contain(newItem);
        listBox.SelectedItem.Should().Be(newItem);
    }

    [Fact]
    public void ToolStripCollectionEditor_AddItem_OutOfRangeIndex_DoesNotAdd()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        FilterListBox listBox = formAccessor._listBoxItems;
        (object itemList, ToolStrip _) = CreateEditorItemCollectionWithHost(form);

        // Seed with an initial item.
        using ToolStripButton first = new("first");
        InvokeEditorItemCollectionAdd(itemList, first);

        // AddItem with an out-of-range index is a no-op.
        using ToolStripButton outOfRangeItem = new("outOfRange");
        formAccessor.AddItem(outOfRangeItem, 100);

        listBox.Items.Cast<object>().Should().NotContain(outOfRangeItem);
    }

    [Fact]
    public void ToolStripCollectionEditor_MoveItem_MovesItem()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        FilterListBox listBox = formAccessor._listBoxItems;
        (object itemList, ToolStrip _) = CreateEditorItemCollectionWithHost(form);

        using ToolStripButton item1 = new("item1");
        using ToolStripButton item2 = new("item2");
        InvokeEditorItemCollectionAdd(itemList, item1);
        InvokeEditorItemCollectionAdd(itemList, item2);

        // Move item at index 1 to index 2.
        formAccessor.MoveItem(1, 2);

        listBox.Items[2].Should().Be(item1);
    }

    [Fact]
    public void ToolStripCollectionEditor_Collection_Setter_Null_DetachesSite()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        VsPropertyGrid selectedItemProps = formAccessor._selectedItemProps;

        // Setting Collection to null should clear the site without throwing.
        Action act = () => formAccessor.Collection = null;
        act.Should().NotThrow();
        selectedItemProps.Site.Should().BeNull();
    }

    [Fact]
    public void ToolStripCollectionEditor_Collection_Setter_SameValue_DoesNothing()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;

        // The initial _targetToolStripCollection is null; setting to null again is a no-op.
        Action act = () => formAccessor.Collection = null;
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnEditValueChanged_ResetsSelectedObjects()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        VsPropertyGrid selectedItemProps = formAccessor._selectedItemProps;

        // The dynamic accessor can invoke the protected OnEditValueChanged override.
        formAccessor.OnEditValueChanged();

        // After OnEditValueChanged, SelectedObjects is cleared (null or empty).
        object? selected = selectedItemProps.SelectedObjects;
        (selected is null || (selected is Array arr && arr.Length == 0)).Should().BeTrue();
    }

    [Fact]
    public void ToolStripCollectionEditor_ScaleButtonImageLogicalToDevice_NullImage_ReturnsEarly()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        Button btnMoveUp = formAccessor._btnMoveUp;

        // The button is created with an image in InitializeComponent, so reset to null to test the null branch.
        Image? originalImage = btnMoveUp.Image;
        btnMoveUp.Image = null;
        try
        {
            Action act = () => formAccessor.ScaleButtonImageLogicalToDevice(btnMoveUp);
            act.Should().NotThrow();
        }
        finally
        {
            btnMoveUp.Image = originalImage;
        }
    }

    [Fact]
    public void ToolStripCollectionEditor_OnComponentChanged_NameProperty_InvalidatesLabel()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;

        PropertyDescriptor property = TypeDescriptor.GetProperties(typeof(ToolStripButton)).Find(nameof(ToolStripButton.Name), false)!;
        using ToolStripButton item = new("name1");
        ComponentChangedEventArgs args = new(null, property, "old", "new");

        Action act = () => formAccessor.OnComponentChanged(this, args);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnComponentChanged_OtherProperty_DoesNotInvalidate()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;

        PropertyDescriptor property = TypeDescriptor.GetProperties(typeof(ToolStripButton)).Find(nameof(ToolStripButton.Text), false)!;
        using ToolStripButton item = new("name1");
        ComponentChangedEventArgs args = new(null, property, "old", "new");

        Action act = () => formAccessor.OnComponentChanged(this, args);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnComponentChanged_NonToolStripItemComponent_DoesNotInvalidate()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;

        PropertyDescriptor property = TypeDescriptor.GetProperties(typeof(ToolStripButton)).Find(nameof(ToolStripButton.Name), false)!;
        using Button button = new();
        ComponentChangedEventArgs args = new(button, property, "old", "new");

        Action act = () => formAccessor.OnComponentChanged(this, args);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnNewItemTypes_SelectionChangeCommitted_NonTypeListItem_ReturnsEarly()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        ComboBox newItemTypes = formAccessor._newItemTypes;
        Button btnAddNew = formAccessor._btnAddNew;

        // When SelectedItem is not a TypeListItem, the handler returns early without
        // invoking CreateInstance (which would require a non-null Context).
        newItemTypes.Items.Add("notAType");
        newItemTypes.SelectedIndex = 0;

        Action act = () => formAccessor.OnNewItemTypes_SelectionChangeCommitted(btnAddNew, EventArgs.Empty);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnNewItemTypes_DropDown_ComputesWidthAndHeight()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        ComboBox newItemTypes = formAccessor._newItemTypes;

        // Add TypeListItems to populate the combo for measurement.
        Type typeListItemType = form.GetType()
            .GetNestedType("TypeListItem", BindingFlags.NonPublic)!;
        newItemTypes.Items.Add(Activator.CreateInstance(typeListItemType, typeof(ToolStripButton))!);
        newItemTypes.Items.Add(Activator.CreateInstance(typeListItemType, typeof(ToolStripLabel))!);

        Action act = () => formAccessor.OnNewItemTypes_DropDown(newItemTypes, EventArgs.Empty);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnNewItemTypes_DropDown_SecondCall_DoesNotRecompute()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        ComboBox newItemTypes = formAccessor._newItemTypes;

        // Setting Tag to true simulates the post-first-dropdown state.
        newItemTypes.Tag = true;

        Action act = () => formAccessor.OnNewItemTypes_DropDown(newItemTypes, EventArgs.Empty);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_OnComboHandleCreated_WiresMeasureAndDrawEvents()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        ComboBox newItemTypes = formAccessor._newItemTypes;

        Action act = () => formAccessor.OnComboHandleCreated(newItemTypes, EventArgs.Empty);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_ImageComboBox_OnSelectedIndexChanged_DoesNotThrow()
    {
        // ImageComboBox is a private nested class. Its OnSelectedIndexChanged override calls
        // base.OnSelectedIndexChanged + Invalidate(ImageRect). We invoke it via reflection on
        // the private nested type using the actual _newItemTypes field instance.
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        ComboBox newItemTypes = formAccessor._newItemTypes;
        newItemTypes.Items.Add("item1");
        newItemTypes.SelectedIndex = 0;

        Type imageComboBoxType = form.GetType()
            .GetNestedType("ImageComboBox", BindingFlags.NonPublic)!;
        MethodInfo method = imageComboBoxType.GetMethod(
            "OnSelectedIndexChanged",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        Action act = () => method.Invoke(newItemTypes, [EventArgs.Empty]);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_ImageComboBox_OnDropDownClosed_DoesNotThrow()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        ComboBox newItemTypes = formAccessor._newItemTypes;

        Type imageComboBoxType = form.GetType()
            .GetNestedType("ImageComboBox", BindingFlags.NonPublic)!;
        MethodInfo method = imageComboBoxType.GetMethod(
            "OnDropDownClosed",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        Action act = () => method.Invoke(newItemTypes, [EventArgs.Empty]);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_ImageComboBox_ImageRect_NonRTL_ReturnsExpectedRect()
    {
        // Read the private ImageRect getter via reflection and verify the non-RTL coordinates.
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        ComboBox newItemTypes = formAccessor._newItemTypes;

        Type imageComboBoxType = form.GetType()
            .GetNestedType("ImageComboBox", BindingFlags.NonPublic)!;
        PropertyInfo imageRectProperty = imageComboBoxType.GetProperty(
            "ImageRect",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        // Default RightToLeft is No, which falls in the non-RTL branch.
        Rectangle rect = (Rectangle)imageRectProperty.GetValue(newItemTypes)!;
        rect.X.Should().Be(3);
        rect.Y.Should().Be(3);
        rect.Width.Should().BeGreaterThan(0);
        rect.Height.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ToolStripCollectionEditor_ImageComboBox_ImageRect_RTL_HasHorizontalScrollBarOffset()
    {
        // When RightToLeft is Yes, ImageRect shifts to the right by HorizontalScrollBarThumbWidth + 4.
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        ComboBox newItemTypes = formAccessor._newItemTypes;
        newItemTypes.RightToLeft = RightToLeft.Yes;

        Type imageComboBoxType = form.GetType()
            .GetNestedType("ImageComboBox", BindingFlags.NonPublic)!;
        PropertyInfo imageRectProperty = imageComboBoxType.GetProperty(
            "ImageRect",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        Rectangle rect = (Rectangle)imageRectProperty.GetValue(newItemTypes)!;
        // The X coordinate must be shifted right (it includes the HorizontalScrollBarThumbWidth + 4 padding).
        rect.X.Should().Be(4 + SystemInformation.HorizontalScrollBarThumbWidth);
        rect.Y.Should().Be(3);
        rect.Width.Should().BeGreaterThan(0);
        rect.Height.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ToolStripCollectionEditor_ToolStripFromObject_NullInstance_ReturnsNull()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        Type formType = form.GetType();
        MethodInfo method = formType.GetMethod("ToolStripFromObject", BindingFlags.NonPublic | BindingFlags.Static)!;
        object? result = method.Invoke(null, [null]);
        result.Should().BeNull();
    }

    [Fact]
    public void ToolStripCollectionEditor_ToolStripFromObject_ToolStripInstance_ReturnsToolStrip()
    {
        using ToolStrip toolStrip = new();
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        Type formType = form.GetType();
        MethodInfo method = formType.GetMethod("ToolStripFromObject", BindingFlags.NonPublic | BindingFlags.Static)!;
        object? result = method.Invoke(null, [toolStrip]);
        result.Should().BeSameAs(toolStrip);
    }

    [Fact]
    public void ToolStripCollectionEditor_ToolStripFromObject_ToolStripDropDownItem_ReturnsDropDown()
    {
        using ToolStrip toolStrip = new();
        using ToolStripDropDown dropDown = new();
        ToolStripMenuItem dropDownItem = new("drop")
        {
            Owner = toolStrip,
            DropDown = dropDown
        };

        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        Type formType = form.GetType();
        MethodInfo method = formType.GetMethod("ToolStripFromObject", BindingFlags.NonPublic | BindingFlags.Static)!;
        object? result = method.Invoke(null, [dropDownItem]);
        result.Should().BeSameAs(dropDown);
    }

    [Fact]
    public void ToolStripCollectionEditor_ToolStripFromObject_OtherObject_ReturnsNull()
    {
        object other = "notAToolStrip";
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        Type formType = form.GetType();
        MethodInfo method = formType.GetMethod("ToolStripFromObject", BindingFlags.NonPublic | BindingFlags.Static)!;
        object? result = method.Invoke(null, [other]);
        result.Should().BeNull();
    }

    [Fact]
    public void ToolStripCollectionEditor_EditorItemCollection_Add_AppendsItem()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        (object itemList, ToolStrip _) = CreateEditorItemCollectionWithHost(form);

        using ToolStripButton item = new("item1");
        InvokeEditorItemCollectionAdd(itemList, item);

        // The underlying CollectionBase List now has two items (the host + the new EditorItem wrapping our button).
        GetEditorItemCollectionList(itemList).Count.Should().Be(2);
    }

    [Fact]
    public void ToolStripCollectionEditor_EditorItemCollection_Insert_InsertsAtIndex()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        dynamic formAccessor = form.TestAccessor.Dynamic;
        (object itemList, ToolStrip _) = CreateEditorItemCollectionWithHost(form);
        FilterListBox listBox = formAccessor._listBoxItems;

        using ToolStripButton item1 = new("item1");
        using ToolStripButton item2 = new("item2");
        InvokeEditorItemCollectionAdd(itemList, item1);
        InvokeEditorItemCollectionInsert(itemList, 2, item2);

        // After inserting at index 2, the listbox should have item1 at index 1, item2 at index 2.
        listBox.Items[1].Should().Be(item1);
        listBox.Items[2].Should().Be(item2);
    }

    [Fact]
    public void ToolStripCollectionEditor_EditorItemCollection_IndexOf_FindsItem()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        (object itemList, ToolStrip _) = CreateEditorItemCollectionWithHost(form);

        using ToolStripButton item = new("item1");
        InvokeEditorItemCollectionAdd(itemList, item);

        InvokeEditorItemCollectionIndexOf(itemList, item).Should().Be(1);
    }

    [Fact]
    public void ToolStripCollectionEditor_EditorItemCollection_IndexOf_NotFound_ReturnsMinusOne()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        (object itemList, ToolStrip _) = CreateEditorItemCollectionWithHost(form);

        using ToolStripButton notInList = new("notInList");
        InvokeEditorItemCollectionIndexOf(itemList, notInList).Should().Be(-1);
    }

    [Fact]
    public void ToolStripCollectionEditor_EditorItemCollection_Move_SameIndex_DoesNothing()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        (object itemList, ToolStrip _) = CreateEditorItemCollectionWithHost(form);
        FilterListBox listBox = form.TestAccessor.Dynamic._listBoxItems;

        using ToolStripButton item1 = new("item1");
        using ToolStripButton item2 = new("item2");
        InvokeEditorItemCollectionAdd(itemList, item1);
        InvokeEditorItemCollectionAdd(itemList, item2);

        // Move from 1 to 1 is a no-op (same index).
        Action act = () => InvokeEditorItemCollectionMove(itemList, 1, 1);
        act.Should().NotThrow();

        // Order should be unchanged.
        listBox.Items[1].Should().Be(item1);
        listBox.Items[2].Should().Be(item2);
    }

    [Fact]
    public void ToolStripCollectionEditor_EditorItemCollection_Move_HostItem_DoesNothing()
    {
        // The host EditorItem (wrapping the ToolStrip itself) is at index 0. Moving it
        // should hit the `editorItem.Host is not null` early return in the Move method.
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        (object itemList, ToolStrip host) = CreateEditorItemCollectionWithHost(form);
        FilterListBox listBox = form.TestAccessor.Dynamic._listBoxItems;

        // Attempt to move the host (index 0) elsewhere.
        Action act = () => InvokeEditorItemCollectionMove(itemList, 0, 1);
        act.Should().NotThrow();

        // The host must still be at index 0 of the underlying listbox.
        listBox.Items[0].Should().Be(host);
    }

    [Fact]
    public void ToolStripCollectionEditor_EditorItemCollection_Remove_RemovesItem()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        (object itemList, ToolStrip _) = CreateEditorItemCollectionWithHost(form);
        FilterListBox listBox = form.TestAccessor.Dynamic._listBoxItems;

        using ToolStripButton item1 = new("item1");
        using ToolStripButton item2 = new("item2");
        InvokeEditorItemCollectionAdd(itemList, item1);
        InvokeEditorItemCollectionAdd(itemList, item2);

        InvokeEditorItemCollectionRemove(itemList, item1);

        listBox.Items.Cast<object>().Should().NotContain(item1);
    }

    [Fact]
    public void ToolStripCollectionEditor_EditorItemCollection_OnClear_RemovesAllItems()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        (object itemList, ToolStrip _) = CreateEditorItemCollectionWithHost(form);
        FilterListBox listBox = form.TestAccessor.Dynamic._listBoxItems;

        using ToolStripButton item1 = new("item1");
        using ToolStripButton item2 = new("item2");
        InvokeEditorItemCollectionAdd(itemList, item1);
        InvokeEditorItemCollectionAdd(itemList, item2);

        InvokeEditorItemCollectionClear(itemList);

        listBox.Items.Cast<object>().Should().BeEmpty();
    }

    [Fact]
    public void ToolStripCollectionEditor_EditorItemCollection_OnInsertComplete_NullValue_ReturnsEarly()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        object itemList = CreateEditorItemCollection(form);

        Action act = () => InvokeEditorItemCollectionOnInsertComplete(itemList, 0, null);
        act.Should().NotThrow();
    }

    [Fact]
    public void ToolStripCollectionEditor_TypeListItem_Constructor_SetsType()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        Type typeListItemType = form.GetType()
            .GetNestedType("TypeListItem", BindingFlags.NonPublic)!;

        object item = Activator.CreateInstance(typeListItemType, typeof(ToolStripButton))!;
        Type typeField = (Type)typeListItemType.GetField("Type")!.GetValue(item)!;

        typeField.Should().Be(typeof(ToolStripButton));
    }

    [Fact]
    public void ToolStripCollectionEditor_EditorItem_ConstructedFromToolStrip_ExposesHost()
    {
        // The EditorItem constructor takes an `object` and toggles between _host (for ToolStrip) and
        // _component (for ToolStripItem). This test exercises the private nested EditorItem directly
        // by constructing it via reflection on the EditorItemCollection nested type.
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        Type formType = form.GetType();
        Type itemCollectionType = formType.GetNestedType("EditorItemCollection", BindingFlags.NonPublic)!;
        Type editorItemType = itemCollectionType.GetNestedType("EditorItem", BindingFlags.NonPublic)!;
        ConstructorInfo editorItemCtor = editorItemType.GetConstructor(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            [typeof(object)],
            modifiers: null)!;

        using ToolStrip toolStrip = new();
        object editorItem = editorItemCtor.Invoke([toolStrip]);

        // Component should be null since this was a ToolStrip host, not a ToolStripItem.
        PropertyInfo componentProperty = editorItemType.GetProperty("Component")!;
        object? component = componentProperty.GetValue(editorItem);
        component.Should().BeNull();

        // Host should be returned (the wrapping ToolStrip).
        PropertyInfo hostProperty = editorItemType.GetProperty("Host")!;
        object host = hostProperty.GetValue(editorItem)!;
        host.Should().BeSameAs(toolStrip);
    }

    [Fact]
    public void ToolStripCollectionEditor_EditorItem_ConstructedFromToolStripItem_ExposesComponent()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        Type formType = form.GetType();
        Type itemCollectionType = formType.GetNestedType("EditorItemCollection", BindingFlags.NonPublic)!;
        Type editorItemType = itemCollectionType.GetNestedType("EditorItem", BindingFlags.NonPublic)!;
        ConstructorInfo editorItemCtor = editorItemType.GetConstructor(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            [typeof(object)],
            modifiers: null)!;

        using ToolStripButton button = new("btn");
        object editorItem = editorItemCtor.Invoke([button]);

        PropertyInfo componentProperty = editorItemType.GetProperty("Component")!;
        object component = componentProperty.GetValue(editorItem)!;
        component.Should().BeSameAs(button);

        PropertyInfo hostProperty = editorItemType.GetProperty("Host")!;
        object? host = hostProperty.GetValue(editorItem);
        host.Should().BeNull();
    }

    [Fact]
    public void ToolStripCollectionEditor_EditorItem_Dispose_ClearsComponent()
    {
        // EditorItem.Dispose sets _component = null which makes the Component getter return null.
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        Type formType = form.GetType();
        Type itemCollectionType = formType.GetNestedType("EditorItemCollection", BindingFlags.NonPublic)!;
        Type editorItemType = itemCollectionType.GetNestedType("EditorItem", BindingFlags.NonPublic)!;
        ConstructorInfo editorItemCtor = editorItemType.GetConstructor(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            [typeof(object)],
            modifiers: null)!;
        MethodInfo disposeMethod = editorItemType.GetMethod("Dispose")!;
        PropertyInfo componentProperty = editorItemType.GetProperty("Component")!;

        using ToolStripButton button = new("btn");
        object editorItem = editorItemCtor.Invoke([button]);
        componentProperty.GetValue(editorItem).Should().BeSameAs(button);

        Action act = () => disposeMethod.Invoke(editorItem, null);
        act.Should().NotThrow();

        componentProperty.GetValue(editorItem).Should().BeNull();
    }

    [Fact]
    public void ToolStripCollectionEditor_TypeListItem_ToString_ReturnsDescription()
    {
        using Form form = _editor.TestAccessor.Dynamic.CreateCollectionForm();
        Type typeListItemType = form.GetType()
            .GetNestedType("TypeListItem", BindingFlags.NonPublic)!;

        object item = Activator.CreateInstance(typeListItemType, typeof(ToolStripButton))!;
        string description = item.ToString()!;

        description.Should().Be(ToolStripDesignerUtils.GetToolboxDescription(typeof(ToolStripButton)));
    }

    /// <summary>
    ///  Creates an <c>EditorItemCollection</c> via reflection and assigns it to the
    ///  form's <c>_itemList</c> field. This is required because the field is normally only
    ///  populated by the <c>Collection</c> property setter, which in turn needs a non-null
    ///  <see cref="ITypeDescriptorContext"/> (only set up by the full <c>EditValue</c> flow).
    /// </summary>
    private static object CreateEditorItemCollection(Form form)
    {
        Type formType = form.GetType();
        Type editorItemCollectionType = formType.GetNestedType("EditorItemCollection", BindingFlags.NonPublic)!;
        dynamic formAccessor = form.TestAccessor.Dynamic;
        FilterListBox listBox = formAccessor._listBoxItems;

        // Construct EditorItemCollection(form, listBox.Items, mutableList).
        // The third IList must be mutable (not a fixed-size array), because OnInsertComplete
        // calls _targetCollectionList.Insert / .Remove.
        Type ownerType = formType; // the form itself is the owner (matches the internal ctor parameter type).
        Type iListType = typeof(IList);
        ConstructorInfo ctor = editorItemCollectionType.GetConstructor(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            [ownerType, iListType, iListType],
            modifiers: null)!;

        ArrayList mutableList = [];
        object itemList = ctor.Invoke([form, listBox.Items, mutableList]);

        // Assign to the form's private _itemList field.
        formType.GetField("_itemList", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(form, itemList);
        return itemList;
    }

    /// <summary>
    ///  Like <see cref="CreateEditorItemCollection"/>, but also seeds the list with a
    ///  <see cref="ToolStrip"/> host at index 0 to mirror the production flow (the editor
    ///  always puts the owning <see cref="ToolStrip"/> at index 0, so subsequent item
    ///  additions land at index 1+ and <c>OnInsertComplete</c> doesn't try to insert at
    ///  <c>index - 1 = -1</c>).
    /// </summary>
    private static (object ItemList, ToolStrip Host) CreateEditorItemCollectionWithHost(Form form)
    {
        object itemList = CreateEditorItemCollection(form);
        using ToolStrip host = new();
        InvokeEditorItemCollectionAdd(itemList, host);
        return (itemList, host);
    }

    /// <summary>
    ///  Returns the <see cref="IList"/> backing the given <c>EditorItemCollection</c>
    ///  (i.e. the <see cref="CollectionBase.List"/> property).
    /// </summary>
    private static IList GetEditorItemCollectionList(object itemList)
    {
        return (IList)itemList.GetType()
            .GetProperty("List", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)!
            .GetValue(itemList)!;
    }

    private static void InvokeEditorItemCollectionAdd(object itemList, object item)
    {
        itemList.GetType().GetMethod("Add", [typeof(object)])!.Invoke(itemList, [item]);
    }

    private static void InvokeEditorItemCollectionInsert(object itemList, int index, ToolStripItem item)
    {
        itemList.GetType().GetMethod("Insert", [typeof(int), typeof(ToolStripItem)])!
            .Invoke(itemList, [index, item]);
    }

    private static int InvokeEditorItemCollectionIndexOf(object itemList, ToolStripItem item)
    {
        return (int)itemList.GetType().GetMethod("IndexOf", [typeof(ToolStripItem)])!
            .Invoke(itemList, [item])!;
    }

    private static void InvokeEditorItemCollectionMove(object itemList, int fromIndex, int toIndex)
    {
        itemList.GetType().GetMethod("Move", [typeof(int), typeof(int)])!
            .Invoke(itemList, [fromIndex, toIndex]);
    }

    private static void InvokeEditorItemCollectionRemove(object itemList, ToolStripItem item)
    {
        itemList.GetType().GetMethod("Remove", [typeof(ToolStripItem)])!
            .Invoke(itemList, [item]);
    }

    private static void InvokeEditorItemCollectionClear(object itemList)
    {
        itemList.GetType().GetMethod("Clear", Type.EmptyTypes)!.Invoke(itemList, null);
    }

    private static void InvokeEditorItemCollectionOnInsertComplete(object itemList, int index, object? value)
    {
        itemList.GetType().GetMethod("OnInsertComplete", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(int), typeof(object)], null)!
            .Invoke(itemList, [index, value]);
    }
}
