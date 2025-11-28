
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shelfie.Models.Dto;
using Shelfie.Services;

namespace Shelfie.Controllers;

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
        
        return Ok(
            new SearchResultDto(
                books,
                Users: new List<UserSearchResultDto>
                {
                    
                }
            )
        );
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookDto>> GetBook(string id)
    {
        id = id.Trim();
        
        if (id.Length == 0) return BadRequest();

        var book = await booksService.GetBookById(id);

        return Ok(book);
    }
}
