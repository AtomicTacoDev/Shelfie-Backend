
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shelfie.Models.Dto;
using Shelfie.Services;

namespace Shelfie.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public partial class LibraryController(
    IAuthService authService,
    ILibraryService libraryService,
    IObjectDefinitionService objectDefinitionService
    ) : ControllerBase
{
    public record PlaceObjectRequest(string ObjectTypeId, PositionDto Position, float Rotation);
    public record MoveObjectRequest(PositionDto Position, float Rotation);
    
    [HttpGet("{userName}")]
    public async Task<IActionResult> GetLibraryData(string userName)
    {
        var user = await authService.GetUserByName(userName);
        if (user == null || string.IsNullOrEmpty(user.UserName))
            return NotFound(new { message = "User not found" });

        return Ok(new {});
    }

    [HttpGet("{userName}/objects")]
    public async Task<ActionResult<IReadOnlyList<PlacedObjectDto>>> GetObjects(string userName)
    {
        var objects = await libraryService.GetObjects(userName);
        if (objects == null)
            return NotFound(new { message = "User or library not found" });

        return Ok(objects);
    }
    
    [HttpPost("{userName}/objects")]
    public async Task<ActionResult<PlacedObjectDto>> PlaceObject(string userName, [FromBody] PlaceObjectRequest request)
    {
        var definition = objectDefinitionService.GetDefinition(request.ObjectTypeId);
        if (definition == null) return BadRequest(new { message = "Object type not found" });
        
        var newObject = await libraryService.TryPlaceObject(userName, request.ObjectTypeId, request.Position, request.Rotation);
        
        if (newObject is null) return BadRequest();

        return Created($"/library/{userName}/objects/{newObject.Id}", newObject); // temporary solution
    }
    
    [HttpPatch("{userName}/objects/{objectId}")]
    public async Task<ActionResult<PlacedObjectDto>> MoveObject(string userName, int objectId, [FromBody] MoveObjectRequest request)
    {
        var objects = await libraryService.GetObjects(userName);
        
        if (objects.Count == 0)
            return NotFound(new { message = "No objects in library" });
        if (objects.All(obj => obj.Id != objectId))
            return NotFound(new { message = "Object not found" });

        var objectTypeId = objects.FirstOrDefault(existingObject => existingObject.Id == objectId)?.ObjectTypeId;
        var movedObject = await libraryService.TryMoveObject(
            userName,
            objectId,
            objectTypeId,
            request.Position,
            request.Rotation);
        
        if (movedObject is null)
            return BadRequest(new { message = "Cannot move object to this position" });
        
        return Ok(movedObject);
    }

    [HttpDelete("{userName}/objects/{objectId}")]
    public async Task<IActionResult> DeleteObject(string userName, int objectId)
    {
        var objects = await libraryService.GetObjects(userName);
    
        if (objects.Count == 0)
            return NotFound(new { message = "No objects in library" });
    
        var objectToDelete = objects.FirstOrDefault(obj => obj.Id == objectId);
        if (objectToDelete == null)
            return NotFound(new { message = "Object not found" });
    
        await libraryService.DeleteObject(objectId);
        
        return NoContent();
    }
    
    [HttpGet("{userName}/objects/bookshelf/{bookshelfId}")]
    public async Task<ActionResult<BookshelfDataDto>> GetBookshelfData(string userName, int bookshelfId)
    {
        var data = await libraryService.GetBookshelfData(userName, bookshelfId);
        if (data == null)
            return NotFound(new { message = "Bookshelf not found" });

        return Ok(data);
    }

    [HttpPut("{userName}/objects/bookshelf/{bookshelfId}")]
    public async Task<ActionResult<BookshelfDataDto>> UpdateBookshelfData(string userName, int bookshelfId, [FromBody] BookshelfDataDto data)
    {
        try
        {
            var updatedData = await libraryService.UpdateBookshelfData(userName, bookshelfId, data);
            return Ok(updatedData);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating bookshelf: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
        }
    }
}
