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

namespace Profisys_Programming_Task.ViewModel
{
    internal class ImportViewModel : ObservableObject
    {
        //SERVICE
        private readonly AppDbContext _appDbContext;

        //DATA FOR IMPORT
        public string FilePath { get; set; }
        public string IsImporting { get; set; }
        public double ImportProgress { get; }

        //COMMAND
        public RelayCommand BackToMenuCommand { get; }
        public RelayCommand ChooseFileCommand { get; }
        public AsyncRelayCommand ImportDataCommand { get; }
        public RelayCommand CancelImportCommand { get; }

        //CONSTRUCTOR
        public ImportViewModel(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;

            //COMMENDS
            BackToMenuCommand = new RelayCommand(BackToMenu);
            ChooseFileCommand = new RelayCommand(ChooseFile);
            ImportDataCommand = new AsyncRelayCommand(ProccesCsvAsycn, FileIsChoosen);
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
                using var reader = new StreamReader(FilePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true
                });

                csv.Read();
                csv.ReadHeader();
                string[] headers = csv.HeaderRecord;
                if (headers.Contains("Id;Type;Date;FirstName;LastName;City")) //documents table
                {
                    //Todo
                }
                else if (headers.Contains("DocumentId;Ordinal;Product;Quantity;Price;TaxRate")) //document items table
                {
                    //toDo
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
        }



        private bool FileIsChoosen()
        {
            if (FilePath.IsNullOrEmpty())
            {
                return false;
            }else
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
