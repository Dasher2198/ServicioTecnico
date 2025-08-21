using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioTecnico.Data;
using ServicioTecnico.Models;
using System.ComponentModel.DataAnnotations;

namespace ServicioTecnico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificadoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CertificadoController> _logger;

        public CertificadoController(AppDbContext context, ILogger<CertificadoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CertificadoResponseDto>>> GetCertificados()
        {
            try
            {
                var certificados = await _context.Certificados
                    .Include(c => c.Inspeccion)
                        .ThenInclude(i => i.Cita)
                        .ThenInclude(cita => cita.Vehiculo)
                        .ThenInclude(v => v.Propietario)
                    .Include(c => c.Inspeccion)
                        .ThenInclude(i => i.Tecnico)
                    .ToListAsync();

                var certificadosResponse = certificados.Select(c => new CertificadoResponseDto
                {
                    IdCertificado = c.IdCertificado,
                    IdInspeccion = c.IdInspeccion,
                    NumeroCertificado = c.NumeroCertificado,
                    FechaEmision = c.FechaEmision,
                    FechaVencimiento = c.FechaVencimiento,
                    RutaArchivoDigital = c.RutaArchivoDigital,
                    EstadoCertificado = c.EstadoCertificado,
                    VehiculoInfo = c.Inspeccion?.Cita?.Vehiculo != null ?
                        $"{c.Inspeccion.Cita.Vehiculo.NumeroPlaca} - {c.Inspeccion.Cita.Vehiculo.Marca} {c.Inspeccion.Cita.Vehiculo.Modelo}" : "N/A",
                    PropietarioInfo = c.Inspeccion?.Cita?.Vehiculo?.Propietario != null ?
                        $"{c.Inspeccion.Cita.Vehiculo.Propietario.Nombre} {c.Inspeccion.Cita.Vehiculo.Propietario.Apellidos}" : "N/A",
                    TecnicoInfo = c.Inspeccion?.Tecnico != null ?
                        $"{c.Inspeccion.Tecnico.Nombre} {c.Inspeccion.Tecnico.Apellidos}" : "N/A",
                    EstaVigente = c.FechaVencimiento > DateTime.Now && c.EstadoCertificado == "valido"
                }).ToList();

                return Ok(certificadosResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener certificados");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CertificadoResponseDto>> GetCertificado(int id)
        {
            try
            {
                var certificado = await _context.Certificados
                    .Include(c => c.Inspeccion)
                        .ThenInclude(i => i.Cita)
                        .ThenInclude(cita => cita.Vehiculo)
                        .ThenInclude(v => v.Propietario)
                    .Include(c => c.Inspeccion)
                        .ThenInclude(i => i.Tecnico)
                    .FirstOrDefaultAsync(c => c.IdCertificado == id);

                if (certificado == null)
                    return NotFound("Certificado no encontrado");

                var certificadoDto = new CertificadoResponseDto
                {
                    IdCertificado = certificado.IdCertificado,
                    IdInspeccion = certificado.IdInspeccion,
                    NumeroCertificado = certificado.NumeroCertificado,
                    FechaEmision = certificado.FechaEmision,
                    FechaVencimiento = certificado.FechaVencimiento,
                    RutaArchivoDigital = certificado.RutaArchivoDigital,
                    EstadoCertificado = certificado.EstadoCertificado,
                    VehiculoInfo = certificado.Inspeccion?.Cita?.Vehiculo != null ?
                        $"{certificado.Inspeccion.Cita.Vehiculo.NumeroPlaca} - {certificado.Inspeccion.Cita.Vehiculo.Marca} {certificado.Inspeccion.Cita.Vehiculo.Modelo}" : "N/A",
                    PropietarioInfo = certificado.Inspeccion?.Cita?.Vehiculo?.Propietario != null ?
                        $"{certificado.Inspeccion.Cita.Vehiculo.Propietario.Nombre} {certificado.Inspeccion.Cita.Vehiculo.Propietario.Apellidos}" : "N/A",
                    TecnicoInfo = certificado.Inspeccion?.Tecnico != null ?
                        $"{certificado.Inspeccion.Tecnico.Nombre} {certificado.Inspeccion.Tecnico.Apellidos}" : "N/A",
                    EstaVigente = certificado.FechaVencimiento > DateTime.Now && certificado.EstadoCertificado == "valido"
                };

                return Ok(certificadoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener certificado {CertificadoId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CertificadoResponseDto>> PostCertificado([FromBody] CertificadoCreateDto certificadoDto)
        {
            try
            {
                _logger.LogInformation("Intentando crear certificado: {@CertificadoDto}", certificadoDto);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                // Validar que la inspección existe y está aprobada
                var inspeccion = await _context.Inspecciones
                    .Include(i => i.Cita)
                        .ThenInclude(c => c.Vehiculo)
                    .FirstOrDefaultAsync(i => i.IdInspeccion == certificadoDto.IdInspeccion);

                if (inspeccion == null)
                {
                    return BadRequest("La inspección especificada no existe");
                }

                if (inspeccion.Resultado != "aprobado")
                {
                    return BadRequest("Solo se pueden generar certificados para inspecciones aprobadas");
                }

                // Verificar que no exista ya un certificado para esta inspección
                var certificadoExistente = await _context.Certificados
                    .AnyAsync(c => c.IdInspeccion == certificadoDto.IdInspeccion);

                if (certificadoExistente)
                {
                    return Conflict("Ya existe un certificado para esta inspección");
                }

                // Validar que el número de certificado no exista
                var numeroExistente = await _context.Certificados
                    .AnyAsync(c => c.NumeroCertificado == certificadoDto.NumeroCertificado);

                if (numeroExistente)
                {
                    return Conflict("Ya existe un certificado con ese número");
                }

                var certificado = new Certificado
                {
                    IdInspeccion = certificadoDto.IdInspeccion,
                    NumeroCertificado = certificadoDto.NumeroCertificado,
                    FechaEmision = certificadoDto.FechaEmision ?? DateTime.Now,
                    FechaVencimiento = certificadoDto.FechaVencimiento,
                    RutaArchivoDigital = certificadoDto.RutaArchivoDigital ?? string.Empty,
                    EstadoCertificado = certificadoDto.EstadoCertificado ?? "valido"
                };

                _context.Certificados.Add(certificado);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Certificado creado exitosamente con ID: {CertificadoId}", certificado.IdCertificado);

                // Cargar el certificado completo para la respuesta
                var certificadoCompleto = await _context.Certificados
                    .Include(c => c.Inspeccion)
                        .ThenInclude(i => i.Cita)
                        .ThenInclude(cita => cita.Vehiculo)
                        .ThenInclude(v => v.Propietario)
                    .Include(c => c.Inspeccion)
                        .ThenInclude(i => i.Tecnico)
                    .FirstOrDefaultAsync(c => c.IdCertificado == certificado.IdCertificado);

                var response = new CertificadoResponseDto
                {
                    IdCertificado = certificadoCompleto.IdCertificado,
                    IdInspeccion = certificadoCompleto.IdInspeccion,
                    NumeroCertificado = certificadoCompleto.NumeroCertificado,
                    FechaEmision = certificadoCompleto.FechaEmision,
                    FechaVencimiento = certificadoCompleto.FechaVencimiento,
                    RutaArchivoDigital = certificadoCompleto.RutaArchivoDigital,
                    EstadoCertificado = certificadoCompleto.EstadoCertificado,
                    VehiculoInfo = certificadoCompleto.Inspeccion?.Cita?.Vehiculo != null ?
                        $"{certificadoCompleto.Inspeccion.Cita.Vehiculo.NumeroPlaca} - {certificadoCompleto.Inspeccion.Cita.Vehiculo.Marca} {certificadoCompleto.Inspeccion.Cita.Vehiculo.Modelo}" : "N/A",
                    PropietarioInfo = certificadoCompleto.Inspeccion?.Cita?.Vehiculo?.Propietario != null ?
                        $"{certificadoCompleto.Inspeccion.Cita.Vehiculo.Propietario.Nombre} {certificadoCompleto.Inspeccion.Cita.Vehiculo.Propietario.Apellidos}" : "N/A",
                    TecnicoInfo = certificadoCompleto.Inspeccion?.Tecnico != null ?
                        $"{certificadoCompleto.Inspeccion.Tecnico.Nombre} {certificadoCompleto.Inspeccion.Tecnico.Apellidos}" : "N/A",
                    EstaVigente = certificadoCompleto.FechaVencimiento > DateTime.Now && certificadoCompleto.EstadoCertificado == "valido"
                };

                return CreatedAtAction(nameof(GetCertificado), new { id = certificado.IdCertificado }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear certificado");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCertificado(int id, [FromBody] CertificadoUpdateDto certificadoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var certificado = await _context.Certificados.FindAsync(id);
                if (certificado == null)
                {
                    return NotFound("Certificado no encontrado");
                }

                // Actualizar propiedades
                certificado.FechaVencimiento = certificadoDto.FechaVencimiento;
                certificado.RutaArchivoDigital = certificadoDto.RutaArchivoDigital ?? certificado.RutaArchivoDigital;
                certificado.EstadoCertificado = certificadoDto.EstadoCertificado ?? certificado.EstadoCertificado;

                _context.Entry(certificado).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar certificado {CertificadoId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCertificado(int id)
        {
            try
            {
                var certificado = await _context.Certificados.FindAsync(id);
                if (certificado == null)
                {
                    return NotFound("Certificado no encontrado");
                }

                // En lugar de eliminar, marcar como anulado
                certificado.EstadoCertificado = "anulado";
                _context.Entry(certificado).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al anular certificado {CertificadoId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("porVehiculo/{idVehiculo}")]
        public async Task<ActionResult<IEnumerable<CertificadoResponseDto>>> GetCertificadosPorVehiculo(int idVehiculo)
        {
            try
            {
                var certificados = await _context.Certificados
                    .Include(c => c.Inspeccion)
                        .ThenInclude(i => i.Cita)
                        .ThenInclude(cita => cita.Vehiculo)
                    .Include(c => c.Inspeccion)
                        .ThenInclude(i => i.Tecnico)
                    .Where(c => c.Inspeccion.Cita.IdVehiculo == idVehiculo)
                    .OrderByDescending(c => c.FechaEmision)
                    .ToListAsync();

                var certificadosResponse = certificados.Select(c => new CertificadoResponseDto
                {
                    IdCertificado = c.IdCertificado,
                    IdInspeccion = c.IdInspeccion,
                    NumeroCertificado = c.NumeroCertificado,
                    FechaEmision = c.FechaEmision,
                    FechaVencimiento = c.FechaVencimiento,
                    RutaArchivoDigital = c.RutaArchivoDigital,
                    EstadoCertificado = c.EstadoCertificado,
                    EstaVigente = c.FechaVencimiento > DateTime.Now && c.EstadoCertificado == "valido"
                }).ToList();

                return Ok(certificadosResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener certificados para vehículo {VehiculoId}", idVehiculo);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("porNumero/{numeroCertificado}")]
        public async Task<ActionResult<CertificadoResponseDto>> GetCertificadoPorNumero(string numeroCertificado)
        {
            try
            {
                var certificado = await _context.Certificados
                    .Include(c => c.Inspeccion)
                        .ThenInclude(i => i.Cita)
                        .ThenInclude(cita => cita.Vehiculo)
                        .ThenInclude(v => v.Propietario)
                    .Include(c => c.Inspeccion)
                        .ThenInclude(i => i.Tecnico)
                    .FirstOrDefaultAsync(c => c.NumeroCertificado == numeroCertificado);

                if (certificado == null)
                    return NotFound("Certificado no encontrado");

                var certificadoDto = new CertificadoResponseDto
                {
                    IdCertificado = certificado.IdCertificado,
                    IdInspeccion = certificado.IdInspeccion,
                    NumeroCertificado = certificado.NumeroCertificado,
                    FechaEmision = certificado.FechaEmision,
                    FechaVencimiento = certificado.FechaVencimiento,
                    RutaArchivoDigital = certificado.RutaArchivoDigital,
                    EstadoCertificado = certificado.EstadoCertificado,
                    VehiculoInfo = certificado.Inspeccion?.Cita?.Vehiculo != null ?
                        $"{certificado.Inspeccion.Cita.Vehiculo.NumeroPlaca} - {certificado.Inspeccion.Cita.Vehiculo.Marca} {certificado.Inspeccion.Cita.Vehiculo.Modelo}" : "N/A",
                    PropietarioInfo = certificado.Inspeccion?.Cita?.Vehiculo?.Propietario != null ?
                        $"{certificado.Inspeccion.Cita.Vehiculo.Propietario.Nombre} {certificado.Inspeccion.Cita.Vehiculo.Propietario.Apellidos}" : "N/A",
                    TecnicoInfo = certificado.Inspeccion?.Tecnico != null ?
                        $"{certificado.Inspeccion.Tecnico.Nombre} {certificado.Inspeccion.Tecnico.Apellidos}" : "N/A",
                    EstaVigente = certificado.FechaVencimiento > DateTime.Now && certificado.EstadoCertificado == "valido"
                };

                return Ok(certificadoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar certificado por número {NumeroCertificado}", numeroCertificado);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("vigentes")]
        public async Task<ActionResult<IEnumerable<CertificadoResponseDto>>> GetCertificadosVigentes()
        {
            try
            {
                var certificados = await _context.Certificados
                    .Include(c => c.Inspeccion)
                        .ThenInclude(i => i.Cita)
                        .ThenInclude(cita => cita.Vehiculo)
                        .ThenInclude(v => v.Propietario)
                    .Include(c => c.Inspeccion)
                        .ThenInclude(i => i.Tecnico)
                    .Where(c => c.FechaVencimiento > DateTime.Now && c.EstadoCertificado == "valido")
                    .OrderByDescending(c => c.FechaEmision)
                    .ToListAsync();

                var certificadosResponse = certificados.Select(c => new CertificadoResponseDto
                {
                    IdCertificado = c.IdCertificado,
                    IdInspeccion = c.IdInspeccion,
                    NumeroCertificado = c.NumeroCertificado,
                    FechaEmision = c.FechaEmision,
                    FechaVencimiento = c.FechaVencimiento,
                    RutaArchivoDigital = c.RutaArchivoDigital,
                    EstadoCertificado = c.EstadoCertificado,
                    VehiculoInfo = c.Inspeccion?.Cita?.Vehiculo != null ?
                        $"{c.Inspeccion.Cita.Vehiculo.NumeroPlaca} - {c.Inspeccion.Cita.Vehiculo.Marca} {c.Inspeccion.Cita.Vehiculo.Modelo}" : "N/A",
                    PropietarioInfo = c.Inspeccion?.Cita?.Vehiculo?.Propietario != null ?
                        $"{c.Inspeccion.Cita.Vehiculo.Propietario.Nombre} {c.Inspeccion.Cita.Vehiculo.Propietario.Apellidos}" : "N/A",
                    TecnicoInfo = c.Inspeccion?.Tecnico != null ?
                        $"{c.Inspeccion.Tecnico.Nombre} {c.Inspeccion.Tecnico.Apellidos}" : "N/A",
                    EstaVigente = true
                }).ToList();

                return Ok(certificadosResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener certificados vigentes");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("{id}/anular")]
        public async Task<IActionResult> AnularCertificado(int id)
        {
            try
            {
                var certificado = await _context.Certificados.FindAsync(id);
                if (certificado == null)
                {
                    return NotFound("Certificado no encontrado");
                }

                if (certificado.EstadoCertificado == "anulado")
                {
                    return BadRequest("El certificado ya está anulado");
                }

                certificado.EstadoCertificado = "anulado";
                _context.Entry(certificado).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Certificado {CertificadoId} anulado exitosamente", id);

                return Ok(new { message = "Certificado anulado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al anular certificado {CertificadoId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }

    // DTOs para Certificado
    public class CertificadoCreateDto
    {
        [Required(ErrorMessage = "La inspección es requerida")]
        public int IdInspeccion { get; set; }

        [Required(ErrorMessage = "El número de certificado es requerido")]
        [StringLength(50, ErrorMessage = "El número de certificado no puede exceder 50 caracteres")]
        public string NumeroCertificado { get; set; } = string.Empty;

        public DateTime? FechaEmision { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es requerida")]
        public DateTime FechaVencimiento { get; set; }

        [StringLength(500, ErrorMessage = "La ruta del archivo no puede exceder 500 caracteres")]
        public string? RutaArchivoDigital { get; set; }

        [RegularExpression("^(valido|vencido|anulado)$", ErrorMessage = "Estado debe ser 'valido', 'vencido' o 'anulado'")]
        public string? EstadoCertificado { get; set; }
    }

    public class CertificadoUpdateDto
    {
        [Required(ErrorMessage = "La fecha de vencimiento es requerida")]
        public DateTime FechaVencimiento { get; set; }

        [StringLength(500, ErrorMessage = "La ruta del archivo no puede exceder 500 caracteres")]
        public string? RutaArchivoDigital { get; set; }

        [RegularExpression("^(valido|vencido|anulado)$", ErrorMessage = "Estado debe ser 'valido', 'vencido' o 'anulado'")]
        public string? EstadoCertificado { get; set; }
    }

    public class CertificadoResponseDto
    {
        public int IdCertificado { get; set; }
        public int IdInspeccion { get; set; }
        public string NumeroCertificado { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string? RutaArchivoDigital { get; set; }
        public string EstadoCertificado { get; set; } = string.Empty;
        public bool EstaVigente { get; set; }

        // Información adicional
        public string? VehiculoInfo { get; set; }
        public string? PropietarioInfo { get; set; }
        public string? TecnicoInfo { get; set; }
    }
}