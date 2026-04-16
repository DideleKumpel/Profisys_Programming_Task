using Microsoft.EntityFrameworkCore;
using Profisys_Programming_Task.Model;
using System.Net;
using System.Windows.Documents;

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
                    throw new Exception($"DocumentItems with id {id} not found.");
                }
                return documentItemsFound;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving DocumentItems with id {id}. Error: {ex.Message}");
            }
        }

        public override List<DocumentItems> GetAll()
        {
            try
            {
                return _appDbContext.DocumentItems.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving DocumentItems. Error: {ex.Message}");
            }
        }

        public override bool Exists(int id)
        {
            try
            {
                return _appDbContext.DocumentItems.Any(item => item.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving DocumentItems with id {id}. Error: {ex.Message}");
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
                throw new Exception(message: $"Error ocurred while adding item. Error: {error.Message}");
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
                        throw new Exception(message: $"Error ocurred while adding item number {succes + 1}. Error: {error.Message}");
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
                throw new Exception(message: $"Error ocurred while updating item with id {item.Id}. Error: {error.Message}");
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
                throw new Exception(message: $"Error ocurred while updating item with id {id}. Error: {error.Message}");
            }
        }

        public override int UpdateMany(List<DocumentItems> items, bool CancelOnError)
        {
            if (items == null || items.Count == 0)
            {
                throw new ArgumentException("Items list cannot be null or empty.", nameof(items));
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
                        throw new Exception(message: $"Error ocurred while updating item with id {item.Id}. Error: {error.Message}");
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
                throw new Exception(message: $"Error ocurred while deleting item with id {item.Id}. Error: {error.Message}");
            }
        }

        public override bool Delete(int id)
        {
            DocumentItems itemToDelete = GetById(id);
            return Delete(itemToDelete);
        }

        public override int DeleteMany(List<DocumentItems> items, bool CancelOnError)
        {
            if (items == null || items.Count == 0)
            {
                throw new ArgumentException("Items list cannot be null or empty.", nameof(items));
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
                        throw new Exception(message: $"Error ocurred while deleting item with id {item.Id}. Error: {error.Message}");
                    }
                }
            }
            CommitTransaction(false);
            return succes;
        }

    }
}
