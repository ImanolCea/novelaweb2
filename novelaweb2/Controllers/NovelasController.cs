using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Infrastructure;
using novelaweb2.Models;

namespace novelaweb2.Controllers
{
    public class NovelasController : Controller
    {
        private readonly WebNovelasDbContext _context;
        private const int PageSize = 20;

        public NovelasController(WebNovelasDbContext context)
        {
            _context = context;
        }

        // GET: Novelas
        public async Task<IActionResult> Index(string? search, int? etiquetaId, int page = 1)
        {
            if (page < 1) page = 1;

            var query = _context.Novelas
                .AsNoTracking()
                .Include(n => n.Autor)
                .Include(n => n.EtiquetaNovelas)
                    .ThenInclude(en => en.Etiqueta)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(n => EF.Functions.Like(n.Titulo, $"%{term}%")
                    || (n.Sinopsis != null && EF.Functions.Like(n.Sinopsis, $"%{term}%"))
                    || (n.Autor != null && EF.Functions.Like(n.Autor.NombreUsuario, $"%{term}%")));
            }

            if (etiquetaId.HasValue)
            {
                query = query.Where(n => n.EtiquetaNovelas.Any(en => en.EtiquetaId == etiquetaId.Value));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var novelas = await query
                .OrderByDescending(n => n.FechaPublicacion)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.EtiquetaId = etiquetaId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Etiquetas = await _context.Etiquetas
                .AsNoTracking()
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            return View(novelas);
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
        [AdminAuthorize]
        public IActionResult Create()
        {
            ViewData["Etiquetas"] = new MultiSelectList(_context.Etiquetas, "Id", "Nombre");
            ViewData["Autores"] = new SelectList(_context.Usuarios.AsNoTracking().OrderBy(u => u.NombreUsuario), "Id", "NombreUsuario");
            return View();
        }

        // POST: Novelas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
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

            ViewData["Etiquetas"] = new MultiSelectList(_context.Etiquetas, "Id", "Nombre", etiquetasSeleccionadas);
            ViewData["Autores"] = new SelectList(_context.Usuarios.AsNoTracking().OrderBy(u => u.NombreUsuario), "Id", "NombreUsuario");
            return View(novela);
        }

        [AdminAuthorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var novela = await _context.Novelas
                .Include(n => n.EtiquetaNovelas)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (novela == null) return NotFound();

            ViewData["Etiquetas"] = new MultiSelectList(_context.Etiquetas, "Id", "Nombre",
                novela.EtiquetaNovelas.Select(e => e.EtiquetaId));
            ViewData["Autores"] = new SelectList(_context.Usuarios.AsNoTracking().OrderBy(u => u.NombreUsuario), "Id", "NombreUsuario", novela.AutorId);
            return View(novela);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public async Task<IActionResult> Edit(int id, Novela novela, int[] etiquetasSeleccionadas)
        {
            if (id != novela.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(novela);
                    await _context.SaveChangesAsync();

                    var existentes = await _context.EtiquetaNovelas
                        .Where(en => en.NovelaId == novela.Id)
                        .ToListAsync();

                    _context.EtiquetaNovelas.RemoveRange(existentes);

                    foreach (var etId in etiquetasSeleccionadas.Distinct())
                    {
                        _context.EtiquetaNovelas.Add(new EtiquetaNovela
                        {
                            NovelaId = novela.Id,
                            EtiquetaId = etId
                        });
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NovelaExists(novela.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["Etiquetas"] = new MultiSelectList(_context.Etiquetas, "Id", "Nombre", etiquetasSeleccionadas);
            ViewData["Autores"] = new SelectList(_context.Usuarios.AsNoTracking().OrderBy(u => u.NombreUsuario), "Id", "NombreUsuario", novela.AutorId);
            return View(novela);
        }

        [AdminAuthorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var novela = await _context.Novelas
                .AsNoTracking()
                .Include(n => n.Autor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (novela == null) return NotFound();

            return View(novela);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var novela = await _context.Novelas.FindAsync(id);
            if (novela != null)
            {
                _context.Novelas.Remove(novela);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool NovelaExists(int id) => _context.Novelas.Any(e => e.Id == id);
    }
}
