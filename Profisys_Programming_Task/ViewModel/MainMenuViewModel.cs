using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Profisys_Programming_Task.ViewModel
{
    internal class MainMenuViewModel : ObservableObject
    {

        public RelayCommand GoToImportViewCommand { get; }
        public AsyncRelayCommand GoToDataViewCommand { get; }
        public MainMenuViewModel()
        {
            GoToDataViewCommand = new AsyncRelayCommand(SwitchToDataView);
            GoToImportViewCommand = new RelayCommand(SwitchToImportView);
        }

        private void SwitchToImportView()
        {
            Application.Current.MainWindow.DataContext = new ImportViewModel();
        }

        private async Task SwitchToDataView()
        {
            Application.Current.MainWindow.DataContext = new DataViewModel();
        }
    }
}
