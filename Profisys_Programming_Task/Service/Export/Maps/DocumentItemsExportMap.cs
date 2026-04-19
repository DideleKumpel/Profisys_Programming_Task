using CsvHelper.Configuration;
using Profisys_Programming_Task.Model;

namespace Profisys_Programming_Task.Service.Export.Maps
{
    internal class DocumentItemsExportMap: ClassMap<DocumentItems>
    {
        public DocumentItemsExportMap()
        {
            Map(m => m.DocumentId);
            Map(m => m.Ordinal);
            Map(m => m.Product);
            Map(m => m.Quantity);
            Map(m => m.Price);
            Map(m => m.TaxRate);
        }
    }
}
