using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TryOn.API.DTOs;

namespace TryOn.API.Models;

public partial class TryOnDbContext : DbContext
{
    public TryOnDbContext()
    {
    }

    public TryOnDbContext(DbContextOptions<TryOnDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<DetallesPedido> DetallesPedidos { get; set; }

    public virtual DbSet<Inventario> Inventarios { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<PreferenciasUsuario> PreferenciasUsuarios { get; set; }

    public virtual DbSet<Prenda> Prendas { get; set; }

    public virtual DbSet<Promocione> Promociones { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Venta> Ventas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=tryon;Username=postgres;Password=upctryon");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categorias_pkey");
        });

        modelBuilder.Entity<DetallesPedido>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("detalles_pedido_pkey");

            entity.HasOne(d => d.Inventario).WithMany(p => p.DetallesPedidos)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("detalles_pedido_inventario_id_fkey");

            entity.HasOne(d => d.Pedido).WithMany(p => p.DetallesPedidos).HasConstraintName("detalles_pedido_pedido_id_fkey");
        });

        modelBuilder.Entity<Inventario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("inventario_pkey");

            entity.HasIndex(e => e.Cantidad, "idx_inventario_bajo_stock").HasFilter("(cantidad <= 5)");

            entity.Property(e => e.Cantidad).HasDefaultValue(0);

            entity.HasOne(d => d.Prenda).WithMany(p => p.Inventarios)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("inventario_prenda_id_fkey");
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pedidos_pkey");

            entity.Property(e => e.FechaPedido).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Pedidos).HasConstraintName("pedidos_usuario_id_fkey");
        });

        modelBuilder.Entity<PreferenciasUsuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("preferencias_usuario_pkey");

            entity.HasOne(d => d.Categoria).WithMany(p => p.PreferenciasUsuarios).HasConstraintName("preferencias_usuario_categoria_id_fkey");

            entity.HasOne(d => d.Usuario).WithMany(p => p.PreferenciasUsuarios).HasConstraintName("preferencias_usuario_usuario_id_fkey");
        });

        modelBuilder.Entity<Prenda>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prendas_pkey");

            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Prenda).HasConstraintName("prendas_categoria_id_fkey");

        });

        modelBuilder.Entity<Promocione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("promociones_pkey");

            entity.HasIndex(e => e.Activa, "idx_promociones_activa").HasFilter("(activa = true)");

            entity.Property(e => e.Activa).HasDefaultValue(true);

            entity.HasOne(d => d.Categoria).WithMany(p => p.Promociones).HasConstraintName("promociones_categoria_id_fkey");

            entity.HasOne(d => d.Prenda).WithMany(p => p.Promociones)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("promociones_prenda_id_fkey");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("usuarios_pkey");

            entity.Property(e => e.EsAdmin).HasDefaultValue(false);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ventas_pkey");

            entity.Property(e => e.FechaVenta).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Pedido).WithMany(p => p.Venta).HasConstraintName("ventas_pedido_id_fkey");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Venta).HasConstraintName("ventas_usuario_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

public DbSet<TryOn.API.DTOs.CategoriaDTO> CategoriaDTO { get; set; } = default!;
}
