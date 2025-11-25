
using Shelfie.Models.Dto;

namespace Shelfie.Services;

public interface IBooksService
{
    public Task<IReadOnlyList<BookDto>> GetBooksByUserName(string Username);
}
