using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Models;

namespace novelaweb2.Controllers
{
    public class CapituloesController : Controller
    {
        private readonly WebNovelasDbContext _context;

        public CapituloesController(WebNovelasDbContext context)
        {
            _context = context;
        }

        // =================== VER CAPÍTULO ===================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var capitulo = await _context.Capitulos
                .Include(c => c.Novela)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (capitulo == null)
                return NotFound();

            var novela = capitulo.Novela;

            var capitulos = await _context.Capitulos
                .Where(c => c.NovelaId == novela.Id)
                .OrderBy(c => c.NumeroCapitulo)
                .ToListAsync();

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            ViewBag.UsuarioId = usuarioId;
            ViewBag.NombreUsuario = HttpContext.Session.GetString("NombreUsuario");


            var comentarios = await _context.Comentarios
                .Where(x => x.CapituloId == id)
                .Include(x => x.Usuario)
                .OrderByDescending(x => x.Fecha)
                .ToListAsync();

            ViewBag.Comentarios = comentarios;

            return View(capitulo);
        }

        // =================== CREAR CAPÍTULO ===================
        public IActionResult Create(int novelaId)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var novela = _context.Novelas.FirstOrDefault(n => n.Id == novelaId);

            if (novela == null || novela.AutorId != usuarioId)
                return Forbid();

            ViewBag.Novela = novela;
            return View(new Capitulo { NovelaId = novelaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NovelaId,NumeroCapitulo,Titulo,Contenido")] Capitulo capitulo)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var novela = await _context.Novelas.FirstOrDefaultAsync(n => n.Id == capitulo.NovelaId);

            if (novela == null || novela.AutorId != usuarioId)
                return Forbid();

            if (ModelState.IsValid)
            {
                capitulo.FechaPublicacion = DateTime.Now;
                capitulo.Palabras = capitulo.Contenido.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

                _context.Add(capitulo);
                await _context.SaveChangesAsync();

                // actualizar total de palabras
                novela.PalabrasTotales = await _context.Capitulos
                    .Where(c => c.NovelaId == novela.Id)
                    .SumAsync(c => c.Palabras);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Novelas", new { id = novela.Id });
            }

            ViewBag.Novela = novela;
            return View(capitulo);
        }
    }
}
