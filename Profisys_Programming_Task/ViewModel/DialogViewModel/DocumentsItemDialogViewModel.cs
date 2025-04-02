using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Profisys_Programming_Task.Model;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace Profisys_Programming_Task.ViewModel.DialogViewModel
{
    internal class DocumentsItemDialogViewModel : ObservableObject
    {
        private readonly AppDbContext _appDbContext;

        public ObservableCollection<Documents> CurrentDocument { get; set; }
        public ObservableCollection<DocumentItems> Items { get; set; }

        private DocumentItems _selectedItem;
        public DocumentItems SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
                DeleteItemCommand.NotifyCanExecuteChanged();
            }
        }

        private Window _dialog;

        public RelayCommand CloseDialogCommand { get; }
        public AsyncRelayCommand DeleteItemCommand { get; }

        public DocumentsItemDialogViewModel(AppDbContext appDbContext, Documents Document, Window dialog)
        {
            _dialog = dialog;
            _appDbContext = appDbContext;
            CurrentDocument = new ObservableCollection<Documents>() { Document };
            OnPropertyChanged(nameof(CurrentDocument));

            CloseDialogCommand = new RelayCommand(CloseDialog);
            DeleteItemCommand = new AsyncRelayCommand(DeleteItemAsyn, IsItemSelected);
        }

        public async Task LoadItemsAsync()
        {
            try
            {
                Items = new ObservableCollection<DocumentItems>(await _appDbContext.DocumentItems.Where(d => d.DocumentId == CurrentDocument[0].Id).ToListAsync());
                OnPropertyChanged(nameof(Items));

            }
            catch (Exception e)
            {
                MessageBox.Show("Could not connect to the database. Please check your connection and try again.");
                CloseDialog();
            }
        }

        public async Task DeleteItemAsyn()
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
                    _appDbContext.DocumentItems.Remove(SelectedItem);
                    await _appDbContext.SaveChangesAsync();
                    MessageBox.Show("Item deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadItemsAsync();
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

        public void CloseDialog()
        {
            _dialog.Close();
        }
    }
}
