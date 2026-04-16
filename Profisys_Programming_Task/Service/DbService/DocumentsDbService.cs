using Microsoft.EntityFrameworkCore;
using Profisys_Programming_Task.Model;
using Profisys_Programming_Task.Service.Exceptions;

namespace Profisys_Programming_Task.Service.DbService
{
    internal class DocumentsDbService : BaseDbService<Documents>
    {
        public DocumentsDbService(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        //Get
        public override Documents GetById(int id)
        {
            try
            {
                Documents documentsFound = _appDbContext.Documents.FirstOrDefault(x => x.Id == id);
                if (documentsFound == null)
                {
                    throw new EntityNotFoundException("Documents", id);
                }
                return documentsFound;
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        public override List<Documents> GetAll()
        {
            try
            {
                return _appDbContext.Documents.ToList();
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
                return _appDbContext.Documents.Any(x => x.Id == id);
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        //Insert
        public override Documents Add(Documents item)
        {
            try
            {
                var addedDocument = _appDbContext.Documents.Add(item).Entity;
                _appDbContext.SaveChanges();
                return addedDocument;
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        public override int AddMany(List<Documents> items, bool CancelOnError)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            BeginTransaction();
            int succes = 0;
            foreach (Documents item in items)
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
            CommitTransaction();
            return succes;
        }

        //Update
        public override bool Update(Documents item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            try
            {
                if (!Exists(item.Id))
                {
                    throw new EntityNotFoundException("Documents", item.Id);
                }
                _appDbContext.Documents.Update(item);
                _appDbContext.SaveChanges();
                return true;
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        public override bool Update(int id, Documents item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            try
            {
                if (!Exists(id))
                {
                    throw new EntityNotFoundException("Documents", id);
                }
                item.Id = id;
                return Update(item);
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        public override int UpdateMany(List<Documents> items, bool CancelOnError)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            BeginTransaction();
            int succes = 0;
            foreach (Documents item in items)
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
            CommitTransaction();
            return succes;
        }

        //Delete
        public override bool Delete(Documents item)
        { 
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            try
            {
                if (!Exists(item.Id))
                {
                    throw new EntityNotFoundException("Documents", item.Id);
                }
                _appDbContext.Documents.Remove(item);
                _appDbContext.SaveChanges();
                return true;
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        public override bool Delete(int id)
        {
            Documents itemToDelete = GetById(id);
            return Delete(itemToDelete);
        }

        public override int DeleteMany(List<Documents> items, bool CancelOnError)
        {
            if (items == null)
            {
                throw new ArgumentException(nameof(items));
            }
            BeginTransaction();
            int succes = 0;
            foreach (Documents item in items)
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
            CommitTransaction();
            return succes;
        }
    }
}