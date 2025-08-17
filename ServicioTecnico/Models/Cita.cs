using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioTecnico.Models
{
    public class Cita
    {
        [Key]
        public int IdCita { get; set; }

        [Required]
        public int IdVehiculo { get; set; }
        [ForeignKey(nameof(IdVehiculo))]
        public Vehiculo Vehiculo { get; set; }

        [Required]
        public int IdEstacion { get; set; }
        [ForeignKey(nameof(IdEstacion))]
        public Estacion Estacion { get; set; }

        [Required]
        public DateTime FechaCita { get; set; }

        [Required]
        public TimeSpan HoraCita { get; set; }

        [StringLength(15)]
        public string EstadoCita { get; set; } = "programada";

        [StringLength(500)]
        public string Observaciones { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relaciones
        public ICollection<Inspeccion> Inspecciones { get; set; }
    }
}