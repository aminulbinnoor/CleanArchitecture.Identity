using Identity.API;
using Identity.Application;
using Identity.Infrastructure;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    };
    
    // For Swagger testing
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully");
            return Task.CompletedTask;
        }
    };
});

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireManagerRole", policy => policy.RequireRole("Manager"));
    options.AddPolicy("UserManagement", policy => policy.RequireClaim("permission", "users.read", "users.create", "users.update", "users.delete"));
});

// Configure Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Identity API", 
        Version = "v1",
        Description = "Clean Architecture Identity Server with JWT Authentication and Role-Based Permissions",
        Contact = new OpenApiContact
        {
            Name = "Identity API Support",
            Email = "support@identityapi.com"
        }
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
        c.RoutePrefix = "swagger";
        c.OAuthClientId("swagger-ui");
        c.OAuthAppName("Swagger UI");
        c.OAuthUsePkce();
    });
    
    // Seed database
    using var scope = app.Services.CreateScope();
    try
    {
        var seeder = new DatabaseSeeder(
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
            scope.ServiceProvider.GetRequiredService<IPasswordHasher>());
        await seeder.SeedAsync();
        Console.WriteLine("Database seeded successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding database: {ex.Message}");
    }
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Add health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

// Add a simple home page
app.MapGet("/", () => Results.Ok(new 
{ 
    message = "Identity API Server is running!",
    documentation = "/swagger",
    endpoints = new 
    {
        auth = new 
        {
            register = "POST /api/auth/register",
            login = "POST /api/auth/login",
            logout = "POST /api/auth/logout",
            refreshToken = "POST /api/auth/refresh-token"
        },
        users = new 
        {
            getCurrentUser = "GET /api/users/me",
            getAllUsers = "GET /api/users (Admin only)",
            getUserById = "GET /api/users/{id} (Admin only)"
        }
    }
}));

Console.WriteLine($"Identity API Server started at: {DateTime.UtcNow}");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Swagger UI: https://localhost:5259/swagger");

app.Run();