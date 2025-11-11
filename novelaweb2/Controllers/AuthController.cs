using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using novelaweb2.Models;
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

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string nombreUsuario, string correo, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
            {
                ViewBag.Error = "Todos los campos son obligatorios.";
                return View();
            }

            var existe = await _context.Usuarios.AnyAsync(u => u.Correo == correo || u.NombreUsuario == nombreUsuario);
            if (existe)
            {
                ViewBag.Error = "El nombre de usuario o correo ya está en uso.";
                return View();
            }

            var rolUsuario = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Usuario");
            if (rolUsuario == null)
            {
                rolUsuario = new Role { Nombre = "Usuario" };
                _context.Roles.Add(rolUsuario);
                await _context.SaveChangesAsync();
            }

            var nuevoUsuario = new Usuario
            {
                NombreUsuario = nombreUsuario,
                Correo = correo,
                Contrasena = HashPassword(contrasena),
                FechaRegistro = DateTime.Now,
                RolId = rolUsuario.Id,
                Rol = rolUsuario
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("UsuarioNombre", nuevoUsuario.NombreUsuario);
            HttpContext.Session.SetInt32("UsuarioId", nuevoUsuario.Id);
            HttpContext.Session.SetString("Rol", rolUsuario.Nombre);

            TempData["Success"] = "Registro exitoso. ¡Bienvenido a NovelaWeb!";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string correo, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
            {
                ViewBag.Error = "Correo y contraseña son obligatorios.";
                return View();
            }

            var hashed = HashPassword(contrasena);
            var usuario = await _context.Usuarios.Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == correo && u.Contrasena == hashed);

            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                return View();
            }

            HttpContext.Session.SetString("UsuarioNombre", usuario.NombreUsuario);
            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            HttpContext.Session.SetString("Rol", usuario.Rol?.Nombre ?? "User");

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private string HashPassword(string input)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
