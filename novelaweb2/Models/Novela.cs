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

        // 🔹 Relaciones
        public virtual Usuario Autor { get; set; } = null!;
        public virtual ICollection<Capitulo> Capitulos { get; set; } = new List<Capitulo>();
        public virtual ICollection<Reseña> Reseñas { get; set; } = new List<Reseña>();
        public virtual ICollection<Seguimiento> Seguimientos { get; set; } = new List<Seguimiento>();

        // ⚠️ ESTA ES LA CLAVE: relación N:N ya configurada en tu DbContext
        public virtual ICollection<Etiqueta> Etiqueta { get; set; } = new List<Etiqueta>();

        // Propiedad temporal (no se guarda en BD)
        public Seguimiento? SeguimientoUsuario { get; set; }
    }
}
