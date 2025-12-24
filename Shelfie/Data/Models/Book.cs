
using System.ComponentModel.DataAnnotations;

namespace Shelfie.Data.Models;

public class Book
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(13)]
    public string? Isbn13 { get; set; }
    
    [MaxLength(10)]
    public string? Isbn10 { get; set; }
    
    [MaxLength(13)]
    public string? Isbn { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Title { get; set; }
    
    [MaxLength(500)]
    public string? Author { get; set; }
    
    public string? Synopsis { get; set; }
    
    [MaxLength(1000)]
    public string? CoverImage { get; set; }
    
    public string? DatePublished { get; set; }
    
    public int? PageCount { get; set; }
    
    public decimal? HeightInches { get; set; }
    public decimal? WidthInches { get; set; }
    public decimal? LengthInches { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
