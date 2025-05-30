using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TryOn.API.Models;

[Table("prendas")]
[Index("CategoriaId", Name = "idx_prendas_categoria")]
[Index("Codigo", Name = "idx_prendas_codigo")]
[Index("Codigo", Name = "prendas_codigo_key", IsUnique = true)]
public partial class Prenda
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("codigo")]
    [StringLength(20)]
    public string Codigo { get; set; } = null!;

    [Column("nombre")]
    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("precio_venta")]
    [Precision(10, 2)]
    public decimal PrecioVenta { get; set; }

    [Column("costo")]
    [Precision(10, 2)]
    public decimal Costo { get; set; }

    [Column("categoria_id")]
    public int? CategoriaId { get; set; }

    [Column("imagen_url")]
    [StringLength(255)]
    public string? ImagenUrl { get; set; }

    [Column("fecha_creacion", TypeName = "timestamp without time zone")]
    public DateTime? FechaCreacion { get; set; }

    [ForeignKey("CategoriaId")]
    [InverseProperty("Prenda")]
    public virtual Categoria? Categoria { get; set; }

    [InverseProperty("Prenda")]
    public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();

    [InverseProperty("Prenda")]
    public virtual ICollection<Promocione> Promociones { get; set; } = new List<Promocione>();
}
