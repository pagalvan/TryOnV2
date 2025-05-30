using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TryOn.API.Models;

[Table("inventario")]
[Index("PrendaId", Name = "idx_inventario_prenda")]
[Index("PrendaId", "Talla", "Color", Name = "idx_inventario_talla_color")]
[Index("PrendaId", "Talla", "Color", Name = "inventario_prenda_id_talla_color_key", IsUnique = true)]
public partial class Inventario
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("prenda_id")]
    public int? PrendaId { get; set; }

    [Column("talla")]
    [StringLength(10)]
    public string Talla { get; set; } = null!;

    [Column("color")]
    [StringLength(50)]
    public string Color { get; set; } = null!;

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("ubicacion")]
    [StringLength(100)]
    public string? Ubicacion { get; set; }

    [InverseProperty("Inventario")]
    public virtual ICollection<DetallesPedido> DetallesPedidos { get; set; } = new List<DetallesPedido>();

    [ForeignKey("PrendaId")]
    [InverseProperty("Inventarios")]
    public virtual Prenda? Prenda { get; set; }
}
