using System;
using System.Collections.Generic;

namespace novelaweb2.Models;

public partial class Etiqueta
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;

    public virtual ICollection<EtiquetaNovela> NovelaEtiquetas { get; set; } = new List<EtiquetaNovela>();
    public virtual ICollection<Novela> Novelas { get; set; } = new List<Novela>();
}
