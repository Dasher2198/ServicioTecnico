using Microsoft.EntityFrameworkCore;
using ServicioTecnico.Data;
using ServicioTecnico.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar para manejar referencias circulares
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;

        // IMPORTANTE: Configurar naming policy para camelCase
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

        // Asegurar que las propiedades se serialicen en camelCase
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;

        // Configurar para ser case-insensitive en deserialización
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Configure Entity Framework with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);

    // Mejorar configuración para desarrollo
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Configure CORS de manera más específica y segura
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5065", "https://localhost:7252")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Swagger/OpenAPI para documentación de API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "REVTEC API", Version = "v1" });
});

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "REVTEC API V1");
        c.RoutePrefix = "swagger";
    });
    app.UseDeveloperExceptionPage();
    app.UseCors("AllowAll"); // Usar política permisiva en desarrollo
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
    app.UseCors("DevelopmentPolicy");
}

// Servir archivos estáticos
app.UseStaticFiles();

// Middleware de autenticación (si se implementa en el futuro)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Configurar ruta por defecto
app.MapFallbackToFile("Login.html");

// Asegurar que la base de datos existe y sembrar datos iniciales
await EnsureDatabaseAsync(app);

app.Run();

// Función para inicializar la base de datos
static async Task EnsureDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        Console.WriteLine("🔄 Verificando conexión a la base de datos...");

        // Asegurar que la base de datos existe
        await context.Database.EnsureCreatedAsync();

        // Aplicar migraciones pendientes
        if (context.Database.GetPendingMigrations().Any())
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("✅ Migraciones aplicadas correctamente");
        }

        // Sembrar datos iniciales
        await DataSeeder.SeedAsync(context);

        logger.LogInformation("✅ Base de datos configurada correctamente");
        logger.LogInformation("🌐 Aplicación disponible en: {Urls}", string.Join(", ", app.Urls));
        logger.LogInformation("📖 Swagger disponible en: {SwaggerUrl}/swagger", app.Urls.FirstOrDefault());

        Console.WriteLine("✅ Base de datos configurada correctamente");
        Console.WriteLine($"🌐 Aplicación disponible en: {string.Join(", ", app.Urls)}");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error al configurar la base de datos");
        Console.WriteLine($"❌ Error al configurar la base de datos: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Detalle: {ex.InnerException.Message}");
        }

        // Mostrar instrucciones de solución
        Console.WriteLine("\n🔧 Soluciones posibles:");
        Console.WriteLine("1. Verificar SQL Server LocalDB: sqllocaldb info");
        Console.WriteLine("2. Crear LocalDB: sqllocaldb create MSSQLLocalDB");
        Console.WriteLine("3. Iniciar LocalDB: sqllocaldb start MSSQLLocalDB");
        Console.WriteLine("4. Aplicar migraciones: dotnet ef database update");
    }
}