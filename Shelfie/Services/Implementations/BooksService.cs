
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shelfie.Data;
using Shelfie.Data.Models;
using Shelfie.Models.Dto;

namespace Shelfie.Services;

public class BooksService(ApplicationDbContext dbContext, HttpClient httpClient) : IBooksService
{
    public async Task<IReadOnlyList<UserBookDto>> GetBooksByUserName(string userName)
    {
        var userBooks = await dbContext.UserBooks
            .Include(ub => ub.Book)
            .Include(ub => ub.User)
            .Where(ub => ub.User.UserName == userName)
            .ToListAsync();

        return userBooks.Select(ub => new UserBookDto(
            ub.Id,
            ub.BookId,
            ub.Book.Title,
            ub.Book.Author,
            ub.Book.CoverImage,
            ub.Book.Isbn13,
            ub.Book.Isbn10,
            ub.Book.Isbn
        )).ToList();
    }

    public async Task<IReadOnlyList<BookSearchResultDto>> QueryBooks(string query)
    {
        try
        {
            var url = $"books/{Uri.EscapeDataString(query)}?column=title&pageSize=20";
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<IsbndbSearchResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (searchResponse?.Books == null)
                return Array.Empty<BookSearchResultDto>();
            
            var seen = new HashSet<string>();
            var results = new List<BookSearchResultDto>();

            foreach (var book in searchResponse.Books)
            {
                if (string.IsNullOrEmpty(book.Title) || 
                    (string.IsNullOrEmpty(book.Isbn13) && 
                     string.IsNullOrEmpty(book.Isbn10) && 
                     string.IsNullOrEmpty(book.Isbn)))
                {
                    continue;
                }
                
                var key = $"{book.Title}-{book.Authors?.FirstOrDefault()}";
                if (seen.Contains(key))
                    continue;

                seen.Add(key);
                results.Add(new BookSearchResultDto(
                    book.Isbn13,
                    book.Isbn10,
                    book.Isbn,
                    book.Title,
                    book.Authors?.FirstOrDefault(),
                    book.Image
                ));
            }

            return results;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to query books: {ex.Message}", ex);
        }
    }

    public async Task<BookDto> GetBookById(int id)
    {
        var book = await dbContext.Books.FindAsync(id);
        if (book == null)
            throw new Exception("Book not found");
            
        return MapBookToDto(book);
    }

    public async Task<BookDto> GetOrCreateBook(string? isbn13, string? isbn10, string? isbn)
    {
        var book = await FindBookByIsbn(isbn13, isbn10, isbn);
        
        if (book != null)
            return MapBookToDto(book);
        
        var isbndbBook = await FetchFromIsbndb(isbn13, isbn10, isbn);
        if (isbndbBook == null)
            throw new Exception("Book not found in ISBNdb");

        book = await CreateBookFromIsbndb(isbndbBook);
        return MapBookToDto(book);
    }

    public async Task<BookDto> AddBookToUser(string userId, string? isbn13, string? isbn10, string? isbn)
    {
        var bookDto = await GetOrCreateBook(isbn13, isbn10, isbn);
        
        var existingUserBook = await dbContext.UserBooks
            .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookDto.Id);

        if (existingUserBook == null)
        {
            var userBook = new UserBook
            {
                UserId = userId,
                BookId = bookDto.Id
            };

            dbContext.UserBooks.Add(userBook);
            await dbContext.SaveChangesAsync();
        }

        return bookDto;
    }

    public async Task RemoveBookFromUser(string userId, int bookId)
    {
        var userBook = await dbContext.UserBooks
            .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId);

        if (userBook != null)
        {
            dbContext.UserBooks.Remove(userBook);
            await dbContext.SaveChangesAsync();
        }
    }
    
    private async Task<Book?> FindBookByIsbn(string? isbn13, string? isbn10, string? isbn)
    {
        if (!string.IsNullOrEmpty(isbn13))
        {
            var book = await dbContext.Books.FirstOrDefaultAsync(b => b.Isbn13 == isbn13);
            if (book != null) return book;
        }

        if (!string.IsNullOrEmpty(isbn10))
        {
            var book = await dbContext.Books.FirstOrDefaultAsync(b => b.Isbn10 == isbn10);
            if (book != null) return book;
        }

        if (!string.IsNullOrEmpty(isbn))
        {
            var book = await dbContext.Books.FirstOrDefaultAsync(b => b.Isbn == isbn);
            if (book != null) return book;
        }

        return null;
    }

    private async Task<IsbndbBook?> FetchFromIsbndb(string? isbn13, string? isbn10, string? isbn)
    {
        var isbnsToTry = new[] { isbn13, isbn10, isbn }.Where(i => !string.IsNullOrEmpty(i));

        foreach (var isbnValue in isbnsToTry)
        {
            try
            {
                var url = $"book/{Uri.EscapeDataString(isbnValue!)}";
                var response = await httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var bookResponse = JsonSerializer.Deserialize<IsbndbBookResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return bookResponse?.Book;
                }
            }
            catch
            {
                // ignored
            }
        }

        return null;
    }

    private async Task<Book> CreateBookFromIsbndb(IsbndbBook isbndbBook)
    {
        var book = new Book
        {
            Isbn13 = isbndbBook.Isbn13,
            Isbn10 = isbndbBook.Isbn10,
            Isbn = isbndbBook.Isbn,
            Title = isbndbBook.Title ?? "Unknown Title",
            Author = isbndbBook.Authors?.FirstOrDefault(),
            Synopsis = isbndbBook.Synopsis,
            CoverImage = isbndbBook.Image,
            DatePublished = isbndbBook.Date_Published,
            PageCount = isbndbBook.Pages
        };
        
        if (isbndbBook.Dimensions_Structured != null)
        {
            book.HeightInches = ConvertToInches(
                isbndbBook.Dimensions_Structured.Height?.Value, 
                isbndbBook.Dimensions_Structured.Height?.Unit
            );
            book.WidthInches = ConvertToInches(
                isbndbBook.Dimensions_Structured.Width?.Value, 
                isbndbBook.Dimensions_Structured.Width?.Unit
            );
            book.LengthInches = ConvertToInches(
                isbndbBook.Dimensions_Structured.Length?.Value, 
                isbndbBook.Dimensions_Structured.Length?.Unit
            );
        }

        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();

        return book;
    }

    private static decimal? ConvertToInches(decimal? value, string? unit)
    {
        if (!value.HasValue || string.IsNullOrEmpty(unit))
            return null;

        return unit.ToLower() switch
        {
            "mm" => value.Value / 25.4m,
            "cm" => value.Value / 2.54m,
            "inches" or "in" => value.Value,
            _ => null
        };
    }

    private static BookDto MapBookToDto(Book book)
    {
        return new BookDto(
            book.Id,
            book.Isbn13,
            book.Isbn10,
            book.Isbn,
            book.Title,
            book.Author,
            book.Synopsis,
            book.CoverImage,
            book.DatePublished,
            book.PageCount,
            book.HeightInches,
            book.WidthInches,
            book.LengthInches
        );
    }
}
