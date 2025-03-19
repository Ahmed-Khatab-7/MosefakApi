using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MosefakApi.Business.Services;
public class AiIntegrationService : IAiIntegrationService
{
    private readonly HttpClient _httpClient;

    public AiIntegrationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> AskAiAsync(string question)
    {
        var payload = new { question };
        var endpoint = "https://ai-medical-assistant-production.up.railway.app/ask";

        var response = await _httpClient.PostAsJsonAsync(endpoint, payload);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"AI Error: {error}");
        }

        var aiReply = await response.Content.ReadFromJsonAsync<AiResponse>();
        return aiReply?.Response ?? "No response from AI";
    }

    private class AiResponse
    {
        public string Response { get; set; } = null!;
    }
}