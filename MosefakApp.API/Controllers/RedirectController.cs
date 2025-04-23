namespace MosefakApp.API.Controllers
{
    [Route("redirect")]
    [ApiController]
    public class RedirectController : ControllerBase
    {
        [HttpGet("confirm-email")]
        public ContentResult ConfirmEmailRedirect([FromQuery] string userId, [FromQuery] string code)
        {
            var appLink = $"mosefak://confirm-email?userId={userId}&code={code}";
            var webLink = $"https://localhost:4200/auth/confirm-email?userId={userId}&code={code}";

            var html = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Redirecting...</title>
                <script>
                    window.onload = function () {{
                        var openInApp = confirm('📱 Do you want to open the app to confirm your email?');

                        if (openInApp) {{
                            window.location.href = '{appLink}';
                        }} else {{
                            window.location.href = '{webLink}';
                        }}
                    }};
                </script>
            </head>
            <body style='background:#f4f4f4;text-align:center;padding-top:100px;font-family:sans-serif;'>
                <h3>Hold on! We're redirecting you...</h3>
                <p>If nothing happens, please check your internet or try again.</p>
            </body>
            </html>
           ";

            return Content(html, "text/html");
        }

        [HttpGet("reset-password")]
        public ContentResult ResetPasswordRedirect([FromQuery] string code)
        {
            var appLink = $"mosefak://reset-password?code={Uri.EscapeDataString(code)}";
            var webLink = $"https://myapp.com/reset-password?code={Uri.EscapeDataString(code)}";

            var html = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Redirecting to Reset Password...</title>
                <script>
                    window.onload = function () {{
                        var openInApp = confirm('🔐 Would you like to reset your password in the app?');
     
                        if (openInApp) {{
                            window.location.href = '{appLink}';
                        }} else {{
                            window.location.href = '{webLink}';
                        }}
                    }};
                </script>
            </head>
            <body style='background:#f4f4f4;text-align:center;padding-top:100px;font-family:sans-serif;'>
                <h3>Hang tight! Redirecting you to reset your password...</h3>
                <p>If nothing happens, click the link in your email again.</p>
            </body>
            </html>
            ";

            return Content(html, "text/html");
        }

    }
}
