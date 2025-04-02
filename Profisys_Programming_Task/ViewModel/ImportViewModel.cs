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

        //DATAGRID
        public object? _currentDataGridSource { get; set; }

        //COMMAND
        public RelayCommand BackToMenuCommand { get; }
        public RelayCommand ChooseFileCommand { get; }
        public AsyncRelayCommand ImportDataCommand { get; }
        public RelayCommand CancelImportCommand { get; }

        //CONSTRUCTOR
        public ImportViewModel(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;

            IsImporting = false;

            //COMMENDS
            BackToMenuCommand = new RelayCommand(BackToMenu, IsNotImporting);
            ChooseFileCommand = new RelayCommand(ChooseFile, IsNotImporting);
            ImportDataCommand = new AsyncRelayCommand(ProccesCsvAsycn, CanImport);
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
                //disable button for import time
                BackToMenuCommand.NotifyCanExecuteChanged();
                ChooseFileCommand.NotifyCanExecuteChanged();
                ImportDataCommand.NotifyCanExecuteChanged();

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
                    //Todo
                }
                else if (headers.Contains("DocumentId") && headers.Contains("Ordinal") && headers.Contains("Product") && headers.Contains("Quantity") && headers.Contains("Price") && headers.Contains("TaxRate")) //document items table
                {
                    await ImportDocumentItems(csv);
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
            IsImporting = false; //flag up
            OnPropertyChanged(nameof(IsImporting));
        }

        private async Task ImportDocumentItems(CsvReader csv)
        {
            ObservableCollection<DocumentItems> importedItems = new ObservableCollection<DocumentItems>();
            int errorCount = 0;
            ImportProgress = 0;

            double RowUpdateProgressBar = 100.0 / File.ReadLines(FilePath).Count() ;  //count how many procent is one row inport  
            _currentDataGridSource = importedItems; //select current DataGrid source
            OnPropertyChanged(nameof(_currentDataGridSource));
            while (await csv.ReadAsync())
            {
                try
                {
                    ImportProgress += RowUpdateProgressBar;
                    OnPropertyChanged(nameof(ImportProgress));
                    DocumentItems item = csv.GetRecord<DocumentItems>();
                    importedItems.Add(item);
                    //await _appDbContext.DocumentItems.AddAsync(item);
                    OnPropertyChanged(nameof(importedItems));
                }
                catch
                {
                    errorCount++;
                }
            }
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
    }
}
