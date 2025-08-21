using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioTecnico.Data;
using ServicioTecnico.Models;
using System.ComponentModel.DataAnnotations;

namespace ServicioTecnico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiculoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VehiculoController> _logger;

        public VehiculoController(AppDbContext context, ILogger<VehiculoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetVehiculos()
        {
            try
            {
                var vehiculos = await _context.Vehiculos
                    .Include(v => v.Propietario)
                    .Select(v => new
                    {
                        v.IdVehiculo,
                        v.NumeroPlaca,
                        v.IdPropietario,
                        v.Marca,
                        v.Modelo,
                        v.Año,
                        v.NumeroChasis,
                        v.Color,
                        v.TipoCombustible,
                        v.Cilindrada,
                        v.FechaRegistro,
                        PropietarioNombre = v.Propietario != null ? $"{v.Propietario.Nombre} {v.Propietario.Apellidos}" : "N/A"
                    })
                    .ToListAsync();

                return Ok(vehiculos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetVehiculo(int id)
        {
            try
            {
                var vehiculo = await _context.Vehiculos
                    .Include(v => v.Propietario)
                    .Where(v => v.IdVehiculo == id)
                    .Select(v => new
                    {
                        v.IdVehiculo,
                        v.NumeroPlaca,
                        v.IdPropietario,
                        v.Marca,
                        v.Modelo,
                        v.Año,
                        v.NumeroChasis,
                        v.Color,
                        v.TipoCombustible,
                        v.Cilindrada,
                        v.FechaRegistro,
                        PropietarioNombre = v.Propietario != null ? $"{v.Propietario.Nombre} {v.Propietario.Apellidos}" : "N/A"
                    })
                    .FirstOrDefaultAsync();

                if (vehiculo == null)
                    return NotFound("Vehículo no encontrado");

                return Ok(vehiculo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículo {VehiculoId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("porPropietario/{idPropietario}")]
        public async Task<ActionResult<IEnumerable<object>>> GetVehiculosPorPropietario(int idPropietario)
        {
            try
            {
                var vehiculos = await _context.Vehiculos
                    .Where(v => v.IdPropietario == idPropietario)
                    .Include(v => v.Propietario)
                    .Select(v => new
                    {
                        v.IdVehiculo,
                        v.NumeroPlaca,
                        v.IdPropietario,
                        v.Marca,
                        v.Modelo,
                        v.Año,
                        v.NumeroChasis,
                        v.Color,
                        v.TipoCombustible,
                        v.Cilindrada,
                        v.FechaRegistro
                    })
                    .ToListAsync();

                return Ok(vehiculos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vehículos del propietario {PropietarioId}", idPropietario);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        public async Task<ActionResult<object>> PostVehiculo([FromBody] VehiculoCreateDto vehiculoDto)
        {
            try
            {
                _logger.LogInformation("Intentando registrar vehículo: {@VehiculoDto}", vehiculoDto);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido para vehículo: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                // Verificar que el propietario existe
                var propietario = await _context.Usuarios.FindAsync(vehiculoDto.IdPropietario);
                if (propietario == null)
                {
                    return BadRequest("El propietario especificado no existe");
                }

                // Verificar que no existe otro vehículo con la misma placa
                var existePlaca = await _context.Vehiculos
                    .AnyAsync(v => v.NumeroPlaca.ToUpper() == vehiculoDto.NumeroPlaca.ToUpper());

                if (existePlaca)
                {
                    return Conflict("Ya existe un vehículo registrado con esa placa");
                }

                var vehiculo = new Vehiculo
                {
                    NumeroPlaca = vehiculoDto.NumeroPlaca.ToUpper().Trim(),
                    IdPropietario = vehiculoDto.IdPropietario,
                    Marca = vehiculoDto.Marca.Trim(),
                    Modelo = vehiculoDto.Modelo.Trim(),
                    Año = vehiculoDto.Año,
                    NumeroChasis = !string.IsNullOrWhiteSpace(vehiculoDto.NumeroChasis) ?
                                  vehiculoDto.NumeroChasis.ToUpper().Trim() : null,
                    Color = !string.IsNullOrWhiteSpace(vehiculoDto.Color) ?
                           vehiculoDto.Color.Trim() : null,
                    TipoCombustible = !string.IsNullOrWhiteSpace(vehiculoDto.TipoCombustible) ?
                                     vehiculoDto.TipoCombustible.Trim() : null,
                    Cilindrada = !string.IsNullOrWhiteSpace(vehiculoDto.Cilindrada) ?
                                vehiculoDto.Cilindrada.Trim() : null,
                    FechaRegistro = DateTime.Now
                };

                _context.Vehiculos.Add(vehiculo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Vehículo registrado exitosamente con ID: {VehiculoId}", vehiculo.IdVehiculo);

                var vehiculoResponse = new
                {
                    vehiculo.IdVehiculo,
                    vehiculo.NumeroPlaca,
                    vehiculo.IdPropietario,
                    vehiculo.Marca,
                    vehiculo.Modelo,
                    vehiculo.Año,
                    vehiculo.NumeroChasis,
                    vehiculo.Color,
                    vehiculo.TipoCombustible,
                    vehiculo.Cilindrada,
                    vehiculo.FechaRegistro
                };

                return CreatedAtAction(nameof(GetVehiculo), new { id = vehiculo.IdVehiculo }, vehiculoResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar vehículo");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutVehiculo(int id, [FromBody] VehiculoUpdateDto vehiculoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var vehiculo = await _context.Vehiculos.FindAsync(id);
                if (vehiculo == null)
                {
                    return NotFound("Vehículo no encontrado");
                }

                // Verificar que no existe otro vehículo con la misma placa (excluyendo el actual)
                var existePlaca = await _context.Vehiculos
                    .AnyAsync(v => v.IdVehiculo != id && v.NumeroPlaca.ToUpper() == vehiculoDto.NumeroPlaca.ToUpper());

                if (existePlaca)
                {
                    return Conflict("Ya existe otro vehículo registrado con esa placa");
                }

                // Actualizar propiedades
                vehiculo.NumeroPlaca = vehiculoDto.NumeroPlaca.ToUpper().Trim();
                vehiculo.Marca = vehiculoDto.Marca.Trim();
                vehiculo.Modelo = vehiculoDto.Modelo.Trim();
                vehiculo.Año = vehiculoDto.Año;
                vehiculo.NumeroChasis = !string.IsNullOrWhiteSpace(vehiculoDto.NumeroChasis) ?
                                      vehiculoDto.NumeroChasis.ToUpper().Trim() : null;
                vehiculo.Color = !string.IsNullOrWhiteSpace(vehiculoDto.Color) ?
                               vehiculoDto.Color.Trim() : null;
                vehiculo.TipoCombustible = !string.IsNullOrWhiteSpace(vehiculoDto.TipoCombustible) ?
                                         vehiculoDto.TipoCombustible.Trim() : null;
                vehiculo.Cilindrada = !string.IsNullOrWhiteSpace(vehiculoDto.Cilindrada) ?
                                    vehiculoDto.Cilindrada.Trim() : null;

                _context.Entry(vehiculo).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar vehículo {VehiculoId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehiculo(int id)
        {
            try
            {
                var vehiculo = await _context.Vehiculos.FindAsync(id);
                if (vehiculo == null)
                {
                    return NotFound("Vehículo no encontrado");
                }

                // Verificar si el vehículo tiene citas asociadas
                var tieneCitas = await _context.Citas.AnyAsync(c => c.IdVehiculo == id);

                if (tieneCitas)
                {
                    return BadRequest("No se puede eliminar el vehículo porque tiene citas asociadas");
                }

                _context.Vehiculos.Remove(vehiculo);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar vehículo {VehiculoId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }

    // DTOs para Vehículo
    public class VehiculoCreateDto
    {
        [Required(ErrorMessage = "La placa es requerida")]
        [StringLength(10, ErrorMessage = "La placa no puede exceder 10 caracteres")]
        public string NumeroPlaca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El propietario es requerido")]
        public int IdPropietario { get; set; }

        [Required(ErrorMessage = "La marca es requerida")]
        [StringLength(50, ErrorMessage = "La marca no puede exceder 50 caracteres")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es requerido")]
        [StringLength(50, ErrorMessage = "El modelo no puede exceder 50 caracteres")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El año es requerido")]
        [Range(1900, 2030, ErrorMessage = "El año debe estar entre 1900 y 2030")]
        public int Año { get; set; }

        [StringLength(50, ErrorMessage = "El número de chasis no puede exceder 50 caracteres")]
        public string? NumeroChasis { get; set; }

        [StringLength(30, ErrorMessage = "El color no puede exceder 30 caracteres")]
        public string? Color { get; set; }

        [StringLength(20, ErrorMessage = "El tipo de combustible no puede exceder 20 caracteres")]
        public string? TipoCombustible { get; set; }

        [StringLength(10, ErrorMessage = "La cilindrada no puede exceder 10 caracteres")]
        public string? Cilindrada { get; set; }
    }

    public class VehiculoUpdateDto
    {
        [Required(ErrorMessage = "La placa es requerida")]
        [StringLength(10, ErrorMessage = "La placa no puede exceder 10 caracteres")]
        public string NumeroPlaca { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca es requerida")]
        [StringLength(50, ErrorMessage = "La marca no puede exceder 50 caracteres")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es requerido")]
        [StringLength(50, ErrorMessage = "El modelo no puede exceder 50 caracteres")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El año es requerido")]
        [Range(1900, 2030, ErrorMessage = "El año debe estar entre 1900 y 2030")]
        public int Año { get; set; }

        [StringLength(50, ErrorMessage = "El número de chasis no puede exceder 50 caracteres")]
        public string? NumeroChasis { get; set; }

        [StringLength(30, ErrorMessage = "El color no puede exceder 30 caracteres")]
        public string? Color { get; set; }

        [StringLength(20, ErrorMessage = "El tipo de combustible no puede exceder 20 caracteres")]
        public string? TipoCombustible { get; set; }

        [StringLength(10, ErrorMessage = "La cilindrada no puede exceder 10 caracteres")]
        public string? Cilindrada { get; set; }
    }
}