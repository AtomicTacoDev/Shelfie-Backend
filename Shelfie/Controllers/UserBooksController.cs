
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shelfie.Models.Dto;
using Shelfie.Services;

namespace Shelfie.Controllers;

[Authorize]
[ApiController]
[Route("books")]
public class UserBooksController(IBooksService booksService, IAuthService authService) : ControllerBase
{
    [HttpGet("{userName}")]
    public async Task<ActionResult<IReadOnlyCollection<BookDto>>> GetUserBooks(string userName)
    {
        var user = await authService.GetUserByName(userName);
        if (user == null || string.IsNullOrEmpty(user.UserName))
            return NotFound(new { message = "User not found" });
        
        var books = await booksService.GetBooksByUserName(userName);
        
        return Ok(books);
    }
}