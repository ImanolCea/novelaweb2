using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Models;

namespace novelaweb2.Controllers
{
    public class ReseñaController : Controller
    {
        private readonly WebNovelasDbContext _context;

        public ReseñaController(WebNovelasDbContext context)
        {
            _context = context;
        }

        // ===================== CREAR RESEÑA =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int novelaId, int puntuacion, string comentario)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                TempData["Error"] = "Debes iniciar sesión para dejar una reseña.";
                return RedirectToAction("Login", "Auth");
            }

            if (puntuacion < 1 || puntuacion > 5)
            {
                TempData["Error"] = "La puntuación debe estar entre 1 y 5 estrellas.";
                return RedirectToAction("Details", "Novelas", new { id = novelaId });
            }

            if (string.IsNullOrWhiteSpace(comentario))
            {
                TempData["Error"] = "El comentario no puede estar vacío.";
                return RedirectToAction("Details", "Novelas", new { id = novelaId });
            }

            // Verificar si el usuario ya hizo una reseña para esta novela
            var existe = await _context.Reseñas
                .AnyAsync(r => r.UsuarioId == usuarioId && r.NovelaId == novelaId);

            if (existe)
            {
                TempData["Error"] = "Ya has dejado una reseña para esta novela.";
                return RedirectToAction("Details", "Novelas", new { id = novelaId });
            }

            var reseña = new Reseña
            {
                NovelaId = novelaId,
                UsuarioId = usuarioId.Value,
                Puntuacion = (byte)puntuacion,
                Comentario = comentario.Trim(),
                Fecha = DateTime.Now
            };

            _context.Add(reseña);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Reseña publicada correctamente.";
            return RedirectToAction("Details", "Novelas", new { id = novelaId });
        }

        // ===================== ELIMINAR RESEÑA =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var reseña = await _context.Reseñas.FindAsync(id);
            if (reseña == null)
                return NotFound();

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null || reseña.UsuarioId != usuarioId)
            {
                TempData["Error"] = "No tienes permiso para eliminar esta reseña.";
                return RedirectToAction("Details", "Novelas", new { id = reseña.NovelaId });
            }

            _context.Reseñas.Remove(reseña);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Reseña eliminada correctamente.";
            return RedirectToAction("Details", "Novelas", new { id = reseña.NovelaId });
        }

        // ===================== VER RESEÑAS (opcional para administración) =====================
        public async Task<IActionResult> Index()
        {
            var reseñas = _context.Reseñas
                .Include(r => r.Novela)
                .Include(r => r.Usuario);
            return View(await reseñas.ToListAsync());
        }
    }
}
