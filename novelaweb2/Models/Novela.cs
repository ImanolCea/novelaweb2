using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace novelaweb2.Models
{
    public partial class Novela
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = null!;

        public string? Sinopsis { get; set; }
        public string? PortadaUrl { get; set; }
        public string? Genero { get; set; }
        public string Estado { get; set; } = "En curso";
        public DateTime FechaPublicacion { get; set; } = DateTime.Now;
        public int AutorId { get; set; }

        // 🔹 Nueva propiedad faltante
        public int PalabrasTotales { get; set; }

        // 🔹 Relaciones
        public virtual Usuario Autor { get; set; } = null!;
        public virtual ICollection<Capitulo> Capitulos { get; set; } = new List<Capitulo>();
        public virtual ICollection<Reseña> Reseñas { get; set; } = new List<Reseña>();
        public virtual ICollection<Seguimiento> Seguimientos { get; set; } = new List<Seguimiento>();

        // 🔹 Corregir nombre: plural para claridad y consistencia
        public virtual ICollection<Etiqueta> Etiquetas { get; set; } = new List<Etiqueta>();

        // 🔹 Agregar relación con listas
        public virtual ICollection<ListaNovela> ListaNovelas { get; set; } = new List<ListaNovela>();

        // Propiedad temporal (no se guarda en BD)
        public Seguimiento? SeguimientoUsuario { get; set; }
    }
}
