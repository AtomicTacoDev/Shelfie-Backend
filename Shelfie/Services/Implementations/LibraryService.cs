
using Microsoft.EntityFrameworkCore;
using Shelfie.Data;
using Shelfie.Models.Dto;

namespace Shelfie.Services;

public class LibraryService(ApplicationDbContext dbContext) : ILibraryService
{
    public async Task<LibraryDto> GetLibraryData(string userId)
    {
        var library = await dbContext.Libraries
            .Include(l => l.User)
            .Include(l => l.Objects)
            .FirstOrDefaultAsync(l => l.UserId == userId);
        
        var libraryDto = new LibraryDto (
            library.Id,
            library.User.UserName,
            library.Objects.Select(obj => new PlacedObjectDto (
                obj.Id,
                obj.PositionX,
                obj.PositionY,
                obj.Rotation
            )).ToList()
        );

        return libraryDto;
    }
}