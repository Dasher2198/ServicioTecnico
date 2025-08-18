using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ServicioTecnico.Data;
using ServicioTecnico.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger correctamente
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "REVTEC API",
        Version = "v1",
        Description = "API para Sistema de Revisión Técnica Vehicular"
    });

    // Configurar esquemas para evitar conflictos
    c.CustomSchemaIds(type => type.FullName);
});

// Configurar Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar CORS para permitir el frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed inicial de datos
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
        await DataSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error occurred seeding the DB.");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "api/swagger/{documentName}/swagger.json";
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "REVTEC API v1");
        c.RoutePrefix = "api/swagger";
        c.DocumentTitle = "REVTEC API Documentation";
    });
}

app.UseHttpsRedirection();

// Servir archivos estáticos ANTES de routing
app.UseStaticFiles();

// Habilitar CORS
app.UseCors("AllowFrontend");

app.UseRouting();

app.UseAuthorization();

// Mapear controladores API con configuración específica
app.MapControllers();

// Configurar rutas específicas
app.MapGet("/", async context =>
{
    context.Response.Redirect("/Login.html");
});

// Ruta específica para acceder a Swagger desde la raíz (opcional)
app.MapGet("/swagger", async context =>
{
    context.Response.Redirect("/api/swagger");
});

// Fallback inteligente
app.MapFallback(async context =>
{
    var path = context.Request.Path.Value?.ToLower();

    // Si es una ruta de API que no existe, devolver 404
    if (path?.StartsWith("/api/") == true)
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("API endpoint not found");
        return;
    }

    // Si es una ruta de Swagger, redirigir
    if (path?.Contains("swagger") == true)
    {
        context.Response.Redirect("/api/swagger");
        return;
    }

    // Para cualquier otra ruta, servir el login
    context.Response.Redirect("/Login.html");
});

app.Run();