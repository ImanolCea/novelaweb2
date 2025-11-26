using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Models;

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

            // Vista: /Views/Admin/Index.cshtml
            return View();
        }

        // =============================
        // 👥 USUARIOS
        // =============================
        public async Task<IActionResult> Usuarios()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();

            // Vista: /Views/Admin/Usuarios/Index.cshtml
            return View("Usuarios/Index", usuarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var usuario = await _context.Usuarios
                .Include(u => u.Novelas)
                .Include(u => u.Reseñas)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Usuarios));
            }

            try
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Usuario eliminado correctamente.";
            }
            catch
            {
                TempData["Error"] = "No se pudo eliminar el usuario (verifica relaciones).";
            }

            return RedirectToAction(nameof(Usuarios));
        }

        // =============================
        // 📚 NOVELAS (CRUD ADMIN)
        // =============================
        public async Task<IActionResult> Novelas()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var novelas = await _context.Novelas
                .Include(n => n.Autor)
                .Include(n => n.Capitulos)
                .OrderBy(n => n.Titulo)
                .ToListAsync();

            // Vista: /Views/Admin/Novelas/Index.cshtml
            return View("Novelas/Index", novelas);
        }

        // GET: Admin/DetalleNovela/5
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

            // Vista: /Views/Admin/Novelas/Details.cshtml
            return View("Novelas/Details", novela);
        }

        // GET: Admin/EditarNovela/5
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

            // Vista: /Views/Admin/Novelas/Edit.cshtml
            return View("Novelas/Edit", novela);
        }

        // POST: Admin/EditarNovela/5
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
                return View("Novelas/Edit", novela);
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

            return RedirectToAction(nameof(Novelas));
        }

        // GET: Admin/EliminarNovela/5
        public async Task<IActionResult> EliminarNovela(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var novela = await _context.Novelas
                .Include(n => n.Autor)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (novela == null)
                return NotFound();

            // Vista: /Views/Admin/Novelas/Delete.cshtml
            return View("Novelas/Delete", novela);
        }

        // POST: Admin/EliminarNovela/5
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
                return RedirectToAction(nameof(Novelas));
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

            return RedirectToAction(nameof(Novelas));
        }

        // =============================
        // 💬 COMENTARIOS
        // =============================
        public async Task<IActionResult> Comentarios()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var comentarios = await _context.Comentarios
                .Include(c => c.Usuario)
                .Include(c => c.Capitulo)
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();

            // Vista: /Views/Admin/Comentarios/Index.cshtml
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
                return RedirectToAction(nameof(Comentarios));
            }

            _context.Comentarios.Remove(comentario);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Comentario eliminado.";

            return RedirectToAction(nameof(Comentarios));
        }

        // =============================
        // ⭐ RESEÑAS
        // =============================
        public async Task<IActionResult> Resenas()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var reseñas = await _context.Reseñas
                .Include(r => r.Usuario)
                .Include(r => r.Novela)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();

            // Vista: /Views/Admin/Reseñas/Index.cshtml  (carpeta con tilde)
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
                return RedirectToAction(nameof(Resenas));
            }

            _context.Reseñas.Remove(reseña);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Reseña eliminada.";

            return RedirectToAction(nameof(Resenas));
        }

        // =============================
        // 🏷️ ETIQUETAS
        // =============================
        public async Task<IActionResult> Etiquetas()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var etiquetas = await _context.Etiquetas
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            // Vista: /Views/Admin/Etiquetas/Index.cshtml
            return View("Etiquetas/Index", etiquetas);
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
                return RedirectToAction(nameof(Etiquetas));
            }

            _context.Etiquetas.Remove(etiqueta);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Etiqueta eliminada.";

            return RedirectToAction(nameof(Etiquetas));
        }

        // =============================
        // 🧩 ROLES
        // =============================
        public async Task<IActionResult> Roles()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var roles = await _context.Roles
                .OrderBy(r => r.Nombre)
                .ToListAsync();

            // Vista: /Views/Admin/Roles/Index.cshtml
            return View("Roles/Index", roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarRol(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                TempData["Error"] = "Rol no encontrado.";
                return RedirectToAction(nameof(Roles));
            }

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Rol eliminado.";

            return RedirectToAction(nameof(Roles));
        }
    }
}
