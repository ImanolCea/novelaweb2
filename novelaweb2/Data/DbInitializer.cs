using System.Linq;
using novelaweb2.Models;

namespace novelaweb2.Data
{
    public static class DbInitializer
    {
        private static readonly string[] DefaultRoles = new[] { "Administrador", "Usuario" };

        public static void SeedRoles(WebNovelasDbContext context)
        {
            if (context == null) return;

            var missingRoles = DefaultRoles
                .Where(role => !context.Roles.Any(r => r.Nombre == role))
                .Select(role => new Role { Nombre = role })
                .ToList();

            if (missingRoles.Count == 0)
            {
                return;
            }

            context.Roles.AddRange(missingRoles);
            context.SaveChanges();
        }
    }
}
