using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Models;
using novelaweb2.Models.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace novelaweb2.Controllers
{
    public class AuthController : Controller
    {
        private readonly WebNovelasDbContext _context;

        public AuthController(WebNovelasDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 🔹 REGISTRO
        // =====================================================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool existe = await _context.Usuarios.AnyAsync(u =>
                u.Correo == model.Correo || u.NombreUsuario == model.NombreUsuario);

            if (existe)
            {
                ViewBag.Error = "El nombre de usuario o correo ya está en uso.";
                return View(model);
            }

            var rolUsuario = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "AutorLector");
            if (rolUsuario == null)
            {
                ViewBag.Error = "No se encontró el rol 'AutorLector'. Contacte al administrador.";
                return View(model);
            }

            var nuevoUsuario = new Usuario
            {
                NombreUsuario = model.NombreUsuario,
                Correo = model.Correo,
                Contrasena = HashPassword(model.Contrasena),
                FechaRegistro = DateTime.Now,
                RolId = rolUsuario.Id
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Registro exitoso. Ahora inicia sesión.";
            return RedirectToAction("Login");
        }

        // =====================================================
        // 🔹 LOGIN
        // =====================================================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string correo, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
            {
                ViewBag.Error = "Debe ingresar correo y contraseña.";
                return View();
            }

            var hashed = HashPassword(contrasena);
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == correo && u.Contrasena == hashed);

            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                return View();
            }

            // Guardar sesión
            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            HttpContext.Session.SetString("UsuarioNombre", usuario.NombreUsuario);
            HttpContext.Session.SetString("Rol", usuario.Rol?.Nombre ?? "Usuario");

            // Redirigir al área de bookmarks
            return RedirectToAction("MisBookmarks", "Seguimientoes");
        }

        // =====================================================
        // 🔹 LOGOUT
        // =====================================================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // =====================================================
        // 🔹 HASH DE CONTRASEÑA
        // =====================================================
        private string HashPassword(string input)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
