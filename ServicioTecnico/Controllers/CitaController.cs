using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioTecnico.Data;
using ServicioTecnico.Models;

namespace ServicioTecnico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CitaController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cita>>> GetCitas()
        {
            return await _context.Citas.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Cita>> GetCita(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null)
                return NotFound();

            return cita;
        }

        [HttpPost]
        public async Task<ActionResult<Cita>> PostCita(Cita cita)
        {
            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCita), new { id = cita.IdCita }, cita);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCita(int id, Cita cita)
        {
            if (id != cita.IdCita)
                return BadRequest();

            _context.Entry(cita).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCita(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null)
                return NotFound();

            _context.Citas.Remove(cita);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}