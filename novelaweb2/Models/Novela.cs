using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace novelaweb2.Models;

public partial class Novela
{
    public int Id { get; set; }
    public string Titulo { get; set; } = null!;
    public string? Sinopsis { get; set; }
    public string? PortadaUrl { get; set; }
    public string Estado { get; set; } = null!;
    public DateTime FechaPublicacion { get; set; }
    public int AutorId { get; set; }
    public int PalabrasTotales { get; set; }
    public string? Genero { get; set; }

    public virtual Usuario Autor { get; set; } = null!;
    public virtual ICollection<Capitulo> Capitulos { get; set; } = new List<Capitulo>();
    public virtual ICollection<ListaNovela> ListaNovelas { get; set; } = new List<ListaNovela>();
    public virtual ICollection<Reseña> Reseñas { get; set; } = new List<Reseña>();
    public virtual ICollection<Seguimiento> Seguimientos { get; set; } = new List<Seguimiento>();
    public virtual ICollection<EtiquetaNovela> EtiquetaNovelas { get; set; } = new List<EtiquetaNovela>();
    public virtual ICollection<Etiqueta> Etiquetas { get; set; } = new List<Etiqueta>();

    [NotMapped]
    public Seguimiento? SeguimientoUsuario { get; set; }
}
