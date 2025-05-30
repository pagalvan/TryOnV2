using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TryOn.API.Models;

[Table("pedidos")]
[Index("Estado", Name = "idx_pedidos_estado")]
[Index("FechaPedido", Name = "idx_pedidos_fecha")]
[Index("UsuarioId", Name = "idx_pedidos_usuario")]
public partial class Pedido
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("usuario_id")]
    public int? UsuarioId { get; set; }

    [Column("fecha_pedido", TypeName = "timestamp without time zone")]
    public DateTime? FechaPedido { get; set; }

    [Column("estado")]
    [StringLength(20)]
    public string Estado { get; set; } = null!;

    [Column("total")]
    [Precision(10, 2)]
    public decimal Total { get; set; }

    [Column("direccion_envio")]
    public string? DireccionEnvio { get; set; }

    [Column("metodo_pago")]
    [StringLength(50)]
    public string? MetodoPago { get; set; }

    [InverseProperty("Pedido")]
    public virtual ICollection<DetallesPedido> DetallesPedidos { get; set; } = new List<DetallesPedido>();

    [ForeignKey("UsuarioId")]
    [InverseProperty("Pedidos")]
    public virtual Usuario? Usuario { get; set; }

    [InverseProperty("Pedido")]
    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
