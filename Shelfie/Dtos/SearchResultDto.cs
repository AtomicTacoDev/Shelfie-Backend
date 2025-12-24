
namespace Shelfie.Models.Dto;

public record BookSearchResultDto(
    string? Isbn13,
    string? Isbn10,
    string? Isbn,
    string Title,
    string? Author,
    string? CoverImage
);

public record UserSearchResultDto(string UserName);

public record SearchResultDto(
    IReadOnlyList<BookSearchResultDto> Books, 
    IReadOnlyList<UserSearchResultDto> Users
);
