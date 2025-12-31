
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shelfie.Models.Dto;
using Shelfie.Services;

namespace Shelfie.Controllers;

public record AddBookRequest
{
    public string? Isbn13 { get; init; }
    public string? Isbn10 { get; init; }
    public string? Isbn { get; init; }
}

[Authorize]
[ApiController]
[Route("books")]
public class UserBooksController(IBooksService booksService, IAuthService authService) : ControllerBase
{
    [HttpGet("{userName}")]
    public async Task<ActionResult<IReadOnlyCollection<UserBookDto>>> GetUserBooks(string userName)
    {
        var user = await authService.GetUserByName(userName);
        if (user == null || string.IsNullOrEmpty(user.UserName))
            return NotFound(new { message = "User not found" });
        
        var books = await booksService.GetBooksByUserName(userName);
        
        return Ok(books);
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> AddBook([FromBody] AddBookRequest request)
    {
        if (string.IsNullOrEmpty(request.Isbn13) && 
            string.IsNullOrEmpty(request.Isbn10) && 
            string.IsNullOrEmpty(request.Isbn))
        {
            return BadRequest("At least one ISBN must be provided");
        }

        var currentUser = await authService.GetUserByName(User.Identity?.Name ?? "");
        if (currentUser == null)
            return Unauthorized();

        try
        {
            var book = await booksService.AddBookToUser(currentUser.Id, request.Isbn13, request.Isbn10, request.Isbn);
            return Ok(book);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{bookId:int}")]
    public async Task<ActionResult> DeleteBook(int bookId)
    {
        var currentUser = await authService.GetUserByName(User.Identity?.Name ?? "");
        if (currentUser == null)
            return Unauthorized();

        await booksService.RemoveBookFromUser(currentUser.Id, bookId);
        return NoContent();
    }
}
