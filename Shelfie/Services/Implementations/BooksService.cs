
using Microsoft.EntityFrameworkCore;
using Shelfie.Data;
using Shelfie.Models.Dto;

namespace Shelfie.Services;

public class BooksService(ApplicationDbContext dbContext) : IBooksService
{
    public async Task<IReadOnlyList<BookDto>> GetBooksByUserName(string userName)
    {
        return await dbContext.UserBooks
            .Include(book => book.User)
            .Where(book => book.User.UserName == userName)
            .Select(book => new BookDto(
                "Placeholder Name",
                "Placeholder Author"
            ))
            .ToListAsync();
    }
}
