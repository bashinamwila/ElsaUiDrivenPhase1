using System.Text.Json.Serialization;

namespace ElsaDrivenWebApp.Services.Models
{
    public class UsertaskViewModel
    {
        [JsonPropertyName("taskDescription")]
        public string TaskDescription { get; set; } = string.Empty;

        [JsonPropertyName("taskName")]
        public string TaskName { get; set; } = string.Empty;

        [JsonPropertyName("taskTitle")]
        public string TaskTitle { get; set; } = string.Empty;

        [JsonPropertyName("taskData")]
        public string TaskData { get; set; } = string.Empty;

        [JsonPropertyName("workflowInstanceId")]
        public string WorkflowInstanceId { get; set; } = string.Empty;

        [JsonPropertyName("uIDefinition")]
        public string UIDefinition { get; set; } = string.Empty;

        [JsonPropertyName("signal")]
        public string Signal { get; set; } = string.Empty;

        [JsonPropertyName("engineId")]
        public string EngineId { get; set; } = string.Empty;
    }
}
