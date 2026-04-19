using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profisys_Programming_Task.Service.Export
{
    internal interface IExportService<T>
    {
        Task ExportToCsvAsync(IEnumerable<T> data, string filePath);
    }
}
