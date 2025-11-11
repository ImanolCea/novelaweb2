using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Models;

namespace novelaweb2.Controllers
{
    public class ComentariosController : Controller
    {
        private readonly WebNovelasDbContext _context;

        public ComentariosController(WebNovelasDbContext context)
        {
            _context = context;
        }

        // GET: Comentarios (solo para administración o debug)
        public async Task<IActionResult> Index()
        {
            var comentarios = _context.Comentarios
                .Include(c => c.Usuario)
                .Include(c => c.Capitulo);
            return View(await comentarios.ToListAsync());
        }

        // ===================== CREAR COMENTARIO =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int capituloId, string contenido)
        {
            // Verificar sesión
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
            {
                TempData["Error"] = "Debes iniciar sesión para comentar.";
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrWhiteSpace(contenido))
            {
                TempData["Error"] = "El comentario no puede estar vacío.";
                return RedirectToAction("Details", "Capituloes", new { id = capituloId });
            }

            var comentario = new Comentario
            {
                CapituloId = capituloId,
                UsuarioId = usuarioId.Value,
                Contenido = contenido.Trim(),
                Fecha = DateTime.Now
            };

            _context.Add(comentario);
            await _context.SaveChangesAsync();

            // Redirigir al capítulo correspondiente
            return RedirectToAction("Details", "Capituloes", new { id = capituloId });
        }

        // ===================== ELIMINAR COMENTARIO =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var comentario = await _context.Comentarios.FindAsync(id);
            if (comentario == null)
            {
                return NotFound();
            }

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null || comentario.UsuarioId != usuarioId)
            {
                TempData["Error"] = "No tienes permiso para eliminar este comentario.";
                return RedirectToAction("Details", "Capituloes", new { id = comentario.CapituloId });
            }

            _context.Comentarios.Remove(comentario);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Comentario eliminado correctamente.";
            return RedirectToAction("Details", "Capituloes", new { id = comentario.CapituloId });
        }
    }
}
