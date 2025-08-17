using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicioTecnico.Data;
using ServicioTecnico.Models;

namespace ServicioTecnico.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificadoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CertificadoController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Certificado>>> GetCertificados()
        {
            return await _context.Certificados.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Certificado>> GetCertificado(int id)
        {
            var certificado = await _context.Certificados.FindAsync(id);
            if (certificado == null)
                return NotFound();

            return certificado;
        }

        [HttpPost]
        public async Task<ActionResult<Certificado>> PostCertificado(Certificado certificado)
        {
            _context.Certificados.Add(certificado);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCertificado), new { id = certificado.IdCertificado }, certificado);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCertificado(int id, Certificado certificado)
        {
            if (id != certificado.IdCertificado)
                return BadRequest();

            _context.Entry(certificado).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCertificado(int id)
        {
            var certificado = await _context.Certificados.FindAsync(id);
            if (certificado == null)
                return NotFound();

            _context.Certificados.Remove(certificado);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
