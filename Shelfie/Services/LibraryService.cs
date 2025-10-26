
using Microsoft.EntityFrameworkCore;
using Shelfie.Data;
using Shelfie.Models;

namespace Shelfie.Services;

public class LibraryService(ApplicationDbContext dbContext) : ILibraryService
{
    public async Task<Library> GetLibraryData(string userId)
    {
        var libraryData = await dbContext.Libraries
            .Include(l => l.User)
            .Include(l => l.Objects)
            .FirstOrDefaultAsync(l => l.UserId == userId);

        return libraryData;
    }
}