// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Windows.Forms;

public class ItemMultiStateCheckedEventArgs : EventArgs
{
    public ItemMultiStateCheckedEventArgs(int index, int value)
    {
        Index = index;
        Value = value;
    }

    public int Index { get; }

    public int Value { get; }
}
