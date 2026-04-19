using Microsoft.EntityFrameworkCore;
using Profisys_Programming_Task.Model;
using Profisys_Programming_Task.Service.Exceptions;

namespace Profisys_Programming_Task.Service.DbService
{
    
    internal class DocumentItemsDbService : BaseDbService<DocumentItems>, IDocumentItemsDbService
    {
        public DocumentItemsDbService(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        //Get
        public override async Task<DocumentItems> GetByIdAsync(int id)
        {
            try
            {
                DocumentItems documentItemsFound = await _appDbContext.DocumentItems.Where(item => item.Id == id).FirstOrDefaultAsync();
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

        public async Task<List<DocumentItems>> GetByDocumentIdAsync(int documentId)
        {
            try
            {
                List<DocumentItems> documentItemsFound = await _appDbContext.DocumentItems.Where(item => item.DocumentId == documentId).ToListAsync();
                if (documentItemsFound == null)
                {
                    throw new EntityNotFoundException("DocumentItems", documentId);
                }
                return documentItemsFound;
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        public override async Task<List<DocumentItems>> GetAllAsync()
        {
            try
            {
                return await _appDbContext.DocumentItems.ToListAsync();
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        public override async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _appDbContext.DocumentItems.AnyAsync(item => item.Id == id);
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        public override async Task<int> GetCountAsync()
        {
            try
            {
                return await _appDbContext.DocumentItems.CountAsync();
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        //Insert
        public override async Task<DocumentItems> AddAsync(DocumentItems item)
        {
            try
            {
                DocumentItems addedItem = _appDbContext.DocumentItems.Add(item).Entity;
                await _appDbContext.SaveChangesAsync();
                return addedItem;
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
            
        }

        public override async Task<int> AddManyAsync(List<DocumentItems> items, bool CancelOnError)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            await BeginTransactionAsync();
            int succes = 0;
            foreach (DocumentItems item in items)
            {
                try
                {
                    await AddAsync(item);
                    succes++;
                }
                catch (Exception error)
                {
                    _appDbContext.Entry(item).State = EntityState.Detached;
                    if (CancelOnError)
                    {
                        await RollbackTransactionAsync();
                        HandleException(error);
                    }
                }
            }
            await CommitTransactionAsync(false);
            return succes;
        }

        //Update
        public override async Task<bool> UpdateAsync(DocumentItems item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            try
            {
                if (!await ExistsAsync(item.Id))
                {
                    throw new EntityNotFoundException("DocumentItems", item.Id);
                }
                _appDbContext.DocumentItems.Update(item);
                await _appDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        public override async Task<bool> UpdateAsync(int id, DocumentItems item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            try
            {
                if (!await ExistsAsync(id))
                {
                    throw new EntityNotFoundException("DocumentsItems", id);
                }
                item.Id = id;
                return await UpdateAsync(item);
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        public override async Task<int> UpdateManyAsync(List<DocumentItems> items, bool CancelOnError)
        {
            if (items == null)
            {
                throw new ArgumentException(nameof(items));
            }
            await BeginTransactionAsync();
            int succes = 0;
            foreach (DocumentItems item in items)
            {
                try
                {
                    await UpdateAsync(item);
                    succes++;
                }
                catch (Exception error)
                {
                    _appDbContext.Entry(item).State = EntityState.Unchanged;
                    if (CancelOnError)
                    {
                        await RollbackTransactionAsync();
                        HandleException(error);
                    }
                }
            }
            await CommitTransactionAsync(false);
            return succes;
        }

        //Delete
        public override async Task<bool> DeleteAsync(DocumentItems item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            try
            {
                if (!await ExistsAsync(item.Id))
                {
                    throw new EntityNotFoundException("DocumentsItems", item.Id);
                }
                _appDbContext.DocumentItems.Remove(item);
                await _appDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            DocumentItems itemToDelete = await GetByIdAsync(id);
            return await DeleteAsync(itemToDelete);
        }

        public override async Task<int> DeleteManyAsync(List<DocumentItems> items, bool CancelOnError)
        {
            if (items == null)
            {
                throw new ArgumentException(nameof(items));
            }
            await BeginTransactionAsync();
            int succes = 0;
            foreach (DocumentItems item in items)
            {
                try
                {
                    await DeleteAsync(item);
                    succes++;
                }
                catch (Exception error)
                {
                    _appDbContext.Entry(item).State = EntityState.Unchanged;
                    if (CancelOnError)
                    {
                        await RollbackTransactionAsync();
                        HandleException(error);
                    }
                }
            }
            await CommitTransactionAsync(false);
            return succes;
        }

    }
}
