using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Profisys_Programming_Task.Service.DbService
{
    internal class BaseDbService<T> : IDbService<T>
    {
        protected readonly AppDbContext _appDbContext;
        private IDbContextTransaction? _transaction;
        public BaseDbService(AppDbContext appDbContext)
        {
            this._appDbContext = appDbContext;
        }

        public virtual void BeginTransaction()
        {
            _transaction = _appDbContext.Database.BeginTransaction();
        }

        public virtual void CommitTransaction(bool SaveChanges = true)
        {
            try
            {
                if (SaveChanges)
                {
                    _appDbContext.SaveChanges();
                }
                _transaction?.Commit();
            }
            catch
            {
                _transaction?.Rollback();
                throw;
            }
        }
        public virtual void RollbackTransaction()
        {
            _transaction?.Rollback();
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

        public virtual int DeleteMany(List<T> items, bool CancelOnError)
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

        public virtual bool Update(T item)
        {
            throw new NotImplementedException();
        }

        public virtual bool Update(int id, T item)
        {
            throw new NotImplementedException();
        }

        public virtual int UpdateMany(List<T> items, bool CancelOnError)
        {
            throw new NotImplementedException();
        }  
    }
}
