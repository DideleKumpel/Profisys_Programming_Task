using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profisys_Programming_Task.Service.Exeptions
{
    internal class ForeignKeyViolationException: DatabaseException
    {
        public ForeignKeyViolationException(string message, Exception innerException)
            : base(message, innerException) { }

        public ForeignKeyViolationException(Exception innerException)
            : base("This operation violates a foreign key constraint (related records exist).", innerException) { }
    }
}
