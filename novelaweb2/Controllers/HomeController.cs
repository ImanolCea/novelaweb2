using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Models;
using System.Diagnostics;

namespace novelaweb2.Controllers
{
    public class HomeController : Controller
    {
        private readonly WebNovelasDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private const int PageSize = 30; // ✅ 30 por página

        public HomeController(ILogger<HomeController> logger, WebNovelasDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Última actualización = Max(fecha cap) o fecha publicación si no tiene caps
        public async Task<IActionResult> Index(int page = 1)
        {
            var baseQuery = _context.Novelas
                .Select(n => new
                {
                    Novela = n,
                    UltimaActualizacion = n.Capitulos.Any()
                        ? n.Capitulos.Max(c => c.FechaPublicacion)
                        : n.FechaPublicacion
                });

            var total = await baseQuery.CountAsync();

            var novelas = await baseQuery
                .OrderByDescending(x => x.UltimaActualizacion)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(x => x.Novela)
                .Include(n => n.Capitulos)
                .ToListAsync();

            // “Destacadas”: las 5 con más reseñas (placeholder simple)
            var destacadas = await _context.Novelas
                .OrderByDescending(n => n.Reseñas.Count)
                .Take(5)
                .ToListAsync();

            ViewBag.Destacadas = destacadas;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)PageSize);
            return View(novelas);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
