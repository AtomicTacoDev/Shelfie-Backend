
namespace Shelfie.Models.Dto;

public record UserBookDto(
    int Id,
    int BookId,
    string Title,
    string? Author,
    string? CoverImage,
    int? PageCount,
    string? Isbn13,
    string? Isbn10,
    string? Isbn
);
