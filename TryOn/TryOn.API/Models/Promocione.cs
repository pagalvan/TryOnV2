using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TryOn.API.Models;

[Table("promociones")]
[Index("CodigoPromocion", Name = "idx_promociones_codigo")]
[Index("FechaInicio", "FechaFin", Name = "idx_promociones_fecha")]
public partial class Promocione
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("titulo")]
    [StringLength(100)]
    public string Titulo { get; set; } = null!;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("porcentaje_descuento")]
    [Precision(5, 2)]
    public decimal PorcentajeDescuento { get; set; }

    [Column("fecha_inicio", TypeName = "timestamp without time zone")]
    public DateTime FechaInicio { get; set; }

    [Column("fecha_fin", TypeName = "timestamp without time zone")]
    public DateTime FechaFin { get; set; }

    [Column("prenda_id")]
    public int? PrendaId { get; set; }

    [Column("categoria_id")]
    public int? CategoriaId { get; set; }

    [Column("codigo_promocion")]
    [StringLength(20)]
    public string CodigoPromocion { get; set; } = null!;

    [Column("activa")]
    public bool? Activa { get; set; }

    [ForeignKey("CategoriaId")]
    [InverseProperty("Promociones")]
    public virtual Categoria? Categoria { get; set; }

    [ForeignKey("PrendaId")]
    [InverseProperty("Promociones")]
    public virtual Prenda? Prenda { get; set; }
}
