using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profisys_Programming_Task.Service.Exeptions
{
    internal class DatabaseException : Exception
    {
        public DatabaseException(string message) : base(message) { }

        public DatabaseException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
