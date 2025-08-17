using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ServicioTecnico.Models
{
    public class Vehiculo
    {
        [Key]
        public int IdVehiculo { get; set; }

        [Required, StringLength(10)]
        public string NumeroPlaca { get; set; }

        [Required]
        public int IdPropietario { get; set; }

        [ForeignKey(nameof(IdPropietario))]
        public Usuario Propietario { get; set; }

        [Required, StringLength(50)]
        public string Marca { get; set; }

        [Required, StringLength(50)]
        public string Modelo { get; set; }

        public int Año { get; set; }

        [StringLength(50)]
        public string NumeroChasis { get; set; }

        [StringLength(30)]
        public string Color { get; set; }

        [StringLength(20)]
        public string TipoCombustible { get; set; }

        [StringLength(10)]
        public string Cilindrada { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Relaciones
        public ICollection<Cita> Citas { get; set; }
    }
}