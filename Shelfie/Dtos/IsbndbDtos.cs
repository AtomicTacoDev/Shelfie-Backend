
namespace Shelfie.Models.Dto;

public class IsbndbSearchResponse
{
    public int Total { get; set; }
    public List<IsbndbBook> Books { get; set; } = new();
}

public class IsbndbBook
{
    public string? Title { get; set; }
    public string? Image { get; set; }
    public string? Synopsis { get; set; }
    public List<string>? Authors { get; set; }
    public string? Isbn13 { get; set; }
    public string? Isbn10 { get; set; }
    public string? Isbn { get; set; }
    public string? Date_Published { get; set; }
    public int? Pages { get; set; }
    public Dimensions_Structured? Dimensions_Structured { get; set; }
}

public class Dimensions_Structured
{
    public Dimension? Height { get; set; }
    public Dimension? Width { get; set; }
    public Dimension? Length { get; set; }
}

public class Dimension
{
    public string? Unit { get; set; }
    public decimal Value { get; set; }
}

public class IsbndbBookResponse
{
    public IsbndbBook Book { get; set; }
}