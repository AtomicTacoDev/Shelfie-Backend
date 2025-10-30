using System.Numerics;
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
    public record PlaceObjectRequest(string ObjectTypeId, PositionDto Position, float Rotation);
    
    [HttpGet("{userName}")]
    public async Task<IActionResult> GetLibraryData(string userName)
    {
        var user = await userService.GetUserByName(userName);
        if (user == null || string.IsNullOrEmpty(user.UserName))
            return NotFound(new { message = "User not found" });

        return Ok(new {});
    }

    [HttpPost("{userName}/objects")]
    public async Task<ActionResult<PlacedObjectDto>> PlaceObject(string userName, [FromBody] PlaceObjectRequest request)
    {
        var newObject = await libraryService.TryPlaceObject(userName, request.ObjectTypeId, request.Position, request.Rotation);
        
        if (newObject is null) return BadRequest();

        return Created($"/library/{userName}/objects/{newObject.Id}", newObject); // temporary solution
    }
    
    [HttpGet("{userName}/objects")]
    public async Task<ActionResult<IReadOnlyList<PlacedObjectDto>>> GetObjects(string userName)
    {
        var objects = await libraryService.GetObjects(userName);
        if (objects == null)
            return NotFound(new { message = "User or library not found" });

        return Ok(objects);
    }
}
