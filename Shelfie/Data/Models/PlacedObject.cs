
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shelfie.Models;

public class PlacedObject
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string ObjectTypeId { get; set; }
    
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public float Rotation { get; set; }
    
    public int LibraryId { get; set; }
    [ForeignKey(nameof(LibraryId))]
    public Library Library { get; set; }
}