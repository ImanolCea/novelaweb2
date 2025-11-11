using System;
using System.Collections.Generic;

namespace novelaweb2.Models;

public partial class Reseña
{
    public int Id { get; set; }

    public int NovelaId { get; set; }

    public int UsuarioId { get; set; }

    public byte Puntuacion { get; set; }

    public string? Comentario { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Novela Novela { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
