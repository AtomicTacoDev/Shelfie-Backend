
using Newtonsoft.Json;

namespace Shelfie.Services;

public class ObjectDefinitionService : IObjectDefinitionService
{
    private readonly Dictionary<string, ObjectDefinition> _definitions;

    public ObjectDefinitionService()
    {
        var json = File.ReadAllText("./objects.json");
        _definitions = JsonConvert.DeserializeObject<Dictionary<string, ObjectDefinition>>(json) 
                       ?? new Dictionary<string, ObjectDefinition>();
    }

    public ObjectDefinition? GetDefinition(string objectTypeId)
    {
        return _definitions.GetValueOrDefault(objectTypeId);
    }

    public Dictionary<string, ObjectDefinition> GetAllDefinitions()
    {
        return _definitions;
    }
}
