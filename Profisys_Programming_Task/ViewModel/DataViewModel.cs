using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Profisys_Programming_Task.Model;
using Profisys_Programming_Task.View.Dialog;
using Profisys_Programming_Task.ViewModel.DialogViewModel;

namespace Profisys_Programming_Task.ViewModel
{
    internal partial class DataViewModel : ObservableObject
    {
        private readonly AppDbContext _appDbContext;
        public ObservableCollection<Documents> _Documents { get; set; }

        private Documents? _selectedDocument;
        public Documents? SelectedDocument
        {
            get { return _selectedDocument; }
            set
            {
                if (_selectedDocument != value)
                {
                    _selectedDocument = value;
                    ShowDocumentDetailsCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public RelayCommand BackToMenuCommand { get; }
        public RelayCommand ShowDocumentDetailsCommand { get; }
        public DataViewModel(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _Documents = new ObservableCollection<Documents>();
            BackToMenuCommand = new RelayCommand(SwitchToMainMenu);
            ShowDocumentDetailsCommand = new RelayCommand(ShowDocumentDetails, CanShowDocumentDetails);
        }

        public async Task LoadDocuments()
        {
            _Documents = new ObservableCollection<Documents>(await _appDbContext.Documents.ToListAsync());
            OnPropertyChanged(nameof(_Documents));
        }

        public void SwitchToMainMenu()
        {
            Application.Current.MainWindow.DataContext = new MainMenuViewModel();
        }

        private void ShowDocumentDetails()
        {
            if (_selectedDocument != null)
            {
                var detailsDialog = new DocumentsItemDialogView();
                var detailsDialogViewModel = new DocumentsItemDialogViewModel(_appDbContext, _selectedDocument, detailsDialog);
                detailsDialog.DataContext = detailsDialogViewModel;

                detailsDialogViewModel.LoadItems();
                detailsDialog.ShowDialog();
            }
        }
        private bool CanShowDocumentDetails()
        {
            return _selectedDocument != null;
        }
    }
}
