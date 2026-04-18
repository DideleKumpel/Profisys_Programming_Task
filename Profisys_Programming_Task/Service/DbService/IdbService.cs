namespace Profisys_Programming_Task.Service.DbService
{
    internal interface IDbService<T>
    {
        public Task<T> GetByIdAsync(int id);
        public Task<List<T>> GetAllAsync();
        Task<bool> ExistsAsync(int id);

        //Create
        Task<T> AddAsync(T item);
        Task<int> AddManyAsync(List<T> items, bool CancelOnError);
        //Update
        Task<bool> UpdateAsync(T item);
        Task<bool> UpdateAsync(int id, T item);
        Task<int> UpdateManyAsync(List<T> items, bool CancelOnError);
        //Delete
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteAsync(T item);
        Task<int> DeleteManyAsync(List<T> items, bool CancelOnError);

        //Transations
        Task BeginTransactionAsync();
        Task CommitTransactionAsync(bool SaveChanges = true);
        Task RollbackTransactionAsync();
    }
}
