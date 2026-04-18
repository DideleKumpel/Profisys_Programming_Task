using CsvHelper;
using CsvHelper.Configuration;
using Profisys_Programming_Task.Model;
using System.IO;

namespace Profisys_Programming_Task.Service.Import
{
    internal class DocumentItemsImportService: ImportServiceBase<DocumentItems>
    {
        public DocumentItemsImportService() : base() { }

        public DocumentItemsImportService(CsvConfiguration csvConfiguration) : base(csvConfiguration) { }

        public override async Task<List<DocumentItems>> ImportFromCsvAsync(string filePath, CancellationToken cancellationToken)
        {
            ValidateCsvFilePath(filePath);

            using StreamReader reader = new StreamReader(filePath);
            using CsvReader csv = new CsvReader(reader, _csvConfiguration);
            try
            {
                await csv.ReadAsync();
                csv.ReadHeader();
            }
            catch
            {
                throw new InvalidDataException("Could not read CSV headers");
            }
            string[] headers = csv.HeaderRecord;
            if (!IsValidFormat(headers))
            {
                throw new InvalidDataException("Invalid CSV format for documents.");
            }
            List<DocumentItems> importedItems = new List<DocumentItems>();
            while (await csv.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();
                DocumentItems item = csv.GetRecord<DocumentItems>();
                importedItems.Add(item);
            }
            return importedItems;
        }

        public override async Task<bool> CanImportAsync(string filePath)
        {
            try
            {
                ValidateCsvFilePath(filePath);

                using StreamReader reader = new StreamReader(filePath);
                using CsvReader csv = new CsvReader(reader, _csvConfiguration);

                await csv.ReadAsync();
                csv.ReadHeader();

                return IsValidFormat(csv.HeaderRecord);
            }
            catch
            {
                return false;
            }
        }

        protected override bool IsValidFormat(string[] headers)
        {
            return headers.Contains("DocumentId") && headers.Contains("Ordinal") && headers.Contains("Product") && headers.Contains("Quantity") && headers.Contains("Price") && headers.Contains("TaxRate");
        }
    }
}
