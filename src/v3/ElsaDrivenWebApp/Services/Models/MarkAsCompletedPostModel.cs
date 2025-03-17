using System.Text.Json.Serialization;
using System.Text.Json;

namespace ElsaDrivenWebApp.Services.Models
{
    public class MarkAsCompletedPostModel
    {
        [JsonPropertyName("workflowInstanceId")]
        public string WorkflowInstanceId { get; set; } = string.Empty;

        [JsonPropertyName("input")]
        public JsonElement? Input { get; set; } = null;
    }
}

