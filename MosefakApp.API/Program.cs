using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.Cookies;
using MosefakApi.Business.Services.FireBase;
using System.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>(); // 👈 Load from User Secrets

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

    // This forces consistent enum handling across all serialization contexts
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
    options.JsonSerializerOptions.WriteIndented = false; // For production
});

builder.Services.OptionsPatternConfig(builder.Configuration); // belong IOptions Pattern

builder.Services.AddDataProtection();
builder.Services.AddMemoryCache();

// ✅ Configure Serilog from `appsettings.json`
builder.Host.UseCustomSerilog();

var firebaseCredentialsPath = Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["Firebase:ServiceAccountKeyPath"]!);

if (System.IO.File.Exists(firebaseCredentialsPath))
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile(firebaseCredentialsPath)
    });
    builder.Services.AddSingleton<IFirebaseService, FirebaseService>();
}
else
{
    Console.WriteLine($"WARNING: Firebase service account key file not found at {firebaseCredentialsPath}. Firebase notifications will be disabled.");
}

// Call Container here

builder.Services.RegisterConfiguration(builder.Configuration);
builder.Services.RegisterIdentityConfig();
builder.Services.AddHttpContextAccessor();
builder.Services.RegisterFluentValidationSettings();

builder.Services.AddRepositories();

// for permission based authorization

builder.Services.AddTransient(typeof(IAuthorizationHandler), typeof(PermissionAuthorizationHandler));
builder.Services.AddTransient(typeof(IAuthorizationPolicyProvider), typeof(PermissionAuthorizationPolicyProvider));
builder.Services.AddSingleton<AppointmentJob>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddSwaggerServices();

// Call Seed Data

await builder.Services.Seeding();

builder.Services.AddAuthentication(builder.Configuration);


#region For Validation Error

builder.Services.Configure();

#endregion
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed roles and permissions on startup
//using (var scope = app.Services.CreateScope())
//{
//    var serviceProvider = scope.ServiceProvider;
//    try
//    {
//        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//        await SeedIdentityData.SeedRolesAndPermissionsAsync(roleManager);
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Error seeding roles and permissions: {ex.Message}");
//    }
//}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStatusCodePagesWithRedirects("/errors/{0}");

app.UseHttpsRedirection();

app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

//app.UseStaticFiles();  // it's very very Important after added wwwroot folder and folder of images that belong each entity. 

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
    }
});

// ✅ Log every request in a structured way
app.UseSerilogRequestLogging();

app.UseRateLimiter();

app.UseAuthentication();
app.UsePermissionAuthorization(); // your custom permission check
app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<CalculateTimeOfRequest>();
app.UseMiddleware<ErrorHandlingMiddleWare>();


app.MapControllers();
// ✅ Schedule the Recurring Job
ScheduleRecurringJob(app.Services);

app.Run();


void ScheduleRecurringJob(IServiceProvider services)
{
    using (var scope = services.CreateScope())
    {
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        var job = scope.ServiceProvider.GetRequiredService<AppointmentJob>();

        string recurringJobId = "activateEmployeesJob";

        recurringJobManager.AddOrUpdate(
            recurringJobId,
            () => job.Run(), // ✅ Uses DI properly
            Cron.Daily
        );
    }
}
