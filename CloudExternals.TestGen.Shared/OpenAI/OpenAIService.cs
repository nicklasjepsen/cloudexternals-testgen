using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CloudExternals.TestGen.Shared.OpenAI
{
    public class OpenAIService
    {
        private struct Endpoint
        {
            internal const string Chat = "/v1/chat/completions";
            internal const string Completion = "/v1/completions";
        }

        private readonly HttpClient _httpClient;
        public OpenAIService(HttpClient http)
        {
            _httpClient = http;
        }

        public async Task<ChatResponse> ChatCompletion(ChatMessage[] messages, OpenAIOptions settings)
        {
            return await Post<ChatResponse>(Endpoint.Chat, new ChatRequest(settings, messages));
        }

        public async Task<CompletionResponse> Completion(string prompt, OpenAIOptions settings)
        {
            return await Post<CompletionResponse>(Endpoint.Completion, new CompletionRequest(settings, prompt));
        }

        private async Task<T> Post<T>(string endpoint, object request)
        {
            var response = await _httpClient.PostAsync(endpoint,
                new StringContent(JsonConvert.SerializeObject(request), Encoding.Default, "application/json"));

            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }
    }
}
