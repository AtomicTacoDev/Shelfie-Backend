using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shelfie.Models.Dto;
using Shelfie.Services;

namespace Shelfie.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public partial class LibraryController(IUserService userService, ILibraryService libraryService) : ControllerBase
{
    [HttpGet("{userName}")]
    public async Task<ActionResult> GetLibraryData(string userName)
    {
        var user = await userService.GetUserByName(userName);
        if (user == null || string.IsNullOrEmpty(user.UserName))
            return NotFound(new { message = "User not found" });
        
        var library = await libraryService.GetLibraryData(user.Id);

        var libraryDto = new LibraryDto
        {
            Id = library.Id,
            UserName = user.UserName,
            Objects = library.Objects.Select(obj => new PlacedObjectDto
            {
                ObjectId = obj.Id,
                PositionX = obj.PositionX,
                PositionY = obj.PositionY,
                Rotation = obj.Rotation
            }).ToList()
        };

        return Ok(libraryDto);
    }
}
