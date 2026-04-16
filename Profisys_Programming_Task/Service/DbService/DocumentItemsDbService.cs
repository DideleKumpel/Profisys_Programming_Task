using Microsoft.EntityFrameworkCore;
using Profisys_Programming_Task.Model;
using Profisys_Programming_Task.Service.Exceptions;

namespace Profisys_Programming_Task.Service.DbService
{
    internal class DocumentItemsDbService : BaseDbService<DocumentItems>
    {
        public DocumentItemsDbService(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        //Get
        public override DocumentItems GetById(int id)
        {
            DocumentItems documentItemsFound = null;
            try
            {
                documentItemsFound = _appDbContext.DocumentItems.Where(item => item.Id == id).FirstOrDefault();
                if (documentItemsFound == null)
                {
                    throw new EntityNotFoundException("DocumentItems", id);
                }
                return documentItemsFound;
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        public override List<DocumentItems> GetAll()
        {
            try
            {
                return _appDbContext.DocumentItems.ToList();
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        public override bool Exists(int id)
        {
            try
            {
                return _appDbContext.DocumentItems.Any(item => item.Id == id);
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        //Insert
        public override DocumentItems Add(DocumentItems item)
        {
            try
            {
                DocumentItems addedItem = _appDbContext.DocumentItems.Add(item).Entity;
                _appDbContext.SaveChanges();
                return addedItem;
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
            
        }

        public override int AddMany(List<DocumentItems> items, bool CancelOnError)
        {
            BeginTransaction();
            int succes = 0;
            foreach (DocumentItems item in items)
            {
                try
                {
                    Add(item);
                    succes++;
                }
                catch (Exception error)
                {
                    _appDbContext.Entry(item).State = EntityState.Detached;
                    if (CancelOnError)
                    {
                        RollbackTransaction();
                        HandleException(error);
                    }
                }
            }
            CommitTransaction(false);
            return succes;
        }

        //Update
        public override bool Update(DocumentItems item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            try
            {
                _appDbContext.DocumentItems.Update(item);
                _appDbContext.SaveChanges();
                return true;
            }
            catch (Exception error)
            {
                HandleException(error);
                return false;
            }
        }

        public override bool Update(int id, DocumentItems item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            try
            {
                item.Id = id;
                return Update(item);
            }
            catch (Exception error)
            {
                HandleException(error);
                return false;
            }
        }

        public override int UpdateMany(List<DocumentItems> items, bool CancelOnError)
        {
            if (items == null)
            {
                throw new ArgumentException(nameof(items));
            }
            BeginTransaction();
            int succes = 0;
            foreach (DocumentItems item in items)
            {
                try
                {
                    Update(item);
                    succes++;
                }
                catch (Exception error)
                {
                    _appDbContext.Entry(item).State = EntityState.Unchanged;
                    if (CancelOnError)
                    {
                        RollbackTransaction();
                        HandleException(error);
                    }
                }
            }
            CommitTransaction(false);
            return succes;
        }

        //Delete
        public override bool Delete(DocumentItems item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            try
            {
                _appDbContext.DocumentItems.Remove(item);
                _appDbContext.SaveChanges();
                return true;
            }
            catch (Exception error)
            {
                HandleException(error);
                return false;
            }
        }

        public override bool Delete(int id)
        {
            DocumentItems itemToDelete = GetById(id);
            return Delete(itemToDelete);
        }

        public override int DeleteMany(List<DocumentItems> items, bool CancelOnError)
        {
            if (items == null)
            {
                throw new ArgumentException(nameof(items));
            }
            BeginTransaction();
            int succes = 0;
            foreach (DocumentItems item in items)
            {
                try
                {
                    Delete(item);
                    succes++;
                }
                catch (Exception error)
                {
                    _appDbContext.Entry(item).State = EntityState.Unchanged;
                    if (CancelOnError)
                    {
                        RollbackTransaction();
                        HandleException(error);
                    }
                }
            }
            CommitTransaction(false);
            return succes;
        }

    }
}
