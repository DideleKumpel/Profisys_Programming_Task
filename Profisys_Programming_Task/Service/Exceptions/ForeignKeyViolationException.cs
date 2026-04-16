namespace Profisys_Programming_Task.Service.Exceptions
{
    internal class ForeignKeyViolationException: DatabaseException
    {
        public ForeignKeyViolationException(string message, Exception innerException)
            : base(message, innerException) { }

        public ForeignKeyViolationException(Exception innerException)
            : base("This operation violates a foreign key constraint (related records exist).", innerException) { }
    }
}
