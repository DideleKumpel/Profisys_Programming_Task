using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
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
        //SERVICES
        private readonly AppDbContext _appDbContext;

        //DATAGRID
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
                    DeleteDocumentCommand.NotifyCanExecuteChanged();
                }
            }
        }
        //FILTERS
        private bool FiltersAreSet = false;
        private IQueryable<Documents> FiltersQuery;
        public ObservableCollection<DocumentTypeFilters> _documentTypes { get; set; }
        private DocumentTypeFilters _selectedDocumentType;
        public DocumentTypeFilters SelectedDocumentType
        {
            get => _selectedDocumentType;
            set
            {
                if (_selectedDocumentType != value)
                {
                    _selectedDocumentType = value;
                    OnPropertyChanged(nameof(SelectedDocumentType));
                }
            }
        }

        private string _firstNameFilter;
        public string FirstNameFilter
        {
            get => _firstNameFilter;
            set
            {
                if (_firstNameFilter != value)
                {
                    _firstNameFilter = value;
                    OnPropertyChanged(nameof(FirstNameFilter));
                }
            }
        }
        private string _lastNameFilter;
        public string LastNameFilter
        {
            get => _lastNameFilter;
            set
            {
                if (_lastNameFilter != value)
                {
                    _lastNameFilter = value;
                    OnPropertyChanged(nameof(LastNameFilter));
                }
            }
        }
        private string _cityFilter;
        public string CityFilter
        {
            get => _cityFilter;
            set
            {
                if (_cityFilter != value)
                {
                    _cityFilter = value;
                    OnPropertyChanged(nameof(CityFilter));
                }
            }
        }
        private DateTime _startDateFilter;
        public DateTime StartDateFilter
        {
            get => _startDateFilter;
            set
            {
                if (value <= DateTime.Today) // Blokowanie przyszłych dat
                {
                    SetProperty(ref _startDateFilter, value);
                }
            }
        }
        private DateTime _endDateFilter;
        public DateTime EndDateFilter
        {
            get => _endDateFilter;
            set
            {
                if (value <= DateTime.Today) // Blokowanie przyszłych dat
                {
                    SetProperty(ref _endDateFilter, value);
                }
            }
        }


        //COMMANDS
        public RelayCommand BackToMenuCommand { get; }
        public RelayCommand ShowDocumentDetailsCommand { get; }
        public AsyncRelayCommand ApplayFiltersCommand { get; }
        public AsyncRelayCommand ClearFiltersCommand { get; }
        public AsyncRelayCommand RefreshDataCommand { get; }
        public AsyncRelayCommand DeleteDocumentCommand { get; }

        //CONSTRUCTOR
        public DataViewModel(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _Documents = new ObservableCollection<Documents>();

            //COMANDS
            BackToMenuCommand = new RelayCommand(SwitchToMainMenu);
            ShowDocumentDetailsCommand = new RelayCommand(ShowDocumentDetails, CanShowDocumentDetails);
            ApplayFiltersCommand = new AsyncRelayCommand(ApplayFiltersAsync);
            ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);
            RefreshDataCommand = new AsyncRelayCommand(RefreshDataAsync);
            DeleteDocumentCommand = new AsyncRelayCommand(DeleteDocumentAsync, CanShowDocumentDetails);

            //Transfer data from enum DocumentType to collection binded to Taks type filter combobox
            _documentTypes = new ObservableCollection<DocumentTypeFilters>(Enum.GetValues(typeof(DocumentTypeFilters)).Cast<DocumentTypeFilters>());
            EndDateFilter = DateTime.Today;
            SelectedDocumentType = DocumentTypeFilters.All;
        }

        public async Task LoadDocumentsAsync()
        {
            try
            {
                _Documents = new ObservableCollection<Documents>(await _appDbContext.Documents.ToListAsync());
                OnPropertyChanged(nameof(_Documents));
            }
            catch (Exception ex) {
                MessageBox.Show("Could not connect to the database. Please check your connection and try again.");
            }
        }

        public async Task LoadFiltredDocumentsAsync( )
        {
            try
            {
                if (FiltersQuery != null)
                {
                    _Documents = new ObservableCollection<Documents>(await FiltersQuery.ToListAsync());
                    OnPropertyChanged(nameof(_Documents));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect to the database. Please check your connection and try again.");
            }
        }

        

        public async Task RefreshDataAsync() {
            if (FiltersAreSet && FiltersQuery != null)
            {
                LoadFiltredDocumentsAsync();
            }
            else
            {
                LoadDocumentsAsync();
            }
        }

        public void SwitchToMainMenu()
        {
            Application.Current.MainWindow.DataContext = new MainMenuViewModel(_appDbContext);
        }

        private async Task DeleteDocumentAsync()
        {
            if(_selectedDocument != null)
            {
                string message = $"Are you sure you want to delete this document?\n\n" +
                                    $"Id: {_selectedDocument.Id}\n" +
                                    $"Type: {_selectedDocument.Type}\n" +
                                    $"First name: {_selectedDocument.FirstName}\n" +
                                    $"Last name: {_selectedDocument.LastName}\n" +
                                    $"City: {_selectedDocument.City}";

                var result = MessageBox.Show(message, "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        _appDbContext.DocumentItems.RemoveRange(_appDbContext.DocumentItems.Where(d => d.DocumentId == _selectedDocument.Id)); //Remove all item assigned to document
                        _appDbContext.Documents.Remove(_selectedDocument);
                        await _appDbContext.SaveChangesAsync();
                    }
                    catch
                    {
                        MessageBox.Show("Failed to delete Document.");
                        return;
                    }
                    MessageBox.Show("Document deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshDataAsync();
                }
                else
                {
                    return; // Exit function if Cancel is clicked
                }
            }
        }

        private void ShowDocumentDetails()
        {
            if (_selectedDocument != null)
            {
                var detailsDialog = new DocumentsItemDialogView();
                var detailsDialogViewModel = new DocumentsItemDialogViewModel(_appDbContext, _selectedDocument, detailsDialog);
                detailsDialog.DataContext = detailsDialogViewModel;

                detailsDialogViewModel.LoadItemsAsync();
                detailsDialog.ShowDialog();
            }
        }
        private bool CanShowDocumentDetails()
        {
            return _selectedDocument != null;
        }
        //FILTERS FUNCTION 
        private async Task ApplayFiltersAsync()
        {
            IQueryable<Documents> query = _appDbContext.Documents.AsQueryable();
            if (SelectedDocumentType != DocumentTypeFilters.All )
            {
                DocumentType documentType = (DocumentType)SelectedDocumentType;
                query = query.Where(d => d.Type == documentType);
            }
            if (!string.IsNullOrEmpty(_firstNameFilter))
            {
                query = query.Where(d => d.FirstName.Contains(_firstNameFilter));
            }
            if (!string.IsNullOrEmpty(_lastNameFilter))
            {
                query = query.Where(d => d.LastName.Contains(_lastNameFilter));
            }
            if (!string.IsNullOrEmpty(_cityFilter))
            {
                query = query.Where(d => d.City.Contains(_cityFilter));
            }
            if (StartDateFilter <= EndDateFilter) 
            {
                if (StartDateFilter != DateTime.MinValue)
                {
                    query = query.Where(d => d.Date >= StartDateFilter);
                }
                if (EndDateFilter != DateTime.MinValue)
                {
                    query = query.Where(d => d.Date <= EndDateFilter);
                }
                
            }else
            {
                MessageBox.Show("Time interval not set correctly");
                return;
            }
            FiltersAreSet = true;
            FiltersQuery = query;
            RefreshDataAsync();
            OnPropertyChanged(nameof(_Documents));
        }

        private async Task ClearFiltersAsync()
        {
            SelectedDocumentType = DocumentTypeFilters.All;
            FirstNameFilter = "";
            LastNameFilter = "";
            CityFilter = "";
            StartDateFilter = DateTime.MinValue;
            EndDateFilter = DateTime.Today;
            FiltersAreSet = false;
            FiltersQuery = null;
            LoadDocumentsAsync();
        }
    }
}
