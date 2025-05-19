namespace MosefakApi.Business.Services.Email
{
    public class EmailBodyBuilder : IEmailBodyBuilder
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailBodyBuilder(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> GenerateEmailBody(
            string templateName,
            string imageUrl,
            string header,
            string TextBody,
            // New optional parameters
            string verificationCode = null,
            string currentDate = null,
            string username = null)
        {
            // Ensure your templates are in wwwroot/templates/
            var templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "templates", templateName);

            if (!System.IO.File.Exists(templatePath))
            {
                // Handle template not found, perhaps log an error or throw an exception
                throw new FileNotFoundException($"Email template not found at {templatePath}");
            }

            var body = await System.IO.File.ReadAllTextAsync(templatePath);

            body = body.Replace("[imageUrl]", imageUrl ?? string.Empty)
                       .Replace("[header]", header ?? string.Empty)
                       .Replace("[TextBody]", TextBody ?? string.Empty); // This will be the main paragraph

            // Replace new placeholders if values are provided
            if (!string.IsNullOrEmpty(verificationCode))
            {
                body = body.Replace("[verificationCode]", verificationCode);
            }
            if (!string.IsNullOrEmpty(currentDate))
            {
                body = body.Replace("[currentDate]", currentDate);
            }
            if (!string.IsNullOrEmpty(username))
            {
                body = body.Replace("[username]", username);
            }

            return body;
        }
    }
}