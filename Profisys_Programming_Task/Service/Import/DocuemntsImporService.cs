using CsvHelper;
using CsvHelper.Configuration;
using Profisys_Programming_Task.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profisys_Programming_Task.Service.Import
{
    internal class DocuemntsImportService : ImportServiceBase<Documents>
    {
        public DocuemntsImportService():base() { }

        public DocuemntsImportService(CsvConfiguration csvConfiguration): base(csvConfiguration) {}

        public override async Task<List<Documents>> ImportDocumentsFromCsvAsync(string filePath)
        {
            ValidateCsvFilePath(filePath);

            using StreamReader reader = new StreamReader(filePath);
            using CsvReader csv = new CsvReader(reader, _csvConfiguration);
            csv.ReadHeader();
            string[] headers = csv.HeaderRecord;
            if (!IsValidDocumentsCsv(headers)) //documents table
            {
                throw new InvalidDataException("Invalid CSV format for documents.");
            }
            List<Documents> importedItems = new List<Documents>();
            while (await csv.ReadAsync())
            {
                Documents item = csv.GetRecord<Documents>();
                importedItems.Add(item);
            }
            return importedItems;
        }


        private bool IsValidDocumentsCsv(string[] headers)
        {
            return headers.Contains("Id") && headers.Contains("Type") && headers.Contains("Date") && headers.Contains("FirstName") && headers.Contains("LastName") && headers.Contains("City");
        }
    }
}
