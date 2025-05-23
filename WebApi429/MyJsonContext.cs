using System.Text.Json.Serialization;

namespace WebApi429
{
    // Class is used for source generation of JSON serialization context and is not used directly in the code.
    [JsonSerializable(typeof(Api429))]
    [JsonSerializable(typeof(Api429Different))]
    public partial class MyJsonContext : JsonSerializerContext
    {
    }
}
