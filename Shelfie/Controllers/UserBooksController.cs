using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public string? CoverUrl { get; init; } // Can be URL or base64
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
}