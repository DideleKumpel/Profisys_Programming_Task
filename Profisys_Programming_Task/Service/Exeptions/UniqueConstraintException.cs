using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profisys_Programming_Task.Service.Exeptions
{
    internal class UniqueConstraintException: DatabaseException
    {
        public UniqueConstraintException(string message, Exception innerException)
            : base(message, innerException) { }

        public UniqueConstraintException(Exception innerException)
            : base("An item with the same unique value already exists in the database.", innerException) { }
    }
}
