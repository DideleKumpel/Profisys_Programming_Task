namespace Profisys_Programming_Task.Service.Export
{
    internal interface IExportService<T>
    {
        Task ExportToCsvAsync(IEnumerable<T> data, string filePath);
    }
}
