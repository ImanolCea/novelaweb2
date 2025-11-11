using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Infrastructure;
using novelaweb2.Models;

namespace novelaweb2.Controllers
{
    [AdminAuthorize]
    public class EtiquetasController : Controller
    {
        private readonly WebNovelasDbContext _context;

        public EtiquetasController(WebNovelasDbContext context)
        {
            _context = context;
        }

        // Listado de etiquetas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Etiquetas.ToListAsync());
        }

        // Crear nueva etiqueta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre")] Etiqueta etiqueta)
        {
            if (ModelState.IsValid)
            {
                _context.Add(etiqueta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(etiqueta);
        }

        // Asignar etiquetas a una novela
        [HttpPost]
        public async Task<IActionResult> AsignarEtiquetas(int novelaId, List<int> etiquetasSeleccionadas)
        {
            var novela = await _context.Novelas
                .Include(n => n.Etiquetas)
                .FirstOrDefaultAsync(n => n.Id == novelaId);

            if (novela == null)
                return NotFound();

            novela.Etiquetas.Clear();

            foreach (var idEtiqueta in etiquetasSeleccionadas)
            {
                var etiqueta = await _context.Etiquetas.FindAsync(idEtiqueta);
                if (etiqueta != null)
                    novela.Etiquetas.Add(etiqueta);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Novelas", new { id = novelaId });
        }
    }
}
