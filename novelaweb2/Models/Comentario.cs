using System;
using System.Collections.Generic;

namespace novelaweb2.Models;

public partial class Comentario
{
    public int Id { get; set; }

    public int CapituloId { get; set; }

    public int UsuarioId { get; set; }

    public string Contenido { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public virtual Capitulo Capitulo { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
