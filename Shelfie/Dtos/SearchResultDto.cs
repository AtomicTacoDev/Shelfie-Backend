
namespace Shelfie.Models.Dto;

public record BookSearchResultDto(string Id, string Title, string Author, string ImageLink);

public record UserSearchResultDto(string UserName);
public record SearchResultDto(IReadOnlyList<BookSearchResultDto> Books, IReadOnlyList<UserSearchResultDto> Users);
