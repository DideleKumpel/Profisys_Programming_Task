namespace Profisys_Programming_Task.Service.DbService
{
    internal interface IDbService<T>
    {
        public T GetById(int id);
        public List<T> GetAll();
        bool Exists(int id);

        //Create
        T Add(T item);
        int AddMany(List<T> items, bool CancelOnError);

        //Update
        bool Update(T item);
        bool Update(int id, T item);
        int UpdateMany(List<T> items, bool CancelOnError);

        //Delete
        bool Delete(int id);
        bool Delete(T item);
        int DeleteMany(List<T> items, bool CancelOnError);

        //Transations
        void BeginTransaction();
        void CommitTransaction(bool SaveChanges = true);
        void RollbackTransaction();

    }
}
