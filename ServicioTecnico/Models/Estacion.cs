using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ServicioTecnico.Models
{
    public class Estacion
    {
        [Key]
        public int IdEstacion { get; set; }

        [Required, StringLength(100)]
        public string NombreEstacion { get; set; }

        [Required, StringLength(200)]
        public string Direccion { get; set; }

        [StringLength(15)]
        public string Telefono { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [Required, StringLength(50)]
        public string Provincia { get; set; }

        [Required, StringLength(50)]
        public string Canton { get; set; }

        [Required, StringLength(50)]
        public string Distrito { get; set; }

        [StringLength(100)]
        public string HorarioAtencion { get; set; }

        [StringLength(10)]
        public string Estado { get; set; } = "activa";

        // Relaciones
        public ICollection<Cita> Citas { get; set; }
    }
}
