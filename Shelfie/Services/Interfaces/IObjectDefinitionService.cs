namespace Shelfie.Services;

public interface IObjectDefinitionService
{
    public ObjectDefinition? GetDefinition(string objectTypeId);
}