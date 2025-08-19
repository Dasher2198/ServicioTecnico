using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioTecnico.Data;
using ServicioTecnico.Models;
using ServicioTecnico.DTOs;
using System.ComponentModel.DataAnnotations;

namespace ServicioTecnico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(AppDbContext context, ILogger<UsuarioController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> GetUsuarios()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Where(u => u.Estado == "activo")
                    .Select(u => new UsuarioResponseDto
                    {
                        IdUsuario = u.IdUsuario,
                        Nombre = u.Nombre,
                        Apellidos = u.Apellidos,
                        Cedula = u.Cedula,
                        Email = u.Email,
                        Telefono = u.Telefono,
                        Direccion = u.Direccion,
                        TipoUsuario = u.TipoUsuario,
                        Estado = u.Estado,
                        FechaRegistro = u.FechaRegistro
                    })
                    .ToListAsync();

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioResponseDto>> GetUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);

                if (usuario == null || usuario.Estado == "inactivo")
                    return NotFound("Usuario no encontrado");

                var usuarioDto = new UsuarioResponseDto
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = usuario.Nombre,
                    Apellidos = usuario.Apellidos,
                    Cedula = usuario.Cedula,
                    Email = usuario.Email,
                    Telefono = usuario.Telefono,
                    Direccion = usuario.Direccion,
                    TipoUsuario = usuario.TipoUsuario,
                    Estado = usuario.Estado,
                    FechaRegistro = usuario.FechaRegistro
                };

                return Ok(usuarioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario {UserId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        public async Task<ActionResult<UsuarioResponseDto>> PostUsuario(UsuarioCreateDto usuarioDto)
        {
            try
            {
                // Validar datos de entrada
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verificar si ya existe usuario con esa cédula o email
                var existeUsuario = await _context.Usuarios
                    .AnyAsync(u => u.Cedula == usuarioDto.Cedula || u.Email == usuarioDto.Email);

                if (existeUsuario)
                {
                    return Conflict("Ya existe un usuario con esa cédula o email");
                }

                var usuario = new Usuario
                {
                    Nombre = usuarioDto.Nombre.Trim(),
                    Apellidos = usuarioDto.Apellidos.Trim(),
                    Cedula = usuarioDto.Cedula.Trim(),
                    Email = usuarioDto.Email.ToLower().Trim(),
                    Telefono = usuarioDto.Telefono?.Trim(),
                    Direccion = usuarioDto.Direccion?.Trim(),
                    TipoUsuario = usuarioDto.TipoUsuario,
                    Password = usuarioDto.Password, // En producción, hash la contraseña
                    Estado = "activo",
                    FechaRegistro = DateTime.Now
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var responseDto = new UsuarioResponseDto
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = usuario.Nombre,
                    Apellidos = usuario.Apellidos,
                    Cedula = usuario.Cedula,
                    Email = usuario.Email,
                    Telefono = usuario.Telefono,
                    Direccion = usuario.Direccion,
                    TipoUsuario = usuario.TipoUsuario,
                    Estado = usuario.Estado,
                    FechaRegistro = usuario.FechaRegistro
                };

                return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuario }, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, UsuarioUpdateDto usuarioDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                // Verificar si la nueva cédula o email ya existe en otro usuario
                var existeOtroUsuario = await _context.Usuarios
                    .AnyAsync(u => u.IdUsuario != id && (u.Cedula == usuarioDto.Cedula || u.Email == usuarioDto.Email));

                if (existeOtroUsuario)
                {
                    return Conflict("Ya existe otro usuario con esa cédula o email");
                }

                // Actualizar solo los campos permitidos
                usuario.Nombre = usuarioDto.Nombre.Trim();
                usuario.Apellidos = usuarioDto.Apellidos.Trim();
                usuario.Cedula = usuarioDto.Cedula.Trim();
                usuario.Email = usuarioDto.Email.ToLower().Trim();
                usuario.Telefono = usuarioDto.Telefono?.Trim();
                usuario.Direccion = usuarioDto.Direccion?.Trim();

                _context.Entry(usuario).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario {UserId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                // Verificar si el usuario tiene vehículos o inspecciones asociadas
                var tieneRelaciones = await _context.Vehiculos.AnyAsync(v => v.IdPropietario == id) ||
                                    await _context.Inspecciones.AnyAsync(i => i.IdTecnico == id);

                if (tieneRelaciones)
                {
                    // Desactivar en lugar de eliminar
                    usuario.Estado = "inactivo";
                    _context.Entry(usuario).State = EntityState.Modified;
                }
                else
                {
                    _context.Usuarios.Remove(usuario);
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario {UserId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UsuarioResponseDto>> Login(LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email.ToLower() &&
                                            u.Password == loginDto.Password &&
                                            u.Estado == "activo");

                if (usuario == null)
                {
                    return Unauthorized("Credenciales incorrectas");
                }

                var responseDto = new UsuarioResponseDto
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = usuario.Nombre,
                    Apellidos = usuario.Apellidos,
                    Cedula = usuario.Cedula,
                    Email = usuario.Email,
                    Telefono = usuario.Telefono,
                    Direccion = usuario.Direccion,
                    TipoUsuario = usuario.TipoUsuario,
                    Estado = usuario.Estado,
                    FechaRegistro = usuario.FechaRegistro
                };

                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("test-connection")]
        public async Task<ActionResult<object>> TestConnection()
        {
            try
            {
                var usuariosCount = await _context.Usuarios.CountAsync();
                return Ok(new
                {
                    connected = true,
                    usuariosCount = usuariosCount,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en test de conexión");
                return Ok(new
                {
                    connected = false,
                    error = ex.Message
                });
            }
        }
    }
}

// DTOs para el manejo de datos
namespace ServicioTecnico.DTOs
{
    public class UsuarioResponseDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Cedula { get; set; }
        public string Email { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string TipoUsuario { get; set; }
        public string Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class UsuarioCreateDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder 100 caracteres")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "La cédula es requerida")]
        [StringLength(20, ErrorMessage = "La cédula no puede exceder 20 caracteres")]
        public string Cedula { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string Email { get; set; }

        [StringLength(15, ErrorMessage = "El teléfono no puede exceder 15 caracteres")]
        public string? Telefono { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        public string? Direccion { get; set; }

        [Required(ErrorMessage = "El tipo de usuario es requerido")]
        [RegularExpression("^(cliente|tecnico)$", ErrorMessage = "Tipo de usuario inválido")]
        public string TipoUsuario { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 255 caracteres")]
        public string Password { get; set; }
    }

    public class UsuarioUpdateDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder 100 caracteres")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "La cédula es requerida")]
        [StringLength(20, ErrorMessage = "La cédula no puede exceder 20 caracteres")]
        public string Cedula { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string Email { get; set; }

        [StringLength(15, ErrorMessage = "El teléfono no puede exceder 15 caracteres")]
        public string? Telefono { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        public string? Direccion { get; set; }
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; }
    }
}