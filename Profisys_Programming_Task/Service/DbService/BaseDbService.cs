using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Profisys_Programming_Task.Service.Exceptions;

namespace Profisys_Programming_Task.Service.DbService
{
    internal class BaseDbService<T> : IDbService<T> where T : class
    {
        protected readonly AppDbContext _appDbContext;
        private IDbContextTransaction? _transaction;
        public BaseDbService(AppDbContext appDbContext)
        {
            this._appDbContext = appDbContext;
        }

        protected void HandleException(Exception exception)
        {
            if (exception is ArgumentNullException)
            {
                throw exception;
            }else if (exception is DatabaseException)
            {
                throw exception;
            }
            else if(exception is DbUpdateException dbUpdateException)
            {
                if (exception.InnerException is SqlException sqlException)
                {
                    switch (sqlException.Number)
                    {
                        case 2627:
                        case 2601:
                            throw new UniqueConstraintException(sqlException);

                        case 547:
                            throw new ForeignKeyViolationException(sqlException);

                        case 2:
                        case 40:
                        case 53:
                        case 4060:
                            throw new DatabaseConnectionException(sqlException);
                    }
                }
                throw new DatabaseException("A database update error occurred.", dbUpdateException);
            }
            else if(exception is DbUpdateConcurrencyException dbUpdateConcurrencyException)
            {
                throw new DatabaseException("The data was modified by another user. Please refresh.", dbUpdateConcurrencyException);
            }
            else if(exception is DatabaseException)
            {
                throw exception;
            }
            else
            {
                throw new DatabaseException("An unexpected database error occurred.", exception);
            }
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
        public virtual Task<T> AddAsync(T item)
        {
            throw new NotImplementedException();
        }

        public virtual Task<int> AddManyAsync(List<T> items, bool CancelOnError)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> DeleteAsync(T item)
        {
            throw new NotImplementedException();
        }

        public virtual Task<int> DeleteManyAsync(List<T> items, bool CancelOnError)
        {
            throw new NotImplementedException();
        }
        public virtual Task<bool> ExistsAsync(int id)
        {
            throw new NotImplementedException();
        }

        public virtual Task<List<T>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public virtual Task<T> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> UpdateAsync(T item)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> UpdateAsync(int id, T item)
        {
            throw new NotImplementedException();
        }

        public virtual Task<int> UpdateManyAsync(List<T> items, bool CancelOnError)
        {
            throw new NotImplementedException();
        }  
    }
}