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
        private readonly AppDbContext _appDbContext;
        public RelayCommand GoToImportViewCommand { get; }
        public AsyncRelayCommand GoToDataViewCommand { get; }
        public MainMenuViewModel()
        {
            _appDbContext = new AppDbContext();
            GoToDataViewCommand = new AsyncRelayCommand(SwitchToDataView);
            GoToImportViewCommand = new RelayCommand(SwitchToImportView);

        }
        public MainMenuViewModel(AppDbContext appDbContext)
        {
            try
            {
                _appDbContext = appDbContext;
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not connect to the database. Please check your connection and try again.");
                Application.Current.Shutdown();
            }
            GoToDataViewCommand = new AsyncRelayCommand(SwitchToDataView);
            GoToImportViewCommand = new RelayCommand(SwitchToImportView);

        }

        private void SwitchToImportView()
        {
            Application.Current.MainWindow.DataContext = new ImportViewModel();
        }

        private async Task SwitchToDataView()
        {
            DataViewModel dataViewModel = new DataViewModel(_appDbContext);
            await dataViewModel.LoadDocuments();
            Application.Current.MainWindow.DataContext = dataViewModel;
        }
    }
}
