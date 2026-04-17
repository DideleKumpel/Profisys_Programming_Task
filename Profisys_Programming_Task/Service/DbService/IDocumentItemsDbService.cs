using Profisys_Programming_Task.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profisys_Programming_Task.Service.DbService
{
    internal interface IDocumentItemsDbService : IDbService<DocumentItems>
    {
        Task<List<DocumentItems>> GetByDocumentIdAsync(int documentId);
    }

}
