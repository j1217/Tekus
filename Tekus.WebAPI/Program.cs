using Microsoft.EntityFrameworkCore;
using Tekus.Core.Interfaces.Repositories;
using Tekus.Infrastructure.Data;
using Tekus.Infrastructure.Repositories;
using Tekus.Infrastructure.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de servicios -------------------------------------------------

// 1. Add DbContext (Entity Framework Core)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add Repositories (Patr�n Repository)
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();

// 3. Add External Services (Pa�ses)
builder.Services.AddHttpClient<ICountryService, CountryService>(client =>
{
    client.BaseAddress = new Uri("https://api.countries.example.com/"); // URL del servicio real
});

// 4. Add Authentication (JWT Bearer)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// 5. Add Swagger/OpenAPI con configuraci�n JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tekus API", Version = "v1" });

    // Configuraci�n para JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
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

// 6. Add Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Configuraci�n del pipeline HTTP --------------------------------------------

// 1. Migraciones autom�ticas (solo en desarrollo)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate(); // Aplica migraciones pendientes
    }
}

// 2. Swagger en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tekus API v1"));
}

// 3. Middlewares
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// 4. Mapeo de controladores
app.MapControllers();

// 5. Endpoint de verificaci�n de salud (opcional pero recomendado)
app.MapGet("/", () => "Tekus API is running!");

app.Run();