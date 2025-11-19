using System.ComponentModel.DataAnnotations;
using Shelfie.Models;

namespace Shelfie.Data.Models;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required string UserId { get; set; }
    
    [Required]
    public required string Token { get; set; }

    [Required]
    public required DateTime CreatedAt { get; set; }

    [Required]
    public required DateTime ExpiresAt { get; set; }
    
    public bool IsRevoked { get; set; }
}