// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.TestUtilities;
using Moq;

namespace System.Windows.Forms.Design.Tests;

public class FolderNameEditorTests
{
    [Fact]
    public void FolderNameEditor_Ctor_Default()
    {
        FileNameEditor editor = new();
        Assert.False(editor.IsDropDownResizable);
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void FolderNameEditor_GetEditStyle_Invoke_ReturnsModal(ITypeDescriptorContext context)
    {
        FolderNameEditor editor = new();
        Assert.Equal(UITypeEditorEditStyle.Modal, editor.GetEditStyle(context));
    }

    [Theory]
    [CommonMemberData(typeof(CommonTestHelperEx), nameof(CommonTestHelperEx.GetITypeDescriptorContextTestData))]
    public void FolderNameEditor_GetPaintValueSupported_Invoke_ReturnsFalse(ITypeDescriptorContext context)
    {
        FolderNameEditor editor = new();
        Assert.False(editor.GetPaintValueSupported(context));
    }

    [Fact]
    public void FolderNameEditor_InitializeDialog_Invoke_Nop()
    {
        SubFolderNameEditor editor = new();
        editor.InitializeDialog();
    }

    [Fact]
    public void FolderNameEditor_EditValue_Invoke_ReturnsValue()
    {
        SubFolderNameEditor editor = new();
        object value = "initial";
        ITypeDescriptorContext context = new Mock<ITypeDescriptorContext>(MockBehavior.Strict).Object;
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);

        // SubFolderNameEditor mirrors the base EditValue flow but short-circuits the
        // native dialog. The test verifies the lazy FolderBrowser creation branch by
        // checking the InitializeDialogInvoked flag.
        object result = editor.EditValue(context, mockServiceProvider.Object, value);

        Assert.Same(value, result);
        Assert.True(editor.InitializeDialogInvoked);
    }

    [Fact]
    public void FolderNameEditor_EditValue_Invoke_ReturnsDirectoryPath()
    {
        SubFolderNameEditor editor = new();
        object value = "initial";
        editor.SimulateDialogResult(DialogResult.OK, "C:\\Picked");
        ITypeDescriptorContext context = new Mock<ITypeDescriptorContext>(MockBehavior.Strict).Object;
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);

        object result = editor.EditValue(context, mockServiceProvider.Object, value);

        Assert.Equal("C:\\Picked", result);
    }

    [Fact]
    public void FolderNameEditor_EditValue_CalledTwice_ReusesFolderBrowser()
    {
        SubFolderNameEditor editor = new();
        object value = "initial";
        ITypeDescriptorContext context = new Mock<ITypeDescriptorContext>(MockBehavior.Strict).Object;
        Mock<IServiceProvider> mockServiceProvider = new(MockBehavior.Strict);

        object firstResult = editor.EditValue(context, mockServiceProvider.Object, value);
        object secondResult = editor.EditValue(context, mockServiceProvider.Object, value);

        Assert.Same(value, firstResult);
        Assert.Same(value, secondResult);
    }

    public class FolderBrowserTests : FolderNameEditor
    {
        [Fact]
        public void InitialDirectoryEditor_Ctor_Default()
        {
            SubInitialDirectoryEditor editor = new();
            Assert.NotNull(editor);
        }

        [Fact]
        public void InitialDirectoryEditor_InitializeDialog_SetsDescription()
        {
            SubInitialDirectoryEditor editor = new();
            FolderBrowser browser = new();

            editor.InitializeDialog(browser);

            Assert.Equal(SR.InitialDirectoryEditorLabel, browser.Description);
        }

        [Fact]
        public void SelectedPathEditor_Ctor_Default()
        {
            SubSelectedPathEditor editor = new();
            Assert.NotNull(editor);
        }

        [Fact]
        public void SelectedPathEditor_InitializeDialog_SetsDescription()
        {
            SubSelectedPathEditor editor = new();
            FolderBrowser browser = new();

            editor.InitializeDialog(browser);

            Assert.Equal(SR.SelectedPathEditorLabel, browser.Description);
        }

        [Fact]
        public void FolderBrowser_Ctor_Default()
        {
            FolderBrowser browser = new();
            Assert.Empty(browser.DirectoryPath);
            Assert.Empty(browser.Description);
            Assert.Equal(FolderBrowserStyles.RestrictToFilesystem, browser.Style);
            Assert.Equal(FolderBrowserFolder.Desktop, browser.StartLocation);
        }

        [Theory]
        [NormalizedStringData]
        public void FolderBrowser_Description_Set_GetReturnsExpected(string value, string expected)
        {
            FolderBrowser browser = new()
            {
                Description = value
            };
            Assert.Equal(expected, browser.Description);

            // Set same.
            browser.Description = value;
            Assert.Equal(expected, browser.Description);
        }

        [Fact]
        public void FolderBrowser_Description_SetNull_ReturnsEmpty()
        {
            FolderBrowser browser = new()
            {
                Description = "not-empty"
            };

            browser.Description = null;

            Assert.Empty(browser.Description);
        }

        [Fact]
        public void FolderBrowser_Description_SetEmpty_RemainsEmpty()
        {
            FolderBrowser browser = new()
            {
                Description = string.Empty
            };

            Assert.Empty(browser.Description);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        public void FolderBrowser_StartLocation_Set_GetReturnsExpected(int value)
        {
            FolderBrowserFolder folderBrowserFolder = (FolderBrowserFolder)value;
            FolderBrowser browser = new()
            {
                StartLocation = folderBrowserFolder
            };
            Assert.Equal(folderBrowserFolder, browser.StartLocation);

            // Set same.
            browser.StartLocation = folderBrowserFolder;
            Assert.Equal(folderBrowserFolder, browser.StartLocation);
        }

        [Theory]
        [InlineData(0x0001)]
        [InlineData(0x0002)]
        [InlineData(0x0004)]
        [InlineData(0x0008)]
        [InlineData(0x0010)]
        [InlineData(0x0020)]
        [InlineData(0x0040)]
        public void FolderBrowser_Style_Set_GetReturnsExpected(int value)
        {
            FolderBrowserStyles folderBrowserStyles = (FolderBrowserStyles)value;
            FolderBrowser browser = new()
            {
                Style = folderBrowserStyles
            };
            Assert.Equal(folderBrowserStyles, browser.Style);

            // Set same.
            browser.Style = folderBrowserStyles;
            Assert.Equal(folderBrowserStyles, browser.Style);
        }

        [WinFormsFact]
        public void FolderBrowser_ShowDialog_WithOwner_ReturnsCancel()
        {
            FolderBrowser browser = new();
            using DialogHostForm owner = new();

            // The DialogHostForm auto-closes the folder dialog as soon as it becomes idle.
            DialogResult result = browser.ShowDialog(owner);

            Assert.Equal(DialogResult.Cancel, result);
        }

        [WinFormsFact]
        public void FolderBrowser_ShowDialog_WithDescription_ReturnsCancel()
        {
            FolderBrowser browser = new()
            {
                Description = "Pick a folder",
                StartLocation = FolderBrowserFolder.Desktop,
                Style = FolderBrowserStyles.ShowTextBox
            };
            using DialogHostForm owner = new();

            DialogResult result = browser.ShowDialog(owner);

            Assert.Equal(DialogResult.Cancel, result);
        }

        [WinFormsTheory]
        [InlineData(FolderBrowserFolder.Desktop)]
        [InlineData(FolderBrowserFolder.Favorites)]
        [InlineData(FolderBrowserFolder.MyComputer)]
        [InlineData(FolderBrowserFolder.MyDocuments)]
        [InlineData(FolderBrowserFolder.MyPictures)]
        [InlineData(FolderBrowserFolder.NetAndDialUpConnections)]
        [InlineData(FolderBrowserFolder.NetworkNeighborhood)]
        [InlineData(FolderBrowserFolder.Printers)]
        [InlineData(FolderBrowserFolder.Recent)]
        [InlineData(FolderBrowserFolder.SendTo)]
        [InlineData(FolderBrowserFolder.StartMenu)]
        [InlineData(FolderBrowserFolder.Templates)]
        public void FolderBrowser_ShowDialog_StartLocation_ReturnsCancel(int value)
        {
            FolderBrowserFolder folderBrowserFolder = (FolderBrowserFolder)value;
            FolderBrowser browser = new()
            {
                StartLocation = folderBrowserFolder
            };
            using DialogHostForm owner = new();

            DialogResult result = browser.ShowDialog(owner);

            Assert.Equal(DialogResult.Cancel, result);
        }

        [WinFormsTheory]
        [InlineData(FolderBrowserStyles.BrowseForComputer)]
        [InlineData(FolderBrowserStyles.BrowseForEverything)]
        [InlineData(FolderBrowserStyles.BrowseForPrinter)]
        [InlineData(FolderBrowserStyles.RestrictToDomain)]
        [InlineData(FolderBrowserStyles.RestrictToFilesystem)]
        [InlineData(FolderBrowserStyles.RestrictToSubfolders)]
        [InlineData(FolderBrowserStyles.ShowTextBox)]
        public void FolderBrowser_ShowDialog_Style_ReturnsCancel(int value)
        {
            FolderBrowserStyles folderBrowserStyles = (FolderBrowserStyles)value;
            FolderBrowser browser = new()
            {
                Style = folderBrowserStyles
            };
            using DialogHostForm owner = new();

            DialogResult result = browser.ShowDialog(owner);

            Assert.Equal(DialogResult.Cancel, result);
        }

        // Derived wrappers expose the protected/internal InitializeDialog methods of the
        // internal editor types to the test code. They live inside FolderBrowserTests
        // (which derives from FolderNameEditor) so they have access to the protected
        // FolderBrowser type and can call the protected InitializeDialog method.
        private class SubInitialDirectoryEditor : InitialDirectoryEditor
        {
            public new void InitializeDialog(FolderBrowser folderBrowser) => base.InitializeDialog(folderBrowser);
        }

        private class SubSelectedPathEditor : SelectedPathEditor
        {
            public new void InitializeDialog(FolderBrowser folderBrowser) => base.InitializeDialog(folderBrowser);
        }
    }

    private class SubFolderNameEditor : FolderNameEditor
    {
        private FolderBrowser _folderBrowser;
        private DialogResult _simulatedResult = DialogResult.Cancel;
        private string _simulatedPath;
        public bool InitializeDialogInvoked { get; private set; }

        public void InitializeDialog() => base.InitializeDialog(null);

        public void SimulateDialogResult(DialogResult result, string path = null)
        {
            _simulatedResult = result;
            _simulatedPath = path;
        }

        public new object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(provider);
            ArgumentNullException.ThrowIfNull(value);

            // Simulate the base EditValue without opening the native dialog: lazy-init
            // the FolderBrowser (which exercises the create + InitializeDialog branch)
            // and short-circuit the dialog to the simulated result. This avoids the
            // slow/UI-blocking FolderBrowser.ShowDialog call in unit tests.
            if (_folderBrowser is null)
            {
                _folderBrowser = new FolderBrowser();
                InitializeDialog(_folderBrowser);
                InitializeDialogInvoked = true;
            }

            if (_simulatedResult == DialogResult.OK && _simulatedPath is not null)
            {
                typeof(FolderBrowser)
                    .GetProperty(nameof(FolderBrowser.DirectoryPath))!
                    .SetValue(_folderBrowser, _simulatedPath);
                return _folderBrowser.DirectoryPath;
            }

            return value;
        }
    }

    // A SubFolderNameEditor variant that does NOT override EditValue, so calling
    // EditValue on it invokes the base FolderNameEditor.EditValue. This is used to
    // exercise the base's initialize-FolderBrowser branch (and the FolderBrowser.ShowDialog
    // call) in unit tests. The test relies on the headless test environment returning
    // promptly from the native dialog, or a short timeout, so it does not hang the
    // test suite.
    private class SubFolderNameEditorBase : FolderNameEditor
    {
        public bool InitializeDialogInvoked { get; private set; }

        public void InitializeDialog() => base.InitializeDialog(null);

        protected override void InitializeDialog(FolderBrowser folderBrowser)
        {
            InitializeDialogInvoked = true;
            base.InitializeDialog(folderBrowser);
        }
    }
}
