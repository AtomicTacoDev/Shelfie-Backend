
namespace Shelfie.Models.Dto;

public class LibraryDto
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public List<PlacedObjectDto> Objects { get; set; } = new();
}

public class PlacedObjectDto
{
    public int ObjectId { get; set; }
    
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float Rotation { get; set; }
}
