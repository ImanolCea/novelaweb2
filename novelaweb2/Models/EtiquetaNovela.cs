namespace novelaweb2.Models;

public class EtiquetaNovela
{
    public int NovelaId { get; set; }
    public int EtiquetaId { get; set; }

    public virtual Novela Novela { get; set; } = null!;
    public virtual Etiqueta Etiqueta { get; set; } = null!;
}
