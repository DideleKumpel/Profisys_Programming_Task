using Microsoft.EntityFrameworkCore;
using Profisys_Programming_Task.Model;

namespace Profisys_Programming_Task.Service.DbService
{
    internal class DocumentItemsDbService : BaseDbService<DocumentItems>
    {
        public DocumentItemsDbService(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        //Insert
        public override DocumentItems Add(DocumentItems item)
        {
            BeginTransaction();
            DocumentItems addedItem = new DocumentItems();
            try
            {
                addedItem = _appDbContext.DocumentItems.Add(item).Entity;
                CommitTransaction();
            }
            catch(Exception error)
            {
                RollbackTransaction();
                throw new Exception(message: $"Error ocurred while adding item. Error: {error.Message}");
            }
            return addedItem;
        }

        public override int AddMany(List<DocumentItems> items, bool CancelOnError)
        {
            BeginTransaction();
            int succes = 0;
            foreach (DocumentItems item in items) {
                try
                {
                    _appDbContext.DocumentItems.Add(item);
                    _appDbContext.SaveChanges();
                    succes++;
                }catch(Exception error)
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
    }
}
