using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioTecnico.Models
{
    public class DetalleInspeccion
    {
        [Key]
        public int IdDetalle { get; set; }

        [Required]
        public int IdInspeccion { get; set; }
        [ForeignKey(nameof(IdInspeccion))]
        public Inspeccion Inspeccion { get; set; }

        [Required, StringLength(50)]
        public string CategoriaRevision { get; set; }

        [Required, StringLength(10)]
        public string ResultadoItem { get; set; } // OK / FALLO

        [StringLength(500)]
        public string ObservacionesItem { get; set; }
    }
}
