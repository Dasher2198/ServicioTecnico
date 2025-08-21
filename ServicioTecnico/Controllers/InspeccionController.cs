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
    public class InspeccionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<InspeccionController> _logger;

        public InspeccionController(AppDbContext context, ILogger<InspeccionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InspeccionResponseDto>>> GetInspecciones()
        {
            try
            {
                var inspecciones = await _context.Inspecciones
                    .Include(i => i.Cita)
                        .ThenInclude(c => c.Vehiculo)
                        .ThenInclude(v => v.Propietario)
                    .Include(i => i.Cita)
                        .ThenInclude(c => c.Estacion)
                    .Include(i => i.Tecnico)
                    .ToListAsync();

                var inspeccionesResponse = inspecciones.Select(i => new InspeccionResponseDto
                {
                    IdInspeccion = i.IdInspeccion,
                    IdCita = i.IdCita,
                    IdTecnico = i.IdTecnico,
                    FechaInspeccion = i.FechaInspeccion,
                    Resultado = i.Resultado,
                    ObservacionesTecnicas = i.ObservacionesTecnicas,
                    FechaVencimiento = i.FechaVencimiento,
                    NumeroCertificado = i.NumeroCertificado,
                    VehiculoInfo = i.Cita?.Vehiculo != null ?
                        $"{i.Cita.Vehiculo.NumeroPlaca} - {i.Cita.Vehiculo.Marca} {i.Cita.Vehiculo.Modelo}" : "N/A",
                    TecnicoInfo = i.Tecnico != null ?
                        $"{i.Tecnico.Nombre} {i.Tecnico.Apellidos}" : "N/A",
                    EstacionInfo = i.Cita?.Estacion?.NombreEstacion ?? "N/A",
                    PropietarioInfo = i.Cita?.Vehiculo?.Propietario != null ?
                        $"{i.Cita.Vehiculo.Propietario.Nombre} {i.Cita.Vehiculo.Propietario.Apellidos}" : "N/A"
                }).ToList();

                return Ok(inspeccionesResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inspecciones");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InspeccionResponseDto>> GetInspeccion(int id)
        {
            try
            {
                var inspeccion = await _context.Inspecciones
                    .Include(i => i.Cita)
                        .ThenInclude(c => c.Vehiculo)
                        .ThenInclude(v => v.Propietario)
                    .Include(i => i.Cita)
                        .ThenInclude(c => c.Estacion)
                    .Include(i => i.Tecnico)
                    .FirstOrDefaultAsync(i => i.IdInspeccion == id);

                if (inspeccion == null)
                    return NotFound("Inspección no encontrada");

                var inspeccionDto = new InspeccionResponseDto
                {
                    IdInspeccion = inspeccion.IdInspeccion,
                    IdCita = inspeccion.IdCita,
                    IdTecnico = inspeccion.IdTecnico,
                    FechaInspeccion = inspeccion.FechaInspeccion,
                    Resultado = inspeccion.Resultado,
                    ObservacionesTecnicas = inspeccion.ObservacionesTecnicas,
                    FechaVencimiento = inspeccion.FechaVencimiento,
                    NumeroCertificado = inspeccion.NumeroCertificado,
                    VehiculoInfo = inspeccion.Cita?.Vehiculo != null ?
                        $"{inspeccion.Cita.Vehiculo.NumeroPlaca} - {inspeccion.Cita.Vehiculo.Marca} {inspeccion.Cita.Vehiculo.Modelo}" : "N/A",
                    TecnicoInfo = inspeccion.Tecnico != null ?
                        $"{inspeccion.Tecnico.Nombre} {inspeccion.Tecnico.Apellidos}" : "N/A",
                    EstacionInfo = inspeccion.Cita?.Estacion?.NombreEstacion ?? "N/A",
                    PropietarioInfo = inspeccion.Cita?.Vehiculo?.Propietario != null ?
                        $"{inspeccion.Cita.Vehiculo.Propietario.Nombre} {inspeccion.Cita.Vehiculo.Propietario.Apellidos}" : "N/A"
                };

                return Ok(inspeccionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inspección {InspeccionId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        public async Task<ActionResult<InspeccionResponseDto>> PostInspeccion([FromBody] InspeccionCreateDto inspeccionDto)
        {
            try
            {
                _logger.LogInformation("Intentando crear inspección: {@InspeccionDto}", inspeccionDto);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                // Validar que la cita existe y está programada
                var cita = await _context.Citas
                    .Include(c => c.Vehiculo)
                    .Include(c => c.Estacion)
                    .FirstOrDefaultAsync(c => c.IdCita == inspeccionDto.IdCita);

                if (cita == null)
                {
                    return BadRequest("La cita especificada no existe");
                }

                if (cita.EstadoCita != "programada")
                {
                    return BadRequest("La cita debe estar en estado 'programada' para poder realizar la inspección");
                }

                // Validar que el técnico existe
                var tecnico = await _context.Usuarios.FindAsync(inspeccionDto.IdTecnico);
                if (tecnico == null || tecnico.TipoUsuario != "tecnico")
                {
                    return BadRequest("El técnico especificado no existe o no es válido");
                }

                // Verificar que no exista ya una inspección para esta cita
                var inspeccionExistente = await _context.Inspecciones
                    .AnyAsync(i => i.IdCita == inspeccionDto.IdCita);

                if (inspeccionExistente)
                {
                    return Conflict("Ya existe una inspección registrada para esta cita");
                }

                var inspeccion = new Inspeccion
                {
                    IdCita = inspeccionDto.IdCita,
                    IdTecnico = inspeccionDto.IdTecnico,
                    FechaInspeccion = inspeccionDto.FechaInspeccion,
                    Resultado = inspeccionDto.Resultado,
                    ObservacionesTecnicas = inspeccionDto.ObservacionesTecnicas ?? string.Empty,
                    FechaVencimiento = inspeccionDto.FechaVencimiento,
                    NumeroCertificado = inspeccionDto.NumeroCertificado ?? string.Empty
                };

                _context.Inspecciones.Add(inspeccion);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Inspección creada exitosamente con ID: {InspeccionId}", inspeccion.IdInspeccion);

                // Cargar la inspección completa para la respuesta
                var inspeccionCompleta = await _context.Inspecciones
                    .Include(i => i.Cita)
                        .ThenInclude(c => c.Vehiculo)
                    .Include(i => i.Tecnico)
                    .FirstOrDefaultAsync(i => i.IdInspeccion == inspeccion.IdInspeccion);

                var response = new InspeccionResponseDto
                {
                    IdInspeccion = inspeccionCompleta.IdInspeccion,
                    IdCita = inspeccionCompleta.IdCita,
                    IdTecnico = inspeccionCompleta.IdTecnico,
                    FechaInspeccion = inspeccionCompleta.FechaInspeccion,
                    Resultado = inspeccionCompleta.Resultado,
                    ObservacionesTecnicas = inspeccionCompleta.ObservacionesTecnicas,
                    FechaVencimiento = inspeccionCompleta.FechaVencimiento,
                    NumeroCertificado = inspeccionCompleta.NumeroCertificado,
                    VehiculoInfo = inspeccionCompleta.Cita?.Vehiculo != null ?
                        $"{inspeccionCompleta.Cita.Vehiculo.NumeroPlaca} - {inspeccionCompleta.Cita.Vehiculo.Marca} {inspeccionCompleta.Cita.Vehiculo.Modelo}" : "N/A",
                    TecnicoInfo = inspeccionCompleta.Tecnico != null ?
                        $"{inspeccionCompleta.Tecnico.Nombre} {inspeccionCompleta.Tecnico.Apellidos}" : "N/A"
                };

                return CreatedAtAction(nameof(GetInspeccion), new { id = inspeccion.IdInspeccion }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear inspección");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutInspeccion(int id, [FromBody] InspeccionUpdateDto inspeccionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var inspeccion = await _context.Inspecciones.FindAsync(id);
                if (inspeccion == null)
                {
                    return NotFound("Inspección no encontrada");
                }

                // Actualizar propiedades
                inspeccion.Resultado = inspeccionDto.Resultado;
                inspeccion.ObservacionesTecnicas = inspeccionDto.ObservacionesTecnicas ?? string.Empty;
                inspeccion.FechaVencimiento = inspeccionDto.FechaVencimiento;
                inspeccion.NumeroCertificado = inspeccionDto.NumeroCertificado ?? string.Empty;

                _context.Entry(inspeccion).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar inspección {InspeccionId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInspeccion(int id)
        {
            try
            {
                var inspeccion = await _context.Inspecciones.FindAsync(id);
                if (inspeccion == null)
                {
                    return NotFound("Inspección no encontrada");
                }

                // Verificar si la inspección tiene certificados asociados
                var tieneCertificados = await _context.Certificados.AnyAsync(c => c.IdInspeccion == id);

                if (tieneCertificados)
                {
                    return BadRequest("No se puede eliminar la inspección porque tiene certificados asociados");
                }

                _context.Inspecciones.Remove(inspeccion);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar inspección {InspeccionId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("porTecnico/{idTecnico}")]
        public async Task<ActionResult<IEnumerable<InspeccionResponseDto>>> GetInspeccionesPorTecnico(int idTecnico)
        {
            try
            {
                var inspecciones = await _context.Inspecciones
                    .Where(i => i.IdTecnico == idTecnico)
                    .Include(i => i.Cita)
                        .ThenInclude(c => c.Vehiculo)
                    .Include(i => i.Cita)
                        .ThenInclude(c => c.Estacion)
                    .Include(i => i.Tecnico)
                    .OrderByDescending(i => i.FechaInspeccion)
                    .ToListAsync();

                var inspeccionesResponse = inspecciones.Select(i => new InspeccionResponseDto
                {
                    IdInspeccion = i.IdInspeccion,
                    IdCita = i.IdCita,
                    IdTecnico = i.IdTecnico,
                    FechaInspeccion = i.FechaInspeccion,
                    Resultado = i.Resultado,
                    ObservacionesTecnicas = i.ObservacionesTecnicas,
                    FechaVencimiento = i.FechaVencimiento,
                    NumeroCertificado = i.NumeroCertificado,
                    VehiculoInfo = i.Cita?.Vehiculo != null ?
                        $"{i.Cita.Vehiculo.NumeroPlaca} - {i.Cita.Vehiculo.Marca} {i.Cita.Vehiculo.Modelo}" : "N/A",
                    TecnicoInfo = i.Tecnico != null ?
                        $"{i.Tecnico.Nombre} {i.Tecnico.Apellidos}" : "N/A",
                    EstacionInfo = i.Cita?.Estacion?.NombreEstacion ?? "N/A"
                }).ToList();

                return Ok(inspeccionesResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inspecciones para técnico {TecnicoId}", idTecnico);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("porVehiculo/{idVehiculo}")]
        public async Task<ActionResult<IEnumerable<InspeccionResponseDto>>> GetInspeccionesPorVehiculo(int idVehiculo)
        {
            try
            {
                var inspecciones = await _context.Inspecciones
                    .Include(i => i.Cita)
                        .ThenInclude(c => c.Vehiculo)
                    .Include(i => i.Cita)
                        .ThenInclude(c => c.Estacion)
                    .Include(i => i.Tecnico)
                    .Where(i => i.Cita.IdVehiculo == idVehiculo)
                    .OrderByDescending(i => i.FechaInspeccion)
                    .ToListAsync();

                var inspeccionesResponse = inspecciones.Select(i => new InspeccionResponseDto
                {
                    IdInspeccion = i.IdInspeccion,
                    IdCita = i.IdCita,
                    IdTecnico = i.IdTecnico,
                    FechaInspeccion = i.FechaInspeccion,
                    Resultado = i.Resultado,
                    ObservacionesTecnicas = i.ObservacionesTecnicas,
                    FechaVencimiento = i.FechaVencimiento,
                    NumeroCertificado = i.NumeroCertificado,
                    VehiculoInfo = i.Cita?.Vehiculo != null ?
                        $"{i.Cita.Vehiculo.NumeroPlaca} - {i.Cita.Vehiculo.Marca} {i.Cita.Vehiculo.Modelo}" : "N/A",
                    TecnicoInfo = i.Tecnico != null ?
                        $"{i.Tecnico.Nombre} {i.Tecnico.Apellidos}" : "N/A",
                    EstacionInfo = i.Cita?.Estacion?.NombreEstacion ?? "N/A"
                }).ToList();

                return Ok(inspeccionesResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inspecciones para vehículo {VehiculoId}", idVehiculo);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }

    // DTOs para Inspección
    public class InspeccionCreateDto
    {
        [Required(ErrorMessage = "La cita es requerida")]
        public int IdCita { get; set; }

        [Required(ErrorMessage = "El técnico es requerido")]
        public int IdTecnico { get; set; }

        [Required(ErrorMessage = "La fecha de inspección es requerida")]
        public DateTime FechaInspeccion { get; set; }

        [Required(ErrorMessage = "El resultado es requerido")]
        [RegularExpression("^(aprobado|rechazado)$", ErrorMessage = "Resultado debe ser 'aprobado' o 'rechazado'")]
        public string Resultado { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres")]
        public string? ObservacionesTecnicas { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        [StringLength(50, ErrorMessage = "El número de certificado no puede exceder 50 caracteres")]
        public string? NumeroCertificado { get; set; }
    }

    public class InspeccionUpdateDto
    {
        [Required(ErrorMessage = "El resultado es requerido")]
        [RegularExpression("^(aprobado|rechazado)$", ErrorMessage = "Resultado debe ser 'aprobado' o 'rechazado'")]
        public string Resultado { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres")]
        public string? ObservacionesTecnicas { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        [StringLength(50, ErrorMessage = "El número de certificado no puede exceder 50 caracteres")]
        public string? NumeroCertificado { get; set; }
    }

    public class InspeccionResponseDto
    {
        public int IdInspeccion { get; set; }
        public int IdCita { get; set; }
        public int IdTecnico { get; set; }
        public DateTime FechaInspeccion { get; set; }
        public string Resultado { get; set; } = string.Empty;
        public string? ObservacionesTecnicas { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string? NumeroCertificado { get; set; }

        // Información adicional
        public string? VehiculoInfo { get; set; }
        public string? TecnicoInfo { get; set; }
        public string? EstacionInfo { get; set; }
        public string? PropietarioInfo { get; set; }
    }
}