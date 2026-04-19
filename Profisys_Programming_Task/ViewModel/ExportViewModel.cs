using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Profisys_Programming_Task.Model;
using Profisys_Programming_Task.Service.DbService;
using Profisys_Programming_Task.Service.Export;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Profisys_Programming_Task.ViewModel
{
    internal partial class ExportViewModel: ObservableObject
    {
        private readonly IExportService<Documents> _documentsExportService;
        private readonly IExportService<DocumentItems> _documentItemsExportService;
        private readonly IDbService<Documents> _documentsDbService;
        private readonly IDocumentItemsDbService _documentItemsDbService;

        [ObservableProperty]
        public int _docuemntsDbCount;
        [ObservableProperty]
        public int _documentItemsDbCount;

        public ExportViewModel(IExportService<Documents> documentsExportService, IExportService<DocumentItems> documentItemsExportService, IDbService<Documents> documentsDbService, IDocumentItemsDbService documentItemsDbService)
        {
            _documentsExportService = documentsExportService;
            _documentItemsExportService = documentItemsExportService;
            _documentsDbService = documentsDbService;
            _documentItemsDbService = documentItemsDbService;
        }

        public async Task LoadCountsAsync()
        {
            DocuemntsDbCount = await _documentsDbService.GetCountAsync();
            DocumentItemsDbCount = await _documentItemsDbService.GetCountAsync();
        }

        [RelayCommand]
        private async Task ExportDocumentsAsync()
        {
            List<Documents> FetchDocuments = await _documentsDbService.GetAllAsync();
            await ExportAsync<Documents>( FetchDocuments, _documentsExportService, "Documents");
        }

        [RelayCommand]
        private async Task ExportDocumentItemsAsync()
        {
            List<DocumentItems> FetchDocumentItems = await _documentItemsDbService.GetAllAsync();
            await ExportAsync<DocumentItems>(FetchDocumentItems, _documentItemsExportService, "DocumentItems");
        }

        private async Task ExportAsync<T>(List<T>fetchData, IExportService<T> exportService, string defaultFileName) where T : class
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Export to CSV",
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"{defaultFileName}_{DateTime.Today:yyyy-MM-dd}.csv"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            try
            {
                await exportService.ExportToCsvAsync(fetchData, saveFileDialog.FileName);
                MessageBox.Show("Export completed successfully.", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void BackToMenu()
        {
            var mainMenuViewModel = ((App)Application.Current).ServiceProvider.GetService<MainMenuViewModel>();
            Application.Current.MainWindow.DataContext = mainMenuViewModel;
        }
    }
}
