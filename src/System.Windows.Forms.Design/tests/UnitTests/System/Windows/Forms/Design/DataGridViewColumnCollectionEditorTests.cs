// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using Moq;
using Moq.Protected;

namespace System.Windows.Forms.Design.Tests;

public class DataGridViewColumnCollectionEditorTests
{
    [Fact]
    public void DataGridViewColumnCollectionEditor_GetEditStyle() =>
        new DataGridViewColumnCollectionEditor().GetEditStyle().Should().Be(UITypeEditorEditStyle.Modal);

    [Fact]
    public void DataGridViewColumnCollectionEditor_IsDropDownResizable() =>
        new DataGridViewColumnCollectionEditor().IsDropDownResizable.Should().Be(false);

    [Fact]
    public void DataGridViewColumnCollectionEditor_EditValue_NullProvider_ReturnsValue()
    {
        DataGridViewColumnCollectionEditor dataGridViewColumnCollectionEditor = new();
        object value = "123";
        dataGridViewColumnCollectionEditor.EditValue(null, null!, value).Should().Be(value);
    }

    [Fact]
    public void DataGridViewColumnCollectionEditor_EditValue_NullContext_ReturnsValue()
    {
        DataGridViewColumnCollectionEditor dataGridViewColumnCollectionEditor = new();
        object value = "123";

        Mock<ITypeDescriptorContext> mockTypeDescriptorContext = new(MockBehavior.Strict);
        dataGridViewColumnCollectionEditor.EditValue(mockTypeDescriptorContext.Object, null!, value).Should().Be(value);
    }

    [Fact]
    public void DataGridViewColumnCollectionEditor_EditValue_NoEditorService_ReturnsValue()
    {
        DataGridViewColumnCollectionEditor dataGridViewColumnCollectionEditor = new();
        object value = "123";

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider.Setup(x => x.GetService(typeof(IWindowsFormsEditorService))).Returns(null!);
        dataGridViewColumnCollectionEditor.EditValue(null, mockServiceProvider.Object, value).Should().Be(value);
    }

    [Fact]
    public void DataGridViewColumnCollectionEditor_EditValue_NullContextInstance_ReturnsValue()
    {
        DataGridViewColumnCollectionEditor dataGridViewColumnCollectionEditor = new();
        object value = "123";

        Mock<ITypeDescriptorContext> mockTypeDescriptorContext = new(MockBehavior.Strict);
        mockTypeDescriptorContext.Setup(x => x.Instance).Returns(null!);

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider.Setup(x => x.GetService(typeof(IWindowsFormsEditorService))).Returns(null!);
        dataGridViewColumnCollectionEditor.EditValue(mockTypeDescriptorContext.Object, mockServiceProvider.Object, value).Should().Be(value);
    }

    [Fact]
    public void DataGridViewColumnCollectionEditor_EditValue_NoDesignerHost_ReturnsValue()
    {
        // The provider exposes IWindowsFormsEditorService, but the call to
        // GetService(typeof(IDesignerHost)) returns null, so the editor must
        // short-circuit and return the original value unchanged.
        DataGridViewColumnCollectionEditor dataGridViewColumnCollectionEditor = new();
        object value = "123";

        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(x => x.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);
        mockServiceProvider
            .Setup(x => x.GetService(typeof(IDesignerHost)))
            .Returns(null!);

        Mock<ITypeDescriptorContext> mockTypeDescriptorContext = new(MockBehavior.Strict);
        mockTypeDescriptorContext.Setup(x => x.Instance).Returns(new DataGridView());

        dataGridViewColumnCollectionEditor.EditValue(mockTypeDescriptorContext.Object, mockServiceProvider.Object, value)
            .Should().Be(value);
    }

    [Fact]
    public void DataGridViewColumnCollectionEditor_EditValue_ValidProviderDialogOK_CommitsTransaction()
    {
        // Exercises the success path: a valid provider, editor service, and
        // designer host are supplied; the dialog is shown and returns OK;
        // the editor must commit the transaction and return the original value.
        DataGridViewColumnCollectionEditor dataGridViewColumnCollectionEditor = new();
        object value = "value";

        using DataGridView dataGridView = new();

        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        mockEditorService
            .Setup(s => s.ShowDialog(It.IsAny<Form>()))
            .Returns(DialogResult.OK)
            .Verifiable();

        // DesignerTransaction is disposed by the editor's `using` block, so use
        // MockBehavior.Loose to allow Dispose to be invoked without an explicit
        // setup, and override the protected OnCommit to verify the commit branch.
        Mock<DesignerTransaction> mockTransaction = new(MockBehavior.Loose);
        mockTransaction
            .Protected()
            .Setup("OnCommit")
            .Verifiable();

        Mock<IDesignerHost> mockDesignerHost = new(MockBehavior.Strict);
        mockDesignerHost
            .Setup(h => h.CreateTransaction(It.IsAny<string>()))
            .Returns(mockTransaction.Object)
            .Verifiable();

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(x => x.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);
        mockServiceProvider
            .Setup(x => x.GetService(typeof(IDesignerHost)))
            .Returns(mockDesignerHost.Object);

        Mock<ITypeDescriptorContext> mockTypeDescriptorContext = new(MockBehavior.Strict);
        mockTypeDescriptorContext.Setup(x => x.Instance).Returns(dataGridView);

        dataGridViewColumnCollectionEditor.EditValue(mockTypeDescriptorContext.Object, mockServiceProvider.Object, value)
            .Should().Be(value);

        mockEditorService.Verify(s => s.ShowDialog(It.IsAny<Form>()), Times.Once);
        mockTransaction.Protected().Verify("OnCommit", Times.Once());
    }

    [Fact]
    public void DataGridViewColumnCollectionEditor_EditValue_ValidProviderDialogCancel_CancelsTransaction()
    {
        // Exercises the cancel path: ShowDialog returns a non-OK result and
        // the editor must cancel (rather than commit) the transaction.
        DataGridViewColumnCollectionEditor dataGridViewColumnCollectionEditor = new();
        object value = "value";

        using DataGridView dataGridView = new();

        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        mockEditorService
            .Setup(s => s.ShowDialog(It.IsAny<Form>()))
            .Returns(DialogResult.Cancel)
            .Verifiable();

        // DesignerTransaction is disposed by the editor's `using` block, so use
        // MockBehavior.Loose to allow Dispose to be invoked without an explicit
        // setup, and override the protected OnCancel to verify the cancel branch.
        Mock<DesignerTransaction> mockTransaction = new(MockBehavior.Loose);
        mockTransaction
            .Protected()
            .Setup("OnCancel")
            .Verifiable();

        Mock<IDesignerHost> mockDesignerHost = new(MockBehavior.Strict);
        mockDesignerHost
            .Setup(h => h.CreateTransaction(It.IsAny<string>()))
            .Returns(mockTransaction.Object)
            .Verifiable();

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(x => x.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);
        mockServiceProvider
            .Setup(x => x.GetService(typeof(IDesignerHost)))
            .Returns(mockDesignerHost.Object);

        Mock<ITypeDescriptorContext> mockTypeDescriptorContext = new(MockBehavior.Strict);
        mockTypeDescriptorContext.Setup(x => x.Instance).Returns(dataGridView);

        dataGridViewColumnCollectionEditor.EditValue(mockTypeDescriptorContext.Object, mockServiceProvider.Object, value)
            .Should().Be(value);

        mockEditorService.Verify(s => s.ShowDialog(It.IsAny<Form>()), Times.Once);
        mockTransaction.Protected().Verify("OnCancel", Times.Once());
    }

    [Fact]
    public void DataGridViewColumnCollectionEditor_EditValue_ReusesDialogOnSubsequentCalls()
    {
        // The editor caches the DataGridViewColumnCollectionDialog instance and
        // reuses it across calls. Both calls should use the same instance, so
        // ShowDialog is called twice but the dialog's lifecycle is shared. The
        // dialog argument passed to ShowDialog must be the same instance both
        // times, confirming the cache via the ??= operator.
        DataGridViewColumnCollectionEditor dataGridViewColumnCollectionEditor = new();
        object value = "value";

        using DataGridView dataGridView = new();

        int dialogCallIndex = 0;
        Form[] dialogsShown = new Form[2];

        Mock<IWindowsFormsEditorService> mockEditorService = new(MockBehavior.Strict);
        mockEditorService
            .Setup(s => s.ShowDialog(It.IsAny<Form>()))
            .Callback<Form>(dialog => dialogsShown[dialogCallIndex++] = dialog)
            .Returns(DialogResult.OK);

        // DesignerTransaction.Commit only invokes OnCommit the first time, so
        // a fresh mock instance must be returned for each call to CreateTransaction.
        Mock<DesignerTransaction> mockTransaction1 = new(MockBehavior.Loose);
        mockTransaction1
            .Protected()
            .Setup("OnCommit")
            .Verifiable();
        Mock<DesignerTransaction> mockTransaction2 = new(MockBehavior.Loose);
        mockTransaction2
            .Protected()
            .Setup("OnCommit")
            .Verifiable();

        Mock<IDesignerHost> mockDesignerHost = new(MockBehavior.Strict);
        Queue<DesignerTransaction> transactions = new();
        transactions.Enqueue(mockTransaction1.Object);
        transactions.Enqueue(mockTransaction2.Object);
        mockDesignerHost
            .Setup(h => h.CreateTransaction(It.IsAny<string>()))
            .Returns(() => transactions.Dequeue());

        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);
        mockServiceProvider
            .Setup(x => x.GetService(typeof(IWindowsFormsEditorService)))
            .Returns(mockEditorService.Object);
        mockServiceProvider
            .Setup(x => x.GetService(typeof(IDesignerHost)))
            .Returns(mockDesignerHost.Object);

        Mock<ITypeDescriptorContext> mockTypeDescriptorContext = new(MockBehavior.Strict);
        mockTypeDescriptorContext.Setup(x => x.Instance).Returns(dataGridView);

        dataGridViewColumnCollectionEditor.EditValue(mockTypeDescriptorContext.Object, mockServiceProvider.Object, value)
            .Should().Be(value);
        dataGridViewColumnCollectionEditor.EditValue(mockTypeDescriptorContext.Object, mockServiceProvider.Object, value)
            .Should().Be(value);

        mockEditorService.Verify(s => s.ShowDialog(It.IsAny<Form>()), Times.Exactly(2));
        mockTransaction1.Protected().Verify("OnCommit", Times.Once());
        mockTransaction2.Protected().Verify("OnCommit", Times.Once());
        dialogsShown[0].Should().BeSameAs(dialogsShown[1]);
    }
}
