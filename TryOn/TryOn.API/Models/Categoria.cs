using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TryOn.API.Models;

[Table("categorias")]
public partial class Categoria
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    [StringLength(50)]
    public string Nombre { get; set; } = null!;

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [InverseProperty("Categoria")]
    public virtual ICollection<PreferenciasUsuario> PreferenciasUsuarios { get; set; } = new List<PreferenciasUsuario>();

    [InverseProperty("Categoria")]
    public virtual ICollection<Prenda> Prenda { get; set; } = new List<Prenda>();

    [InverseProperty("Categoria")]
    public virtual ICollection<Promocione> Promociones { get; set; } = new List<Promocione>();
}
