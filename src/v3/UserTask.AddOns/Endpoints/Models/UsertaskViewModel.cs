namespace UserTask.AddOns.Endpoints.Models
{
    public class UsertaskViewModel
    {
        public string? WorkflowInstanceId { get; set; }
        public string? Signal { get; set; }
        public string? TaskName { get; set; }
        public string? TaskTitle { get; set; }
        public string? TaskDescription { get; set; }
        public string? UIDefinition { get; set; }
        public string? TaskData { get; set; }
        public string? EngineId { get; set; }
    }
}
