using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioTecnico.Models
{
    public class Certificado
    {
        [Key]
        public int IdCertificado { get; set; }

        [Required]
        public int IdInspeccion { get; set; }
        [ForeignKey(nameof(IdInspeccion))]
        public Inspeccion Inspeccion { get; set; }

        [Required, StringLength(50)]
        public string NumeroCertificado { get; set; }

        public DateTime FechaEmision { get; set; } = DateTime.Now;

        [Required]
        public DateTime FechaVencimiento { get; set; }

        [StringLength(500)]
        public string RutaArchivoDigital { get; set; }

        [StringLength(10)]
        public string EstadoCertificado { get; set; } = "valido";
    }
}