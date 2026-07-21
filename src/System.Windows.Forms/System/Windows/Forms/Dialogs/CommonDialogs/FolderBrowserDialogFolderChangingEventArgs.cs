// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Windows.Forms;

/// <summary>
/// Provides data for the <see cref="FolderBrowserDialog.FolderChanging"/> event.
/// </summary>
public sealed class FolderBrowserDialogFolderChangingEventArgs : CancelEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FolderBrowserDialogFolderChangingEventArgs"/> class.
    /// </summary>
    public FolderBrowserDialogFolderChangingEventArgs(string folder)
    {
        Folder = folder.OrThrowIfNull();
    }

    /// <summary>
    /// Gets the folder that the dialog is about to navigate to.
    /// </summary>
    public string Folder { get; }
}
