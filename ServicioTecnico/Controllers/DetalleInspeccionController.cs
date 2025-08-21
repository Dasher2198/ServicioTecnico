using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioTecnico.Data;
using ServicioTecnico.Models;

namespace ServicioTecnico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetalleInspeccionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DetalleInspeccionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DetalleInspeccion>>> GetDetalles()
        {
            return await _context.DetalleInspecciones.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DetalleInspeccion>> GetDetalle(int id)
        {
            var detalle = await _context.DetalleInspecciones.FindAsync(id);
            if (detalle == null)
                return NotFound();

            return detalle;
        }

        [HttpPost]
        public async Task<ActionResult<DetalleInspeccion>> PostDetalle(DetalleInspeccion detalle)
        {
            _context.DetalleInspecciones.Add(detalle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDetalle), new { id = detalle.IdDetalle }, detalle);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDetalle(int id, DetalleInspeccion detalle)
        {
            if (id != detalle.IdDetalle)
                return BadRequest();

            _context.Entry(detalle).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDetalle(int id)
        {
            var detalle = await _context.DetalleInspecciones.FindAsync(id);
            if (detalle == null)
                return NotFound();

            _context.DetalleInspecciones.Remove(detalle);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("porInspeccion/{idInspeccion}")]
        public async Task<ActionResult<IEnumerable<DetalleInspeccion>>> GetDetallesPorInspeccion(int idInspeccion)
        {
            return await _context.DetalleInspecciones
                .Where(d => d.IdInspeccion == idInspeccion)
                .ToListAsync();
        }
    }
}