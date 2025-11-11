using System;
using System.Collections.Generic;

namespace novelaweb2.Models;

public partial class Capitulo
{
    public int Id { get; set; }

    public int NovelaId { get; set; }

    public int NumeroCapitulo { get; set; }

    public string Titulo { get; set; } = null!;

    public string Contenido { get; set; } = null!;

    public DateTime FechaPublicacion { get; set; }

    public int Palabras { get; set; }

    public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public virtual Novela Novela { get; set; } = null!;

    public virtual ICollection<Seguimiento> Seguimientos { get; set; } = new List<Seguimiento>();
}
