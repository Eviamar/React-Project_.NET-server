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
                        ?? "Data Source=E:\\React_Project\\Server\\Database\\mydatabase.db"; // Use a default value
connectionString = connectionString
                        .Replace("${DB_PATH}", dbPath)
                        .Replace("${DB_FILE_NAME}", dbFileName);

// Register services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Add authentication and JWT bearer configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]
                ?? "692a822cd595fc5125b2da4e54241eebeac8a1b178be78380b05daef4864436e"))
        };
    });
// Register JwtTokenHelper as a service
builder.Services.AddScoped<JwtTokenHelper>();
// Add authorization services
builder.Services.AddAuthorization();

// Get the HTTPS port number from the environment variable (or use a default)
string httpsPort = Environment.GetEnvironmentVariable("PORT") ?? "5003";

// Configure Kestrel to listen only on the HTTPS port
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000); // HTTP port
    options.ListenLocalhost(int.Parse(httpsPort), listenOptions =>
    {
        listenOptions.UseHttps(); // Enable HTTPS
    });
});

// Register controllers and other services
builder.Services.AddControllers();

// Build the application
var app = builder.Build();

// Application lifecycle events
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    Logger.Log("Shutdown is in progress...");
});
lifetime.ApplicationStarted.Register(() =>
{
    Logger.Log($"Server started and running on Https port {httpsPort} | Http port 5000");
});
lifetime.ApplicationStopped.Register(() =>
{
    Logger.Log("Server has fully stopped");
});

// Apply pending migrations to the database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();  // This will apply any pending migrations
}

// Middleware setup
app.UseAuthentication();
app.UseAuthorization();
app.UseRouting();

// Map controller routes
app.MapControllers();

// Log server startup
Logger.Log("Server is starting...");

// Run the application
app.Run();
