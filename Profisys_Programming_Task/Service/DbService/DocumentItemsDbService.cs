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
            } catch (Exception ex) {
                throw new Exception($"Error occurred while retrieving DocumentItems with id {id}. Error: {ex.Message}");
            }
            if (documentItemsFound == null)
            {
                throw new Exception($"DocumentItems with id {id} not found.");
            }
            return documentItemsFound;
        }

        public override List<DocumentItems> GetAll()
        {

            List<DocumentItems> documentItemsList = null;
            try
            {
                documentItemsList = _appDbContext.DocumentItems.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving DocumentItems. Error: {ex.Message}");
            }
            if (documentItemsList == null || documentItemsList.Count == 0)
            {
                throw new Exception($"Error occurred while retrieving DocumentItems.");
            }
            return documentItemsList;
        }

        public override bool Exists(int id)
        {
            DocumentItems documentItemsFound = null;
            try
            {
                documentItemsFound = _appDbContext.DocumentItems.Where(item => item.Id == id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving DocumentItems with id {id}. Error: {ex.Message}");
            }
            if (documentItemsFound == null)
            {
                return false;
            }
            else {
                return true; 
            }
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
