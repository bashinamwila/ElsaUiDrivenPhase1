using ElsaDrivenWebApp.Services.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ElsaDrivenWebApp.Services
{
    public class UsertaskService
    {
        private readonly HttpClient httpClient;
        public Dictionary<string, UsertaskViewModel> workflowInstancesCache = new Dictionary<string, UsertaskViewModel>();

        public UsertaskService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<UsertaskViewModel[]> GetWorkflowsForSignal(string signal)
        {
            return await httpClient.GetFromJsonAsync<UsertaskViewModel[]>($"/v1/usertask-signals/{signal}");
        }

        public async Task<UsertaskViewModel[]> GetWorkflowsForSignals(List<string> signals)
        {
            var result = new List<UsertaskViewModel>();
            await Task.WhenAll(signals.Select(async i => result.AddRange(await GetWorkflowsForSignal(i))));
            return result.ToArray();
        }

        public async Task MarkAsCompleteAsync(string workflowInstanceId, string signal, JsonElement? signalData)
        {
            var data = new MarkAsCompletedPostModel
            {
                WorkflowInstanceId = workflowInstanceId,
                Input = signalData
            };

            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            await httpClient.PostAsync($"/v1/usertask-signals/{signal}/execute", content);
        }
    }
}

