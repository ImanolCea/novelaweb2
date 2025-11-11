using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace novelaweb2.Models
{
    public partial class Etiqueta
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = null!;

        // Relación N:N con novelas
        public virtual ICollection<Novela> Novelas { get; set; } = new List<Novela>();
    }
}
