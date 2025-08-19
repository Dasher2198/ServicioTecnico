using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ServicioTecnico.Models
{
    public class Cita
    {
        [Key]
        public int IdCita { get; set; }

        [Required]
        public int IdVehiculo { get; set; }

        [ForeignKey(nameof(IdVehiculo))]
        [JsonIgnore] // Evitar referencia circular en JSON
        public Vehiculo? Vehiculo { get; set; }

        [Required]
        public int IdEstacion { get; set; }

        [ForeignKey(nameof(IdEstacion))]
        [JsonIgnore] // Evitar referencia circular en JSON
        public Estacion? Estacion { get; set; }

        [Required]
        public DateTime FechaCita { get; set; }

        [Required]
        public TimeSpan HoraCita { get; set; }

        [StringLength(15)]
        public string EstadoCita { get; set; } = "programada";

        [StringLength(500)]
        public string? Observaciones { get; set; } // Hacer opcional

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relaciones - Ignorar en JSON para evitar referencias circulares
        [JsonIgnore]
        public ICollection<Inspeccion>? Inspecciones { get; set; }
    }
}