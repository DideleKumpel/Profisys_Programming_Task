using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Profisys_Programming_Task.Model;
using Profisys_Programming_Task.Service.DbService;
using Profisys_Programming_Task.Service.Exceptions;
using Profisys_Programming_Task.View.Dialog;
using Profisys_Programming_Task.ViewModel.DialogViewModel;
using Profisys_Programming_Task.ViewModel.Filters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Profisys_Programming_Task.ViewModel
{
    internal partial class DataViewModel : ObservableObject
    {
        //SERVICES
        private readonly IDbService<DocumentItems> _documentItemsDbService;
        private readonly IDbService<Documents> _documentDbService;
        private ObservableCollection<Documents> _documents;

        //DATAGRID
        [ObservableProperty]
        private ObservableCollection<Documents> _documentsDisplay;

        [ObservableProperty]
        private Documents? _selectedDocument;

        //FILTERS
        private IQueryable<Documents> _filtersQuery;
        [ObservableProperty]
        private DocumentFilters _documentDisplayFilters = new DocumentFilters();

        //CONSTRUCTOR
        public DataViewModel(IDbService<Documents> docService, IDbService<DocumentItems> itemService)
        {
            _documentDbService = docService;
            _documentItemsDbService = itemService;
        }

        private void FetchDocuments()
        {
            try
            {
                List<Documents> documentsList = _documentDbService.GetAll();
                _documents = new ObservableCollection<Documents>(documentsList);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SwitchToMainMenu();
            }
        }

        private void LoadFiltredDocuments()
        {
            try
            {
                if (_filtersQuery != null)
                {
                    DocumentsDisplay = new ObservableCollection<Documents>(_filtersQuery.ToList());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect to the database. Please check your connection and try again.");
            }
        }

        private void LoadDocuments()
        {
            DocumentsDisplay = _documents;
        }   

        [RelayCommand]
        private void RefreshData()
        {
            FetchDocuments();
            if (DocumentDisplayFilters.FiltersAreSet() && _filtersQuery != null)
            {
                LoadFiltredDocuments();
            }
            else
            {
                LoadDocuments();
            }
        }

        [RelayCommand]
        private void SwitchToMainMenu()
        {
            var mainMenuViewModel = ((App)Application.Current).ServiceProvider.GetService<MainMenuViewModel>();
            Application.Current.MainWindow.DataContext = mainMenuViewModel;
        }

        [RelayCommand]
        private void DeleteDocument()
        {
            if (SelectedDocument != null)
            {
                string message = $"Are you sure you want to delete this document?\n\n" +
                                    $"Id: {SelectedDocument.Id}\n" +
                                    $"Type: {SelectedDocument.Type}\n" +
                                    $"First name: {SelectedDocument.FirstName}\n" +
                                    $"Last name: {SelectedDocument.LastName}\n" +
                                    $"City: {SelectedDocument.City}";

                var result = MessageBox.Show(message, "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        _documentDbService.Delete(SelectedDocument);
                    }
                    catch(EntityNotFoundException error)
                    {
                        MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        RefreshData();
                        return;
                    }
                    catch (DatabaseConnectionException error)
                    {
                        MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        SwitchToMainMenu();
                        return;
                    }
                    catch(Exception error)
                    {
                        MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    MessageBox.Show("Document deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshData();
                }
                else
                {
                    return;
                }
            }
            else
            {
                MessageBox.Show("No document selected for deletion.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ShowDocumentDetails()
        {
            if (_selectedDocument != null)
            {
                var detailsDialog = new DocumentsItemDialogView();
                var detailsDialogViewModel = new DocumentsItemDialogViewModel(new AppDbContext(), _selectedDocument, detailsDialog);
                detailsDialog.DataContext = detailsDialogViewModel;

                detailsDialogViewModel.LoadItemsAsync();
                detailsDialog.ShowDialog();
            }
            else
            {
                MessageBox.Show("No document selected for deletion.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanShowDocumentDetails()
        {
            return _selectedDocument != null;
        }

        //FILTERS FUNCTION 
        [RelayCommand]
        private void ApplyFilters()
        {
            IQueryable<Documents> query = _documents.AsQueryable();
            if (DocumentDisplayFilters.StartDateFilter <= DocumentDisplayFilters.EndDateFilter)
            {
                if (DocumentDisplayFilters.StartDateFilter != DateTime.MinValue)
                {
                    query = query.Where(d => d.Date >= DocumentDisplayFilters.StartDateFilter);
                }
                if (DocumentDisplayFilters.EndDateFilter != DateTime.MinValue)
                {
                    query = query.Where(d => d.Date <= DocumentDisplayFilters.EndDateFilter);
                }

            }else
            {
                MessageBox.Show("Time interval not set correctly");
                return;
            }

            if (DocumentDisplayFilters.SelectedDocumentType != DocumentTypeFilters.All)
            {
                DocumentType documentType = (DocumentType)DocumentDisplayFilters.SelectedDocumentType;
                query = query.Where(d => d.Type == documentType);
            }

            if (!string.IsNullOrEmpty(DocumentDisplayFilters.FirstNameFilter))
            {
                query = query.Where(d => d.FirstName.ToLower().Contains(DocumentDisplayFilters.FirstNameFilter.ToLower()));
            }

            if (!string.IsNullOrEmpty(DocumentDisplayFilters.LastNameFilter))
            {
                query = query.Where(d => d.LastName.ToLower().Contains(DocumentDisplayFilters.LastNameFilter.ToLower()));
            }

            if (!string.IsNullOrEmpty(DocumentDisplayFilters.CityFilter))
            {
                query = query.Where(d => d.City.ToLower().Contains(DocumentDisplayFilters.CityFilter.ToLower()));
            }
            
            _filtersQuery = query;
            RefreshData();
        }

        [RelayCommand]
        private void ClearFilters()
        {
            DocumentDisplayFilters.SelectedDocumentType = DocumentTypeFilters.All;
            DocumentDisplayFilters.FirstNameFilter = string.Empty;
            DocumentDisplayFilters.LastNameFilter = string.Empty;
            DocumentDisplayFilters.CityFilter = string.Empty;
            DocumentDisplayFilters.StartDateFilter = DateTime.MinValue;
            DocumentDisplayFilters.EndDateFilter = DateTime.Today;
            _filtersQuery = null;
            RefreshData();
        }
    }
}
