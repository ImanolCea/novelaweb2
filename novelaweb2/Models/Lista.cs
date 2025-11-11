using System;
using System.Collections.Generic;

namespace novelaweb2.Models;

public partial class Lista
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<ListaNovela> ListaNovelas { get; set; } = new List<ListaNovela>();

    public virtual Usuario Usuario { get; set; } = null!;
}
