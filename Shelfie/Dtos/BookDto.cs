
namespace Shelfie.Models.Dto;

public record BookDto(
    string Title,
    string Author,
    string Description,
    string CoverUrl,
    string PublishedDate,
    int PageCount,
    float Rating,
    int RatingsCount
    );
