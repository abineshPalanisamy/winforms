// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Windows.Forms;

/// <summary>
///  Provides string resources for the Windows Forms hosting library.
/// </summary>
internal static class SR
{
    internal const string FormProvider_MustBeCalledOnUiThread =
        "ShowForm must be called on the Windows Forms UI thread. Use ShowFormAsync to show a form from a background thread.";

    internal const string WinFormsHostedService_AlreadyRunning =
        "The Windows Forms hosted service is already running.";

    internal const string WinFormsApplication_NotInitialized =
        "WinFormsApplication has not been initialized. Call WinFormsApplication.CreateBuilder() and host.RunAsync() before accessing this property.";

    internal const string WinFormsHostedService_MainFormRequired =
        "A main form type must be configured. Call AddWinFormsMainForm<TForm>() during service registration.";
}
