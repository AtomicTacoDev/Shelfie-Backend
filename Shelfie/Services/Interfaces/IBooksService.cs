
using Shelfie.Models.Dto;

namespace Shelfie.Services;

public interface IBooksService
{
    public Task<IReadOnlyList<BookDto>> GetBooksByUserName(string username);
    public Task<IReadOnlyList<BookSearchResultDto>> QueryBooks(string query);
    public Task<BookDto> GetBookById(string id);
}
