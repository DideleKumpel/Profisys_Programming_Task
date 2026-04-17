using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profisys_Programming_Task.Service.Import
{
    internal interface IImportService<T> where T : class
    {
        public List<T> ImportDocumentsFromCsv(string filePath);
        public Task<List<T>> ImportDocumentsFromCsvAsync(string filePath);
    }
}
