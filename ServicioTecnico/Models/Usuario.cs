using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace ServicioTecnico.Models
{
    // son las clases que estan en los proyectos//
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required, StringLength(50)]
        public string Nombre { get; set; }

        [Required, StringLength(100)]
        public string Apellidos { get; set; }

        [Required, StringLength(20)]
        public string Cedula { get; set; }

        [Required, StringLength(100)]
        public string Email { get; set; }

        [StringLength(15)]
        public string Telefono { get; set; }

        [StringLength(200)]
        public string Direccion { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Required, StringLength(10)]
        public string TipoUsuario { get; set; } // cliente / tecnico

        [Required, StringLength(255)]
        public string Password { get; set; }

        [StringLength(10)]
        public string Estado { get; set; } = "activo";

        // Relaciones
        public ICollection<Vehiculo> Vehiculos { get; set; }
        public ICollection<Inspeccion> InspeccionesTecnico { get; set; }
    }
}