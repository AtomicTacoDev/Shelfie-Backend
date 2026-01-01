
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shelfie.Data;
using Shelfie.Data.Models;
using Shelfie.Models;
using Shelfie.Models.Dto;

namespace Shelfie.Services;

public class LibraryService(ApplicationDbContext dbContext, IBooksService booksService) : ILibraryService
{
    private const int MaxRetries = 3;
    
    public async Task<LibraryDto> GetLibraryData(string userId)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<PlacedObjectDto>> GetObjects(string userName)
    {
        var library = await GetLibrary(userName);
        
        return library.Objects
            .Select(obj => new PlacedObjectDto(
                obj.Id,
                obj.ObjectTypeId,
                new PositionDto(obj.PositionX, obj.PositionY, obj.PositionZ),
                obj.Rotation
            ))
            .ToList();
    }
    
    public async Task<PlacedObjectDto?> TryPlaceObject(
        string userName,
        string objectTypeId,
        PositionDto position,
        float rotation)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                var library = await GetLibrary(userName);
                
                if (library == null)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                if (IsPositionOccupied(library, position, rotation, -1))
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                var newObject = new PlacedObject
                {
                    ObjectTypeId = objectTypeId,
                    PositionX = position.x,
                    PositionY = position.y,
                    PositionZ = position.z,
                    Rotation = rotation,
                    LibraryId = library.Id
                };

                dbContext.PlacedObjects.Add(newObject);
                await dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return new PlacedObjectDto(newObject.Id, objectTypeId, position, rotation);
            }
            catch (PostgresException ex) when (ex.SqlState == "40001")
            {
                if (attempt == MaxRetries) throw;
            }
        }

        return null;
    }

    public async Task<PlacedObjectDto?> TryMoveObject(
        string userName,
        int objectId,
        string objectTypeId,
        PositionDto position,
        float rotation)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var objectToMove = await dbContext.PlacedObjects.FindAsync(objectId);
                if (objectToMove == null) return null;

                var library = await GetLibrary(userName);
                if (IsPositionOccupied(library, position, rotation, objectId)) return null;

                objectToMove.PositionX = position.x;
                objectToMove.PositionY = position.y;
                objectToMove.PositionZ = position.z;
                objectToMove.Rotation = rotation;

                await dbContext.SaveChangesAsync();

                return new PlacedObjectDto(objectId, objectTypeId, position, rotation);
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await dbContext.PlacedObjects.AnyAsync(o => o.Id == objectId);
                if (!exists) return null;

                await dbContext.Entry(await dbContext.PlacedObjects.FindAsync(objectId)!).ReloadAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == "40001")
            {
                if (attempt == MaxRetries) throw;
            }
        }

        return null;
    }

    public async Task DeleteObject(int objectId)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var objectToDelete = await dbContext.PlacedObjects.FindAsync(objectId);
                if (objectToDelete == null) return;

                dbContext.PlacedObjects.Remove(objectToDelete);
                await dbContext.SaveChangesAsync();
                return;
            }
            catch (DbUpdateConcurrencyException)
            {
                var objectToReload = await dbContext.PlacedObjects.FindAsync(objectId);
                if (objectToReload != null) await dbContext.Entry(objectToReload).ReloadAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == "40001")
            {
                if (attempt == MaxRetries) throw;
            }
        }
    }
    
    public async Task<BookshelfDataDto?> GetBookshelfData(string userName, int bookshelfId)
    {
        var library = await GetLibrary(userName);

        var placedObject = library?.Objects.FirstOrDefault(o => o.Id == bookshelfId);
        if (placedObject == null) return null;

        var bookshelfBooks = await dbContext.BookshelfBooks
            .Include(bb => bb.UserBook)
            .ThenInclude(ub => ub.Book)
            .Where(bb => bb.PlacedObjectId == bookshelfId)
            .OrderBy(bb => bb.ShelfId)
            .ThenBy(bb => bb.Index)
            .ToListAsync();

        var groupedByShelves = bookshelfBooks
            .GroupBy(bb => bb.ShelfId)
            .Select(group => new BookshelfShelfDto(
                group.Key,
                group.Select(bb =>
                {
                    var book = bb.UserBook.Book;
                    return new BookshelfBookDto(
                        $"{bb.UserBookId}-{bb.Id}",
                        bb.UserBookId,
                        book.Title,
                        book.Author,
                        bb.Index
                    );
                }).ToList()
            ))
            .ToList();

        return new BookshelfDataDto(groupedByShelves);
    }

    public async Task<BookshelfDataDto> UpdateBookshelfData(string userName, int bookshelfId, BookshelfDataDto data)
    {
        var library = await GetLibrary(userName);
        if (library == null)
            throw new InvalidOperationException("Library not found");
        
        var bookshelf = library.Objects.FirstOrDefault(o => o.Id == bookshelfId);
        if (bookshelf == null)
            throw new InvalidOperationException("Bookshelf not found");
        
        var userBookIds = await dbContext.UserBooks
            .Where(ub => ub.UserId == library.UserId)
            .Select(ub => ub.Id)
            .ToListAsync();
        
        var invalidBooks = data.Shelves
            .SelectMany(s => s.Books)
            .Where(b => !userBookIds.Contains(b.UserBookId))
            .ToList();

        if (invalidBooks.Any())
        {
            throw new InvalidOperationException(
                $"Invalid books: {string.Join(", ", invalidBooks.Select(b => b.UserBookId))}. " +
                "These books don't belong to this user."
            );
        }
        
        var existingBooks = await dbContext.BookshelfBooks
            .Where(book => book.PlacedObjectId == bookshelfId)
            .ToListAsync();
        
        dbContext.BookshelfBooks.RemoveRange(existingBooks);
        await dbContext.SaveChangesAsync();
        
        foreach (var shelf in data.Shelves)
        {
            foreach (var book in shelf.Books)
            {
                var bookshelfBook = new BookshelfBook
                {
                    PlacedObjectId = bookshelfId,
                    UserBookId = book.UserBookId,
                    ShelfId = shelf.Id,
                    Index = book.Index
                };
                dbContext.BookshelfBooks.Add(bookshelfBook);
            }
        }

        await dbContext.SaveChangesAsync();

        return await GetBookshelfData(userName, bookshelfId) ?? data;
    }
    
    /* HELPERS */
    public async Task<Library?> GetLibrary(string userName) => await dbContext.Libraries
        .Include(l => l.Objects)
        .Include(l => l.User)
        .FirstOrDefaultAsync(l => l.User.UserName == userName);

    private static Vector3 ToVector3(PositionDto pos) => new Vector3(pos.x, pos.y, pos.z);

    private static bool IsPositionOccupied(Library library, PositionDto position, float rotation, int excludeObjectId)
    {
        var size = new Vector3(0.5f, 0.5f, 0.5f); // TODO: get by objectTypeId

        foreach (var obj in library.Objects)
        {
            if (obj.Id == excludeObjectId) continue;

            var objSize = new Vector3(0.5f, 0.5f, 0.5f); // TODO: get by objectTypeId
            var objPos = new Vector3(obj.PositionX, obj.PositionY, obj.PositionZ);

            if (CheckObbIntersection(ToVector3(position), size, rotation, objPos, objSize, obj.Rotation))
                return true;
        }

        return false;
    }

    private static bool CheckObbIntersection(Vector3 pos1, Vector3 size1, float rot1, Vector3 pos2, Vector3 size2, float rot2)
    {
        var quat1 = Quaternion.CreateFromAxisAngle(Vector3.UnitY, rot1);
        var quat2 = Quaternion.CreateFromAxisAngle(Vector3.UnitY, rot2);

        var axes1 = GetRotationAxes(quat1);
        var axes2 = GetRotationAxes(quat2);

        var halfSize1 = size1 * 0.5f;
        var halfSize2 = size2 * 0.5f;

        var centerDiff = pos2 - pos1;

        for (var i = 0; i < 3; i++)
            if (TestAxis(axes1[i], centerDiff, halfSize1, halfSize2, axes1, axes2)) return false;

        for (var i = 0; i < 3; i++)
            if (TestAxis(axes2[i], centerDiff, halfSize1, halfSize2, axes1, axes2)) return false;

        for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
            {
                var axis = Vector3.Cross(axes1[i], axes2[j]);
                if (axis.LengthSquared() < 0.0001f) continue;

                axis = Vector3.Normalize(axis);
                if (TestAxis(axis, centerDiff, halfSize1, halfSize2, axes1, axes2)) return false;
            }

        return true;
    }

    private static Vector3[] GetRotationAxes(Quaternion quat) =>
        new[]
        {
            Vector3.Transform(Vector3.UnitX, quat),
            Vector3.Transform(Vector3.UnitY, quat),
            Vector3.Transform(Vector3.UnitZ, quat)
        };

    private static bool TestAxis(Vector3 axis, Vector3 centerDiff, Vector3 halfSize1, Vector3 halfSize2, Vector3[] axes1, Vector3[] axes2)
    {
        var centerProjection = Math.Abs(Vector3.Dot(centerDiff, axis));

        var projection1 =
            Math.Abs(Vector3.Dot(axes1[0], axis)) * halfSize1.X +
            Math.Abs(Vector3.Dot(axes1[1], axis)) * halfSize1.Y +
            Math.Abs(Vector3.Dot(axes1[2], axis)) * halfSize1.Z;

        var projection2 =
            Math.Abs(Vector3.Dot(axes2[0], axis)) * halfSize2.X +
            Math.Abs(Vector3.Dot(axes2[1], axis)) * halfSize2.Y +
            Math.Abs(Vector3.Dot(axes2[2], axis)) * halfSize2.Z;

        return centerProjection > projection1 + projection2;
    }
}

