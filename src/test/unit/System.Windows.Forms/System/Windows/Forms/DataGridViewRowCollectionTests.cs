// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
namespace System.Windows.Forms.Tests;

public class DataGridViewRowCollectionTests
{
    [WinFormsFact]
    public void DataGridViewRowCollection_ImplementsGenericEnumerable()
    {
        using DataGridView control = new();
        DataGridViewRowCollection rows = control.Rows;

        Assert.IsAssignableFrom<IEnumerable<DataGridViewRow>>(rows);
    }

    [WinFormsFact]
    public void DataGridViewRowCollection_ForEachVar_IsStronglyTyped()
    {
        using DataGridView control = new();
        control.AllowUserToAddRows = false;
        control.Columns.Add("Id", "Id");
        control.Rows.Add("1");
        control.Rows.Add("2");

        int sumOfIndexes = 0;

        foreach (var row in control.Rows)
        {
            sumOfIndexes += row.Index;
        }

        Assert.Equal(1, sumOfIndexes);
    }

    [WinFormsFact]
    public void DataGridViewRowCollection_NonGenericEnumeration_Unchanged()
    {
        using DataGridView control = new();
        control.AllowUserToAddRows = false;
        control.Columns.Add("Id", "Id");
        control.Rows.Add("1");
        control.Rows.Add("2");

        IEnumerable enumerable = control.Rows;
        ArrayList items = new();

        foreach (object item in enumerable)
        {
            items.Add(item);
        }

        Assert.Equal(2, items.Count);
        Assert.All(items.Cast<object>(), item => Assert.IsType<DataGridViewRow>(item));
    }

    [WinFormsFact]
    public void DataGridViewRowCollection_Linq_WorksDirectly()
    {
        using DataGridView control = new();
        control.AllowUserToAddRows = false;
        control.Columns.Add("Id", "Id");
        control.Rows.Add("1");
        control.Rows.Add("2");

        int count = control.Rows.Count(r => r.Index >= 0);

        Assert.Equal(2, count);
    }
}
