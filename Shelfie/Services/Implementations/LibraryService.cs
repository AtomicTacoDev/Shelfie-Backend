
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Shelfie.Data;
using Shelfie.Models;
using Shelfie.Models.Dto;

namespace Shelfie.Services;

public class LibraryService(ApplicationDbContext dbContext) : ILibraryService
{
    public async Task<LibraryDto> GetLibraryData(string userId)
    {
        /*var library = await dbContext.Libraries
            .Include(l => l.User)
            .Include(l => l.Objects)
            .FirstOrDefaultAsync(l => l.UserId == userId);
        
        var libraryDto = new LibraryDto (
            library.Id,
            library.User.UserName,
            library.Objects.Select(obj => new PlacedObjectDto (
                obj.Id,
                obj.PositionX,
                obj.PositionY,
                obj.Rotation
            )).ToList()
        );

        return libraryDto;*/

        throw new NotImplementedException();
    }

    public async Task<PlacedObjectDto?> TryPlaceObject(
        string userName,
        string objectTypeId,
        PositionDto position,
        float rotation)
    {
        var library = await dbContext.Libraries
            .Include(l => l.Objects)
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.User.UserName == userName);
        
        var size = new Vector3(0.5f, 0.5f, 0.5f); // TODO: get by objectTypeId
        
        foreach (var obj in library.Objects)
        {
            var objSize = new Vector3(0.5f, 0.5f, 0.5f); // TODO: get by objectTypeId
            var objPos = new Vector3(obj.PositionX, obj.PositionY, obj.PositionZ);
            var objRot = obj.Rotation;

            if (WouldCollide(ToVector3(position), size, rotation, objPos, objSize, objRot))
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
        
        return new PlacedObjectDto(
            newObject.Id,
            objectTypeId,
            position,
            rotation
        );
    }
    
    public static Vector3 ToVector3(PositionDto pos) => new Vector3(pos.x, pos.y, pos.z);
    
    public async Task<IReadOnlyList<PlacedObjectDto>?> GetObjects(string userName)
    {
        var library = await dbContext.Libraries
            .Include(l => l.Objects)
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.User.UserName == userName);

        return library.Objects
            .Select(obj => new PlacedObjectDto(
                obj.Id,
                obj.ObjectTypeId,
                new PositionDto(obj.PositionX, obj.PositionY, obj.PositionZ),
                obj.Rotation
            ))
            .ToList();
    }
    
    private static bool WouldCollide(
        Vector3 pos1, Vector3 size1, float rot1,
        Vector3 pos2, Vector3 size2, float rot2)
    {
        var corners1 = GetRotatedCorners(pos1, size1, rot1);
        var corners2 = GetRotatedCorners(pos2, size2, rot2);

        var axes = new List<Vector2>();
        
        AddAxes(corners1, axes);
        AddAxes(corners2, axes);

        foreach (var axis in axes)
        {
            var len = axis.Length();
            if (len == 0) continue;
            var normalized = axis / len;

            var (min1, max1) = ProjectOntoAxis(corners1, normalized);
            var (min2, max2) = ProjectOntoAxis(corners2, normalized);

            if (max1 < min2 || max2 < min1)
                return false;
        }

        return true;
    }

    private static void AddAxes(Vector2[] corners, List<Vector2> axes)
    {
        for (int i = 0; i < 4; i++)
        {
            var j = (i + 1) % 4;
            var edge = corners[j] - corners[i];
            axes.Add(new Vector2(-edge.Y, edge.X));
        }
    }

    private static Vector2[] GetRotatedCorners(Vector3 pos, Vector3 size, float rotDeg)
    {
        float halfW = size.X / 2;
        float halfD = size.Z / 2;

        var corners = new[]
        {
            new Vector2(-halfW, -halfD),
            new Vector2( halfW, -halfD),
            new Vector2( halfW,  halfD),
            new Vector2(-halfW,  halfD)
        };

        float rad = MathF.PI / 180f * rotDeg;
        float cos = MathF.Cos(rad);
        float sin = MathF.Sin(rad);

        for (int i = 0; i < corners.Length; i++)
        {
            var x = corners[i].X;
            var z = corners[i].Y;
            var rx = x * cos - z * sin;
            var rz = x * sin + z * cos;
            corners[i] = new Vector2(pos.X + rx, pos.Z + rz);
        }

        return corners;
    }

    private static (float min, float max) ProjectOntoAxis(Vector2[] corners, Vector2 axis)
    {
        var projections = corners.Select(c => Vector2.Dot(c, axis));
        return (projections.Min(), projections.Max());
    }

}