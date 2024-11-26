using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Server.Utils;


var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
Env.Load();


// Get DB path and file name from environment variables, or use default values
string dbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "Data";  // Default to "Data" folder
string dbFileName = Environment.GetEnvironmentVariable("DB_FILE_NAME") ?? "mydatabase"; // Default to "mydatabase"

// Get the connection string from appsettings.json and replace placeholders
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                        .Replace("${DB_PATH}", dbPath)
                        .Replace("${DB_FILE_NAME}", dbFileName);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));


// Add authentication, controllers, etc.
builder.Services.AddControllers();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
        };
    });


builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

// Get the HTTPS port number from the environment variable (or use a default)
string httpsPort = Environment.GetEnvironmentVariable("PORT") ?? "5003";


// Configure Kestrel to listen only on the HTTPS port
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(int.Parse(httpsPort), listenOptions =>
    {
        listenOptions.UseHttps(); // Enable HTTPS
    });
});

// Add services to the container (e.g., controllers, Swagger, etc.)
builder.Services.AddControllers();

// Build the application
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();  // This will apply any pending migrations
}


// Middleware setup.
app.UseAuthentication();
app.UseAuthorization();
// Use HTTPS redirection if desired (although not needed if we're only using HTTPS)
app.UseHttpsRedirection();

// Map controller routes (create controllers as needed)
app.MapControllers();
// Example usage
// Run the application
Logger.Log($"Server is starting...");
app.Lifetime.ApplicationStarted.Register(() =>
{
    Logger.Log($"Server started and running on port {httpsPort}");
});
app.Lifetime.ApplicationStopping.Register(() =>
{
    Logger.Log("Server is shutting down.");
});
app.Run();






