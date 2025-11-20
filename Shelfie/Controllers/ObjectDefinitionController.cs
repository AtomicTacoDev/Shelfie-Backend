
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shelfie.Services;

namespace Shelfie.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ObjectDefinitionController(IObjectDefinitionService objectDefinitionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Dictionary<string, ObjectDefinition>>> GetObjectDefinitions()
    {
        return Ok(objectDefinitionService.GetAllDefinitions());
    }
}
