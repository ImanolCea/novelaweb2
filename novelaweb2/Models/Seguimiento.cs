using System;

namespace novelaweb2.Models;

public partial class Seguimiento
{
    public int UsuarioId { get; set; }
    public int NovelaId { get; set; }
    public int? UltimoCapituloLeidoId { get; set; }
    public DateTime FechaUltimaLectura { get; set; }

    public virtual Novela Novela { get; set; } = null!;
    public virtual Capitulo? UltimoCapituloLeido { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;
}
