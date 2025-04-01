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

        private Window _dialog;

        public RelayCommand CloseDialogCommand { get; }

        public DocumentsItemDialogViewModel(AppDbContext appDbContext, Documents Document, Window dialog)
        {
            _dialog = dialog;
            _appDbContext = appDbContext;
            CurrentDocument = new ObservableCollection<Documents>() { Document };
            OnPropertyChanged(nameof(CurrentDocument));

            CloseDialogCommand = new RelayCommand(CloseDialog);
        }

        public async Task LoadItems()
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

        public void CloseDialog()
        {
            _dialog.Close();
        }
    }
}
