using System.ComponentModel.DataAnnotations;

namespace ServicioTecnico.DTOs
{
    public class CitaCreateDto
    {
        [Required(ErrorMessage = "El vehículo es requerido")]
        public int IdVehiculo { get; set; }

        [Required(ErrorMessage = "La estación es requerida")]
        public int IdEstacion { get; set; }

        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime FechaCita { get; set; }

        [Required(ErrorMessage = "La hora es requerida")]
        public string HoraCita { get; set; } // Recibir como string "HH:mm:ss"

        [StringLength(15)]
        public string EstadoCita { get; set; } = "programada";

        [StringLength(500)]
        public string? Observaciones { get; set; }
    }

    public class CitaResponseDto
    {
        public int IdCita { get; set; }
        public int IdVehiculo { get; set; }
        public int IdEstacion { get; set; }
        public DateTime FechaCita { get; set; }
        public TimeSpan HoraCita { get; set; }
        public string EstadoCita { get; set; }
        public string? Observaciones { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Información adicional para mostrar
        public string? VehiculoInfo { get; set; }
        public string? EstacionInfo { get; set; }
    }

    public class CitaUpdateDto
    {
        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime FechaCita { get; set; }

        [Required(ErrorMessage = "La hora es requerida")]
        public string HoraCita { get; set; }

        [StringLength(15)]
        public string EstadoCita { get; set; } = "programada";

        [StringLength(500)]
        public string? Observaciones { get; set; }
    }
}