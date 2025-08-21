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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración para Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.IdUsuario);
                entity.HasIndex(e => e.Cedula).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Apellidos).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Cedula).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Telefono).HasMaxLength(15);
                entity.Property(e => e.Direccion).HasMaxLength(200);
                entity.Property(e => e.TipoUsuario).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Estado).HasMaxLength(10).HasDefaultValue("activo");
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");

                entity.HasCheckConstraint("CK_Usuario_TipoUsuario", "[TipoUsuario] IN ('cliente', 'tecnico')");
                entity.HasCheckConstraint("CK_Usuario_Estado", "[Estado] IN ('activo', 'inactivo')");
            });

            // Configuración para Estacion
            modelBuilder.Entity<Estacion>(entity =>
            {
                entity.HasKey(e => e.IdEstacion);

                entity.Property(e => e.NombreEstacion).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Direccion).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Telefono).HasMaxLength(15);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Provincia).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Canton).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Distrito).IsRequired().HasMaxLength(50);
                entity.Property(e => e.HorarioAtencion).HasMaxLength(100);
                entity.Property(e => e.Estado).HasMaxLength(10).HasDefaultValue("activa");

                entity.HasCheckConstraint("CK_Estacion_Estado", "[Estado] IN ('activa', 'inactiva')");
            });

            // Configuración para Vehiculo - CORREGIDO
            modelBuilder.Entity<Vehiculo>(entity =>
            {
                entity.HasKey(e => e.IdVehiculo);
                entity.HasIndex(e => e.NumeroPlaca).IsUnique();

                entity.Property(e => e.NumeroPlaca).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Marca).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Modelo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.NumeroChasis).HasMaxLength(50); // OPCIONAL
                entity.Property(e => e.Color).HasMaxLength(30); // OPCIONAL
                entity.Property(e => e.TipoCombustible).HasMaxLength(20); // OPCIONAL
                entity.Property(e => e.Cilindrada).HasMaxLength(10); // OPCIONAL
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.Propietario)
                    .WithMany(p => p.Vehiculos)
                    .HasForeignKey(d => d.IdPropietario)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración para Cita
            modelBuilder.Entity<Cita>(entity =>
            {
                entity.HasKey(e => e.IdCita);
                entity.HasIndex(e => new { e.FechaCita, e.HoraCita });

                entity.Property(e => e.EstadoCita).HasMaxLength(15).HasDefaultValue("programada");
                entity.Property(e => e.Observaciones).HasMaxLength(500);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");

                entity.HasCheckConstraint("CK_Cita_EstadoCita", "[EstadoCita] IN ('programada', 'completada', 'cancelada')");

                entity.HasOne(d => d.Vehiculo)
                    .WithMany(p => p.Citas)
                    .HasForeignKey(d => d.IdVehiculo)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Estacion)
                    .WithMany(p => p.Citas)
                    .HasForeignKey(d => d.IdEstacion)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración para Inspeccion
            modelBuilder.Entity<Inspeccion>(entity =>
            {
                entity.HasKey(e => e.IdInspeccion);
                entity.HasIndex(e => e.FechaInspeccion);

                entity.Property(e => e.Resultado).IsRequired().HasMaxLength(10);
                entity.Property(e => e.ObservacionesTecnicas).HasMaxLength(1000);
                entity.Property(e => e.NumeroCertificado).HasMaxLength(50);

                entity.HasCheckConstraint("CK_Inspeccion_Resultado", "[Resultado] IN ('aprobado', 'rechazado')");

                entity.HasOne(d => d.Cita)
                    .WithMany(p => p.Inspecciones)
                    .HasForeignKey(d => d.IdCita)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Tecnico)
                    .WithMany(p => p.InspeccionesTecnico)
                    .HasForeignKey(d => d.IdTecnico)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración para DetalleInspeccion
            modelBuilder.Entity<DetalleInspeccion>(entity =>
            {
                entity.HasKey(e => e.IdDetalle);

                entity.Property(e => e.CategoriaRevision).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ResultadoItem).IsRequired().HasMaxLength(10);
                entity.Property(e => e.ObservacionesItem).HasMaxLength(500);

                entity.HasCheckConstraint("CK_DetalleInspeccion_ResultadoItem", "[ResultadoItem] IN ('OK', 'FALLO')");

                entity.HasOne(d => d.Inspeccion)
                    .WithMany(p => p.Detalles)
                    .HasForeignKey(d => d.IdInspeccion)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración para Certificado
            modelBuilder.Entity<Certificado>(entity =>
            {
                entity.HasKey(e => e.IdCertificado);
                entity.HasIndex(e => e.NumeroCertificado).IsUnique();

                entity.Property(e => e.NumeroCertificado).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FechaEmision).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.RutaArchivoDigital).HasMaxLength(500);
                entity.Property(e => e.EstadoCertificado).HasMaxLength(10).HasDefaultValue("valido");

                entity.HasCheckConstraint("CK_Certificado_EstadoCertificado", "[EstadoCertificado] IN ('valido', 'vencido', 'anulado')");

                entity.HasOne(d => d.Inspeccion)
                    .WithMany(p => p.Certificados)
                    .HasForeignKey(d => d.IdInspeccion)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}