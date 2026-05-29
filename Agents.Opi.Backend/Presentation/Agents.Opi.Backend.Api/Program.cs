using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using Agents.Opi.Backend.Api.Middleware;
using Agents.Opi.Backend.Api.Resources;
using Agents.Opi.Backend.Api.Security;
using Agents.Opi.Backend.Application;
using Agents.Opi.Backend.Infrastructure;
using Agents.Opi.Backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
LoadDotEnv();

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddTransient<GlobalExceptionMiddleware>();

var extensionSigningKeys = ExtensionSecretOptions.GetSigningKeys(builder.Configuration);
if (extensionSigningKeys.Count == 0)
{
    throw new InvalidOperationException(ApiMessages.MissingExtensionSecret);
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKeys = extensionSigningKeys,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateActor = false
        };
    });

builder.Services
    .AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AzureDevOpsExtension", policy =>
    {
        policy
            .WithOrigins("https://dev.azure.com", "https://*.visualstudio.com")
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders(ApiHeaders.ConversationId);
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AgentDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AzureDevOpsExtension");
app.UseAuthentication();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();

static void LoadDotEnv()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

    while (directory is not null)
    {
        var path = Path.Combine(directory.FullName, ".env");
        if (File.Exists(path))
        {
            foreach (var rawLine in File.ReadAllLines(path))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                {
                    continue;
                }

                var separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var key = line[..separatorIndex].Trim();
                var value = line[(separatorIndex + 1)..].Trim().Trim('"');
                Environment.SetEnvironmentVariable(key, value);
            }

            return;
        }

        directory = directory.Parent;
    }
}
