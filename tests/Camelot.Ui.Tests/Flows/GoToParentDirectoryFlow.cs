using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Camelot.Ui.Tests.Common;
using Camelot.Ui.Tests.Conditions;
using Camelot.Ui.Tests.Extensions;
using Camelot.Ui.Tests.Steps;
using Xunit;

namespace Camelot.Ui.Tests.Flows
{
    public class GoToParentDirectoryFlow : IDisposable
    {
        private string _directoryFullPath;

        [Fact(DisplayName = "Go to parent directory and back using directory selector")]
        public async Task GoToParentDirectoryAndBackTest()
        {
            var window = AvaloniaApp.GetMainWindow();
            await FocusFilePanelStep.FocusFilePanelAsync(window);

            var viewModel = ActiveFilePanelProvider.GetActiveFilePanelViewModel(window);
            _directoryFullPath = viewModel.CurrentDirectory;

            GoToParentDirectoryStep.GoToParentDirectoryViaFilePanel(window);
            var isParentDirectoryOpened = await DirectoryOpenedCondition.CheckIfParentDirectoryIsOpenedAsync(window, _directoryFullPath);
            Assert.True(isParentDirectoryOpened);

            var filesPanel = ActiveFilePanelProvider.GetActiveFilePanelView(window);
            var directoryTextBox = filesPanel
                .GetVisualDescendants()
                .OfType<TextBox>()
                .SingleOrDefault(t => t.Name == "DirectoryTextBox");
            Assert.NotNull(directoryTextBox);

            directoryTextBox.CaretIndex = directoryTextBox.Text.Length;
            var directoryName = Path.GetFileNameWithoutExtension(_directoryFullPath);
            directoryTextBox.SendText(Path.DirectorySeparatorChar + directoryName);

            var childDirectoryWasOpened =
                await DirectoryOpenedCondition.CheckIfDirectoryIsOpenedAsync(window, _directoryFullPath);
            Assert.True(childDirectoryWasOpened);
        }

        public void Dispose()
        {
            if (_directoryFullPath is null)
            {
                return;
            }

            var window = AvaloniaApp.GetMainWindow();
            var viewModel = ActiveFilePanelProvider.GetActiveFilePanelViewModel(window);
            viewModel.CurrentDirectory = _directoryFullPath;
        }
    }
}