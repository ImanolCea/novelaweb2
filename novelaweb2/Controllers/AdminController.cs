using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Models;
// QuestPDF (para exportar PDF)
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;


namespace novelaweb2.Controllers
{
    public class AdminController : Controller
    {
        private readonly WebNovelasDbContext _context;

        public AdminController(WebNovelasDbContext context)
        {
            _context = context;
        }

        // =============================
        // 🔐 MÉTODOS DE VERIFICACIÓN
        // =============================
        private bool EsAdminOModerador()
        {
            var rol = HttpContext.Session.GetString("Rol");
            return rol == "Administrador" || rol == "Moderador";
        }

        private IActionResult AccesoDenegado()
        {
            TempData["Error"] = "No tienes permiso para acceder al panel de administración.";
            return RedirectToAction("Index", "Home");
        }

        private IActionResult VerificarAcceso()
        {
            if (!EsAdminOModerador())
                return AccesoDenegado();
            return null;
        }

        // =============================
        // 🏠 PANEL PRINCIPAL
        // =============================
        public IActionResult Index()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            return View();
        }

        // =============================
        // 👥 USUARIOS (ADMIN)
        // =============================
        public async Task<IActionResult> Usuarios(string? search, int? rolId, int page = 1, int pageSize = 10)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var query = _context.Usuarios
                .Include(u => u.Rol)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(u =>
                    u.NombreUsuario.ToLower().Contains(s) ||
                    (u.Correo != null && u.Correo.ToLower().Contains(s)));
            }

            if (rolId.HasValue && rolId.Value > 0)
                query = query.Where(u => u.RolId == rolId.Value);

            var total = await query.CountAsync();

            if (pageSize <= 0) pageSize = 10;
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var usuarios = await query
                .OrderBy(u => u.NombreUsuario)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.RolId = rolId;
            ViewBag.Roles = await _context.Roles.OrderBy(r => r.Nombre).ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.TotalPages = totalPages;

            // Vista en /Views/Admin/Usuarios/Index.cshtml
            return View("Usuarios/Index", usuarios);
        }

        // --------- EXPORTAR USUARIOS A EXCEL ----------
        public async Task<IActionResult> ExportUsuariosExcel(string? search, int? rolId)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var query = _context.Usuarios
                .Include(u => u.Rol)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(u =>
                    u.NombreUsuario.ToLower().Contains(s) ||
                    (u.Correo != null && u.Correo.ToLower().Contains(s)));
            }

            if (rolId.HasValue && rolId.Value > 0)
                query = query.Where(u => u.RolId == rolId.Value);

            var usuarios = await query
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Usuarios");

            ws.Cell(1, 1).Value = "ID";
            ws.Cell(1, 2).Value = "Nombre de usuario";
            ws.Cell(1, 3).Value = "Email";
            ws.Cell(1, 4).Value = "Rol";

            var row = 2;
            foreach (var u in usuarios)
            {
                ws.Cell(row, 1).Value = u.Id;
                ws.Cell(row, 2).Value = u.NombreUsuario;
                ws.Cell(row, 3).Value = u.Correo;
                ws.Cell(row, 4).Value = u.Rol?.Nombre ?? "";
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "usuarios.xlsx");
        }

        // --------- EXPORTAR USUARIOS A PDF ----------
        public async Task<IActionResult> ExportUsuariosPdf(string? search, int? rolId)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var query = _context.Usuarios
                .Include(u => u.Rol)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(u =>
                    u.NombreUsuario.ToLower().Contains(s) ||
                    (u.Correo != null && u.Correo.ToLower().Contains(s)));
            }

            if (rolId.HasValue && rolId.Value > 0)
                query = query.Where(u => u.RolId == rolId.Value);

            var usuarios = await query
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.Grey.Darken4);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.White));

                    page.Content().Column(col =>
                    {
                        col.Item()
   .Element(e => e.PaddingBottom(10))
   .Text("Listado de usuarios")
   .FontSize(16).SemiBold()
   .FontColor(Colors.Blue.Medium);


                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40); // ID
                                columns.RelativeColumn(2);  // Nombre
                                columns.RelativeColumn(3);  // Email
                                columns.RelativeColumn(2);  // Rol
                            });

                            // Encabezado
                            table.Header(header =>
                            {
                                header.Cell().Element(CellHeader).Text("ID");
                                header.Cell().Element(CellHeader).Text("Usuario");
                                header.Cell().Element(CellHeader).Text("Email");
                                header.Cell().Element(CellHeader).Text("Rol");
                            });

                            foreach (var u in usuarios)
                            {
                                table.Cell().Element(CellBody).Text(u.Id.ToString());
                                table.Cell().Element(CellBody).Text(u.NombreUsuario);
                                table.Cell().Element(CellBody).Text(u.Correo ?? "");
                                table.Cell().Element(CellBody).Text(u.Rol?.Nombre ?? "");
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", "usuarios.pdf");

            static IContainer CellHeader(IContainer container) =>
                container.PaddingVertical(4).PaddingHorizontal(4)
                    .Background(Colors.Grey.Darken3)
                    .ShowOnce()
                    .DefaultTextStyle(x => x.SemiBold());

            static IContainer CellBody(IContainer container) =>
                container.PaddingVertical(2).PaddingHorizontal(4);
        }


        // ==========================================================
        // 📚 NOVELAS (LISTAR + PAGINAR + BUSCAR + EXPORTAR)
        // ==========================================================
        public async Task<IActionResult> Novelas(string? q, string? estado, int page = 1, int pageSize = 20)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _context.Novelas
                .Include(n => n.Autor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(n =>
                    n.Titulo.Contains(q) ||
                    (n.Genero != null && n.Genero.Contains(q)) ||
                    (n.Autor != null && n.Autor.NombreUsuario.Contains(q)));
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(n => n.Estado == estado);
            }

            var total = await query.CountAsync();

            var novelas = await query
                .OrderBy(n => n.Titulo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = q;
            ViewBag.Estado = estado;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = total;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

            ViewBag.Estados = new[] { "En curso", "Finalizada", "Hiatus" };

            return View("Novelas/Index", novelas);
        }

        public async Task<IActionResult> DetalleNovela(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var novela = await _context.Novelas
                .Include(n => n.Autor)
                .Include(n => n.Capitulos)
                .Include(n => n.Reseñas)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (novela == null)
                return NotFound();

            return View("DetalleNovela", novela);
        }

        public async Task<IActionResult> EditarNovela(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var novela = await _context.Novelas
                .Include(n => n.Autor)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (novela == null)
                return NotFound();

            ViewBag.Autores = await _context.Usuarios
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();

            return View("EditarNovela", novela);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarNovela(int id, Novela novela)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            if (id != novela.Id)
                return NotFound();

            var original = await _context.Novelas.FirstOrDefaultAsync(n => n.Id == id);
            if (original == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Autores = await _context.Usuarios
                    .OrderBy(u => u.NombreUsuario)
                    .ToListAsync();
                return View("EditarNovela", novela);
            }

            original.Titulo = novela.Titulo;
            original.Sinopsis = novela.Sinopsis;
            original.Genero = novela.Genero;
            original.Estado = novela.Estado;
            original.PortadaUrl = novela.PortadaUrl;
            original.AutorId = novela.AutorId;

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Novela actualizada correctamente.";
            }
            catch
            {
                TempData["Error"] = "Error al actualizar la novela.";
            }

            return RedirectToAction("Novelas");
        }

        public async Task<IActionResult> EliminarNovela(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var novela = await _context.Novelas
                .Include(n => n.Autor)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (novela == null)
                return NotFound();

            return View("EliminarNovela", novela);
        }

        [HttpPost, ActionName("EliminarNovela")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarNovelaConfirmado(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var novela = await _context.Novelas
                .Include(n => n.Capitulos)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (novela == null)
            {
                TempData["Error"] = "Novela no encontrada.";
                return RedirectToAction("Novelas");
            }

            try
            {
                _context.Novelas.Remove(novela);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Novela eliminada correctamente.";
            }
            catch
            {
                TempData["Error"] = "Error al eliminar la novela (verifica dependencias).";
            }

            return RedirectToAction("Novelas");
        }

        // Exportar NOVELAS a Excel
        public async Task<IActionResult> ExportarNovelasExcel(string? q, string? estado)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var query = _context.Novelas
                .Include(n => n.Autor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(n =>
                    n.Titulo.Contains(q) ||
                    (n.Genero != null && n.Genero.Contains(q)) ||
                    (n.Autor != null && n.Autor.NombreUsuario.Contains(q)));
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(n => n.Estado == estado);
            }

            var data = await query
                .OrderBy(n => n.Titulo)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id;Titulo;Autor;Genero;Estado");
            foreach (var n in data)
            {
                sb.AppendLine($"{n.Id};{n.Titulo};{n.Autor?.NombreUsuario};{n.Genero};{n.Estado}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "novelas.csv");
        }

        // Exportar NOVELAS a PDF
        public async Task<IActionResult> ExportarNovelasPdf(string? q, string? estado)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var query = _context.Novelas
                .Include(n => n.Autor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(n =>
                    n.Titulo.Contains(q) ||
                    (n.Genero != null && n.Genero.Contains(q)) ||
                    (n.Autor != null && n.Autor.NombreUsuario.Contains(q)));
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(n => n.Estado == estado);
            }

            var data = await query
                .OrderBy(n => n.Titulo)
                .ToListAsync();

            var headers = new[] { "Id", "Título", "Autor", "Género", "Estado" };
            var rows = data
                .Select(n => new[]
                {
                    n.Id.ToString(),
                    n.Titulo,
                    n.Autor?.NombreUsuario ?? "",
                    n.Genero ?? "",
                    n.Estado ?? ""
                })
                .ToList();

            var pdfBytes = GenerarPdfTabla("Listado de novelas", headers, rows);
            return File(pdfBytes, "application/pdf", "novelas.pdf");
        }

        // ==========================================================
        // 💬 COMENTARIOS (LISTAR + PAGINAR + BUSCAR + EXPORTAR)
        // ==========================================================
        public async Task<IActionResult> Comentarios(string? q, int page = 1, int pageSize = 20)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _context.Comentarios
                .Include(c => c.Usuario)
                .Include(c => c.Capitulo)
                .ThenInclude(cap => cap.Novela)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(c =>
                    c.Contenido.Contains(q) ||
                    (c.Usuario != null && c.Usuario.NombreUsuario.Contains(q)) ||
                    (c.Capitulo != null && c.Capitulo.Titulo.Contains(q)) ||
                    (c.Capitulo != null && c.Capitulo.Novela != null && c.Capitulo.Novela.Titulo.Contains(q)));
            }

            var total = await query.CountAsync();

            var comentarios = await query
                .OrderByDescending(c => c.Fecha)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = q;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = total;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

            return View("Comentarios/Index", comentarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarComentario(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var comentario = await _context.Comentarios.FindAsync(id);
            if (comentario == null)
            {
                TempData["Error"] = "Comentario no encontrado.";
                return RedirectToAction("Comentarios");
            }

            _context.Comentarios.Remove(comentario);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Comentario eliminado.";

            return RedirectToAction("Comentarios");
        }

        public async Task<IActionResult> ExportarComentariosExcel(string? q)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var query = _context.Comentarios
                .Include(c => c.Usuario)
                .Include(c => c.Capitulo)
                .ThenInclude(cap => cap.Novela)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(c =>
                    c.Contenido.Contains(q) ||
                    (c.Usuario != null && c.Usuario.NombreUsuario.Contains(q)) ||
                    (c.Capitulo != null && c.Capitulo.Titulo.Contains(q)) ||
                    (c.Capitulo != null && c.Capitulo.Novela != null && c.Capitulo.Novela.Titulo.Contains(q)));
            }

            var data = await query
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id;Usuario;Novela;Capitulo;Fecha;Contenido");
            foreach (var c in data)
            {
                sb.AppendLine($"{c.Id};{c.Usuario?.NombreUsuario};{c.Capitulo?.Novela?.Titulo};{c.Capitulo?.Titulo};{c.Fecha:yyyy-MM-dd HH:mm};\"{c.Contenido}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "comentarios.csv");
        }

        public async Task<IActionResult> ExportarComentariosPdf(string? q)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var query = _context.Comentarios
                .Include(c => c.Usuario)
                .Include(c => c.Capitulo)
                .ThenInclude(cap => cap.Novela)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(c =>
                    c.Contenido.Contains(q) ||
                    (c.Usuario != null && c.Usuario.NombreUsuario.Contains(q)) ||
                    (c.Capitulo != null && c.Capitulo.Titulo.Contains(q)) ||
                    (c.Capitulo != null && c.Capitulo.Novela != null && c.Capitulo.Novela.Titulo.Contains(q)));
            }

            var data = await query
                .OrderByDescending(c => c.Fecha)
                .Take(500) // limitar filas en PDF
                .ToListAsync();

            var headers = new[] { "Id", "Usuario", "Novela", "Capítulo", "Fecha" };
            var rows = data
                .Select(c => new[]
                {
                    c.Id.ToString(),
                    c.Usuario?.NombreUsuario ?? "",
                    c.Capitulo?.Novela?.Titulo ?? "",
                    c.Capitulo?.Titulo ?? "",
                    c.Fecha.ToString("yyyy-MM-dd HH:mm")
                })
                .ToList();

            var pdfBytes = GenerarPdfTabla("Listado de comentarios", headers, rows);
            return File(pdfBytes, "application/pdf", "comentarios.pdf");
        }

        // ==========================================================
        // ⭐ RESEÑAS (LISTAR + PAGINAR + BUSCAR + EXPORTAR)
        // ==========================================================
        public async Task<IActionResult> Resenas(string? q, int page = 1, int pageSize = 20)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _context.Reseñas
                .Include(r => r.Usuario)
                .Include(r => r.Novela)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(r =>
                    r.Comentario.Contains(q) ||
                    (r.Usuario != null && r.Usuario.NombreUsuario.Contains(q)) ||
                    (r.Novela != null && r.Novela.Titulo.Contains(q)));
            }

            var total = await query.CountAsync();

            var reseñas = await query
                .OrderByDescending(r => r.Fecha)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = q;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = total;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

            return View("Reseñas/Index", reseñas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarResena(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var reseña = await _context.Reseñas.FindAsync(id);
            if (reseña == null)
            {
                TempData["Error"] = "Reseña no encontrada.";
                return RedirectToAction("Resenas");
            }

            _context.Reseñas.Remove(reseña);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Reseña eliminada.";

            return RedirectToAction("Resenas");
        }

        public async Task<IActionResult> ExportarResenasExcel(string? q)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var query = _context.Reseñas
                .Include(r => r.Usuario)
                .Include(r => r.Novela)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(r =>
                    r.Comentario.Contains(q) ||
                    (r.Usuario != null && r.Usuario.NombreUsuario.Contains(q)) ||
                    (r.Novela != null && r.Novela.Titulo.Contains(q)));
            }

            var data = await query
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id;Usuario;Novela;Puntuacion;Fecha;Comentario");
            foreach (var r in data)
            {
                sb.AppendLine($"{r.Id};{r.Usuario?.NombreUsuario};{r.Novela?.Titulo};{r.Puntuacion};{r.Fecha:yyyy-MM-dd};\"{r.Comentario}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "resenas.csv");
        }

        public async Task<IActionResult> ExportarResenasPdf(string? q)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var query = _context.Reseñas
                .Include(r => r.Usuario)
                .Include(r => r.Novela)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(r =>
                    r.Comentario.Contains(q) ||
                    (r.Usuario != null && r.Usuario.NombreUsuario.Contains(q)) ||
                    (r.Novela != null && r.Novela.Titulo.Contains(q)));
            }

            var data = await query
                .OrderByDescending(r => r.Fecha)
                .Take(500)
                .ToListAsync();

            var headers = new[] { "Id", "Usuario", "Novela", "Puntuación", "Fecha" };
            var rows = data
                .Select(r => new[]
                {
                    r.Id.ToString(),
                    r.Usuario?.NombreUsuario ?? "",
                    r.Novela?.Titulo ?? "",
                    r.Puntuacion.ToString(),
                    r.Fecha.ToString("yyyy-MM-dd")
                })
                .ToList();

            var pdfBytes = GenerarPdfTabla("Listado de reseñas", headers, rows);
            return File(pdfBytes, "application/pdf", "resenas.pdf");
        }

        // ==========================================================
        // 🏷️ ETIQUETAS (LISTAR + PAGINAR + BUSCAR + EXPORTAR)
        // ==========================================================
        public async Task<IActionResult> Etiquetas(string? q, int page = 1, int pageSize = 20)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _context.Etiquetas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(e => e.Nombre.Contains(q));
            }

            var total = await query.CountAsync();

            var etiquetas = await query
                .OrderBy(e => e.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = q;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = total;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

            return View("Etiquetas/Index",etiquetas);
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarEtiqueta(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var etiqueta = await _context.Etiquetas.FindAsync(id);
            if (etiqueta == null)
            {
                TempData["Error"] = "Etiqueta no encontrada.";
                return RedirectToAction("Etiquetas");
            }

            _context.Etiquetas.Remove(etiqueta);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Etiqueta eliminada.";

            return RedirectToAction("Etiquetas");
        }

        public async Task<IActionResult> ExportarEtiquetasExcel(string? q)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var query = _context.Etiquetas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(e => e.Nombre.Contains(q));
            }

            var data = await query
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id;Nombre");
            foreach (var e in data)
            {
                sb.AppendLine($"{e.Id};{e.Nombre}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "etiquetas.csv");
        }

        public async Task<IActionResult> ExportarEtiquetasPdf(string? q)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var query = _context.Etiquetas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(e => e.Nombre.Contains(q));
            }

            var data = await query
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            var headers = new[] { "Id", "Nombre" };
            var rows = data
                .Select(e => new[]
                {
                    e.Id.ToString(),
                    e.Nombre
                })
                .ToList();

            var pdfBytes = GenerarPdfTabla("Listado de etiquetas", headers, rows);
            return File(pdfBytes, "application/pdf", "etiquetas.pdf");
        }

        // ==========================================================
        // 🧩 ROLES (LISTAR + PAGINAR + BUSCAR + EXPORTAR)
        // ==========================================================
        // =============================
        // 🧩 ROLES (ADMIN)
        // =============================
        public async Task<IActionResult> Roles(string? search, int page = 1, int pageSize = 10)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var query = _context.Roles.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(r => r.Nombre.ToLower().Contains(s));
            }

            var total = await query.CountAsync();
            if (pageSize <= 0) pageSize = 10;
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var roles = await query
                .OrderBy(r => r.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.TotalPages = totalPages;

            // Vista en /Views/Admin/Roles/Index.cshtml
            return View("Roles/Index", roles);
        }

        // --------- EXPORTAR ROLES A EXCEL ----------
        public async Task<IActionResult> ExportRolesExcel(string? search)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var query = _context.Roles.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(r => r.Nombre.ToLower().Contains(s));
            }

            var roles = await query.OrderBy(r => r.Nombre).ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Roles");

            ws.Cell(1, 1).Value = "ID";
            ws.Cell(1, 2).Value = "Nombre";

            var row = 2;
            foreach (var r in roles)
            {
                ws.Cell(row, 1).Value = r.Id;
                ws.Cell(row, 2).Value = r.Nombre;
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "roles.xlsx");
        }

        // --------- EXPORTAR ROLES A PDF ----------
        public async Task<IActionResult> ExportRolesPdf(string? search)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var query = _context.Roles.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(r => r.Nombre.ToLower().Contains(s));
            }

            var roles = await query.OrderBy(r => r.Nombre).ToListAsync();

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.Grey.Darken4);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.White));

                    page.Content().Column(col =>
                    {
                        col.Item()
   .Element(e => e.PaddingBottom(10))
   .Text("Listado de roles")
   .FontSize(16).SemiBold()
   .FontColor(Colors.Blue.Medium);


                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn(3);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellHeader).Text("ID");
                                header.Cell().Element(CellHeader).Text("Nombre");
                            });

                            foreach (var r in roles)
                            {
                                table.Cell().Element(CellBody).Text(r.Id.ToString());
                                table.Cell().Element(CellBody).Text(r.Nombre);
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", "roles.pdf");

            static IContainer CellHeader(IContainer container) =>
                container.PaddingVertical(4).PaddingHorizontal(4)
                    .Background(Colors.Grey.Darken3)
                    .DefaultTextStyle(x => x.SemiBold());

            static IContainer CellBody(IContainer container) =>
                container.PaddingVertical(2).PaddingHorizontal(4);
        }


        // ==========================================================
        // 🧾 MÉTODO COMPARTIDO PARA PDF
        // ==========================================================
        private byte[] GenerarPdfTabla(string titulo, string[] encabezados, List<string[]> filas)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text(titulo)
                            .FontSize(18)
                            .Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                foreach (var _ in encabezados)
                                {
                                    columns.RelativeColumn();
                                }
                            });

                            // Encabezados
                            table.Header(header =>
                            {
                                for (int i = 0; i < encabezados.Length; i++)
                                {
                                    header.Cell()
                                        .Element(c => c.PaddingVertical(4))
                                        .Text(encabezados[i])
                                        .FontSize(10)
                                        .Bold();
                                }
                            });

                            // Filas
                            foreach (var fila in filas)
                            {
                                for (int i = 0; i < encabezados.Length; i++)
                                {
                                    string texto = i < fila.Length ? fila[i] ?? "" : "";
                                    table.Cell()
                                        .Element(c => c.PaddingVertical(2))
                                        .Text(texto)
                                        .FontSize(9);
                                }
                            }
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
