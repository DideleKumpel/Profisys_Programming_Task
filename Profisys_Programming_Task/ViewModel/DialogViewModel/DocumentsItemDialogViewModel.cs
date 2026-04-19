using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using Profisys_Programming_Task.Model;
using Profisys_Programming_Task.Service.DbService;
using Profisys_Programming_Task.Service.Exceptions;
using Profisys_Programming_Task.Service.Export;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Profisys_Programming_Task.ViewModel.DialogViewModel
{
    internal partial class DocumentsItemDialogViewModel : ObservableObject
    {
        private readonly IDocumentItemsDbService _documentItemsDbService;
        private readonly DocumentPdfExportService _pdfExportService;
        [ObservableProperty]
        private ObservableCollection<Documents> _currentDocument;
        [ObservableProperty]
        private ObservableCollection<DocumentItems> _items;

        [ObservableProperty]
        private DocumentItems _selectedItem;

        partial void OnSelectedItemChanged(DocumentItems value)
        {
            deleteItemCommand.NotifyCanExecuteChanged();
        }

        private Window _dialog;


        public DocumentsItemDialogViewModel(IDocumentItemsDbService itemService, DocumentPdfExportService pdfExportService, Documents Document, Window dialog)
        {
            _dialog = dialog;
            _documentItemsDbService = itemService;
            _pdfExportService = pdfExportService;
            CurrentDocument = new ObservableCollection<Documents>() { Document };
        }

        [RelayCommand(CanExecute = nameof(IsItemsLoaded))]
        private void ExportToPdf()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Export to PDF",
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"Document_{CurrentDocument[0].Id}_{DateTime.Today:yyyy-MM-dd}.pdf"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            try
            {
                _pdfExportService.Export(CurrentDocument[0], Items.ToList(), saveFileDialog.FileName);
                MessageBox.Show("PDF exported successfully.", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsItemsLoaded() => Items != null && Items.Any();

        public async Task LoadItemsAsync()
        {
            try
            {
                List<DocumentItems> documentItems = await _documentItemsDbService.GetByDocumentIdAsync(CurrentDocument[0].Id);
                Items = new ObservableCollection<DocumentItems>(documentItems);
                ExportToPdfCommand.NotifyCanExecuteChanged();
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not connect to the database. Please check your connection and try again.");
                CloseDialog();
            }
        }
        [RelayCommand(CanExecute = nameof(IsItemSelected))]
        private async Task DeleteItemAsync()
        {
            if (SelectedItem != null) {
                string message = $"Are you sure you want to delete this item?\n\n" +
                                    $"Product: {SelectedItem.Product}\n" +
                                    $"Quantity: {SelectedItem.Quantity}\n" +
                                    $"Price: {SelectedItem.Price}\n" +
                                    $"Tax Rate: {SelectedItem.TaxRate}%";

                var result = MessageBox.Show(message, "Confirmation",MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        await _documentItemsDbService.DeleteAsync(SelectedItem);
                    }
                    catch (EntityNotFoundException error)
                    {
                        MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        await LoadItemsAsync();
                        return;
                    }
                    catch (DatabaseConnectionException error)
                    {
                        MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        CloseDialog();
                        return;
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    MessageBox.Show("Item deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadItemsAsync();
                }
                else
                {
                    return; // Exit function if Cancel is clicked
                }
            }
        }

        public bool IsItemSelected()
        {
            if (SelectedItem == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        [RelayCommand]
        private void CloseDialog()
        {
            _dialog.Close();
        }
    }
}
