using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;

namespace Profisys_Programming_Task.Service.Export
{
    internal class ExportServiceBase<T> : IExportService<T> where T : class
    {
        protected readonly CsvConfiguration _csvConfiguration;

        public ExportServiceBase()
        {
            _csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            };
        }

        public virtual async Task ExportToCsvAsync(IEnumerable<T> items, string filePath)
        {
            await using StreamWriter writer = new StreamWriter(filePath);
            await using CsvWriter csv = new CsvWriter(writer, _csvConfiguration);
            await csv.WriteRecordsAsync(items);
        }
    }
}
