
using Shelfie.Models;
using Shelfie.Models.Dto;

namespace Shelfie.Services;

public interface ILibraryService
{
    public Task<LibraryDto> GetLibraryData(string userId);
}