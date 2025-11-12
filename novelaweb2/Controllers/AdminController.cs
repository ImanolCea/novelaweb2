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

        // =========================================================
        // 🔐 MÉTODOS DE VERIFICACIÓN DE ACCESO
        // =========================================================
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

        // =========================================================
        // 🏠 PANEL PRINCIPAL
        // =========================================================
        public IActionResult Index()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;
            return View();
        }

        // =========================================================
        // 👥 CRUD USUARIOS
        // =========================================================
        public async Task<IActionResult> Usuarios()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var usuarios = await _context.Usuarios.Include(u => u.Rol).ToListAsync();
            return View(usuarios);
        }

        public async Task<IActionResult> DetalleUsuario(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        public IActionResult CrearUsuario()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;
            ViewBag.Roles = _context.Roles.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuario(Usuario usuario)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            if (ModelState.IsValid)
            {
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Usuario creado correctamente.";
                return RedirectToAction("Usuarios");
            }

            ViewBag.Roles = _context.Roles.ToList();
            TempData["Error"] = "Datos inválidos.";
            return View(usuario);
        }

        public async Task<IActionResult> EditarUsuario(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            ViewBag.Roles = _context.Roles.ToList();
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(Usuario usuario)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Datos inválidos.";
                return View(usuario);
            }

            _context.Update(usuario);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Usuario actualizado.";
            return RedirectToAction("Usuarios");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("Usuarios");
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Usuario eliminado.";
            return RedirectToAction("Usuarios");
        }

        // =========================================================
        // 📚 CRUD NOVELAS
        // =========================================================
        public async Task<IActionResult> Novelas()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var novelas = await _context.Novelas.Include(n => n.Autor).ToListAsync();
            return View(novelas);
        }

        public async Task<IActionResult> DetalleNovela(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var novela = await _context.Novelas.Include(n => n.Autor).Include(n => n.Capitulos).FirstOrDefaultAsync(n => n.Id == id);
            if (novela == null) return NotFound();

            return View(novela);
        }

        public IActionResult CrearNovela()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;
            ViewBag.Usuarios = _context.Usuarios.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearNovela(Novela novela)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            if (ModelState.IsValid)
            {
                _context.Novelas.Add(novela);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Novela creada correctamente.";
                return RedirectToAction("Novelas");
            }

            ViewBag.Usuarios = _context.Usuarios.ToList();
            TempData["Error"] = "Datos inválidos.";
            return View(novela);
        }

        public async Task<IActionResult> EditarNovela(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var novela = await _context.Novelas.FindAsync(id);
            if (novela == null) return NotFound();

            ViewBag.Usuarios = _context.Usuarios.ToList();
            return View(novela);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarNovela(Novela novela)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Datos inválidos.";
                return View(novela);
            }

            _context.Update(novela);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Novela actualizada.";
            return RedirectToAction("Novelas");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarNovela(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var novela = await _context.Novelas.FindAsync(id);
            if (novela == null)
            {
                TempData["Error"] = "Novela no encontrada.";
                return RedirectToAction("Novelas");
            }

            _context.Novelas.Remove(novela);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Novela eliminada.";
            return RedirectToAction("Novelas");
        }

        // =========================================================
        // 💬 CRUD COMENTARIOS
        // =========================================================
        public async Task<IActionResult> Comentarios()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var comentarios = await _context.Comentarios.Include(c => c.Usuario).Include(c => c.Capitulo).ToListAsync();
            return View(comentarios);
        }

        public async Task<IActionResult> DetalleComentario(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var comentario = await _context.Comentarios.Include(c => c.Usuario).Include(c => c.Capitulo).FirstOrDefaultAsync(c => c.Id == id);
            if (comentario == null) return NotFound();

            return View(comentario);
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

        // =========================================================
        // ⭐ CRUD RESEÑAS
        // =========================================================
        public async Task<IActionResult> Resenas()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var reseñas = await _context.Reseñas.Include(r => r.Usuario).Include(r => r.Novela).ToListAsync();
            return View(reseñas);
        }

        public async Task<IActionResult> DetalleResena(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var reseña = await _context.Reseñas.Include(r => r.Usuario).Include(r => r.Novela).FirstOrDefaultAsync(r => r.Id == id);
            if (reseña == null) return NotFound();

            return View(reseña);
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

        // =========================================================
        // 🏷️ CRUD ETIQUETAS
        // =========================================================
        public async Task<IActionResult> Etiquetas()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var etiquetas = await _context.Etiquetas.OrderBy(e => e.Nombre).ToListAsync();
            return View(etiquetas);
        }

        public async Task<IActionResult> DetalleEtiqueta(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var etiqueta = await _context.Etiquetas.FindAsync(id);
            if (etiqueta == null) return NotFound();

            return View(etiqueta);
        }

        public IActionResult CrearEtiqueta()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearEtiqueta(Etiqueta etiqueta)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            if (ModelState.IsValid)
            {
                _context.Etiquetas.Add(etiqueta);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Etiqueta creada.";
                return RedirectToAction("Etiquetas");
            }

            TempData["Error"] = "Datos inválidos.";
            return View(etiqueta);
        }

        public async Task<IActionResult> EditarEtiqueta(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var etiqueta = await _context.Etiquetas.FindAsync(id);
            if (etiqueta == null) return NotFound();

            return View(etiqueta);
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
                return View(etiqueta);
            }

            _context.Update(etiqueta);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Etiqueta actualizada.";
            return RedirectToAction("Etiquetas");
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

        // =========================================================
        // 🧩 CRUD ROLES
        // =========================================================
        public async Task<IActionResult> Roles()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var roles = await _context.Roles.OrderBy(r => r.Nombre).ToListAsync();
            return View(roles);
        }

        public async Task<IActionResult> DetalleRol(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var rol = await _context.Roles.FindAsync(id);
            if (rol == null) return NotFound();

            return View(rol);
        }

        public IActionResult CrearRol()
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearRol(Role rol)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            if (ModelState.IsValid)
            {
                _context.Roles.Add(rol);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Rol creado.";
                return RedirectToAction("Roles");
            }

            TempData["Error"] = "Datos inválidos.";
            return View(rol);
        }

        public async Task<IActionResult> EditarRol(int id)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            var rol = await _context.Roles.FindAsync(id);
            if (rol == null) return NotFound();

            return View(rol);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRol(Role rol)
        {
            var acceso = VerificarAcceso();
            if (acceso != null) return acceso;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Datos inválidos.";
                return View(rol);
            }

            _context.Update(rol);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Rol actualizado.";
            return RedirectToAction("Roles");
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
                return RedirectToAction("Roles");
            }

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Rol eliminado.";
            return RedirectToAction("Roles");
        }
    }
}
