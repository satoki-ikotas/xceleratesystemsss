using APIPSI16.Data;
using APIPSI16.Models;
using APIPSI16.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Ensure Kestrel binds to both HTTP & HTTPS dev URLs even if a different launch profile is chosen.
// You can still override by setting ASPNETCORE_URLS externally.
var defaultUrls = "https://localhost:7263;http://localhost:5270";
var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? defaultUrls;
builder.WebHost.UseUrls(urls);

// ---- CORS policy ----
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMvcFrontend", policy =>
    {
        if (corsOrigins.Length > 0)
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    });
});

// ---- Controllers & Swagger ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "APIPSI16", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, new string[] { } } });
});

// ---- DbContext registration ----
// Check if we should use SQLite for testing (via environment variable)
var useSqliteForTesting = Environment.GetEnvironmentVariable("USE_SQLITE_FOR_TESTING")?.ToLower() == "true";

if (useSqliteForTesting)
{
    // Use SQLite file-based database for testing/CI environments
    // File-based is more reliable than in-memory for API testing
    Console.WriteLine("⚠️  Using SQLite file-based database for TESTING purposes");
    var dbPath = Path.Combine(Path.GetTempPath(), "xcelerate_test.db");
    builder.Services.AddDbContext<xcleratesystemslinks_SampleDBContext>(options =>
        options.UseSqlite($"Data Source={dbPath}"));
}
else
{
    // Use SQL Server (default production behavior)
    var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(defaultConn))
    {
        throw new InvalidOperationException("ConnectionStrings:DefaultConnection not configured. Add it to appsettings.Development.json or set the environment variable ConnectionStrings__DefaultConnection.");
    }
    
    Console.WriteLine($"Using SQL Server database (from connection string)");
    builder.Services.AddDbContext<xcleratesystemslinks_SampleDBContext>(options =>
        options.UseSqlServer(defaultConn));
}

// ---- JWT key ----
var keyBase64 = Environment.GetEnvironmentVariable("XCELERATE_JWT_KEY")
               ?? builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(keyBase64))
{
    throw new InvalidOperationException("JWT key not configured. Set environment variable XCELERATE_JWT_KEY or Jwt:Key in configuration.");
}

byte[] keyBytes;
try
{
    keyBytes = Convert.FromBase64String(keyBase64);
}
catch (FormatException)
{
    throw new InvalidOperationException("JWT key (XCELERATE_JWT_KEY or Jwt:Key) is not valid base64.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "xcelerate-links-api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "xcelerate-links-clients";

// ---- Services ----
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// ---- Authentication (JWT Bearer) ----
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = ctx =>
        {
            var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ctx.Exception, "JWT authentication failed.");
            return Task.CompletedTask;
        },
        OnChallenge = ctx =>
        {
            var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("JWT challenge triggered. Error: {err} - Description: {desc}", ctx.Error, ctx.ErrorDescription);
            return Task.CompletedTask;
        }
    };
});

// ---- Build pipeline ----
var app = builder.Build();

// ---- Initialize SQLite database if in testing mode ----
if (useSqliteForTesting)
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<xcleratesystemslinks_SampleDBContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            // Ensure database is created
            dbContext.Database.EnsureCreated();
            logger.LogInformation("SQLite database created/opened");
            
            // Seed with test users if no users exist
            if (!dbContext.Users.Any())
            {
                logger.LogInformation("Seeding test data...");
                
                var testUser = new User
                {
                    Name = "Test User",
                    Email = "test@example.com",
                    PhoneNumber = "1234567890",
                    Nationality = 1,  // int value
                    JobPreference = 1,  // int value
                    ProfileBio = "Test user for development",
                    DoB = DateOnly.FromDateTime(new DateTime(1990, 1, 1)),
                    Role = 1  // int value (JobSeeker role)
                };
                testUser.PasswordHash = passwordHasher.HashPassword(testUser, "test");
                
                var adminUser = new User
                {
                    Name = "Admin User",
                    Email = "admin@example.com",
                    PhoneNumber = "9876543210",
                    Nationality = 1,  // int value
                    JobPreference = 2,  // int value
                    ProfileBio = "Admin user",
                    DoB = DateOnly.FromDateTime(new DateTime(1985, 1, 1)),
                    Role = 2  // int value (Admin role)
                };
                adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "admin");
                
                dbContext.Users.AddRange(testUser, adminUser);
                dbContext.SaveChanges();
                
                logger.LogInformation("✅ Test users created:");
                logger.LogInformation("   📧 test@example.com / password: test");
                logger.LogInformation("   📧 admin@example.com / password: admin");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing SQLite database");
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // show detailed errors in development
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowMvcFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();