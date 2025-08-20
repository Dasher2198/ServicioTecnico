using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioTecnico.Data;
using ServicioTecnico.Models;

namespace ServicioTecnico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseCheckController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DatabaseCheckController> _logger;

        public DatabaseCheckController(AppDbContext context, ILogger<DatabaseCheckController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("status")]
        public async Task<ActionResult<object>> GetDatabaseStatus()
        {
            try
            {
                // Verificar conexión a base de datos
                await _context.Database.CanConnectAsync();

                // Obtener estadísticas de usuarios
                var totalUsuarios = await _context.Usuarios.CountAsync();
                var usuariosActivos = await _context.Usuarios.CountAsync(u => u.Estado == "activo");
                var usuariosTecnicos = await _context.Usuarios.CountAsync(u => u.TipoUsuario == "tecnico" && u.Estado == "activo");
                var usuariosClientes = await _context.Usuarios.CountAsync(u => u.TipoUsuario == "cliente" && u.Estado == "activo");

                // Obtener estadísticas de otras entidades
                var totalEstaciones = await _context.Estaciones.CountAsync();
                var totalVehiculos = await _context.Vehiculos.CountAsync();
                var totalCitas = await _context.Citas.CountAsync();
                var citasProgramadas = await _context.Citas.CountAsync(c => c.EstadoCita == "programada");

                var response = new
                {
                    status = "connected",
                    timestamp = DateTime.Now,
                    database = new
                    {
                        connected = true,
                        name = _context.Database.GetDbConnection().Database
                    },
                    usuarios = new
                    {
                        total = totalUsuarios,
                        activos = usuariosActivos,
                        tecnicos = usuariosTecnicos,
                        clientes = usuariosClientes
                    },
                    entidades = new
                    {
                        estaciones = totalEstaciones,
                        vehiculos = totalVehiculos,
                        citas = totalCitas,
                        citasProgramadas = citasProgramadas
                    }
                };

                _logger.LogInformation(" Check de base de datos exitoso: {Response}", System.Text.Json.JsonSerializer.Serialize(response));

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error en check de base de datos");

                return Ok(new
                {
                    status = "error",
                    timestamp = DateTime.Now,
                    database = new
                    {
                        connected = false,
                        error = ex.Message
                    }
                });
            }
        }

        [HttpGet("usuarios")]
        public async Task<ActionResult<object>> GetUsuariosDetalle()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Select(u => new
                    {
                        u.IdUsuario,
                        u.Nombre,
                        u.Apellidos,
                        u.Email,
                        u.TipoUsuario,
                        u.Estado,
                        u.FechaRegistro
                    })
                    .OrderBy(u => u.TipoUsuario)
                    .ThenBy(u => u.Nombre)
                    .ToListAsync();

                var tecnicos = usuarios.Where(u => u.TipoUsuario == "tecnico").ToList();
                var clientes = usuarios.Where(u => u.TipoUsuario == "cliente").ToList();

                return Ok(new
                {
                    timestamp = DateTime.Now,
                    total = usuarios.Count,
                    tecnicos = new
                    {
                        cantidad = tecnicos.Count,
                        usuarios = tecnicos
                    },
                    clientes = new
                    {
                        cantidad = clientes.Count,
                        usuarios = clientes
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error al obtener detalle de usuarios");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("test-login")]
        public async Task<ActionResult<object>> TestLogin([FromBody] TestLoginRequest request)
        {
            try
            {
                _logger.LogInformation(" Probando login para: {Email}", request.Email);

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() &&
                                            u.Password == request.Password &&
                                            u.Estado == "activo");

                if (usuario == null)
                {
                    _logger.LogWarning(" Usuario no encontrado o credenciales incorrectas: {Email}", request.Email);

                    // Buscar por email para ver si existe
                    var usuarioExiste = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                    if (usuarioExiste == null)
                    {
                        return Ok(new
                        {
                            success = false,
                            message = "Usuario no existe",
                            email = request.Email
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            success = false,
                            message = "Contraseña incorrecta o usuario inactivo",
                            email = request.Email,
                            estado = usuarioExiste.Estado,
                            passwordMatch = usuarioExiste.Password == request.Password
                        });
                    }
                }

                _logger.LogInformation(" Login exitoso: {Email} - {TipoUsuario}", request.Email, usuario.TipoUsuario);

                return Ok(new
                {
                    success = true,
                    message = "Login exitoso",
                    usuario = new
                    {
                        id = usuario.IdUsuario,
                        nombre = usuario.Nombre,
                        apellidos = usuario.Apellidos,
                        email = usuario.Email,
                        tipoUsuario = usuario.TipoUsuario,
                        estado = usuario.Estado
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error en test login para: {Email}", request.Email);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("seed-test-data")]
        public async Task<ActionResult<object>> SeedTestData()
        {
            try
            {
                _logger.LogInformation(" Iniciando seed de datos de prueba...");

                // Verificar si ya existen técnicos
                var tecnicosExistentes = await _context.Usuarios.CountAsync(u => u.TipoUsuario == "tecnico");

                if (tecnicosExistentes == 0)
                {
                    var tecnicos = new List<Usuario>
                    {
                        new Usuario
                        {
                            Nombre = "Carlos",
                            Apellidos = "Rodríguez Méndez",
                            Cedula = "123456789",
                            Email = "tecnico@revtec.cr",
                            Telefono = "8888-7777",
                            Direccion = "San José, Costa Rica",
                            TipoUsuario = "tecnico",
                            Password = "tecnico123",
                            Estado = "activo",
                            FechaRegistro = DateTime.Now
                        },
                        new Usuario
                        {
                            Nombre = "Ana",
                            Apellidos = "López Vargas",
                            Cedula = "987654321",
                            Email = "ana.tecnico@revtec.cr",
                            Telefono = "8777-6666",
                            Direccion = "Cartago, Costa Rica",
                            TipoUsuario = "tecnico",
                            Password = "tecnico123",
                            Estado = "activo",
                            FechaRegistro = DateTime.Now
                        }
                    };

                    await _context.Usuarios.AddRangeAsync(tecnicos);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(" {Count} técnicos creados", tecnicos.Count);
                }

                // Verificar si ya existen clientes
                var clientesExistentes = await _context.Usuarios.CountAsync(u => u.TipoUsuario == "cliente");

                if (clientesExistentes == 0)
                {
                    var clientes = new List<Usuario>
                    {
                        new Usuario
                        {
                            Nombre = "María",
                            Apellidos = "González López",
                            Cedula = "555666777",
                            Email = "cliente@test.cr",
                            Telefono = "8888-1234",
                            Direccion = "Cartago, Costa Rica",
                            TipoUsuario = "cliente",
                            Password = "cliente123",
                            Estado = "activo",
                            FechaRegistro = DateTime.Now
                        }
                    };

                    await _context.Usuarios.AddRangeAsync(clientes);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(" {Count} clientes creados", clientes.Count);
                }

                var usuariosFinales = await _context.Usuarios.CountAsync();
                var tecnicosFinales = await _context.Usuarios.CountAsync(u => u.TipoUsuario == "tecnico");
                var clientesFinales = await _context.Usuarios.CountAsync(u => u.TipoUsuario == "cliente");

                return Ok(new
                {
                    success = true,
                    message = "Datos de prueba verificados/creados",
                    usuarios = new
                    {
                        total = usuariosFinales,
                        tecnicos = tecnicosFinales,
                        clientes = clientesFinales
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error al crear datos de prueba");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("migration-status")]
        public async Task<ActionResult<object>> GetMigrationStatus()
        {
            try
            {
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();

                return Ok(new
                {
                    timestamp = DateTime.Now,
                    database = _context.Database.GetDbConnection().Database,
                    migrations = new
                    {
                        applied = appliedMigrations.ToList(),
                        pending = pendingMigrations.ToList(),
                        hasPending = pendingMigrations.Any()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error al verificar migraciones");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class TestLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}