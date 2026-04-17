using CsvHelper;
using CsvHelper.Configuration;
using Profisys_Programming_Task.Model;
using Profisys_Programming_Task.Model;
using System.IO;

namespace Profisys_Programming_Task.Service.Import
{
    internal class DocumentItemsImportService: ImportServiceBase<DocumentItems>
    {
        public DocumentItemsImportService() : base() { }

        public DocumentItemsImportService(CsvConfiguration csvConfiguration) : base(csvConfiguration) { }

        public override async Task<List<DocumentItems>> ImportFromCsvAsync(string filePath)
        {
            ValidateCsvFilePath(filePath);

            using StreamReader reader = new StreamReader(filePath);
            using CsvReader csv = new CsvReader(reader, _csvConfiguration);
            csv.ReadHeader();
            string[] headers = csv.HeaderRecord;
            if (!IsValidDocumentItemsCsv(headers))
            {
                throw new InvalidDataException("Invalid CSV format for documents.");
            }
            List<DocumentItems> importedItems = new List<DocumentItems>();
            while (await csv.ReadAsync())
            {
                DocumentItems item = csv.GetRecord<DocumentItems>();
                importedItems.Add(item);
            }
            return importedItems;
        }


        private bool IsValidDocumentItemsCsv(string[] headers)
        {
            return headers.Contains("DocumentId") && headers.Contains("Ordinal") && headers.Contains("Product") && headers.Contains("Quantity") && headers.Contains("Price") && headers.Contains("TaxRate");
        }
    }
}
