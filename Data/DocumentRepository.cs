//DocumentRepository
using System.Collections.Generic;
using System.Linq;
using DigitalLockerSystem.Models;

namespace DigitalLockerSystem.Data
{
    public class DocumentRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Document> GetAllDocuments()
        {
            return _context.Documents.ToList();
        }
    }
}