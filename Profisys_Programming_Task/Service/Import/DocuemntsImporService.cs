using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Profisys_Programming_Task.Model;

namespace Profisys_Programming_Task.Service.Import
{
    internal class DocuemntsImporService : IImportService<DocumentItems>
    {

        public DocuemntsImporService() { }

        public List<DocumentItems> ImportDocumentsFromCsv(string filePath)
        {
            throw new NotImplementedException();
        }

        public async Task<List<DocumentItems>> ImportDocumentsFromCsvAsync(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
