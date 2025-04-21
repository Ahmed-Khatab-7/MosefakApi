using Microsoft.AspNetCore.Authentication.OAuth;

namespace MosefakApp.API.Extensions
{
    public static class AuthenticationConfig
    {

        public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var JwtOptions = configuration.GetSection("Jwt").Get<JwtSetting>();

            services.AddSingleton(JwtOptions!);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).
            AddJwtBearer(options =>
            {
                options.SaveToken = true;

                // here will validate parameters of Token.
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = JwtOptions!.Issuer,
                    ValidateAudience = true,
                    ValidAudience = JwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtOptions.Key))
                };
            })
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                googleOptions.CallbackPath = new PathString("/api/account/google-callback");

                googleOptions.Events = new OAuthEvents
                {
                    OnRedirectToAuthorizationEndpoint = context =>
                    {
                        var builder = new UriBuilder(context.RedirectUri)
                        {
                            Host = "yth3d4dbpe.eu-west-1.awsapprunner.com", // Ensure the correct domain
                            Scheme = "https"
                        };
                        context.RedirectUri = builder.Uri.ToString();
                        context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    },
                    OnRemoteFailure = context =>
                    {
                        // Handle error gracefully
                        context.Response.Redirect("/login?error=google_auth_failed");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    }
                };
            });


            return services;
        }
    }
}
