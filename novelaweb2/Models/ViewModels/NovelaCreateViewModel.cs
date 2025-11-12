using System.ComponentModel.DataAnnotations;

namespace novelaweb2.Models.ViewModels
{
    public class NovelaCreateViewModel
    {
        // Datos de la novela
        [Required, StringLength(200)]
        public string Titulo { get; set; } = null!;

        public string? Sinopsis { get; set; }
        public string? PortadaUrl { get; set; }
        public string? Genero { get; set; }

        [Required]
        public string Estado { get; set; } = "En curso";

        // Etiquetas seleccionadas
        [Required(ErrorMessage = "Debes seleccionar al menos una etiqueta.")]
        public List<int> EtiquetasSeleccionadas { get; set; } = new();

        // Datos del primer capítulo
        [Required, StringLength(200)]
        public string TituloCapitulo { get; set; } = null!;

        [Required]
        public string ContenidoCapitulo { get; set; } = null!;
    }
}
