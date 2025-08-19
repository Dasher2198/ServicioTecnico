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
    public class CitaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CitaController> _logger;

        public CitaController(AppDbContext context, ILogger<CitaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CitaResponseDto>>> GetCitas()
        {
            try
            {
                var citas = await _context.Citas
                    .Include(c => c.Vehiculo)
                    .Include(c => c.Estacion)
                    .ToListAsync();

                var citasResponse = citas.Select(c => new CitaResponseDto
                {
                    IdCita = c.IdCita,
                    IdVehiculo = c.IdVehiculo,
                    IdEstacion = c.IdEstacion,
                    FechaCita = c.FechaCita,
                    HoraCita = c.HoraCita,
                    EstadoCita = c.EstadoCita ?? "programada",
                    Observaciones = c.Observaciones,
                    FechaCreacion = c.FechaCreacion,
                    VehiculoInfo = c.Vehiculo != null ? $"{c.Vehiculo.NumeroPlaca} - {c.Vehiculo.Marca} {c.Vehiculo.Modelo}" : "N/A",
                    EstacionInfo = c.Estacion?.NombreEstacion ?? "N/A"
                }).ToList();

                return Ok(citasResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CitaResponseDto>> GetCita(int id)
        {
            try
            {
                var cita = await _context.Citas
                    .Include(c => c.Vehiculo)
                    .Include(c => c.Estacion)
                    .FirstOrDefaultAsync(c => c.IdCita == id);

                if (cita == null)
                    return NotFound("Cita no encontrada");

                var citaDto = new CitaResponseDto
                {
                    IdCita = cita.IdCita,
                    IdVehiculo = cita.IdVehiculo,
                    IdEstacion = cita.IdEstacion,
                    FechaCita = cita.FechaCita,
                    HoraCita = cita.HoraCita,
                    EstadoCita = cita.EstadoCita ?? "programada",
                    Observaciones = cita.Observaciones,
                    FechaCreacion = cita.FechaCreacion,
                    VehiculoInfo = cita.Vehiculo != null ? $"{cita.Vehiculo.NumeroPlaca} - {cita.Vehiculo.Marca} {cita.Vehiculo.Modelo}" : "N/A",
                    EstacionInfo = cita.Estacion?.NombreEstacion ?? "N/A"
                };

                return Ok(citaDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cita {CitaId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostCita([FromForm] CitaCreateDto citaDto)
        {
            try
            {
                _logger.LogInformation("Intentando crear cita: {@CitaDto}", citaDto);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                // Validar que el vehículo existe
                var vehiculo = await _context.Vehiculos.FindAsync(citaDto.IdVehiculo);
                if (vehiculo == null)
                {
                    return BadRequest("El vehículo especificado no existe");
                }

                // Validar que la estación existe
                var estacion = await _context.Estaciones.FindAsync(citaDto.IdEstacion);
                if (estacion == null)
                {
                    return BadRequest("La estación especificada no existe");
                }

                // Convertir la hora de string a TimeSpan
                if (!TimeSpan.TryParse(citaDto.HoraCita, out TimeSpan horaCita))
                {
                    return BadRequest("Formato de hora inválido. Use HH:mm:ss");
                }

                // Validar que no hay conflicto de horarios
                var conflicto = await _context.Citas
                    .AnyAsync(c => c.IdEstacion == citaDto.IdEstacion &&
                                   c.FechaCita.Date == citaDto.FechaCita.Date &&
                                   c.HoraCita == horaCita &&
                                   c.EstadoCita == "programada");

                if (conflicto)
                {
                    return Conflict("Ya existe una cita programada para esa fecha y hora en la estación seleccionada");
                }

                var cita = new Cita
                {
                    IdVehiculo = citaDto.IdVehiculo,
                    IdEstacion = citaDto.IdEstacion,
                    FechaCita = citaDto.FechaCita,
                    HoraCita = horaCita,
                    EstadoCita = citaDto.EstadoCita ?? "programada",
                    Observaciones = citaDto.Observaciones,
                    FechaCreacion = DateTime.Now
                };

                _context.Citas.Add(cita);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cita creada exitosamente con ID: {CitaId}", cita.IdCita);

                // Redireccionar a página de confirmación con parámetros
                var fechaFormateada = cita.FechaCita.ToString("yyyy-MM-dd");
                var horaFormateada = cita.HoraCita.ToString(@"hh\:mm");
                var estacionNombre = Uri.EscapeDataString(estacion.NombreEstacion);
                var vehiculoId = cita.IdVehiculo;

                return Redirect($"/CitaConfirmacion.html?fecha={fechaFormateada}&hora={horaFormateada}&estacion={estacionNombre}&vehiculo={vehiculoId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cita");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCita(int id, CitaUpdateDto citaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var cita = await _context.Citas.FindAsync(id);
                if (cita == null)
                {
                    return NotFound("Cita no encontrada");
                }

                // Convertir la hora de string a TimeSpan
                if (!TimeSpan.TryParse(citaDto.HoraCita, out TimeSpan horaCita))
                {
                    return BadRequest("Formato de hora inválido. Use HH:mm:ss");
                }

                // Validar que no hay conflicto de horarios (excluyendo la cita actual)
                var conflicto = await _context.Citas
                    .AnyAsync(c => c.IdCita != id &&
                                   c.IdEstacion == cita.IdEstacion &&
                                   c.FechaCita.Date == citaDto.FechaCita.Date &&
                                   c.HoraCita == horaCita &&
                                   c.EstadoCita == "programada");

                if (conflicto)
                {
                    return Conflict("Ya existe una cita programada para esa fecha y hora en la estación");
                }

                cita.FechaCita = citaDto.FechaCita;
                cita.HoraCita = horaCita;
                cita.EstadoCita = citaDto.EstadoCita ?? "programada";
                cita.Observaciones = citaDto.Observaciones;

                _context.Entry(cita).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cita {CitaId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCita(int id)
        {
            try
            {
                var cita = await _context.Citas.FindAsync(id);
                if (cita == null)
                {
                    return NotFound("Cita no encontrada");
                }

                // Verificar si la cita tiene inspecciones asociadas
                var tieneInspecciones = await _context.Inspecciones.AnyAsync(i => i.IdCita == id);

                if (tieneInspecciones)
                {
                    // Cambiar estado a cancelada en lugar de eliminar
                    cita.EstadoCita = "cancelada";
                    _context.Entry(cita).State = EntityState.Modified;
                }
                else
                {
                    _context.Citas.Remove(cita);
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cita {CitaId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("porVehiculo/{idVehiculo}")]
        public async Task<ActionResult<IEnumerable<CitaResponseDto>>> GetCitasPorVehiculo(int idVehiculo)
        {
            try
            {
                var citas = await _context.Citas
                    .Where(c => c.IdVehiculo == idVehiculo)
                    .Include(c => c.Vehiculo)
                    .Include(c => c.Estacion)
                    .OrderByDescending(c => c.FechaCita)
                    .ToListAsync();

                var citasResponse = citas.Select(c => new CitaResponseDto
                {
                    IdCita = c.IdCita,
                    IdVehiculo = c.IdVehiculo,
                    IdEstacion = c.IdEstacion,
                    FechaCita = c.FechaCita,
                    HoraCita = c.HoraCita,
                    EstadoCita = c.EstadoCita ?? "programada",
                    Observaciones = c.Observaciones,
                    FechaCreacion = c.FechaCreacion,
                    VehiculoInfo = c.Vehiculo != null ? $"{c.Vehiculo.NumeroPlaca} - {c.Vehiculo.Marca} {c.Vehiculo.Modelo}" : "N/A",
                    EstacionInfo = c.Estacion?.NombreEstacion ?? "N/A"
                }).ToList();

                return Ok(citasResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas para vehículo {VehiculoId}", idVehiculo);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("porEstacion/{idEstacion}")]
        public async Task<ActionResult<IEnumerable<CitaResponseDto>>> GetCitasPorEstacion(int idEstacion, [FromQuery] DateTime? fecha = null)
        {
            try
            {
                IQueryable<Cita> query = _context.Citas
                    .Where(c => c.IdEstacion == idEstacion);

                if (fecha.HasValue)
                {
                    query = query.Where(c => c.FechaCita.Date == fecha.Value.Date);
                }

                var citas = await query
                    .Include(c => c.Vehiculo)
                    .Include(c => c.Estacion)
                    .OrderBy(c => c.FechaCita)
                    .ThenBy(c => c.HoraCita)
                    .ToListAsync();

                var citasResponse = citas.Select(c => new CitaResponseDto
                {
                    IdCita = c.IdCita,
                    IdVehiculo = c.IdVehiculo,
                    IdEstacion = c.IdEstacion,
                    FechaCita = c.FechaCita,
                    HoraCita = c.HoraCita,
                    EstadoCita = c.EstadoCita ?? "programada",
                    Observaciones = c.Observaciones,
                    FechaCreacion = c.FechaCreacion,
                    VehiculoInfo = c.Vehiculo != null ? $"{c.Vehiculo.NumeroPlaca} - {c.Vehiculo.Marca} {c.Vehiculo.Modelo}" : "N/A",
                    EstacionInfo = c.Estacion?.NombreEstacion ?? "N/A"
                }).ToList();

                return Ok(citasResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas para estación {EstacionId}", idEstacion);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}