// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Windows.Forms;

namespace Microsoft.Extensions.WinForms;

/// <summary>
///  The concrete Windows Forms implementation of <see cref="IGuiContext"/>.
/// </summary>
/// <remarks>
///  <para>
///   <see cref="GuiContext"/> is created by <see cref="WinFormsHostedService"/> on the
///   dedicated STA UI thread after <see cref="Application.EnableVisualStyles"/> and the
///   high-DPI mode have been configured. The instance is registered in the DI container so
///   any service can resolve <see cref="IGuiContext"/> to dispatch work back to the UI
///   thread or to request application exit.
///  </para>
/// </remarks>
internal sealed class GuiContext : IGuiContext
{
    private readonly SynchronizationContext _synchronizationContext;
    private readonly Thread _uiThread;

    /// <summary>
    ///  Initializes a new instance of <see cref="GuiContext"/> from a
    ///  <see cref="WindowsFormsSynchronizationContext"/> that is already installed on the
    ///  current (STA) thread.
    /// </summary>
    /// <param name="synchronizationContext">
    ///  The <see cref="WindowsFormsSynchronizationContext"/> installed on the UI thread.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///  <paramref name="synchronizationContext"/> is <see langword="null"/>.
    /// </exception>
    internal GuiContext(SynchronizationContext synchronizationContext)
    {
        ArgumentNullException.ThrowIfNull(synchronizationContext);

        _synchronizationContext = synchronizationContext;
        _uiThread = Thread.CurrentThread;
    }

    /// <inheritdoc/>
    public SynchronizationContext SynchronizationContext => _synchronizationContext;

    /// <inheritdoc/>
    public bool IsUiThread => Thread.CurrentThread == _uiThread;

    /// <inheritdoc/>
    public void RequestClose()
    {
        if (IsUiThread)
        {
            Application.Exit();
        }
        else
        {
            _synchronizationContext.Post(static _ => Application.Exit(), state: null);
        }
    }
}
