using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Profisys_Programming_Task.Model;
using Profisys_Programming_Task.Service.DbService;
using Profisys_Programming_Task.Service.Exceptions;
using Profisys_Programming_Task.Service.Export;
using Profisys_Programming_Task.View.Dialog;
using Profisys_Programming_Task.ViewModel.DialogViewModel;
using Profisys_Programming_Task.ViewModel.Filters;
using System.Collections.ObjectModel;
using System.Windows;

namespace Profisys_Programming_Task.ViewModel
{
    internal partial class DataViewModel : ObservableObject
    {
        //SERVICES
        private readonly IDbService<Documents> _documentDbService;
        private ObservableCollection<Documents> _documents;

        //DATAGRID
        [ObservableProperty]
        private ObservableCollection<Documents> _documentsDisplay;

        [ObservableProperty]
        private Documents? _selectedDocument;

        partial void OnSelectedDocumentChanged(Documents? value)
        {
            DeleteDocumentCommand.NotifyCanExecuteChanged();
            ShowDocumentDetailsCommand.NotifyCanExecuteChanged();
        }

        //FILTERS
        private IQueryable<Documents> _filtersQuery;
        [ObservableProperty]
        private DocumentFilters _documentDisplayFilters = new DocumentFilters();

        //CONSTRUCTOR
        public DataViewModel(IDbService<Documents> docService)
        {
            _documentDbService = docService;
        }

        private async Task FetchDocumentsAsync()
        {
            try
            {
                List<Documents> documentsList = await _documentDbService.GetAllAsync();
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
        private async Task RefreshDataAsync()
        {
            await FetchDocumentsAsync();
            if (DocumentDisplayFilters.FiltersAreSet())
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

        [RelayCommand(CanExecute = nameof(DocumentIsSelected))]
        private async Task DeleteDocumentAsync()
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
                        await _documentDbService.DeleteAsync(SelectedDocument);
                    }
                    catch(EntityNotFoundException error)
                    {
                        MessageBox.Show(error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        await RefreshDataAsync();
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
                    await RefreshDataAsync();
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

        [RelayCommand(CanExecute = nameof(DocumentIsSelected))]
        private async Task ShowDocumentDetailsAsync()
        {
            if (_selectedDocument != null)
            {
                var detailsDialog = new DocumentsItemDialogView();
                IDocumentItemsDbService documentItemsDbService = ((App)Application.Current).ServiceProvider.GetService<IDocumentItemsDbService>();

                var detailsDialogViewModel = new DocumentsItemDialogViewModel(documentItemsDbService, new DocumentPdfExportService(), _selectedDocument, detailsDialog);
                detailsDialog.DataContext = detailsDialogViewModel;

                await detailsDialogViewModel.LoadItemsAsync();
                detailsDialog.ShowDialog();
            }
            else
            {
                MessageBox.Show("No document selected for deletion.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool DocumentIsSelected()
        {
            return SelectedDocument != null;
        }

        //FILTERS  
        [RelayCommand]
        private async Task ApplyFiltersAsync()
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
            await RefreshDataAsync();
        }

        [RelayCommand]
        private async Task ClearFiltersAsync()
        {
            DocumentDisplayFilters.SelectedDocumentType = DocumentTypeFilters.All;
            DocumentDisplayFilters.FirstNameFilter = string.Empty;
            DocumentDisplayFilters.LastNameFilter = string.Empty;
            DocumentDisplayFilters.CityFilter = string.Empty;
            DocumentDisplayFilters.StartDateFilter = DateTime.MinValue;
            DocumentDisplayFilters.EndDateFilter = DateTime.Today;
            await RefreshDataAsync();
        }
    }
}
