namespace MosefakApi.Business.Services;
public class AiIntegrationService : IAiIntegrationService
{
    private readonly HttpClient _httpClient;

    public AiIntegrationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> AskAiAsync(string question, string token)
    {
        var payload = new { question };
        var endpoint = "https://ai-medical-assistant-production.up.railway.app/ask";

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"AI Error: {error}");
        }

        var aiReply = await response.Content.ReadFromJsonAsync<AiResponse>();
        return aiReply?.Response ?? "No response from AI";
    }
}
