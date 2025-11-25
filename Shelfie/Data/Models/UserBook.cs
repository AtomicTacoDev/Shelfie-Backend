
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
    
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
}
