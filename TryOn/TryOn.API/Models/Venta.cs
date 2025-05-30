using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TryOn.API.Models;

[Table("ventas")]
[Index("FechaVenta", Name = "idx_ventas_fecha")]
[Index("PedidoId", Name = "idx_ventas_pedido")]
[Index("UsuarioId", Name = "idx_ventas_usuario")]
public partial class Venta
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("pedido_id")]
    public int? PedidoId { get; set; }

    [Column("fecha_venta", TypeName = "timestamp without time zone")]
    public DateTime? FechaVenta { get; set; }

    [Column("total")]
    [Precision(10, 2)]
    public decimal Total { get; set; }

    [Column("metodo_pago")]
    [StringLength(50)]
    public string? MetodoPago { get; set; }

    [Column("usuario_id")]
    public int? UsuarioId { get; set; }

    [ForeignKey("PedidoId")]
    [InverseProperty("Venta")]
    public virtual Pedido? Pedido { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("Venta")]
    public virtual Usuario? Usuario { get; set; }
}
