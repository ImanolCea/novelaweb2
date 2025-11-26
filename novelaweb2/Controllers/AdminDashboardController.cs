using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;

namespace novelaweb2.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly WebNovelasDbContext _context;

        public AdminDashboardController(WebNovelasDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // ====== Verificación de admin / moderador ======
        private bool EsAdminOModerador()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Administrador" || rol == "Moderador";
        }

        private IActionResult AccesoDenegado()
        {
            TempData["Error"] = "No tienes permiso para acceder al dashboard.";
            return RedirectToAction("Index", "Home");
        }

        // ====== DASHBOARD ======
        public async Task<IActionResult> Index()
        {
            if (!EsAdminOModerador())
                return AccesoDenegado();

            var hoy = DateTime.Now;
            var inicioHoy = hoy.Date;
            var hace7Dias = hoy.AddDays(-7);

            // MÉTRICAS
            ViewBag.TotalUsuarios = await _context.Usuarios.CountAsync();
            ViewBag.TotalNovelas = await _context.Novelas.CountAsync();
            ViewBag.TotalCapitulos = await _context.Capitulos.CountAsync();
            ViewBag.TotalComentarios = await _context.Comentarios.CountAsync();

            ViewBag.UsuariosActivosHoy = await _context.Usuarios
                .CountAsync(u => u.FechaRegistro >= inicioHoy);

            ViewBag.NovelasUltimos7Dias = await _context.Novelas
                .CountAsync(n => n.FechaPublicacion >= hace7Dias);

            ViewBag.ComentariosHoy = await _context.Comentarios
                .CountAsync(c => c.Fecha >= inicioHoy);

            ViewBag.ResenasUltimos7Dias = await _context.Reseñas
                .CountAsync(r => r.Fecha >= hace7Dias);

            // CRECIMIENTO MENSUAL (últimos 6 meses)
            var inicioPeriodo = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-5);

            var crecimiento = await _context.Novelas
                .Where(n => n.FechaPublicacion >= inicioPeriodo)
                .GroupBy(n => new { n.FechaPublicacion.Year, n.FechaPublicacion.Month })
                .Select(g => new
                {
                    año = g.Key.Year,
                    mes = g.Key.Month,
                    cantidad = g.Count()
                })
                .OrderBy(g => g.año).ThenBy(g => g.mes)
                .ToListAsync();

            ViewBag.CrecimientoMensual = crecimiento;

            // ESTADOS DE NOVELAS
            var estados = await _context.Novelas
                .GroupBy(n => n.Estado ?? "Sin estado")
                .Select(g => new
                {
                    estado = g.Key,
                    cantidad = g.Count()
                })
                .ToListAsync();

            ViewBag.EstadosNovelas = estados;

            // TOP 5 NOVELAS MÁS LEÍDAS (por nº de capítulos)
            var topNovelas = await _context.Novelas
                .Include(n => n.Capitulos)
                .OrderByDescending(n => n.Capitulos.Count)
                .Take(5)
                .ToListAsync();

            ViewBag.TopNovelas = topNovelas;

            // USUARIOS RECIENTES
            var usuariosRecientes = await _context.Usuarios
                .OrderByDescending(u => u.FechaRegistro)
                .Take(8)
                .ToListAsync();

            ViewBag.Online = usuariosRecientes;

            return View();
        }

        // ====== EXPORTAR A "EXCEL" (CSV) ======
        public async Task<IActionResult> ExportarExcel()
        {
            if (!EsAdminOModerador())
                return AccesoDenegado();

            var hoy = DateTime.Now;
            var inicioHoy = hoy.Date;
            var hace7Dias = hoy.AddDays(-7);

            int totalUsuarios = await _context.Usuarios.CountAsync();
            int totalNovelas = await _context.Novelas.CountAsync();
            int totalCapitulos = await _context.Capitulos.CountAsync();
            int totalComentarios = await _context.Comentarios.CountAsync();
            int usuariosActivosHoy = await _context.Usuarios.CountAsync(u => u.FechaRegistro >= inicioHoy);
            int novelasUltimos7 = await _context.Novelas.CountAsync(n => n.FechaPublicacion >= hace7Dias);
            int comentariosHoy = await _context.Comentarios.CountAsync(c => c.Fecha >= inicioHoy);
            int resenasUltimos7 = await _context.Reseñas.CountAsync(r => r.Fecha >= hace7Dias);

            var sb = new StringBuilder();
            sb.AppendLine("Métrica,Valor");
            sb.AppendLine($"Total usuarios,{totalUsuarios}");
            sb.AppendLine($"Total novelas,{totalNovelas}");
            sb.AppendLine($"Total capítulos,{totalCapitulos}");
            sb.AppendLine($"Total comentarios,{totalComentarios}");
            sb.AppendLine($"Usuarios activos hoy,{usuariosActivosHoy}");
            sb.AppendLine($"Novelas creadas últimos 7 días,{novelasUltimos7}");
            sb.AppendLine($"Comentarios de hoy,{comentariosHoy}");
            sb.AppendLine($"Reseñas últimos 7 días,{resenasUltimos7}");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "dashboard_webnovelas.csv");
        }

        // ====== EXPORTAR A PDF (QuestPDF) ======
        public async Task<IActionResult> ExportarPdf()
        {
            if (!EsAdminOModerador())
                return AccesoDenegado();

            var hoy = DateTime.Now;
            var inicioHoy = hoy.Date;
            var hace7Dias = hoy.AddDays(-7);

            int totalUsuarios = await _context.Usuarios.CountAsync();
            int totalNovelas = await _context.Novelas.CountAsync();
            int totalCapitulos = await _context.Capitulos.CountAsync();
            int totalComentarios = await _context.Comentarios.CountAsync();
            int usuariosActivosHoy = await _context.Usuarios.CountAsync(u => u.FechaRegistro >= inicioHoy);
            int novelasUltimos7 = await _context.Novelas.CountAsync(n => n.FechaPublicacion >= hace7Dias);
            int comentariosHoy = await _context.Comentarios.CountAsync(c => c.Fecha >= inicioHoy);
            int resenasUltimos7 = await _context.Reseñas.CountAsync(r => r.Fecha >= hace7Dias);

            var topNovelas = await _context.Novelas
                .Include(n => n.Capitulos)
                .OrderByDescending(n => n.Capitulos.Count)
                .Take(5)
                .ToListAsync();

            byte[] pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Grey.Darken3));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Reporte Dashboard - Web Novelas")
                                .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"Generado: {hoy:dd/MM/yyyy HH:mm}");
                        });
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(15);

                        // Métricas
                        col.Item().Text("Métricas generales").FontSize(14).SemiBold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(220);
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("Métrica");
                                header.Cell().Element(HeaderCell).Text("Valor");
                            });

                            void Row(string nombre, string valor)
                            {
                                table.Cell().Element(NormalCell).Text(nombre);
                                table.Cell().Element(NormalCell).Text(valor);
                            }

                            Row("Total usuarios", totalUsuarios.ToString());
                            Row("Total novelas", totalNovelas.ToString());
                            Row("Total capítulos", totalCapitulos.ToString());
                            Row("Total comentarios", totalComentarios.ToString());
                            Row("Usuarios activos hoy", usuariosActivosHoy.ToString());
                            Row("Novelas creadas últimos 7 días", novelasUltimos7.ToString());
                            Row("Comentarios de hoy", comentariosHoy.ToString());
                            Row("Reseñas últimos 7 días", resenasUltimos7.ToString());
                        });

                        // Top novelas
                        // Top novelas
                        col.Item()
                            .PaddingTop(10)
                            .Text("Top 5 novelas más leídas")
                            .FontSize(14)
                            .SemiBold();


                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn();
                                columns.ConstantColumn(90);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("#");
                                header.Cell().Element(HeaderCell).Text("Novela");
                                header.Cell().Element(HeaderCell).Text("Capítulos");
                            });

                            int i = 1;
                            foreach (var n in topNovelas)
                            {
                                table.Cell().Element(NormalCell).Text(i.ToString());
                                table.Cell().Element(NormalCell).Text(n.Titulo);
                                table.Cell().Element(NormalCell).Text(n.Capitulos.Count.ToString());
                                i++;
                            }
                        });

                        // Nota final
                        col.Item().Text("");
                        col.Item().Text("Reporte generado automáticamente por el panel de administración de Web Novelas.")
                            .FontSize(9);
                    });

                    static IContainer HeaderCell(IContainer container) =>
                        container.Padding(4).Background(Colors.Grey.Lighten3)
                            .BorderBottom(1).BorderColor(Colors.Grey.Darken2);

                    static IContainer NormalCell(IContainer container) =>
                        container.Padding(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "dashboard_webnovelas.pdf");
        }
    }
}
