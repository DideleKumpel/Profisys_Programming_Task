using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Profisys_Programming_Task.ViewModel
{
    internal partial class MainMenuViewModel : ObservableObject
    {
        public MainMenuViewModel()
        {
        }
        [RelayCommand]
        private void SwitchToImportView()
        {
            var importViewModel = ((App)Application.Current).ServiceProvider.GetService<ImportViewModel>();
            Application.Current.MainWindow.DataContext = importViewModel;
        }
        [RelayCommand]
        private async Task SwitchToDataView()
        {

            var dataViewModel = ((App)Application.Current).ServiceProvider.GetService<DataViewModel>();
            dataViewModel.RefreshDataCommand.Execute(null);
            Application.Current.MainWindow.DataContext = dataViewModel;
        }
    }
}
