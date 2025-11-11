using System;
using System.Collections.Generic;

namespace novelaweb2.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Contrasena { get; set; } = null!;

    public DateTime FechaRegistro { get; set; }

    public int RolId { get; set; }

    public string? Bio { get; set; }

    public string? AvatarUrl { get; set; }

    public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public virtual ICollection<Lista> Lista { get; set; } = new List<Lista>();

    public virtual ICollection<Novela> Novelas { get; set; } = new List<Novela>();

    public virtual ICollection<Reseña> Reseñas { get; set; } = new List<Reseña>();

    public virtual Role Rol { get; set; } = null!;

    public virtual ICollection<Seguimiento> Seguimientos { get; set; } = new List<Seguimiento>();

    public virtual ICollection<UserFollow> UserFollowIdSeguidoNavigations { get; set; } = new List<UserFollow>();

    public virtual ICollection<UserFollow> UserFollowIdSeguidorNavigations { get; set; } = new List<UserFollow>();
}
