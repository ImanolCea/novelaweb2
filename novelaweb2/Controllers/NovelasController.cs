using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Models;
using novelaweb2.Models.ViewModels;

namespace novelaweb2.Controllers
{
    public class NovelasController : Controller
    {
        private readonly WebNovelasDbContext _context;

        public NovelasController(WebNovelasDbContext context)
        {
            _context = context;
        }

        // =====================================
        // 🔹 REDIRECCIÓN PRINCIPAL
        // =====================================
        [HttpGet]
        public IActionResult Index()
        {
            // Redirige siempre a Series
            return RedirectToAction(nameof(Series));
        }

        // =====================================
        // 🔹 LISTADO GENERAL / SERIES
        // =====================================
        [HttpGet]
        public async Task<IActionResult> Series(string? q, string? genero, string? estado, int? calificacionMin, int page = 1, int pageSize = 30)
        {
            var query = _context.Novelas
                .Include(n => n.Reseñas)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(n => n.Titulo.Contains(q));

            if (!string.IsNullOrWhiteSpace(genero))
                query = query.Where(n => n.Genero == genero);

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(n => n.Estado == estado);

            if (calificacionMin.HasValue && calificacionMin.Value > 0)
                query = query.Where(n => n.Reseñas.Any() &&
                                         n.Reseñas.Average(r => r.Puntuacion) >= calificacionMin.Value);

            var total = await query.CountAsync();

            var novelas = await query
                .OrderBy(n => n.Titulo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(n => n.Autor)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.FiltroQ = q;
            ViewBag.FiltroGenero = genero;
            ViewBag.FiltroEstado = estado;
            ViewBag.FiltroCalificacion = calificacionMin;

            ViewBag.Generos = await _context.Novelas
                .Where(n => n.Genero != null && n.Genero != "")
                .Select(n => n.Genero!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            ViewBag.Estados = new[] { "En curso", "Finalizada", "Hiatus" };

            return View("Series", novelas);
        }

        // =====================================
        // CREAR NOVELA
        // =====================================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Etiquetas = new MultiSelectList(_context.Etiquetas, "Id", "Nombre");
            return View(new NovelaCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NovelaCreateViewModel model)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                TempData["Error"] = "Debes iniciar sesión para crear una novela.";
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Etiquetas = new MultiSelectList(_context.Etiquetas, "Id", "Nombre");
                return View(model);
            }

            if (model.EtiquetasSeleccionadas == null || !model.EtiquetasSeleccionadas.Any())
            {
                ModelState.AddModelError("EtiquetasSeleccionadas", "Debes seleccionar al menos una etiqueta.");
                ViewBag.Etiquetas = new MultiSelectList(_context.Etiquetas, "Id", "Nombre");
                return View(model);
            }

            var novela = new Novela
            {
                Titulo = model.Titulo,
                Sinopsis = model.Sinopsis,
                PortadaUrl = model.PortadaUrl,
                Genero = model.Genero,
                Estado = model.Estado,
                AutorId = usuarioId.Value,
                FechaPublicacion = DateTime.Now
            };

            // Etiquetas seleccionadas
            novela.Etiquetas = await _context.Etiquetas
                .Where(e => model.EtiquetasSeleccionadas.Contains(e.Id))
                .ToListAsync();

            // Guardar novela
            _context.Add(novela);
            await _context.SaveChangesAsync();

            // Crear el primer capítulo
            var capitulo = new Capitulo
            {
                NovelaId = novela.Id,
                NumeroCapitulo = 1,
                Titulo = model.TituloCapitulo,
                Contenido = model.ContenidoCapitulo,
                FechaPublicacion = DateTime.Now,
                Palabras = model.ContenidoCapitulo.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
            };

            _context.Capitulos.Add(capitulo);
            await _context.SaveChangesAsync();

            novela.PalabrasTotales = capitulo.Palabras;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Novela creada correctamente.";
            return RedirectToAction("Details", new { id = novela.Id });
        }

        // =====================================
        // MIS NOVELAS
        // =====================================
        public async Task<IActionResult> MisNovelas()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var novelas = await _context.Novelas
                .Where(n => n.AutorId == usuarioId)
                .Include(n => n.Capitulos)
                .ToListAsync();

            return View(novelas);
        }

        // =====================================
        // DETALLES DE NOVELA
        // =====================================
        [HttpGet]
        public async Task<IActionResult> Details(int? id, int capPage = 1, int capPageSize = 15)
        {
            if (id == null) return NotFound();

            var novela = await _context.Novelas
                .Include(n => n.Autor)
                .Include(n => n.Capitulos)
                .Include(n => n.Reseñas).ThenInclude(r => r.Usuario)
                .Include(n => n.Etiquetas)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (novela == null) return NotFound();

            // Paginado de capítulos
            var totalCaps = novela.Capitulos.Count;
            var caps = novela.Capitulos
                .OrderBy(c => c.NumeroCapitulo)
                .Skip((capPage - 1) * capPageSize)
                .Take(capPageSize)
                .ToList();

            ViewBag.Capitulos = caps;
            ViewBag.CapCurrent = capPage;
            ViewBag.CapTotal = (int)Math.Ceiling(totalCaps / (double)capPageSize);

            return View("Details", novela);
        }
    }
}
