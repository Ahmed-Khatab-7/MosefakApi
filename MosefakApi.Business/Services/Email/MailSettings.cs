namespace MosefakApi.Business.Services.Email
{
    public class MailSettings
    {
        public string DisplayName { get; set; } = null!;
        public int Port { get; set; }
        public string Host { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
