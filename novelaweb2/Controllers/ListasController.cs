using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Models;

namespace novelaweb2.Controllers
{
    public class ListasController : Controller
    {
        private readonly WebNovelasDbContext _context;

        public ListasController(WebNovelasDbContext context)
        {
            _context = context;
        }

        // ====================== MIS LISTAS ======================
        public async Task<IActionResult> MisListas()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var listas = await _context.Listas
                .Where(l => l.UsuarioId == usuarioId)
                .Include(l => l.ListaNovelas).ThenInclude(ln => ln.Novela)
                .OrderByDescending(l => l.FechaCreacion)
                .ToListAsync();

            return View(listas);
        }

        // ====================== CREAR LISTA ======================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion")] Lista lista)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                lista.UsuarioId = usuarioId.Value;
                lista.FechaCreacion = DateTime.Now;
                _context.Add(lista);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Lista creada correctamente.";
                return RedirectToAction(nameof(MisListas));
            }
            return View(lista);
        }

        // ====================== DETALLES DE UNA LISTA ======================
        public async Task<IActionResult> Details(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var lista = await _context.Listas
                .Include(l => l.ListaNovelas)
                .ThenInclude(ln => ln.Novela)
                .FirstOrDefaultAsync(l => l.Id == id && l.UsuarioId == usuarioId);

            if (lista == null) return NotFound();

            return View(lista);
        }

        // ====================== ELIMINAR LISTA ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var lista = await _context.Listas.FirstOrDefaultAsync(l => l.Id == id && l.UsuarioId == usuarioId);
            if (lista == null) return NotFound();

            _context.Listas.Remove(lista);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Lista eliminada correctamente.";
            return RedirectToAction(nameof(MisListas));
        }

        // ====================== AÑADIR NOVELA ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarNovela(int listaId, int novelaId)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var lista = await _context.Listas
                .FirstOrDefaultAsync(l => l.Id == listaId && l.UsuarioId == usuarioId);
            if (lista == null) return NotFound();

            bool existe = await _context.ListaNovelas
                .AnyAsync(ln => ln.ListaId == listaId && ln.NovelaId == novelaId);

            if (!existe)
            {
                var nueva = new ListaNovela
                {
                    ListaId = listaId,
                    NovelaId = novelaId,
                    FechaAgregado = DateTime.Now
                };
                _context.ListaNovelas.Add(nueva);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Novela añadida a la lista.";
            return RedirectToAction("Details", "Novelas", new { id = novelaId });
        }

        // ====================== ELIMINAR NOVELA DE LISTA ======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarNovela(int listaId, int novelaId)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Auth");

            var ln = await _context.ListaNovelas
                .Include(x => x.Lista)
                .FirstOrDefaultAsync(x => x.ListaId == listaId && x.NovelaId == novelaId && x.Lista.UsuarioId == usuarioId);

            if (ln != null)
            {
                _context.ListaNovelas.Remove(ln);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Novela eliminada de la lista.";
            return RedirectToAction("Details", new { id = listaId });
        }
    }
}
