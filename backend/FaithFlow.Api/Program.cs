using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FaithFlow.Backend.Interfaces;
using FaithFlow.Backend.Services;
using FaithFlow.Backend.Data;
using Microsoft.EntityFrameworkCore;
using FaithFlow.Backend.Common;
using FluentValidation;
using FaithFlow.Backend.DTOs.Validators;
using Amazon.CognitoIdentityProvider;
using Microsoft.Extensions.Options;
using Amazon;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Register services
builder.Services.AddScoped<IProgressNoteRepository, ProgressNoteService>();
builder.Services.AddScoped<IPrayerRepository, PrayerService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<PrayerCreateDtoValidator>();

// ====================== Swagger ======================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FaithFlow API",
        Version = "v1",
        Description = "Prayer Tracking API with AWS Cognito"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

// ====================== Cognito Settings ======================
builder.Services.Configure<CognitoSettings>(
    builder.Configuration.GetSection("Cognito"));

// ====================== Authentication ======================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var cognitoSettings = builder.Configuration.GetSection("Cognito").Get<CognitoSettings>();

        var authority = cognitoSettings?.Authority;

        options.Authority = authority;
        options.MetadataAddress = $"{authority}/.well-known/openid-configuration";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authority,
            // TODO: Fix audience validation before production
            ValidateAudience = false,
            ValidAudience = cognitoSettings?.ClientId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var header = ctx.Request.Headers.Authorization.FirstOrDefault() ?? "[NO HEADER]";
                Console.WriteLine($"[OnMessageReceived] Header: {header.Substring(0, Math.Min(100, header.Length))}...");
                if (header.StartsWith("Bearer "))
                {
                    ctx.Token = header.Substring(7).Trim();
                    Console.WriteLine($"[OnMessageReceived] Token extracted, length: {ctx.Token.Length}");
                }
                return Task.CompletedTask;
            },

            OnTokenValidated = ctx =>
            {
                Console.WriteLine("✅ TOKEN VALIDATED SUCCESSFULLY");
                var sub = ctx.Principal?.FindFirst("sub")?.Value;
                Console.WriteLine($"   Sub claim: {sub}");
                return Task.CompletedTask;
            },

            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine("🔴 AUTH FAILED: " + ctx.Exception.Message);
                if (ctx.Exception.InnerException != null)
                    Console.WriteLine("   Inner: " + ctx.Exception.InnerException.Message);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddSingleton<IAmazonCognitoIdentityProvider>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<CognitoSettings>>().Value;

    // Use default credential chain + explicit profile if needed
    return new AmazonCognitoIdentityProviderClient(
        RegionEndpoint.GetBySystemName(settings.Region)
    );
});

builder.Services.AddAuthorization();

// ====================== CORS ======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ====================== Middleware ======================
if (app.Environment.IsDevelopment())
{
    app.UseStaticFiles();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocumentTitle = "FaithFlow API";
        c.DefaultModelsExpandDepth(-1);
        c.InjectStylesheet("/swagger-ui/custom.css");
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.UseMiddleware<ApiExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();