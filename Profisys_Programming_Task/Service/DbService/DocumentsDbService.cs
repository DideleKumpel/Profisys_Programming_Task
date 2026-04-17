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
        public override async Task<Documents> GetByIdAsync(int id)
        {
            try
            {
                Documents documentsFound = await _appDbContext.Documents.FirstOrDefaultAsync(x => x.Id == id);
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

        public override async Task<List<Documents>> GetAllAsync()
        {
            try
            {
                return await _appDbContext.Documents.ToListAsync();
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
                return await _appDbContext.Documents.AnyAsync(x => x.Id == id);
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        //Insert
        public override async Task<Documents> AddAsync(Documents item)
        {
            try
            {
                var addedDocument = _appDbContext.Documents.Add(item).Entity;
                await _appDbContext.SaveChangesAsync();
                return addedDocument;
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        public override async Task<int> AddManyAsync(List<Documents> items, bool CancelOnError)
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
                    await AddAsync(item);
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
        public override async Task<bool> UpdateAsync(Documents item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            try
            {
                if (!await ExistsAsync(item.Id))
                {
                    throw new EntityNotFoundException("Documents", item.Id);
                }
                _appDbContext.Documents.Update(item);
                await _appDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception error)
            {
                HandleException(error);
                throw;
            }
        }

        public override async Task<bool> UpdateAsync(int id, Documents item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            try
            {
                if (!await ExistsAsync(id))
                {
                    throw new EntityNotFoundException("Documents", id);
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

        public override async Task<int> UpdateManyAsync(List<Documents> items, bool CancelOnError)
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
                    await UpdateAsync(item);
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
        public override async Task<bool> DeleteAsync(Documents item)
        { 
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            try
            {
                if (!await ExistsAsync(item.Id))
                {
                    throw new EntityNotFoundException("Documents", item.Id);
                }
                _appDbContext.Documents.Remove(item);
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
            Documents itemToDelete = await GetByIdAsync(id);
            return await DeleteAsync(itemToDelete);
        }

        public override async Task<int> DeleteManyAsync(List<Documents> items, bool CancelOnError)
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
                    await DeleteAsync(item);
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