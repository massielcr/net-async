using System.Text.Json.Serialization;

// Add int to your context so OpenAPI can generate the schema for it
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(IAsyncEnumerable<string>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}