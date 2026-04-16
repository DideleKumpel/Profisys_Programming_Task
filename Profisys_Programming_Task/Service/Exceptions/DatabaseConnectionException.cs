
namespace Profisys_Programming_Task.Service.Exceptions
{
    internal class DatabaseConnectionException: DatabaseException
    {
        public DatabaseConnectionException(string message, Exception innerException)
            : base(message, innerException) { }

        // Default constructor for general connection issues
        public DatabaseConnectionException(Exception innerException)
            : base("Unable to connect to the database server.", innerException) { }
    }
}
