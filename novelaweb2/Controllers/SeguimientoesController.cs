using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Models;

namespace novelaweb2.Controllers
{
    public class SeguimientoesController : Controller
    {
        private readonly WebNovelasDbContext _context;

        public SeguimientoesController(WebNovelasDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Seguir(int novelaId)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return RedirectToAction("Login", "Auth");

            var existente = await _context.Seguimientos
                .FirstOrDefaultAsync(s => s.UsuarioId == usuarioId && s.NovelaId == novelaId);

            if (existente == null)
            {
                var seg = new Seguimiento
                {
                    UsuarioId = usuarioId.Value,
                    NovelaId = novelaId,
                    FechaUltimaLectura = DateTime.Now
                };
                _context.Add(seg);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Novelas", new { id = novelaId });
        }

        [HttpPost]
        public async Task<IActionResult> DejarSeguir(int novelaId)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return RedirectToAction("Login", "Auth");

            var seguimiento = await _context.Seguimientos
                .FirstOrDefaultAsync(s => s.UsuarioId == usuarioId && s.NovelaId == novelaId);

            if (seguimiento != null)
            {
                _context.Seguimientos.Remove(seguimiento);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Novelas", new { id = novelaId });
        }

        [HttpPost]
        public async Task<IActionResult> GuardarProgreso(int novelaId, int capituloId)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return RedirectToAction("Login", "Auth");

            var seguimiento = await _context.Seguimientos
                .FirstOrDefaultAsync(s => s.UsuarioId == usuarioId && s.NovelaId == novelaId);

            if (seguimiento != null)
            {
                seguimiento.UltimoCapituloLeidoId = capituloId;
                seguimiento.FechaUltimaLectura = DateTime.Now;
                _context.Update(seguimiento);
            }
            else
            {
                seguimiento = new Seguimiento
                {
                    UsuarioId = usuarioId.Value,
                    NovelaId = novelaId,
                    UltimoCapituloLeidoId = capituloId,
                    FechaUltimaLectura = DateTime.Now
                };
                _context.Add(seguimiento);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Capituloes", new { id = capituloId });
        }
    }
}
