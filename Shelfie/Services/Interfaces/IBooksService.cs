
using Shelfie.Models.Dto;

namespace Shelfie.Services;

public interface IBooksService
{
    public Task<IReadOnlyList<BookDto>> GetBooksByUserName(string username);
    public Task<IReadOnlyList<BookSearchResultDto>> QueryBooks(string query);
    public Task<BookDto> GetBookById(string id);
    public Task<BookDto> CreateBook(
        string userId,
        string title,
        string author,
        string? description,
        string? pageCount,
        string? publishedDate,
        int rating,
        string? coverUrl,
        IFormFile? coverFile
    );
    public Task<BookDto> UpdateBook(
        int id,
        string title,
        string author,
        string? description,
        string? pageCount,
        string? publishedDate,
        int rating,
        string? coverUrl,
        IFormFile? coverFile
    );
}
