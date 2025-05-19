namespace MosefakApp.Core.IServices.Email
{
    public interface IEmailBodyBuilder
    {
        Task<string> GenerateEmailBody(string templateName, string imageUrl, string header, string TextBody,
            string verificationCode = null, string currentDate = null, string username = null);
    }
}
