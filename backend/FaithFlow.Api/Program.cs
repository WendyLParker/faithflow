using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FaithFlow.Backend.Interfaces;
using FaithFlow.Backend.Services;
using FaithFlow.Backend.Extensions;
using FaithFlow.Backend.Common;
using FluentValidation;
using FaithFlow.Backend.DTOs.Validators;
using Amazon.CognitoIdentityProvider;
using Amazon.BedrockRuntime;
using Microsoft.Extensions.Options;
using Amazon;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.AddService<ValidationFilter>();
});
builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

// Register services
builder.Services.AddScoped<IProgressNoteRepository, ProgressNoteService>();
builder.Services.AddScoped<IRequestCommentRepository, RequestCommentService>();
builder.Services.AddScoped<IRequestRepository, RequestService>();
builder.Services.AddScoped<IRequestTypeRepository, RequestTypeService>();
builder.Services.AddScoped<IGroupRepository, GroupService>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleService>();
builder.Services.AddScoped<INotificationRepository, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddFaithFlowDatabase(builder.Configuration);

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RequestCreateDtoValidator>();

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
            // TODO: Enable audience validation before production (ValidAudience = ClientId)
            ValidateAudience = false,
            ValidAudience = cognitoSettings?.ClientId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddSingleton<IAmazonCognitoIdentityProvider>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<CognitoSettings>>().Value;

    return new AmazonCognitoIdentityProviderClient(
        RegionEndpoint.GetBySystemName(settings.Region)
    );
});

// ====================== Bedrock (AI cost estimation) ======================
builder.Services.Configure<BedrockSettings>(builder.Configuration.GetSection("Bedrock"));

builder.Services.AddSingleton<IAmazonBedrockRuntime>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<BedrockSettings>>().Value;

    return new AmazonBedrockRuntimeClient(
        new AmazonBedrockRuntimeConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(settings.Region),
        });
});

builder.Services.AddScoped<ICostEstimationService, BedrockCostEstimationService>();

builder.Services.AddAuthorization();

// ====================== CORS ======================
var configuredOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

var allowedOrigins = configuredOrigins.ToList();
if (builder.Environment.IsDevelopment())
{
    foreach (var origin in new[] { "http://localhost:5173", "http://127.0.0.1:5173" })
    {
        if (!allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
            allowedOrigins.Add(origin);
    }
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod();

        if (allowedOrigins.Count > 0)
        {
            policy.WithOrigins(allowedOrigins.ToArray());
        }
        else if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                    return false;

                return uri.Host is "localhost" or "127.0.0.1";
            });
        }
    });
});

var app = builder.Build();

app.ApplyFaithFlowMigrations();

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

app.UseCors("AllowFrontend");
app.UseMiddleware<ApiExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
