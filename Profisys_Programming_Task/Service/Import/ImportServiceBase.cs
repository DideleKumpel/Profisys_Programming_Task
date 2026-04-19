using CsvHelper.Configuration;
using System.Globalization;
using System.IO;

namespace Profisys_Programming_Task.Service.Import
{
    internal abstract class ImportServiceBase<T> : IImportService<T> where T : class
    {
        protected CsvConfiguration _csvConfiguration;

        public ImportServiceBase()
        {
            _csvConfiguration = new CsvConfiguration(new System.Globalization.CultureInfo("pl-PL"))
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                Delimiter = ";"
            };
        }

        public ImportServiceBase(CsvConfiguration csvConfiguration)
        {
            _csvConfiguration = csvConfiguration ?? throw new ArgumentNullException(nameof(csvConfiguration));
        }

        protected void ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }
            if (!System.IO.File.Exists(filePath))
            {
                throw new System.IO.FileNotFoundException($"The file '{filePath}' does not exist.");
            }
        }
        protected void ValidateCsvFilePath(string filePath)
        {
            ValidateFilePath(filePath);

            if (!Path.GetExtension(filePath).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Invalid file format. Only CSV files are supported.");
            }
        }
        public abstract Task<List<T>> ImportFromCsvAsync(string filePath, CancellationToken cancellationToken);

        public abstract Task<bool> CanImportAsync(string filePath);

        protected abstract bool IsValidFormat(string[] headers);
    }
}
