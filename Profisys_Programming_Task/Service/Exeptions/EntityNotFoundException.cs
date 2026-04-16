using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profisys_Programming_Task.Service.Exeptions
{
    internal class EntityNotFoundException: DatabaseException
    {
        public string EntityName { get; }
        public object EntityKey { get; }

        public EntityNotFoundException(string entityName, object key)
            : base($"Entity '{entityName}' with ID '{key}' was not found in the database.")
        {
            EntityName = entityName;
            EntityKey = key;
        }
    }
}
