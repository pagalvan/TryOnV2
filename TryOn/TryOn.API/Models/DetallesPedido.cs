using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TryOn.API.Models;

[Table("detalles_pedido")]
[Index("InventarioId", Name = "idx_detalles_inventario")]
[Index("PedidoId", Name = "idx_detalles_pedido")]
public partial class DetallesPedido
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("pedido_id")]
    public int? PedidoId { get; set; }

    [Column("inventario_id")]
    public int? InventarioId { get; set; }

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("precio_unitario")]
    [Precision(10, 2)]
    public decimal PrecioUnitario { get; set; }

    [Column("subtotal")]
    [Precision(10, 2)]
    public decimal Subtotal { get; set; }

    [ForeignKey("InventarioId")]
    [InverseProperty("DetallesPedidos")]
    public virtual Inventario? Inventario { get; set; }

    [ForeignKey("PedidoId")]
    [InverseProperty("DetallesPedidos")]
    public virtual Pedido? Pedido { get; set; }
}
