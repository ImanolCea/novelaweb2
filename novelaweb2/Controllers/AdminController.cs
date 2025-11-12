using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Models;

namespace novelaweb2.Controllers
{
    [Route("Admin/[action]")]
    public class AdminController : Controller
    {
        private readonly WebNovelasDbContext _context;

        public AdminController(WebNovelasDbContext context)
        {
            _context = context;
        }

        // =============================
        //  MÉTODOS DE VERIFICACIÓN
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
        // PANEL PRINCIPAL
        // =============================
        [HttpGet]
        [Route("/Admin")]
        public IActionResult Index()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            return View("Index");
        }
        // =============================
        //  CRUD DE ETIQUETAS
        // =============================
        [HttpGet]
        public async Task<IActionResult> Etiquetas()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var etiquetas = await _context.Etiquetas
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            return View("Etiquetas/Index", etiquetas);
        }

        [HttpGet]
        public IActionResult CrearEtiqueta()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            return View("Etiquetas/Create", new Etiqueta());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearEtiqueta(Etiqueta etiqueta)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Los datos ingresados no son válidos.";
                return View("Etiquetas/Create", etiqueta);
            }

            _context.Etiquetas.Add(etiqueta);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Etiqueta creada correctamente.";
            return RedirectToAction(nameof(Etiquetas));
        }

        [HttpGet]
        public async Task<IActionResult> EditarEtiqueta(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var etiqueta = await _context.Etiquetas.FindAsync(id);
            if (etiqueta == null)
            {
                TempData["Error"] = "Etiqueta no encontrada.";
                return RedirectToAction(nameof(Etiquetas));
            }

            return View("Etiquetas/Edit", etiqueta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEtiqueta(Etiqueta etiqueta)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Datos inválidos.";
                return View("Etiquetas/Edit", etiqueta);
            }

            try
            {
                _context.Update(etiqueta);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Etiqueta actualizada correctamente.";
                return RedirectToAction(nameof(Etiquetas));
            }
            catch
            {
                TempData["Error"] = "Error al actualizar la etiqueta.";
                return View("Etiquetas/Edit", etiqueta);
            }
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

            try
            {
                _context.Etiquetas.Remove(etiqueta);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Etiqueta eliminada correctamente.";
            }
            catch
            {
                TempData["Error"] = "No se pudo eliminar la etiqueta.";
            }

            return RedirectToAction(nameof(Etiquetas));
        }

        // =============================
        //  USUARIOS
        // =============================
        [HttpGet]
        public async Task<IActionResult> Usuarios()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();

            return View("Usuarios/Index", usuarios);
        }

        [HttpPost]
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
                TempData["Error"] = "No se pudo eliminar el usuario. Verifica relaciones.";
            }

            return RedirectToAction(nameof(Usuarios));
        }

        // =============================
        //  NOVELAS
        // =============================
        [HttpGet]
        public async Task<IActionResult> Novelas()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var novelas = await _context.Novelas
                .Include(n => n.Autor)
                .OrderBy(n => n.Titulo)
                .ToListAsync();

            return View("Novelas/Index", novelas);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarNovela(int id)
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
        // COMENTARIOS
        // =============================
        [HttpGet]
        public async Task<IActionResult> Comentarios()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var comentarios = await _context.Comentarios
                .Include(c => c.Usuario)
                .Include(c => c.Capitulo)
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();

            return View("Comentarios/Index", comentarios);
        }

        [HttpPost]
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
            TempData["Success"] = "Comentario eliminado correctamente.";

            return RedirectToAction(nameof(Comentarios));
        }

        // =============================
        // ⭐ RESEÑAS
        // =============================
        [HttpGet]
        public async Task<IActionResult> Resenas()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var reseñas = await _context.Reseñas
                .Include(r => r.Usuario)
                .Include(r => r.Novela)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();

            return View("Reseñas/Index", reseñas);
        }

        [HttpPost]
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
            TempData["Success"] = "Reseña eliminada correctamente.";

            return RedirectToAction(nameof(Resenas));
        }

       

        // =============================
        // 🧩 ROLES
        // =============================
        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var roles = await _context.Roles
                .OrderBy(r => r.Nombre)
                .ToListAsync();

            return View("Roles/Index", roles);
        }

        [HttpPost]
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
            TempData["Success"] = "Rol eliminado correctamente.";

            return RedirectToAction(nameof(Roles));
        }
    }
}
