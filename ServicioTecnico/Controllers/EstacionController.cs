using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioTecnico.Data;
using ServicioTecnico.Models;

namespace ServicioTecnico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstacionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EstacionController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Estacion>>> GetEstaciones()
        {
            return await _context.Estaciones.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Estacion>> GetEstacion(int id)
        {
            var estacion = await _context.Estaciones.FindAsync(id);
            if (estacion == null) return NotFound();
            return estacion;
        }

        [HttpPost]
        public async Task<ActionResult<Estacion>> PostEstacion(Estacion estacion)
        {
            _context.Estaciones.Add(estacion);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEstacion), new { id = estacion.IdEstacion }, estacion);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEstacion(int id, Estacion estacion)
        {
            if (id != estacion.IdEstacion) return BadRequest();
            _context.Entry(estacion).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEstacion(int id)
        {
            var estacion = await _context.Estaciones.FindAsync(id);
            if (estacion == null) return NotFound();
            _context.Estaciones.Remove(estacion);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
