namespace MosefakApp.Core.Dtos.ChatBot.Responses;
public class ChatResponseDto
{
    public bool Success { get; set; }
    public string Reply { get; set; } = null!;
    public string Error { get; set; } = null!;
}
