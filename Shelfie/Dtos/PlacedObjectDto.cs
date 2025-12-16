
namespace Shelfie.Models.Dto;

public record PositionDto(float x, float y, float z);
public record PlacedObjectDto(int Id, string ObjectTypeId, PositionDto Position, float Rotation);