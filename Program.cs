using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SillyChatBackend.Config;
using SillyChatBackend.Data;
using SillyChatBackend.Repositories;
using SillyChatBackend.Services;
using SillyChatBackend.Utils;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = new JwtSettings
{
    AccessSecret = builder.Configuration["JwtSettings:AccessSecret"] ?? string.Empty,
    RefreshSecret = builder.Configuration["JwtSettings:RefreshSecret"] ?? string.Empty
};
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<IUserContext, UserContext>();

var websocketManager = new WebsocketConnectionManager();
builder.Services.AddSingleton(websocketManager);

// 🔹 1. Add Services (Dependency Injection)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SillyChat API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter JWT token in the format: Bearer {your token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Logging
// builder.Services.AddHttpLogging(options => { });
builder.Logging.AddConsole();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.AccessSecret))
        };
    });
builder.Services.AddHttpContextAccessor();

// 🔹 2. Database Configuration (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// 🔹 3. Add Custom Repositories & Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFriendRepository, FriendRepository>();
builder.Services.AddScoped<IFriendService, FriendService>();

// 🔹 4. Enable CORS (for frontend communication)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// 🔹 5. Build Application
var app = builder.Build();

// 🔹 6. Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseCors("AllowAll"); // Enable CORS
app.Use(async (context, next) =>
{
    var start = DateTime.UtcNow;
    await next(); // Call the next middleware
    var elapsed = DateTime.UtcNow - start;

    Console.WriteLine($"{context.Response.StatusCode} {context.Request.Method} {context.Request.Path} ({elapsed.TotalMilliseconds}ms)");
});
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120),
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 🔹 7. Run the App
app.Run();
