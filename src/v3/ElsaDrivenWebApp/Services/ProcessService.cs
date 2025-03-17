using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ElsaDrivenWebApp.Services
{
    public class ProcessService
    {
        private readonly HttpClient httpClient;

        public ProcessService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task SendSignal(string signal)
        {
            await PostObjectJson(new JsonObject(), $"v1/signals/{signal}/execute");
        }

        private async Task PostObjectJson(object data, string url)
        {
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            await httpClient.PostAsync(url, content);
        }
    }
}
