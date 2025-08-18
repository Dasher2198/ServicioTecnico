using Microsoft.EntityFrameworkCore;
using ServicioTecnico.Data;
using ServicioTecnico.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure CORS - MÁS PERMISIVO para desarrollo
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Swagger/OpenAPI para documentación de API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

// Solo usar HTTPS en producción
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Servir archivos estáticos (HTML, CSS, JS)
app.UseStaticFiles();

// IMPORTANTE: CORS debe ir ANTES de Authorization
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Configurar ruta por defecto para servir Login.html
app.MapFallbackToFile("Login.html");

// Asegurar que la base de datos existe y sembrar datos iniciales
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        // Asegurar que la base de datos existe
        await context.Database.EnsureCreatedAsync();

        // Sembrar datos iniciales
        await DataSeeder.SeedAsync(context);

        Console.WriteLine("✅ Base de datos configurada correctamente");
        Console.WriteLine($"🌐 Aplicación disponible en: https://localhost:{app.Urls.FirstOrDefault()?.Split(':').LastOrDefault() ?? "7252"}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error al configurar la base de datos: {ex.Message}");
        Console.WriteLine($"Detalles: {ex.InnerException?.Message}");
    }
}

app.Run();