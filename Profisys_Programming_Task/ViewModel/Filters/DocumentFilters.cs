using CommunityToolkit.Mvvm.ComponentModel;
using Profisys_Programming_Task.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profisys_Programming_Task.ViewModel.Filters
{
    internal partial class DocumentFilters: ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<DocumentTypeFilters> _documentTypes = new ObservableCollection<DocumentTypeFilters>(Enum.GetValues(typeof(DocumentTypeFilters)).Cast<DocumentTypeFilters>());
        [ObservableProperty]
        private DocumentTypeFilters _selectedDocumentType = DocumentTypeFilters.All;
        [ObservableProperty]
        private string _firstNameFilter = string.Empty;
        [ObservableProperty]
        private string _lastNameFilter = string.Empty;
        [ObservableProperty]
        private string _cityFilter = string.Empty;
        [ObservableProperty]
        private DateTime _startDateFilter = DateTime.MinValue;
        [ObservableProperty]
        private DateTime _endDateFilter = DateTime.Today;

        public bool FiltersAreSet()
        {
            bool filtersSet = false;
            if(SelectedDocumentType != DocumentTypeFilters.All)
            {
                filtersSet = true;
            }
            if(!string.IsNullOrWhiteSpace(FirstNameFilter))
            {
                filtersSet = true;
            }
            if(!string.IsNullOrWhiteSpace(LastNameFilter))
            {
                filtersSet = true;
            }
            if(!string.IsNullOrWhiteSpace(CityFilter))
            {
                filtersSet = true;
            }
            if(StartDateFilter != DateTime.MinValue)
            {
                filtersSet = true;
            }
            if(EndDateFilter != DateTime.Today)
            {
                filtersSet = true;
            }
            return filtersSet;
        }
    }
}
