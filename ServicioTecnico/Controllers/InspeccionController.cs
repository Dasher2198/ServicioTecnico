using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioTecnico.Data;
using ServicioTecnico.Models;

namespace ServicioTecnico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InspeccionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InspeccionController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inspeccion>>> GetInspecciones()
        {
            return await _context.Inspecciones.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Inspeccion>> GetInspeccion(int id)
        {
            var inspeccion = await _context.Inspecciones.FindAsync(id);
            if (inspeccion == null)
                return NotFound();

            return inspeccion;
        }

        [HttpPost]
        public async Task<ActionResult<Inspeccion>> PostInspeccion(Inspeccion inspeccion)
        {
            _context.Inspecciones.Add(inspeccion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInspeccion), new { id = inspeccion.IdInspeccion }, inspeccion);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutInspeccion(int id, Inspeccion inspeccion)
        {
            if (id != inspeccion.IdInspeccion)
                return BadRequest();

            _context.Entry(inspeccion).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInspeccion(int id)
        {
            var inspeccion = await _context.Inspecciones.FindAsync(id);
            if (inspeccion == null)
                return NotFound();

            _context.Inspecciones.Remove(inspeccion);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
