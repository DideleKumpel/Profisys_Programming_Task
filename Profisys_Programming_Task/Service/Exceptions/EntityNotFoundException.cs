namespace Profisys_Programming_Task.Service.Exceptions
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
