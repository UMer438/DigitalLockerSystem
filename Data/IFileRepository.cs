using DigitalLockerSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitalLockerSystem.Data;

public interface IFileRepository
{
    Task<Models.File?> GetFileByIdAsync(int id);
    Task<List<Models.File>> GetFilesByUserIdAsync(string userId);
    Task AddFileAsync(Models.File file);
    Task DeleteFileAsync(Models.File file);


}