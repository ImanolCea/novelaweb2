using System;
using System.Collections.Generic;

namespace novelaweb2.Models;

public partial class ListaNovela
{
    public int ListaId { get; set; }

    public int NovelaId { get; set; }

    public DateTime FechaAgregado { get; set; }

    public virtual Lista Lista { get; set; } = null!;

    public virtual Novela Novela { get; set; } = null!;
}
