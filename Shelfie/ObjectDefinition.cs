
using System.Numerics;
using Newtonsoft.Json;
using Shelfie.Utils;

namespace Shelfie;

public class ObjectDefinition
{
    public string Name { get; set; } = string.Empty;
    
    public string ModelPath { get; set; } = string.Empty;
    
    public bool? IsBookshelf { get; set; } = null;
    
    [JsonConverter(typeof(Vector3JsonConverter))]
    public Vector3 Size { get; set; }
}
