
namespace Shelfie.Models.Dto;

public record BookDto(
    int Id,
    string Title,
    string Author,
    string? Description,
    string? CoverUrl,
    string? PublishedDate,
    int? PageCount,
    int? Rating
    );
