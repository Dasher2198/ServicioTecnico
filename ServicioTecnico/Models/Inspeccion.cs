using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioTecnico.Models
{
    public class Inspeccion
    {
        [Key]
        public int IdInspeccion { get; set; }

        [Required]
        public int IdCita { get; set; }
        [ForeignKey(nameof(IdCita))]
        public Cita Cita { get; set; }

        [Required]
        public int IdTecnico { get; set; }
        [ForeignKey(nameof(IdTecnico))]
        public Usuario Tecnico { get; set; }

        [Required]
        public DateTime FechaInspeccion { get; set; }

        [Required, StringLength(10)]
        public string Resultado { get; set; } // aprobado / rechazado

        [StringLength(1000)]
        public string ObservacionesTecnicas { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        [StringLength(50)]
        public string NumeroCertificado { get; set; }

        // Relaciones
        public ICollection<DetalleInspeccion> Detalles { get; set; }
        public ICollection<Certificado> Certificados { get; set; }
    }
}