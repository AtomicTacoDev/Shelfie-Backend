
using System.Numerics;
using Shelfie.Models;
using Shelfie.Models.Dto;

namespace Shelfie.Services;

public interface ILibraryService
{
    public Task<LibraryDto> GetLibraryData(string userId);
    public Task<PlacedObjectDto?> TryPlaceObject(string userName, string objectTypeId, PositionDto position, float rotation);
    public Task<IReadOnlyList<PlacedObjectDto>?> GetObjects(string userName);
}