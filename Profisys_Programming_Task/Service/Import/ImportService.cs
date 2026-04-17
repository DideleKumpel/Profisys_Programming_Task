using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profisys_Programming_Task.Service.Import
{
    internal interface ImportService<T> where T : class
    {
        public Task<List<T>> ImportDocumentsFromCsvAsync(string filePath);
    }
}
