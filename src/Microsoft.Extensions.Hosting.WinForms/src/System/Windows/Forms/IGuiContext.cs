// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Extensions.WinForms;

/// <summary>
///  Provides access to the Windows Forms GUI execution context.
/// </summary>
/// <remarks>
///  <para>
///   <see cref="IGuiContext"/> is the DI-injectable abstraction over the Windows Forms
///   message-loop execution model. It exposes <see cref="SynchronizationContext"/> so
///   work can be dispatched back to the UI thread, and provides high-level control over
///   application lifetime.
///  </para>
/// </remarks>
public interface IGuiContext
{
    /// <summary>
    ///  Gets the <see cref="SynchronizationContext"/> bound to the Windows Forms UI thread.
    /// </summary>
    SynchronizationContext SynchronizationContext { get; }

    /// <summary>
    ///  Requests termination of the Windows Forms message loop.
    /// </summary>
    void RequestClose();

    /// <summary>
    ///  Gets a value indicating whether the GUI context is currently running on the UI thread.
    /// </summary>
    bool IsUiThread { get; }
}
