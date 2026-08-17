// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Drawing;
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
}
