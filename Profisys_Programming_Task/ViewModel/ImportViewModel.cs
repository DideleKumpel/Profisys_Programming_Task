using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using Profisys_Programming_Task.Model;
using Profisys_Programming_Task.Model.DTO;
using Profisys_Programming_Task.Service.DbService;
using Profisys_Programming_Task.Service.Exceptions;
using Profisys_Programming_Task.Service.Import;
using Profisys_Programming_Task.View.Dialog;
using Profisys_Programming_Task.ViewModel.DialogViewModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace Profisys_Programming_Task.ViewModel
{
    internal partial class ImportViewModel : ObservableObject
    {
        //SERVICE
        private readonly IImportService<Documents> _documentsImportService;
        private readonly IImportService<DocumentItems> _documentItemsImportService;
        private readonly IDbService<Documents> _documentsDbService;
        private readonly IDocumentItemsDbService _documentItemsDbService;

        //DATA FOR IMPORT
        [ObservableProperty]
        private string _filePath;
        partial void OnFilePathChanged(string value)
        {
            ProccesCsvAsycnCommand.NotifyCanExecuteChanged();
        }
        [ObservableProperty]
        private bool _isImporting = false;
        partial void OnIsImportingChanged(bool value)
        {
            ProccesCsvAsycnCommand.NotifyCanExecuteChanged();
            CancelImportCommand.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        private double _importProgress;
        private CancellationTokenSource _cancellation = new CancellationTokenSource();

        //DATAGRID
        [ObservableProperty]
        private Visibility _documetnsDataGridVisibility = Visibility.Visible;
        [ObservableProperty]
        private Visibility _documentItemsDataGridVisibility = Visibility.Collapsed;
        [ObservableProperty]
        private ObservableCollection<Documents> _importedDocuments;
        [ObservableProperty]
        private ObservableCollection<ImportResult<Documents>> _failedImportedDocuemnts;
        [ObservableProperty]
        private ObservableCollection<DocumentItems> _importedDocumentItems;
        [ObservableProperty]
        private ObservableCollection<ImportResult<DocumentItems>> _failedImportedDocumentItems;

        //CONSTRUCTOR
        public ImportViewModel(IDbService<Documents> documentsDbService, IDocumentItemsDbService documentItemsDbService, IImportService<Documents> documentsImportService, IImportService<DocumentItems> documentItemsImportService)
        {
            _documentsDbService = documentsDbService;
            _documentItemsDbService = documentItemsDbService;
            _documentsImportService = documentsImportService;
            _documentItemsImportService = documentItemsImportService;
        }

        [RelayCommand]
        private void BackToMenu()
        {
            var mainMenuViewModel = ((App)Application.Current).ServiceProvider.GetService<MainMenuViewModel>();
            Application.Current.MainWindow.DataContext = mainMenuViewModel;
        }

        [RelayCommand]
        private void ChooseFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Choose CSV file",
                Filter = "Pliki CSV (*.csv)|*.csv",
                InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false
            };

            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                FilePath = openFileDialog.FileName;
            }
            else
            {
                FilePath = "";
            }
        }

        //FILE IMPORT METHODS
        [RelayCommand(CanExecute = nameof(CanImport))]
        private async Task ProccesCsvAsycn()
        {
            if (FilePath.IsNullOrEmpty())
            {
                MessageBox.Show("Please select a CSV file to import.");
                return;
            }
            IsImporting = true; //flag up
            CancellationToken cancellationToken = _cancellation.Token;

            List<Documents> loadedDocuments = new();
            List<DocumentItems> loadedDocumentItems = new();
            bool importedDocuments = false;
            try {
                if(await _documentsImportService.CanImportAsync(FilePath)) 
                {
                    loadedDocuments = await TryImportDocuments(cancellationToken);
                    loadedDocuments = ShowImportPreview(loadedDocuments, "Document Import Preview");
                    DocumentItemsDataGridVisibility = Visibility.Collapsed;
                    DocumetnsDataGridVisibility = Visibility.Visible;
                    await AddImportedDocumentsToDb(loadedDocuments, cancellationToken);
                }
                else if(await _documentItemsImportService.CanImportAsync(FilePath))
                {
                    loadedDocumentItems = await TryImportDocumentItems(cancellationToken);
                    loadedDocumentItems = ShowImportPreview(loadedDocumentItems, "Document Items Import Preview");
                    DocumetnsDataGridVisibility = Visibility.Collapsed;
                    DocumentItemsDataGridVisibility = Visibility.Visible;
                    await AddImportedDocumentItemsToDb(loadedDocumentItems, cancellationToken);
                }
                else
                {
                    MessageBox.Show("The selected file does not match the expected format for either Documents or Document Items.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    IsImporting = false;
                    return;
                }      
            }
            catch(Exception error)
            {
                HandleImportException(error);
                IsImporting = false;
                return;
            }
            IsImporting = false; //flag down
        }

        private async Task AddImportedDocumentsToDb(List<Documents> documents, CancellationToken cancellationToken)
        {
            ImportedDocuments = new ObservableCollection<Documents>();
            FailedImportedDocuemnts = new ObservableCollection<ImportResult<Documents>>();
            foreach (Documents doc in documents)
            {
                ImportProgress = (double)(ImportedDocuments.Count + FailedImportedDocuemnts.Count) / documents.Count * 100;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await _documentsDbService.AddAsync(doc);
                    ImportedDocuments.Add(doc);
                }
                catch (Exception error) when (error is DatabaseConnectionException || error is OperationCanceledException)
                {
                    MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                catch (UniqueConstraintException error)
                {
                    FailedImportedDocuemnts.Add(new ImportResult<Documents>(doc, false, "Id is taken"));
                }
                catch(ArgumentNullException error)
                {
                    FailedImportedDocuemnts.Add(new ImportResult<Documents>(doc, false, "One or more required fields are empty"));
                }
                catch (Exception error)
                {
                    FailedImportedDocuemnts.Add(new ImportResult<Documents>(doc, false, error.Message));
                }
            }
        }

        private async Task AddImportedDocumentItemsToDb(List<DocumentItems> documentItems, CancellationToken cancellationToken)
        {
            ImportedDocumentItems = new ObservableCollection<DocumentItems>();
            FailedImportedDocumentItems = new ObservableCollection<ImportResult<DocumentItems>>();
            foreach (DocumentItems item in documentItems)
            {
                ImportProgress = (double)(ImportedDocumentItems.Count + FailedImportedDocumentItems.Count) / documentItems.Count * 100;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await _documentItemsDbService.AddAsync(item);
                    ImportedDocumentItems.Add(item);
                }
                catch (Exception error) when (error is DatabaseConnectionException || error is OperationCanceledException)
                {
                    MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                catch (UniqueConstraintException error)
                {
                    FailedImportedDocumentItems.Add(new ImportResult<DocumentItems>(item, false, "Id is taken"));
                }
                catch (ArgumentNullException error)
                {
                    FailedImportedDocumentItems.Add(new ImportResult<DocumentItems>(item, false, "One or more required fields are empty"));
                }
                catch (Exception error)
                {
                    FailedImportedDocumentItems.Add(new ImportResult<DocumentItems>(item, false, error.Message));
                }
            }
        }

        private List<T> ShowImportPreview<T>(List<T> items, string title)
        {
            ImportPreviewDialogView importDialog = new ImportPreviewDialogView();
            ImportPreviewDialogViewModel<T> importDialogViewModel = new ImportPreviewDialogViewModel<T>(new ObservableCollection<T>(items), title, importDialog);
            importDialog.DataContext = importDialogViewModel;
            importDialog.ShowDialog();
            return new List<T>(importDialogViewModel.Items);
        }

        private async Task<List<Documents>> TryImportDocuments(CancellationToken cancellationToken)
        {
            List<Documents> loadedDocuments = new();
            try
            {
                loadedDocuments = await _documentsImportService.ImportFromCsvAsync(FilePath, cancellationToken);
                return loadedDocuments;
            }
            catch (Exception error)
            {
                throw;
            }
        }

        private async Task<List<DocumentItems>> TryImportDocumentItems(CancellationToken cancellationToken)
        {
            List<DocumentItems> loadedDocumentsItems = new();
            try
            {
                loadedDocumentsItems = await _documentItemsImportService.ImportFromCsvAsync(FilePath, cancellationToken);
                return loadedDocumentsItems;
            }
            catch (Exception error)
            {
                throw;
            }
        }

        private void HandleImportException(Exception ex) {
            if(ex is InvalidDataException)
            {
                MessageBox.Show($"Unknown CSV data format");
            }
            else if (ex is OperationCanceledException)
            {
                MessageBox.Show("Import has been cancelled.");
            }
            else
            {
                MessageBox.Show($"An unexpected error occurred during import: {ex.Message}");
            }
        }

        private bool CanImport()
        {
            if (FilePath.IsNullOrEmpty() && !IsImporting)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        [RelayCommand(CanExecute = nameof(CanCancelImporting))]
        private void CancelImport()
        {
            _cancellation?.Cancel();
        }

        private bool CanCancelImporting()
        {
            if (IsImporting)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
