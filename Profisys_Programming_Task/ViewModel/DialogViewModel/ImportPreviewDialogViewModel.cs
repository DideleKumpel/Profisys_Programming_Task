using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Profisys_Programming_Task.ViewModel.DialogViewModel
{
    internal partial class ImportPreviewDialogViewModel<T> : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<T> _items;
        [ObservableProperty]
        private T _selectedItem;

        partial void OnSelectedItemChanged(T value)
        {
            DeleteItemCommand.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        private string _title;

        private Window _dialog;

        public ImportPreviewDialogViewModel(ObservableCollection<T> list, string title, Window window)
        {
            _dialog = window;
            _items = list;
            _title = title;
        }

        [RelayCommand(CanExecute = nameof(ItemIsSelected))]
        private void DeleteItem()
        {
            if (SelectedItem != null)
            {
                Items.Remove(SelectedItem);
            }
        }

        private bool ItemIsSelected()
        {
            return SelectedItem != null;
        }   

        [RelayCommand]
        private void AcceptImport()
        {
            _dialog.DialogResult = true;
            _dialog.Close();
        }
        [RelayCommand]
        private void CancelImport()
        {
            _dialog.DialogResult = false;
            Items.Clear();
            _dialog.Close();
        }
    }
}
