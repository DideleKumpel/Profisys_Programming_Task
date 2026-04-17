using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using Profisys_Programming_Task.Model;
using Microsoft.EntityFrameworkCore;
using Profisys_Programming_Task.Service.DbService;
using Profisys_Programming_Task.Service.Import;

namespace Profisys_Programming_Task.ViewModel
{
    internal partial class ImportViewModel : ObservableObject
    {
        //SERVICE
        private readonly IImportService<Documents> _documentsImportService;
        private readonly IImportService<DocumentItems> _documentItemsImportService;
        private readonly IDbService<Documents> _documentsDbService;
        private readonly IDbService<DocumentItems> _documentItemsDbService;

        //DATA FOR IMPORT
        [ObservableProperty]
        private string _filePath;
        [ObservableProperty]
        private bool _isImporting;
        [ObservableProperty]
        private double _importProgress;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _abortImportToken;

        //DATAGRID
        [ObservableProperty]
        private Visibility _documetnsDataGridVisibility;
        [ObservableProperty]
        private Visibility _documentItemsDataGridVisibility;
        [ObservableProperty]
        private ObservableCollection<Documents> _importedDocuments;
        [ObservableProperty]
        private ObservableCollection<DocumentItems> _importedDocumentItems;

        //CONSTRUCTOR
        public ImportViewModel(IDbService<Documents> documentsDbService, IDbService<DocumentItems> documentItemsDbService, IImportService<Documents> documentsImportService, IImportService<DocumentItems> documentItemsImportService)
        {
            _documentsDbService = documentsDbService;
            _documentItemsDbService = documentItemsDbService;
            _documentsImportService = documentsImportService;
            _documentItemsImportService = documentItemsImportService;

            IsImporting = false;
            //DataGrid info
            DocumetnsDataGridVisibility = "Visible";
            DocumentItemsDataGridVisibility = "Collapsed";
        }
        

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
            OnPropertyChanged(nameof(FilePath));
            ImportDataCommand.NotifyCanExecuteChanged();
        }

        //FILE IMPORT METHODS
        private async Task ProccesCsvAsycn() //methods definign witch table we import data into
        {
            try
            {
                IsImporting = true; //flag up
                OnPropertyChanged(nameof(IsImporting));
                _cancellationTokenSource = new CancellationTokenSource();  // initialize the cancellation token
                _abortImportToken = new CancellationTokenSource();
                //disable button for import time
                BackToMenuCommand.NotifyCanExecuteChanged();
                ChooseFileCommand.NotifyCanExecuteChanged();
                ImportDataCommand.NotifyCanExecuteChanged();
                CancelImportCommand.NotifyCanExecuteChanged();
                AbortImportCommand.NotifyCanExecuteChanged();

                using StreamReader reader = new StreamReader(FilePath);
                using CsvReader csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.GetCultureInfo("pl-PL"))
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null, //DocumentItem model has Id that isint present in CSV file
                    Delimiter = ";"       //CSV uses ';' as delimiter deafult value for CsvReader is ','
                });

                csv.Read();
                csv.ReadHeader();
                string[] headers = csv.HeaderRecord;
                if (headers.Contains("Id") && headers.Contains("Type") && headers.Contains("Date") && headers.Contains("FirstName") && headers.Contains("LastName") && headers.Contains("City")) //documents table
                {
                    //Set datagrid to display and hidde another one
                    DocumentItemsDataGridVisibility = "Collapsed";
                    DocumetnsDataGridVisibility = "Visible";
                    OnPropertyChanged(nameof (DocumentItemsDataGridVisibility));
                    OnPropertyChanged(nameof (DocumetnsDataGridVisibility));
                    await ImportDocuments(csv, _cancellationTokenSource.Token, _abortImportToken.Token);
                }
                else if (headers.Contains("DocumentId") && headers.Contains("Ordinal") && headers.Contains("Product") && headers.Contains("Quantity") && headers.Contains("Price") && headers.Contains("TaxRate")) //document items table
                {
                    //Set datagrid to display and hidde another one
                    DocumetnsDataGridVisibility = "Collapsed";
                    DocumentItemsDataGridVisibility = "Visible";
                    OnPropertyChanged(nameof(DocumetnsDataGridVisibility));
                    OnPropertyChanged(nameof(DocumentItemsDataGridVisibility));
                    await ImportDocumentItems(csv, _cancellationTokenSource.Token, _abortImportToken.Token);
                }
                else
                {
                    MessageBox.Show("CSV file format not recognized.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Import error: {ex.Message}");
            }
            IsImporting = false; //flag down
            OnPropertyChanged(nameof(IsImporting));
            BackToMenuCommand.NotifyCanExecuteChanged();
            ChooseFileCommand.NotifyCanExecuteChanged();
            ImportDataCommand.NotifyCanExecuteChanged();
            CancelImportCommand.NotifyCanExecuteChanged();
            AbortImportCommand.NotifyCanExecuteChanged();
        }

        private async Task ImportDocumentItems(CsvReader csv, CancellationToken cancellationToken, CancellationToken abortImportToken)
        {
            ObservableCollection<DocumentItems> importedItems = new ObservableCollection<DocumentItems>();
            int errorCount = 0;
            ImportProgress = 0;

            double RowUpdateProgressBar = 100.0 / File.ReadLines(FilePath).Count() ;  //count how many procent is one row inport  
            ImportedDocumentItems = importedItems; //select current DataGrid source
            OnPropertyChanged(nameof(ImportedDocumentItems));
            while (await csv.ReadAsync())
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)    // Check if cancellation has been requested
                    {
                        _appDbContext.ChangeTracker.Clear();
                        MessageBox.Show("Import has been cancelled.");
                        return; // Exit the method if the import is cancelled without saving
                    }
                    if (abortImportToken.IsCancellationRequested)
                    {
                        MessageBox.Show("Import has been aborted.");
                        break; // Import
                    }
                    ImportProgress += RowUpdateProgressBar;
                    OnPropertyChanged(nameof(ImportProgress));
                    DocumentItems item = csv.GetRecord<DocumentItems>();
                    if( await _appDbContext.Documents.FirstOrDefaultAsync( i => i.Id == item.DocumentId) != null)
                    {
                        await _appDbContext.DocumentItems.AddAsync(item);
                        importedItems.Add(item);
                        OnPropertyChanged(nameof(importedItems));
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch
                {
                    errorCount++;
                }
            }
            await _appDbContext.SaveChangesAsync();
            ImportProgress = 100;
            OnPropertyChanged(nameof(ImportProgress));
            if (errorCount > 0) {
                MessageBox.Show($"Import ended with {errorCount} errors.");
            }
            else
            {
                MessageBox.Show($"Import ended with succes.");
            }
        }
        private async Task ImportDocuments(CsvReader csv, CancellationToken cancellationToken, CancellationToken abortImportToken)
        {
            ObservableCollection<Documents> importedItems = new ObservableCollection<Documents>();
            int errorCount = 0;
            ImportProgress = 0;

            double RowUpdateProgressBar = 100.0 / File.ReadLines(FilePath).Count();  //count how many procent is one row inport  
            ImportedDocuments = importedItems; //select current DataGrid source
            OnPropertyChanged(nameof(ImportedDocuments));
            
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync()) // Start transaction
            {
                try
                {
                    await _appDbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Documents ON;");
                    while (await csv.ReadAsync())
                    {
                        try
                        {
                            if (cancellationToken.IsCancellationRequested)    // Check if cancellation has been requested
                            {
                                _appDbContext.ChangeTracker.Clear();
                                MessageBox.Show("Import has been cancelled.");
                                return; // Exit the method if the import is cancelled without saving
                            }
                            if (abortImportToken.IsCancellationRequested)
                            {
                                MessageBox.Show("Import has been aborted.");
                                break; // Import
                            }
                            ImportProgress += RowUpdateProgressBar;
                            OnPropertyChanged(nameof(ImportProgress));
                            Documents item = csv.GetRecord<Documents>();
                            if (await _appDbContext.Documents.FirstOrDefaultAsync(i => i.Id == item.Id) == null)
                            {
                                await _appDbContext.Documents.AddAsync(item);
                                importedItems.Add(item);
                                OnPropertyChanged(nameof(importedItems));
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }

                        catch
                        {
                            errorCount++;
                        }
                    }
                    await _appDbContext.SaveChangesAsync(); // Save all records at once
                    await _appDbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Documents OFF;");

                    await transaction.CommitAsync(); // Commit transaction
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(); // Rollback if an error occurs
                    MessageBox.Show($"Import failed: {ex.Message}");
                    return;
                }
            }
            ImportProgress = 100;
            OnPropertyChanged(nameof(ImportProgress));
            if (errorCount > 0)
            {
                MessageBox.Show($"Import ended with {errorCount} errors.");
            }
            else
            {
                MessageBox.Show($"Import ended with succes.");
            }
        }
        private bool CanImport()
        {
            if (FilePath.IsNullOrEmpty() && !IsImporting)
            {
                return false;
            }else
            {
                return true;
            }
        }
        private bool IsNotImporting()
        {
            if (IsImporting)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private void BackToMenu()
        {
            Application.Current.MainWindow.DataContext = new MainMenuViewModel();
        }

        private void CancelImport()
        {
            _cancellationTokenSource?.Cancel();  // Cancel the import operation
        }
        private void AbortImport()
        {
            _abortImportToken?.Cancel();
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
