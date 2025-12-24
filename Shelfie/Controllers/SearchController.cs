
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shelfie.Models.Dto;
using Shelfie.Services;

namespace Shelfie.Controllers;

public record StageBookRequest(string? Isbn13, string? Isbn10, string? Isbn);

[Authorize]
[ApiController]
[Route("[controller]")]
public class SearchController(IBooksService booksService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SearchResultDto>> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest();

        q = q.Trim();
        
        var books = await booksService.QueryBooks(q);
        
        return Ok(new SearchResultDto(
            books,
            Users: new List<UserSearchResultDto>()
        ));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> GetBook(int id)
    {
        try
        {
            var book = await booksService.GetBookById(id);
            return Ok(book);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    
    [HttpPost("stage")]
    public async Task<ActionResult<BookDto>> StageBook([FromBody] StageBookRequest request)
    {
        if (string.IsNullOrEmpty(request.Isbn13) && 
            string.IsNullOrEmpty(request.Isbn10) && 
            string.IsNullOrEmpty(request.Isbn))
        {
            return BadRequest("At least one ISBN must be provided");
        }

        try
        {
            var book = await booksService.GetOrCreateBook(request.Isbn13, request.Isbn10, request.Isbn);
            return Ok(book);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
