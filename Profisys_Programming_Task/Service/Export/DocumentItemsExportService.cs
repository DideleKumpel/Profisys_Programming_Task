using CsvHelper;
using Profisys_Programming_Task.Model;
using Profisys_Programming_Task.Service.Export.Maps;
using System.IO;

namespace Profisys_Programming_Task.Service.Export
{
    internal class DocumentItemsExportService: ExportServiceBase<DocumentItems>
    {
        public override async Task ExportToCsvAsync(IEnumerable<DocumentItems> items, string filePath)
        {
            await using StreamWriter writer = new StreamWriter(filePath);
            await using CsvWriter csv = new CsvWriter(writer, _csvConfiguration);

            csv.Context.RegisterClassMap<DocumentItemsExportMap>();
            await csv.WriteRecordsAsync(items);
        }
    }
}
