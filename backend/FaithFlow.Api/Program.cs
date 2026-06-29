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

builder.Services.AddControllers(options =>
{
    options.Filters.AddService<ValidationFilter>();
});
builder.Services.AddScoped<ValidationFilter>();
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