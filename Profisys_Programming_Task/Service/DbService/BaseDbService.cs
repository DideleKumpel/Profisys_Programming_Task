using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Profisys_Programming_Task.Service.DbService
{
    internal class BaseDbService<T> : IDbService<T>
    {
        public readonly AppDbContext _appDbContext;
        public BaseDbService(AppDbContext appDbContext)
        {
            this._appDbContext = appDbContext;
        }

        public virtual void BeginTransaction()
        {
            _appDbContext.Database.BeginTransaction();
        }

        public virtual void CommitTransaction()
        {
            try
            {
                _appDbContext.SaveChanges();
                _appDbContext.Database.CommitTransaction();
            }
            catch (Exception e)
            {
                RollbackTransaction();
                throw e;
            }
        }
        public virtual void RollbackTransaction()
        {
            _appDbContext.Database.RollbackTransaction();
        }
        public virtual T Add(T item)
        {
            throw new NotImplementedException();
        }

        public virtual int AddMany(List<T> items, bool CancelOnError)
        {
            throw new NotImplementedException();
        }

        public virtual bool Delete(int id)
        {
            throw new NotImplementedException();
        }

        public virtual bool Delete(T item)
        {
            throw new NotImplementedException();
        }

        public virtual bool Exists(int id)
        {
            throw new NotImplementedException();
        }

        public virtual List<T> GetAll()
        {
            throw new NotImplementedException();
        }

        public virtual T GetById(int id)
        {
            throw new NotImplementedException();
        }

        public virtual T Update(T item)
        {
            throw new NotImplementedException();
        }

        public virtual T Update(int id, T item)
        {
            throw new NotImplementedException();
        }

        public virtual int UpdateMany(List<T> items, bool CancelOnError)
        {
            throw new NotImplementedException();
        }
    }
}
