using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Camelot.ViewModels.Interfaces.MainWindow.FilePanels.Tabs
{
    public interface ITabsListViewModel
    {
        ITabViewModel SelectedTab { get; }

        IReadOnlyList<ITabViewModel> Tabs { get; }

        event EventHandler<EventArgs> SelectedTabChanged;

        ICommand SelectTabToTheLeftCommand { get; }

        ICommand SelectTabToTheRightCommand { get; }

        public ICommand ReopenClosedTabCommand { get; }

        void CreateNewTab(string directory = null, bool switchTo = false);

        void InsertBeforeTab(ITabViewModel tabViewModel, ITabViewModel tabViewModelToInsert);

        void SaveState();
    }
}