using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace novelaweb2.Models
{
    [Table("Seguimientos")]
    public class Seguimiento
    {
        [Key, Column(Order = 0)]
        public int UsuarioId { get; set; }

        [Key, Column(Order = 1)]
        public int NovelaId { get; set; }

        public int? UltimoCapituloLeidoId { get; set; }

        public DateTime FechaUltimaLectura { get; set; }

        // 🔹 Relaciones
        [ForeignKey(nameof(UsuarioId))]
        public Usuario Usuario { get; set; }

        [ForeignKey(nameof(NovelaId))]
        public Novela Novela { get; set; }

        [ForeignKey(nameof(UltimoCapituloLeidoId))]
        public Capitulo? UltimoCapituloLeido { get; set; }
    }
}
