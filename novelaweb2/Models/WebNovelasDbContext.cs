using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace novelaweb2.Models;

public partial class WebNovelasDbContext : DbContext
{
    public WebNovelasDbContext()
    {
    }

    public WebNovelasDbContext(DbContextOptions<WebNovelasDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Capitulo> Capitulos { get; set; }

    public virtual DbSet<Comentario> Comentarios { get; set; }

    public virtual DbSet<Etiqueta> Etiquetas { get; set; }

    public virtual DbSet<Lista> Listas { get; set; }

    public virtual DbSet<ListaNovela> ListaNovelas { get; set; }

    public virtual DbSet<Novela> Novelas { get; set; }

    public virtual DbSet<Reseña> Reseñas { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Seguimiento> Seguimientos { get; set; }

    public virtual DbSet<UserFollow> UserFollows { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Capitulo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Capitulo__3214EC07D1B65080");

            entity.HasIndex(e => e.NovelaId, "IX_Capitulos_NovelaId");

            entity.HasIndex(e => new { e.NovelaId, e.NumeroCapitulo }, "UQ_Capitulos_Novela_Numero").IsUnique();

            entity.Property(e => e.Contenido).IsUnicode(false);
            entity.Property(e => e.FechaPublicacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Titulo)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.Novela).WithMany(p => p.Capitulos)
                .HasForeignKey(d => d.NovelaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Capitulos_Novelas");
        });

        modelBuilder.Entity<Comentario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comentar__3214EC07766C23C6");

            entity.HasIndex(e => e.CapituloId, "IX_Comentarios_CapituloId");

            entity.HasIndex(e => e.UsuarioId, "IX_Comentarios_UsuarioId");

            entity.Property(e => e.Contenido)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Capitulo).WithMany(p => p.Comentarios)
                .HasForeignKey(d => d.CapituloId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comentarios_Capitulos");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Comentarios)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comentarios_Usuarios");
        });

        modelBuilder.Entity<Etiqueta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Etiqueta__3214EC07BC8FB395");

            entity.HasIndex(e => e.Nombre, "UQ__Etiqueta__75E3EFCF45456161").IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Lista>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Listas__3214EC0711320AA6");

            entity.HasIndex(e => e.UsuarioId, "IX_Listas_UsuarioId");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Usuario).WithMany(p => p.Lista)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Listas_Usuarios");
        });

        modelBuilder.Entity<ListaNovela>(entity =>
        {
            entity.HasKey(e => new { e.ListaId, e.NovelaId });

            entity.HasIndex(e => e.ListaId, "IX_ListaNovelas_ListaId");

            entity.Property(e => e.FechaAgregado)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Lista).WithMany(p => p.ListaNovelas)
                .HasForeignKey(d => d.ListaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ListaNovelas_Listas");

            entity.HasOne(d => d.Novela).WithMany(p => p.ListaNovelas)
                .HasForeignKey(d => d.NovelaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ListaNovelas_Novelas");
        });

        modelBuilder.Entity<Novela>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Novelas__3214EC07A02B0D25");

            entity.HasIndex(e => e.AutorId, "IX_Novelas_AutorId");

            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("En curso");
            entity.Property(e => e.FechaPublicacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Genero)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PortadaUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Sinopsis).IsUnicode(false);
            entity.Property(e => e.Titulo)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.Autor).WithMany(p => p.Novelas)
                .HasForeignKey(d => d.AutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Novelas_Usuarios");

            entity.HasMany(d => d.Etiquetas).WithMany(p => p.Novelas)
                .UsingEntity<Dictionary<string, object>>(
                    "NovelaEtiqueta",
                    r => r.HasOne<Etiqueta>().WithMany()
                        .HasForeignKey("EtiquetaId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_NovelaEtiquetas_Etiquetas"),
                    l => l.HasOne<Novela>().WithMany()
                        .HasForeignKey("NovelaId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_NovelaEtiquetas_Novelas"),
                    j =>
                    {
                        j.HasKey("NovelaId", "EtiquetaId");
                        j.ToTable("NovelaEtiquetas");
                        j.HasIndex(new[] { "NovelaId" }, "IX_NovelaEtiquetas_NovelaId");
                    });
        });

        modelBuilder.Entity<Reseña>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reseñas__3214EC071796CB9F");

            entity.HasIndex(e => e.NovelaId, "IX_Resenas_NovelaId");

            entity.HasIndex(e => new { e.NovelaId, e.UsuarioId }, "UQ_Resenas_Novela_Usuario").IsUnique();

            entity.Property(e => e.Comentario)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Novela).WithMany(p => p.Reseñas)
                .HasForeignKey(d => d.NovelaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Resenas_Novelas");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Reseñas)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Resenas_Usuarios");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC0791262D82");

            entity.HasIndex(e => e.Nombre, "UQ__Roles__75E3EFCFB5EEC026").IsUnique();

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Seguimiento>()
     .HasKey(s => new { s.UsuarioId, s.NovelaId }); 

        modelBuilder.Entity<Seguimiento>()
            .HasOne(s => s.Usuario)
            .WithMany(u => u.Seguimientos)
            .HasForeignKey(s => s.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Seguimiento>()
            .HasOne(s => s.Novela)
            .WithMany(n => n.Seguimientos)
            .HasForeignKey(s => s.NovelaId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<UserFollow>(entity =>
        {
            entity.HasKey(e => new { e.IdSeguidor, e.IdSeguido }).HasName("PK__UserFoll__70A1204391833967");

            entity.Property(e => e.FechaSeguimiento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdSeguidoNavigation).WithMany(p => p.UserFollowIdSeguidoNavigations)
                .HasForeignKey(d => d.IdSeguido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserFollows_Seguido");

            entity.HasOne(d => d.IdSeguidorNavigation).WithMany(p => p.UserFollowIdSeguidorNavigations)
                .HasForeignKey(d => d.IdSeguidor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserFollows_Seguidor");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC074C1A65F8");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__60695A19A92D75BB").IsUnique();

            entity.HasIndex(e => e.NombreUsuario, "UQ__Usuarios__6B0F5AE0FBBA6C13").IsUnique();

            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Bio)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
