
namespace Shelfie.Models.Dto;

public record LibraryDto(
    int Id,
    string? UserName,
    IReadOnlyList<PlacedObjectDto> Objects
);

public record PlacedObjectDto(
    int ObjectId,
    float PositionX,
    float PositionY,
    float Rotation
);
