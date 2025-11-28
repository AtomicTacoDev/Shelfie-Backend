
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shelfie.Models;

namespace Shelfie.Data.Models;

public class UserBook
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string UserId { get; set; }
    
    [Required] public string Title { get; set; }
    [Required] public string Author { get; set; }
    public string Description { get; set; }
    [Column(TypeName = "text")] public string CoverUrl { get; set; }
    public int PageCount { get; set; }
    public int Rating { get; set; }
    [Required] public string PublishedDate { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
}
