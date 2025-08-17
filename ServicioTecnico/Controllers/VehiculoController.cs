using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioTecnico.Data;
using ServicioTecnico.Models;

namespace ServicioTecnico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiculoController : ControllerBase
    {
        public class VehiculosController : ControllerBase
        {
            private readonly AppDbContext _context;

            public VehiculosController(AppDbContext context)
            {
                _context = context;
            }
            [HttpGet]
            public async Task<ActionResult<IEnumerable<Vehiculo>>> GetVehiculos()
            {
                return await _context.Vehiculos.Include(v => v.Propietario).ToListAsync();
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<Vehiculo>> GetVehiculo(int id)
            {
                var vehiculo = await _context.Vehiculos.Include(v => v.Propietario)
                                                       .FirstOrDefaultAsync(v => v.IdVehiculo == id);
                if (vehiculo == null) return NotFound();
                return vehiculo;
            }

            [HttpPost]
            public async Task<ActionResult<Vehiculo>> PostVehiculo(Vehiculo vehiculo)
            {
                _context.Vehiculos.Add(vehiculo);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetVehiculo), new { id = vehiculo.IdVehiculo }, vehiculo);
            }

            [HttpPut("{id}")]
            public async Task<IActionResult> PutVehiculo(int id, Vehiculo vehiculo)
            {
                if (id != vehiculo.IdVehiculo) return BadRequest();
                _context.Entry(vehiculo).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }

            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteVehiculo(int id)
            {
                var vehiculo = await _context.Vehiculos.FindAsync(id);
                if (vehiculo == null) return NotFound();
                _context.Vehiculos.Remove(vehiculo);
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }
    }
}