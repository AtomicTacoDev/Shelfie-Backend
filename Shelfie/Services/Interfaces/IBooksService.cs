
using Shelfie.Models.Dto;

namespace Shelfie.Services;

public interface IBooksService
{
    Task<IReadOnlyList<BookDto>> GetBooksByUserName(string userName);
    Task<IReadOnlyList<BookSearchResultDto>> QueryBooks(string query);
    Task<BookDto> GetBookById(int id);
    Task<BookDto> GetOrCreateBook(string? isbn13, string? isbn10, string? isbn);
    Task<BookDto> AddBookToUser(string userId, string? isbn13, string? isbn10, string? isbn);
    Task RemoveBookFromUser(string userId, int bookId);
}
