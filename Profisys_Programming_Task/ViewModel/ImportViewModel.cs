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
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using Profisys_Programming_Task.Model;
using System.Diagnostics;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace Profisys_Programming_Task.ViewModel
{
    internal class ImportViewModel : ObservableObject
    {
        //SERVICE
        private readonly AppDbContext _appDbContext;

        //DATA FOR IMPORT
        public string FilePath { get; set; }
        public bool IsImporting { get; set; }
        public double ImportProgress { get; set; }
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _abortImportToken;

        //DATAGRID
        public string DocumetnsDataGridVisibility { get; set; }
        public string DocumentItemsDataGridVisibility { get; set; }
        public ObservableCollection<Documents> ImportedDocuments { get; set; }
        public ObservableCollection<DocumentItems> ImportedDocumentItems { get; set; } 

        //COMMAND
        public RelayCommand BackToMenuCommand { get; }
        public RelayCommand ChooseFileCommand { get; }
        public AsyncRelayCommand ImportDataCommand { get; }
        public RelayCommand CancelImportCommand { get; }
        public RelayCommand AbortImportCommand { get; }

        //CONSTRUCTOR
        public ImportViewModel(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;

            IsImporting = false;
            //DataGrid info
            DocumetnsDataGridVisibility = "Visible";
            DocumentItemsDataGridVisibility = "Collapsed";

            //COMMENDS
            BackToMenuCommand = new RelayCommand(BackToMenu, IsNotImporting);
            ChooseFileCommand = new RelayCommand(ChooseFile, IsNotImporting);
            ImportDataCommand = new AsyncRelayCommand(ProccesCsvAsycn, CanImport);
            CancelImportCommand = new RelayCommand(CancelImport, CanCancelImporting);
            AbortImportCommand = new RelayCommand(AbortImport, CanCancelImporting);
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
                    //Todo
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
            bool ImportCanceled = false; 

            double RowUpdateProgressBar = 100.0 / File.ReadLines(FilePath).Count() ;  //count how many procent is one row inport  
            ImportedDocumentItems = importedItems; //select current DataGrid source
            OnPropertyChanged(nameof(ImportedDocumentItems));
            while (await csv.ReadAsync())
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)    // Check if cancellation has been requested
                    {
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
                catch (OperationCanceledException)
                {
                    MessageBox.Show("Import has been cancelled.");
                    return; // Exit the loop if the import is cancelled
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
            Application.Current.MainWindow.DataContext = new MainMenuViewModel(_appDbContext);
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
