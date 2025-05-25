using DigitalLockerSystem.Data;
using DigitalLockerSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalLockerSystem.Data
{
    public class FileRepository : IFileRepository
    {
        private readonly ApplicationDbContext _context;

        public FileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Models.File?> GetFileByIdAsync(int id)
        {
            return await _context.Files.FindAsync(id);
        }

        public async Task<List<Models.File>> GetFilesByUserIdAsync(string userId)
        {
            return await _context.Files
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.UploadDate)
                .ToListAsync();
        }
        public async Task AddFileAsync(Models.File file)
        {
            _context.Files.Add(file);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteFileAsync(Models.File file)
        {
            _context.Files.Remove(file);
            await _context.SaveChangesAsync();
        }


    }
}
