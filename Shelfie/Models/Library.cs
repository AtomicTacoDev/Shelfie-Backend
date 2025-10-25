using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shelfie.Models;

public class Library
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
    
    public ICollection<PlacedObject> Objects { get; set; } = new List<PlacedObject>();
}