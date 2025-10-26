
using Shelfie.Models;

namespace Shelfie.Services;

public interface ILibraryService
{
    public Task<Library> GetLibraryData(string userId);
}