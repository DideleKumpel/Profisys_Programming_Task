using CsvHelper.Configuration;
using System.Globalization;
using System.IO;

namespace Profisys_Programming_Task.Service.Import
{
    internal class ImportServiceBase<T> : IImportService<T> where T : class
    {
        protected CsvConfiguration _csvConfiguration;

        public ImportServiceBase()
        {
            _csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
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
        public virtual async Task<List<T>> ImportFromCsvAsync(string filePath, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> CanImportAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        protected virtual bool IsValidFormat(string[] headers)
        {
            throw new NotImplementedException();
        }
    }
}
