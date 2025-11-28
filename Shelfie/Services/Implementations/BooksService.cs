
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shelfie.Data;
using Shelfie.Models.Dto;

namespace Shelfie.Services;

public class BooksService(ApplicationDbContext dbContext, HttpClient httpClient) : IBooksService
{
    public async Task<IReadOnlyList<BookDto>> GetBooksByUserName(string userName)
    {
        return await dbContext.UserBooks
            .Include(book => book.User)
            .Where(book => book.User.UserName == userName)
            .Select(book => new BookDto(
                "Placeholder Name",
                "Placeholder Author",
                "Placeholder Description",
                "url",
                "1990-01-01",
                999,
                4.5f,
                50
            ))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<BookSearchResultDto>> QueryBooks(string query)
    {
        try
        {
            var response = await httpClient.GetAsync($"search.json?q={Uri.EscapeDataString(query)}&limit=3");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var jsonObject = JObject.Parse(json);

            var topBooks = jsonObject.SelectToken("docs")
                ?.Select(book =>
                {
                    var workKey = book["key"]?.ToString();
                    var workId = workKey?.Split('/').Last();
                    var title = book["title"]?.ToString();
                    var authorName = book["author_name"]?[0]?.ToString();
                    var coverId = book["cover_i"]?.ToString();
                    
                    if (string.IsNullOrEmpty(workId) || string.IsNullOrEmpty(title))
                        return null;

                    var coverUrl = !string.IsNullOrEmpty(coverId) 
                        ? $"https://covers.openlibrary.org/b/id/{coverId}-L.jpg"
                        : null;

                    return new BookSearchResultDto(
                        workId,
                        title,
                        authorName,
                        coverUrl);
                })
                .Where(book => book != null)
                .ToList();

            return topBooks;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to query books: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception($"Failed to parse book data: {ex.Message}", ex);
        }
    }

    public async Task<BookDto> GetBookById(string id)
    {
        try
        {
            var response = await httpClient.GetAsync($"works/{id}.json");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var jsonObject = JObject.Parse(json);
            
            var title = jsonObject["title"]?.ToString();
            var description = ExtractDescription(jsonObject["description"]);
            
            var authorKey = jsonObject["authors"]?[0]?["author"]?["key"]?.ToString();
            var authorName = "Unknown Author";
            
            if (!string.IsNullOrEmpty(authorKey))
            {
                var authorResponse = await httpClient.GetAsync($"{authorKey}.json");
                if (authorResponse.IsSuccessStatusCode)
                {
                    var authorJson = await authorResponse.Content.ReadAsStringAsync();
                    var authorObject = JObject.Parse(authorJson);
                    authorName = authorObject["name"]?.ToString() ?? authorName;
                }
            }
            
            var coverIds = jsonObject["covers"];
            var coverUrl = coverIds?.FirstOrDefault()?.ToString();
            var imageUrl = !string.IsNullOrEmpty(coverUrl) 
                ? $"https://covers.openlibrary.org/b/id/{coverUrl}-L.jpg"
                : null;
            
            var editionsResponse = await httpClient.GetAsync($"works/{id}/editions.json?limit=10");
            var pageCount = 0;
            var publishedDate = "";

            if (editionsResponse.IsSuccessStatusCode)
            {
                var editionsJson = await editionsResponse.Content.ReadAsStringAsync();
                var editionsObject = JObject.Parse(editionsJson);
                var entries = editionsObject["entries"];
                
                if (entries != null)
                {
                    foreach (var edition in entries)
                    {
                        var pages = edition["number_of_pages"]?.Value<int>();
                        if (pages.HasValue && pages.Value > 0)
                        {
                            pageCount = pages.Value;
                            publishedDate = edition["publish_date"]?.ToString() ?? "";
                            break;
                        }
                    }
                }
            }
            
            if (string.IsNullOrEmpty(publishedDate))
            {
                var firstPublishYear = jsonObject["first_publish_date"]?.ToString();
                publishedDate = firstPublishYear ?? "Unknown";
            }

            return new BookDto(
                title ?? "Unknown Title",
                authorName,
                description,
                imageUrl,
                publishedDate,
                pageCount,
                0.0f,
                0
            );
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to retrieve book with ID '{id}': {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception($"Failed to parse book data for ID '{id}': {ex.Message}", ex);
        }
    }

    private string ExtractDescription(JToken descriptionToken)
    {
        if (descriptionToken == null)
            return null;
        
        if (descriptionToken.Type == JTokenType.String)
        {
            return descriptionToken.ToString();
        }

        if (descriptionToken.Type == JTokenType.Object)
        {
            return descriptionToken["value"]?.ToString();
        }

        return null;
    }
}
