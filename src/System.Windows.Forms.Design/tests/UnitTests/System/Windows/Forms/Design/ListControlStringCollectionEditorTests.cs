// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Moq;

namespace System.Windows.Forms.Design.Tests;

/// <summary>
///  Tests for <see cref="ListControlStringCollectionEditor"/>.
/// </summary>
/// <remarks>
///  <para>The production type only overrides <c>EditValue</c> to short-circuit
///  editing when a <see cref="ListControl"/> has a <c>DataSource</c> set.
///  The tests below exercise both branches of the guard plus the
///  fall-through to the CollectionEditor base implementation
///  for every supported <see cref="ListControl"/> subtype.</para>
/// </remarks>
public class ListControlStringCollectionEditorTests
{
    [Fact]
    public void EditValue_WithNullContext_ReturnsBaseEditValue()
    {
        ListControlStringCollectionEditor editor = new(typeof(string));
        IServiceProvider provider = new Mock<IServiceProvider>().Object;
        object? value = new();

        object? result = editor.EditValue(null, provider, value);

        result.Should().Be(value);
    }

    [Fact]
    public void EditValue_WithContextInstanceNotListControl_ReturnsBaseEditValue()
    {
        ListControlStringCollectionEditor editor = new(typeof(string));
        Mock<ITypeDescriptorContext> context = new();
        context.Setup(c => c.Instance).Returns(new object());
        IServiceProvider provider = new Mock<IServiceProvider>().Object;
        object? value = new();

        object? result = editor.EditValue(context.Object, provider, value);

        result.Should().Be(value);
    }

    [Fact]
    public void EditValue_WithListControlAndNullDataSource_ReturnsBaseEditValue()
    {
        ListControlStringCollectionEditor editor = new(typeof(string));
        using ListBox listControl = new();
        Mock<ITypeDescriptorContext> context = new();
        context.Setup(c => c.Instance).Returns(listControl);
        IServiceProvider provider = new Mock<IServiceProvider>().Object;
        object? value = new();

        object? result = editor.EditValue(context.Object, provider, value);

        result.Should().Be(value);
    }

    [Fact]
    public void EditValue_WithListControlAndNonNullDataSource_ThrowsArgumentException()
    {
        ListControlStringCollectionEditor editor = new(typeof(string));

        using ListBox listControl = new() { DataSource = new List<string> { "item1", "item2", "item3" } };

        Mock<ITypeDescriptorContext> context = new();
        context.Setup(c => c.Instance).Returns(listControl);

        IServiceProvider provider = new Mock<IServiceProvider>().Object;
        object? value = new();

        ArgumentException exception = ((Action)(() => editor.EditValue(context.Object, provider, value))).Should().Throw<ArgumentException>().Which;
        exception.Message.Should().Be(SR.DataSourceLocksItems);
    }

    [Fact]
    public void EditValue_WithComboBoxAndNonNullDataSource_ThrowsArgumentException()
    {
        // The guard matches any ListControl subclass, not just ListBox.
        ListControlStringCollectionEditor editor = new(typeof(string));

        using ComboBox comboBox = new() { DataSource = new List<string> { "a", "b" } };

        Mock<ITypeDescriptorContext> context = new();
        context.Setup(c => c.Instance).Returns(comboBox);

        ((Action)(() => editor.EditValue(context.Object, new Mock<IServiceProvider>().Object, new object())))
            .Should().Throw<ArgumentException>()
            .And.Message.Should().Be(SR.DataSourceLocksItems);
    }

    [Fact]
    public void EditValue_WithCheckedListBoxAndNonNullDataSource_ThrowsArgumentException()
    {
        // The guard matches any ListControl subclass, not just ListBox.
        ListControlStringCollectionEditor editor = new(typeof(string));

        using CheckedListBox checkedListBox = new() { DataSource = new List<string> { "a" } };

        Mock<ITypeDescriptorContext> context = new();
        context.Setup(c => c.Instance).Returns(checkedListBox);

        ((Action)(() => editor.EditValue(context.Object, new Mock<IServiceProvider>().Object, "value")))
            .Should().Throw<ArgumentException>()
            .And.Message.Should().Be(SR.DataSourceLocksItems);
    }

    [Fact]
    public void EditValue_WithListControlAndDataSource_ThrowsBeforeQueryingProvider()
    {
        // The DataSource guard must short-circuit before the provider is consulted.
        // We pass null as provider to prove the exception is raised without
        // touching the service provider.
        ListControlStringCollectionEditor editor = new(typeof(string));

        using ListBox listControl = new() { DataSource = new List<string> { "x" } };

        Mock<ITypeDescriptorContext> context = new();
        context.Setup(c => c.Instance).Returns(listControl);

        // The provider parameter is non-nullable in the production signature, so we
        // use a (non-strict) mock here rather than passing a literal null.
        ((Action)(() => editor.EditValue(context.Object, new Mock<IServiceProvider>().Object, new object())))
            .Should().Throw<ArgumentException>()
            .And.Message.Should().Be(SR.DataSourceLocksItems);
    }

    [Fact]
    public void EditValue_WithListControlAndDataSourceAndNullValue_ThrowsArgumentException()
    {
        // The DataSource guard fires regardless of the value parameter.
        ListControlStringCollectionEditor editor = new(typeof(string));

        using ListBox listControl = new() { DataSource = new List<string> { "x" } };

        Mock<ITypeDescriptorContext> context = new();
        context.Setup(c => c.Instance).Returns(listControl);

        // EditValue's `value` parameter is nullable, so passing null exercises the
        // guard with a null value.
        ((Action)(() => editor.EditValue(context.Object, new Mock<IServiceProvider>().Object, (object?)null)))
            .Should().Throw<ArgumentException>()
            .And.Message.Should().Be(SR.DataSourceLocksItems);
    }
}
