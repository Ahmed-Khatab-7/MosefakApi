namespace MosefakApp.Core.IServices;
public interface IAiIntegrationService
{
    Task<string> AskAiAsync(string userQuestion, string token);
}
