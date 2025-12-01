using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shelfie.Data;
using Shelfie.Data.Models;
using Shelfie.Models.Dto;
using Shelfie.Services;

namespace Shelfie.Controllers;

[Authorize]
[ApiController]
[Route("books")]
public class UserBooksController(IBooksService booksService, IAuthService authService, ApplicationDbContext context) : ControllerBase
{
    public record CreateBookRequest
    {
        public string Username { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Author { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? PageCount { get; init; }
        public string? PublishedDate { get; init; }
        public int Rating { get; init; }
        public string? CoverUrl { get; init; }
    }
    public record UpdateBookRequest
    {
        public string Title { get; init; } = string.Empty;
        public string Author { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? PageCount { get; init; }
        public string? PublishedDate { get; init; }
        public int Rating { get; init; }
        public string? CoverUrl { get; init; }
    }
    
    [HttpGet("{userName}")]
    public async Task<ActionResult<IReadOnlyCollection<BookDto>>> GetUserBooks(string userName)
    {
        var user = await authService.GetUserByName(userName);
        if (user == null || string.IsNullOrEmpty(user.UserName))
            return NotFound(new { message = "User not found" });
        
        var books = await booksService.GetBooksByUserName(userName);
        
        return Ok(books);
    }
    
    [HttpGet("details/{id}")]
    public async Task<ActionResult<BookDto>> GetBookById(int id)
    {
        var book = await context.UserBooks
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
            return NotFound(new { message = "Book not found" });

        return Ok(new BookDto(
            book.Id,
            book.Title,
            book.Author,
            book.Description,
            book.CoverUrl,
            book.PublishedDate,
            book.PageCount,
            book.Rating
        ));
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> CreateBook([FromForm] CreateBookRequest request, [FromForm] IFormFile? cover)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Title) ||
            string.IsNullOrWhiteSpace(request.Author))
        {
            return BadRequest("Username, title and author are required");
        }

        var user = await authService.GetUserByName(request.Username);
        if (user == null)
            return NotFound(new { message = "User not found" });

        var book = await booksService.CreateBook(
            user.Id,
            request.Title,
            request.Author,
            request.Description,
            request.PageCount,
            request.PublishedDate,
            request.Rating,
            request.CoverUrl,
            cover
        );

        return Ok(book);
    }
    
    [HttpPatch("{id}")]
    public async Task<ActionResult<BookDto>> UpdateBook(int id, [FromForm] UpdateBookRequest request, [FromForm] IFormFile? cover)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Author))
        {
            return BadRequest("Title and author are required");
        }

        var book = await context.UserBooks
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
            return NotFound(new { message = "Book not found" });
        
        var currentUser = await authService.GetUserByName(User.Identity?.Name ?? "");
        if (currentUser == null || book.UserId != currentUser.Id)
            return Forbid();

        var updatedBook = await booksService.UpdateBook(
            id,
            request.Title,
            request.Author,
            request.Description,
            request.PageCount,
            request.PublishedDate,
            request.Rating,
            request.CoverUrl,
            cover
        );

        return Ok(updatedBook);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBook(int id)
    {
        var book = await context.UserBooks
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
            return NotFound(new { message = "Book not found" });
    
        var currentUser = await authService.GetUserByName(User.Identity?.Name ?? "");
    
        if (currentUser == null || book.UserId != currentUser.Id)
            return Forbid();

        context.UserBooks.Remove(book);
        await context.SaveChangesAsync();

        return NoContent();
    }
}