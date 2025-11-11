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
        private const int PageSize = 8; // número de novelas por página

        public HomeController(ILogger<HomeController> logger, WebNovelasDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var totalNovelas = await _context.Novelas.CountAsync();

            var novelas = await _context.Novelas
                .Include(n => n.Capitulos)
                .OrderByDescending(n => n.FechaPublicacion)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalNovelas / (double)PageSize);

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
