using Microsoft.AspNetCore.Mvc;
using TryOn.API.DTOs;
using TryOn.BLL;
using System.Linq;

namespace TryOn.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventarioController : ControllerBase
    {
        private readonly InventarioService _inventarioService;

        public InventarioController(InventarioService inventarioService)
        {
            _inventarioService = inventarioService;
        }

        // GET: api/Inventario
        [HttpGet]
        public IActionResult GetInventario([FromQuery] int? prendaId = null, [FromQuery] string talla = null, [FromQuery] string color = null)
        {
            try
            {
                var inventario = _inventarioService.GetAll();

                // Filtrar por prendaId si se especifica
                if (prendaId.HasValue)
                {
                    inventario = inventario.Where(i => i.PrendaId == prendaId.Value);
                }

                // Filtrar por talla si se especifica
                if (!string.IsNullOrEmpty(talla))
                {
                    inventario = inventario.Where(i => i.Talla.Equals(talla, StringComparison.OrdinalIgnoreCase));
                }

                // Filtrar por color si se especifica
                if (!string.IsNullOrEmpty(color))
                {
                    inventario = inventario.Where(i => i.Color.Equals(color, StringComparison.OrdinalIgnoreCase));
                }

                var inventarioDTO = inventario.Select(i => new InventarioDTO
                {
                    Id = i.Id,
                    PrendaId = i.PrendaId,
                    Prenda = i.Prenda != null ? new PrendaDTO
                    {
                        Id = i.Prenda.Id,
                        Nombre = i.Prenda.Nombre,
                        Codigo = i.Prenda.Codigo,
                        Descripcion = i.Prenda.Descripcion,
                        Precio = i.Prenda.Costo,
                        CategoriaId = i.Prenda.CategoriaId,
                        ImagenUrl = i.Prenda.ImagenUrl ?? "/placeholder.svg?height=250&width=280"
                    } : null,
                    Talla = i.Talla,
                    Color = i.Color,
                    Stock = i.Cantidad
                }).ToList();

                return Ok(inventarioDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        // GET: api/Inventario/{id}
        [HttpGet("{id}")]
        public IActionResult GetInventarioById(int id)
        {
            try
            {
                var item = _inventarioService.GetById(id);
                if (item == null)
                {
                    return NotFound(new { message = "Producto no encontrado" });
                }

                var inventarioDTO = new InventarioDTO
                {
                    Id = item.Id,
                    PrendaId = item.PrendaId,
                    Prenda = item.Prenda != null ? new PrendaDTO
                    {
                        Id = item.Prenda.Id,
                        Nombre = item.Prenda.Nombre,
                        Codigo = item.Prenda.Codigo,
                        Descripcion = item.Prenda.Descripcion,
                        Precio = item.Prenda.Costo,
                        CategoriaId = item.Prenda.CategoriaId,
                        ImagenUrl = item.Prenda.ImagenUrl ?? "/placeholder.svg?height=250&width=280"
                    } : null,
                    Talla = item.Talla,
                    Color = item.Color,
                    Stock = item.Cantidad
                };

                return Ok(inventarioDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }
    }
}
