
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shelfie.Models;

namespace Shelfie.Data.Models;

public class BookshelfBook
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int PlacedObjectId { get; set; }
    
    [Required]
    public int UserBookId { get; set; }
    
    [Required]
    public string ShelfId { get; set; }
    
    [Required]
    public int Index { get; set; }
    
    [ForeignKey(nameof(PlacedObjectId))]
    public PlacedObject PlacedObject { get; set; }
    
    [ForeignKey(nameof(UserBookId))]
    public UserBook UserBook { get; set; }
}
