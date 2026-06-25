// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;

namespace System.Private.Windows.Ole;

internal static class DataObjectExtensions
{
    extension(IComVisibleDataObject dataObject)
    {
        internal void SetFileDropList(StringCollection filePaths)
        {
            if (filePaths.OrThrowIfNull().Count == 0)
            {
                throw new ArgumentException(SR.CollectionEmptyException);
            }

            // Validate the paths to make sure they don't contain invalid characters
            string[] filePathsArray = new string[filePaths.Count];
            filePaths.CopyTo(filePathsArray, 0);

            foreach (string path in filePathsArray)
            {
                // These are the only error states for Path.GetFullPath
                if (string.IsNullOrEmpty(path) || path.Contains('\0'))
                {
                    throw new ArgumentException(string.Format(SR.Clipboard_InvalidPath, path ?? "<null>", nameof(filePaths)));
                }
            }

            dataObject.SetData(DataFormatNames.FileDrop, autoConvert: true, filePathsArray);
        }

        internal void SetFileDropList(IEnumerable<string> filePaths)
        {
            string[] filePathsArray = MaterializeFileDropList(filePaths.OrThrowIfNull());
            SetFileDropListCore(targetDataObject: dataObject, filePathsArray, nameof(filePaths));
        }
    }

    private static string[] MaterializeFileDropList(IEnumerable<string> filePaths)
    {
        if (filePaths is ICollection<string> collection)
        {
            if (collection.Count == 0)
            {
                throw new ArgumentException(SR.CollectionEmptyException);
            }

            string[] filePathsArray = new string[collection.Count];
            collection.CopyTo(filePathsArray, 0);
            return filePathsArray;
        }

        if (filePaths is IReadOnlyCollection<string> readOnlyCollection)
        {
            if (readOnlyCollection.Count == 0)
            {
                throw new ArgumentException(SR.CollectionEmptyException);
            }

            string[] filePathsArray = new string[readOnlyCollection.Count];
            int index = 0;

            foreach (string path in filePaths)
            {
                filePathsArray[index++] = path;
            }

            Debug.Assert(index == filePathsArray.Length);
            return filePathsArray;
        }

        List<string> filePathsList = new();

        foreach (string path in filePaths)
        {
            filePathsList.Add(path);
        }

        if (filePathsList.Count == 0)
        {
            throw new ArgumentException(SR.CollectionEmptyException);
        }

        return [.. filePathsList];
    }

    private static void SetFileDropListCore(
        IComVisibleDataObject targetDataObject,
        string[] filePathsArray,
        string paramName)
    {
        if (filePathsArray.Length == 0)
        {
            throw new ArgumentException(SR.CollectionEmptyException);
        }

        foreach (string path in filePathsArray)
        {
            if (string.IsNullOrEmpty(path) || path.Contains('\0'))
            {
                throw new ArgumentException(
                    string.Format(SR.Clipboard_InvalidPath, path ?? "<null>", paramName));
            }
        }

        targetDataObject.SetData(DataFormatNames.FileDrop, autoConvert: true, filePathsArray);
    }
}
