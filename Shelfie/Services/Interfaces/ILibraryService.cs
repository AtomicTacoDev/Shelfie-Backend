
using Shelfie.Models.Dto;

namespace Shelfie.Services;

public interface ILibraryService
{
    Task<LibraryDto> GetLibraryData(string userId);
    Task<IReadOnlyList<PlacedObjectDto>> GetObjects(string userName);
    Task<PlacedObjectDto?> TryPlaceObject(string userName, string objectTypeId, PositionDto position, float rotation);
    Task<PlacedObjectDto?> TryMoveObject(string userName, int objectId, string objectTypeId, PositionDto position, float rotation);
    Task DeleteObject(int objectId);
    Task<BookshelfDataDto?> GetBookshelfData(string userName, int bookshelfId);
    Task<BookshelfDataDto> UpdateBookshelfData(string userName, int bookshelfId, BookshelfDataDto data);
}
