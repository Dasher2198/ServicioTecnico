using ServicioTecnico.Models;
using Microsoft.EntityFrameworkCore;

namespace ServicioTecnico.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Estacion> Estaciones { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<Inspeccion> Inspecciones { get; set; }
        public DbSet<DetalleInspeccion> DetalleInspecciones { get; set; }
        public DbSet<Certificado> Certificados { get; set; }

    }
}