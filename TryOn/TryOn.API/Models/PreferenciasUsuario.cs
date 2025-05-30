using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TryOn.API.Models;

[Table("preferencias_usuario")]
[Index("UsuarioId", Name = "idx_preferencias_usuario")]
public partial class PreferenciasUsuario
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("usuario_id")]
    public int? UsuarioId { get; set; }

    [Column("categoria_id")]
    public int? CategoriaId { get; set; }

    [Column("talla_preferida")]
    [StringLength(10)]
    public string? TallaPreferida { get; set; }

    [Column("color_preferido")]
    [StringLength(50)]
    public string? ColorPreferido { get; set; }

    [ForeignKey("CategoriaId")]
    [InverseProperty("PreferenciasUsuarios")]
    public virtual Categoria? Categoria { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("PreferenciasUsuarios")]
    public virtual Usuario? Usuario { get; set; }
}
