
namespace Shelfie.Models.Dto;

public record BookshelfBookDto(
    string Id,
    int UserBookId,
    string Title,
    string Author,
    int Index
);

public record BookshelfShelfDto(
    string Id,
    List<BookshelfBookDto> Books
);

public record BookshelfDataDto(
    List<BookshelfShelfDto> Shelves
);
