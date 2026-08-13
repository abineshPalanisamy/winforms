// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Windows.Forms;

namespace Microsoft.Extensions.WinForms;

/// <summary>
///  Creates scoped <see cref="Form"/> instances from the DI container.
/// </summary>
/// <remarks>
///  <para>
///   Each form shown via <see cref="IFormProvider"/> is created within its own
///   DI scope. When the form is closed, the scope—and all scoped services
///   resolved within it—are disposed.
///  </para>
/// </remarks>
public interface IFormProvider
{
    /// <summary>
    ///  Creates and shows a new instance of <typeparamref name="TForm"/> in its own DI scope.
    /// </summary>
    /// <typeparam name="TForm">
    ///  The type of the <see cref="Form"/> to create and display.
    /// </typeparam>
    /// <returns>The <typeparamref name="TForm"/> instance that was shown.</returns>
    TForm ShowForm<TForm>() where TForm : Form;

    /// <summary>
    ///  Creates and shows a new instance of <typeparamref name="TForm"/> in its own DI scope,
    ///  and waits asynchronously for the form to close.
    /// </summary>
    /// <typeparam name="TForm">
    ///  The type of the <see cref="Form"/> to create, display, and await.
    /// </typeparam>
    /// <param name="cancellationToken">
    ///  A <see cref="CancellationToken"/> to observe while waiting for the form to close.
    /// </param>
    /// <returns>
    ///  A <see cref="Task{TResult}"/> representing the asynchronous operation. The result is
    ///  the <see cref="DialogResult"/> returned by the form.
    /// </returns>
    Task<DialogResult> ShowFormAsync<TForm>(CancellationToken cancellationToken = default) where TForm : Form;
}
