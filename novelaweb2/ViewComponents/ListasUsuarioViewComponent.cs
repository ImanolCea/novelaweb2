using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Models;

namespace novelaweb2.ViewComponents
{
    public class ListasUsuarioViewComponent : ViewComponent
    {
        private readonly WebNovelasDbContext _context;
        public ListasUsuarioViewComponent(WebNovelasDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return View(Enumerable.Empty<Lista>()); // ✅ Devuelve una lista vacía, nunca null

            var listas = await _context.Listas
                .Where(l => l.UsuarioId == usuarioId)
                .OrderBy(l => l.Nombre)
                .ToListAsync();

            return View(listas);
        }

    }
}
