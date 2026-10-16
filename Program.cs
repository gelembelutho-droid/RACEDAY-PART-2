using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RaceDay.API.Data;
using RaceDay.API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. DATABASE CONFIGURATION
// ============================================================
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================
// 2. SERVICES REGISTRATION
// ============================================================
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ============================================================
// 3. JWT AUTHENTICATION CONFIGURATION
// ============================================================
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// ============================================================
// 4. AUTHORIZATION CONFIGURATION
// ============================================================
builder.Services.AddAuthorization();

// ============================================================
// 5. CONTROLLERS
// ============================================================
builder.Services.AddControllers();

// ============================================================
// 6. SWAGGER CONFIGURATION
// ============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "RESTful API for the RaceDay event management system - Part 2",
        Contact = new OpenApiContact
        {
            Name = "Student of IIE - PROG6212",
            Email = "student@iie.ac.za"
        }
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the text input below.\n\nExample: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'"
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
            new string[] { }
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// ============================================================
// 7. CORS CONFIGURATION (for Part 3 MVC)
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ============================================================
// 8. MIDDLEWARE PIPELINE
// ============================================================

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
        c.RoutePrefix = "swagger"; // ✅ FIXED: Swagger now at /swagger
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ============================================================
// 9. DATABASE MIGRATION AND SEED DATA ON STARTUP
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();

    try
    {
        // Apply pending migrations
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("✅ Database migrated successfully.");

        // Seed data
        await DbSeeder.SeedData(dbContext);
        Console.WriteLine("✅ Database seeded successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database error: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

app.Run();