
namespace Shelfie.Models.Dto;

public record BookDto(
    int Id,
    string? Isbn13,
    string? Isbn10,
    string? Isbn,
    string Title,
    string? Author,
    string? Synopsis,
    string? CoverImage,
    string? DatePublished,
    int? PageCount,
    decimal? HeightInches,
    decimal? WidthInches,
    decimal? LengthInches
);
