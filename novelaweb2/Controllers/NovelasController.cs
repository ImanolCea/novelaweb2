using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Models;

namespace novelaweb2.Controllers
{
    public class NovelasController : Controller
    {
        private readonly WebNovelasDbContext _context;

        public NovelasController(WebNovelasDbContext context)
        {
            _context = context;
        }

        // GET: Novelas/Details/5a
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var novela = await _context.Novelas
                .Include(n => n.Autor)
                .Include(n => n.Capitulos)
                .Include(n => n.Reseñas).ThenInclude(r => r.Usuario)
                .Include(n => n.EtiquetaNovelas).ThenInclude(en => en.Etiqueta)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (novela == null) return NotFound();

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId != null)
            {
                novela.SeguimientoUsuario = await _context.Seguimientos
                    .FirstOrDefaultAsync(s => s.UsuarioId == usuarioId && s.NovelaId == novela.Id);
            }

            return View(novela);
        }

        // GET: Novelas/Create
        public IActionResult Create()
        {
            ViewData["Etiquetas"] = new MultiSelectList(_context.Etiquetas, "Id", "Nombre");
            return View();
        }

        // POST: Novelas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Novela novela, int[] etiquetasSeleccionadas)
        {
            if (ModelState.IsValid)
            {
                _context.Add(novela);
                await _context.SaveChangesAsync();

                foreach (var etId in etiquetasSeleccionadas)
                {
                    _context.EtiquetaNovelas.Add(new EtiquetaNovela
                    {
                        NovelaId = novela.Id,
                        EtiquetaId = etId
                    });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = novela.Id });
            }

            ViewData["Etiquetas"] = new MultiSelectList(_context.Etiquetas, "Id", "Nombre");
            return View(novela);
        }
    }
}
