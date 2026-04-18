using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profisys_Programming_Task.Model.DTO
{
    public class ImportResult<T>
    {
        public T Item { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }

        public ImportResult(T item, bool isSuccess, string? errorMessage = null)
        {
            Item = item;
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }
    }
}
