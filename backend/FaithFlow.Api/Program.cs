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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    //options.Filters.Add<ValidationFilter>();
});
builder.Services.AddEndpointsApiExplorer();

// Register our services
builder.Services.AddScoped<IPrayerRepository, PrayerService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

//FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<PrayerCreateDtoValidator>();

//Validation Filter
//builder.Services.AddScoped<ValidationFilter>();
// ====================== Swagger Configuration ======================
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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ====================== AWS Cognito Authentication ======================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Cognito:Authority"];
        options.Audience = builder.Configuration["Cognito:ClientId"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = false,
            RequireSignedTokens = false
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"❌ AUTH FAILED: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("✅ Token validated successfully!");
                Console.WriteLine($"Claims count: {context.Principal?.Claims.Count() ?? 0}");
                return Task.CompletedTask;
            }
        };
    });

// ====================== CORS (Important for Frontend) ======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
             .AllowAnyHeader()
             .AllowAnyMethod();
        // policy.WithOrigins("http://localhost:5173")   // Vite default port
        //       .AllowAnyHeader()
        //       .AllowAnyMethod()
        //       .AllowCredentials();                    // Important for auth later
    });
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Explicitly allow anonymous access to Swagger paths
    app.MapSwagger().AllowAnonymous();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowFrontend");
app.UseMiddleware<ApiExceptionMiddleware>();

app.UseAuthentication();   // Must come before Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();