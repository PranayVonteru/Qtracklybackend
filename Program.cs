


using System.Security.Claims;
using System.Text.Json.Serialization;
using Demoproject.Data;
using Demoproject.Extensions;
using Demoproject.Hubs;
using Demoproject.Hubs.Demoproject.Hubs;
using Demoproject.Services;
using Demoproject.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

string clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET") ?? builder.Configuration["AzureAd:ClientSecret"];
string aesKey = Environment.GetEnvironmentVariable("AES_SECRET_KEY") ?? builder.Configuration["Encryption:AESKey"];
string aesIV = Environment.GetEnvironmentVariable("AES_SECRET_IV") ?? builder.Configuration["Encryption:AESIV"];

// Add CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowSpecificOrigin", builder =>
//    {
//        builder.WithOrigins("http://localhost:5173", "http://localhost:3000", "https://localhost:5173")
//               .AllowAnyMethod()
//               .AllowAnyHeader()
//               .AllowCredentials();
//    });
//});
//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(policy =>
//    {
//        policy.WithOrigins("https://your-firebase-app.web.app")
//              .AllowAnyHeader()
//              .AllowAnyMethod();
//    });
//});

//app.UseCors();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins(
                "https://qtrackley.web.app",           // Your production frontend
                "http://localhost:5173",               // Development frontend (optional)
                "http://localhost:3000"                // Development frontend (optional)
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();  // Important for authentication
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
// Add session support for secure key exchange
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Add SignalR
builder.Services.AddSignalR();

// Add Authentication
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd")
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph(builder.Configuration.GetSection("DownstreamApi"))
    .AddInMemoryTokenCaches();

// Configure JWT Bearer options
builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuers = new[]
        {
            "https://login.microsoftonline.com/0eadb77e-42dc-47f8-bbe3-ec2395e0712c/v2.0",
            "https://sts.windows.net/0eadb77e-42dc-47f8-bbe3-ec2395e0712c/"
        },
        ValidateAudience = true,
        ValidAudiences = new[]
        {
            "api://4a8ac758-c6bb-408f-8fdc-03e0a8bf72e9",
            "4a8ac758-c6bb-408f-8fdc-03e0a8bf72e9"
        },
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5),
        NameClaimType = "name",
        RoleClaimType = "roles"
    };

    options.MetadataAddress = "https://login.microsoftonline.com/0eadb77e-42dc-47f8-bbe3-ec2395e0712c/v2.0/.well-known/openid-configuration";

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            Console.WriteLine($"Token: {context.Request.Headers.Authorization}");
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            Console.WriteLine("Token validated successfully");
            var claims = context.Principal?.Claims?.Select(c => $"{c.Type}: {c.Value}");
            Console.WriteLine($"Claims: {string.Join(", ", claims ?? new string[0])}");

            var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
            await userService.SaveBasicUserToDatabaseAsync(
                context.Principal?.FindFirst("oid")?.Value
                ?? context.Principal?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                ?? context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                context.Principal
            );

            return;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"OnChallenge: {context.Error}, {context.ErrorDescription}");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            Console.WriteLine($"Token received: {context.Token?[..Math.Min(50, context.Token?.Length ?? 0)]}...");
            return Task.CompletedTask;
        }
    };
});

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserAccess", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
    options.AddPolicy("ManagerAccess", policy => policy.RequireRole("manager", "admin"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
});

// Add EF Core
builder.Services.AddDbContext<QTraklyDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the encryption service
builder.Services.AddSingleton<EncryptionService>();

// Register other services
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ISubTaskService, SubTaskService>();
builder.Services.AddScoped<IDependencyService, DependencyService>();
builder.Services.AddScoped<IFocusBoardService, FocusBoardService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskLogService, TaskLogService>();
builder.Services.AddScoped<ITimesheetService, TimesheetService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Quadrant Technologies API", Version = "v1" });
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows // Corrected to OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow // Define AuthorizationCode flow directly
            {
                AuthorizationUrl = new Uri("https://login.microsoftonline.com/0eadb77e-42dc-47f8-bbe3-ec2395e0712c/oauth2/v2.0/authorize"),
                TokenUrl = new Uri("https://login.microsoftonline.com/0eadb77e-42dc-47f8-bbe3-ec2395e0712c/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "api://4a8ac758-c6bb-408f-8fdc-03e0a8bf72e9/QTask_Backend", "Access the API as a user" }
                }
            }
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            new[] { "api://4a8ac758-c6bb-408f-8fdc-03e0a8bf72e9/QTask_Backend" }
        }
    });
});

// Ensure encryption keys exist on startup
builder.Configuration.EnsureEncryptionKeysExist();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<QTraklyDBContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Quadrant Technologies API v1");
        c.OAuthClientId("76064fb0-a595-4e08-8f74-48fdd4bcd5d0");
        c.OAuthUsePkce();
        c.OAuthScopeSeparator(" ");
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable CORS
app.UseCors("AllowSpecificOrigin");

// Enable session
app.UseSession();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map SignalR hub
app.MapHub<FeedbackHub>("/feedbackHub");

app.MapControllers();
app.MapGet("/health", () => "API is running");

app.Run();