using Microsoft.AspNetCore.Http;

namespace novelaweb2.Helpers
{
    public static class RoleHelper
    {
        public static bool EsAdmin(this ISession session)
        {
            return session.GetString("Rol") == "Administrador";
        }

        public static bool EsModerador(this ISession session)
        {
            return session.GetString("Rol") == "Moderador";
        }

        public static bool EsAdminOModerador(this ISession session)
        {
            var rol = session.GetString("Rol");
            return rol == "Administrador" || rol == "Moderador";
        }
    }
}
