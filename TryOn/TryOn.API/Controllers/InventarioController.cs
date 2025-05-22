using Microsoft.AspNetCore.Mvc;
using TryOn.API.DTOs;
using TryOn.BLL; // Namespace donde está tu InventarioService
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
            var inventario = _inventarioService.GetAll();

            // Filtrar por prendaId si se especifica
            if (prendaId.HasValue)
            {
                inventario = inventario.Where(i => i.PrendaId == prendaId.Value);
            }

            // Filtrar por talla si se especifica
            if (!string.IsNullOrEmpty(talla))
            {
                inventario = inventario.Where(i => i.Talla == talla);
            }

            // Filtrar por color si se especifica
            if (!string.IsNullOrEmpty(color))
            {
                inventario = inventario.Where(i => i.Color == color);
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
                    ImagenUrl = i.Prenda.ImagenUrl // Asumiendo que tienes esta propiedad
                } : null,
                Talla = i.Talla,
                Color = i.Color,
                Stock = i.Cantidad
            });

            return Ok(inventarioDTO);
        }
    }
}
