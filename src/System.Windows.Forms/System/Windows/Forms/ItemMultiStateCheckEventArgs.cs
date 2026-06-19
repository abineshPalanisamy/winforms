// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Windows.Forms;

public class ItemMultiStateCheckEventArgs : EventArgs
{
    public ItemMultiStateCheckEventArgs(int index, int currentValue, int newValue)
    {
        Index = index;
        CurrentValue = currentValue;
        NewValue = newValue;
    }

    public int Index { get; }

    public int CurrentValue { get; }

    public int NewValue { get; set; }
}
