using System.Text.Json;
using System.Text.Json.Serialization;

namespace AsyncGenerateAndConsumeStreams.Models
{
    public class GraphQLRequest
    {
        [JsonPropertyName("query")]
        public string? Query { get; set; }

        [JsonPropertyName("variables")]
        public IDictionary<string, object> Variables { get; } = new Dictionary<string, object>();

        public string ToJsonText() => JsonSerializer.Serialize(this);
    }
}
