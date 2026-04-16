using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        T Update(T item);
        T Update(int id, T item);
        int UpdateMany(List<T> items, bool CancelOnError);

        //Delete
        bool Delete(int id);
        bool Delete(T item);

        //Transations
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();

    }
}
