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

        // GET: Capituloes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var capitulo = await _context.Capitulos
                .Include(c => c.Novela)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (capitulo == null)
                return NotFound();

            // Cargar capítulos anterior y siguiente
            var anterior = await _context.Capitulos
                .Where(c => c.NovelaId == capitulo.NovelaId && c.NumeroCapitulo < capitulo.NumeroCapitulo)
                .OrderByDescending(c => c.NumeroCapitulo)
                .FirstOrDefaultAsync();

            var siguiente = await _context.Capitulos
                .Where(c => c.NovelaId == capitulo.NovelaId && c.NumeroCapitulo > capitulo.NumeroCapitulo)
                .OrderBy(c => c.NumeroCapitulo)
                .FirstOrDefaultAsync();

            ViewBag.CapituloAnterior = anterior;
            ViewBag.CapituloSiguiente = siguiente;

            return View(capitulo);
        }

        // GET: Capituloes/Create
        public IActionResult Create(int novelaId)
        {
            ViewBag.NovelaId = novelaId;
            return View();
        }

        // POST: Capituloes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Capitulo capitulo)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.NovelaId = capitulo.NovelaId;
                return View(capitulo);
            }

            // ✅ Validar número de capítulo único
            bool existeNumero = await _context.Capitulos
                .AnyAsync(c => c.NovelaId == capitulo.NovelaId && c.NumeroCapitulo == capitulo.NumeroCapitulo);

            if (existeNumero)
            {
                ModelState.AddModelError("NumeroCapitulo", "Ya existe un capítulo con este número.");
                ViewBag.NovelaId = capitulo.NovelaId;
                return View(capitulo);
            }

            // ✅ Validar orden
            var maxCap = await _context.Capitulos
                .Where(c => c.NovelaId == capitulo.NovelaId)
                .MaxAsync(c => (int?)c.NumeroCapitulo) ?? 0;

            if (capitulo.NumeroCapitulo < maxCap + 1)
            {
                ModelState.AddModelError("NumeroCapitulo", $"El número de capítulo debe ser mayor o igual a {maxCap + 1}.");
                ViewBag.NovelaId = capitulo.NovelaId;
                return View(capitulo);
            }

            // Calcular cantidad de palabras
            capitulo.Palabras = capitulo.Contenido?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
            capitulo.FechaPublicacion = DateTime.Now;

            _context.Add(capitulo);
            await _context.SaveChangesAsync();

            // Actualizar palabras totales de la novela
            var novela = await _context.Novelas.FirstOrDefaultAsync(n => n.Id == capitulo.NovelaId);
            if (novela != null)
            {
                novela.PalabrasTotales = await _context.Capitulos
                    .Where(c => c.NovelaId == capitulo.NovelaId)
                    .SumAsync(c => c.Palabras);
                _context.Update(novela);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Capítulo agregado correctamente.";
            return RedirectToAction("Details", "Novelas", new { id = capitulo.NovelaId });
        }
        // GET: Capituloes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var capitulo = await _context.Capitulos.Include(c => c.Novela).FirstOrDefaultAsync(c => c.Id == id);
            if (capitulo == null) return NotFound();

            var usuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            if (usuarioNombre != capitulo.Novela?.Autor?.NombreUsuario)
                return Unauthorized();

            return View(capitulo);
        }

        // POST: Capituloes/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Capitulo capitulo)
        {
            if (id != capitulo.Id) return NotFound();

            var original = await _context.Capitulos.Include(c => c.Novela).FirstOrDefaultAsync(c => c.Id == id);
            if (original == null) return NotFound();

            var usuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            if (usuarioNombre != original.Novela?.Autor?.NombreUsuario)
                return Unauthorized();

            original.Titulo = capitulo.Titulo;
            original.Contenido = capitulo.Contenido;
            original.Palabras = capitulo.Contenido?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Capítulo editado correctamente.";
            return RedirectToAction("Details", new { id });
        }

        // POST: Capituloes/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var capitulo = await _context.Capitulos
                .Include(c => c.Novela)
                .ThenInclude(n => n.Autor)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (capitulo == null)
                return NotFound();

            var usuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            if (usuarioNombre != capitulo.Novela?.Autor?.NombreUsuario)
                return Unauthorized();

            _context.Capitulos.Remove(capitulo);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Capítulo eliminado correctamente.";
            return RedirectToAction("Details", "Novelas", new { id = capitulo.NovelaId });
        }



    }
}
