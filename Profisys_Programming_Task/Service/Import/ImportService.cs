
namespace Profisys_Programming_Task.Service.Import
{
    internal interface ImportService<T> where T : class
    {
        public Task<List<T>> ImportFromCsvAsync(string filePath);
    }
}
