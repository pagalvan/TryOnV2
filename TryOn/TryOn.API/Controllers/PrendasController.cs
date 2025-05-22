using Microsoft.AspNetCore.Mvc;
using TryOn.API.DTOs;
using TryOn.BLL; // Namespace donde está tu PrendaService
using System.Linq;

namespace TryOn.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrendasController : ControllerBase
    {
        private readonly PrendaService _prendaService;

        public PrendasController(PrendaService prendaService)
        {
            _prendaService = prendaService;
        }

        // GET: api/Prendas
        [HttpGet]
        public IActionResult GetPrendas([FromQuery] int? categoriaId = null, [FromQuery] string search = null)
        {
            var prendas = _prendaService.GetAll();

            // Filtrar por categoría si se especifica
            if (categoriaId.HasValue)
            {
                prendas = prendas.Where(p => p.CategoriaId == categoriaId.Value);
            }

            // Filtrar por término de búsqueda si se especifica
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                prendas = prendas.Where(p =>
                    p.Nombre.ToLower().Contains(search) ||
                    p.Codigo.ToLower().Contains(search) ||
                    (p.Descripcion != null && p.Descripcion.ToLower().Contains(search))
                );
            }

            var prendasDTO = prendas.Select(p => new PrendaDTO
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Codigo = p.Codigo,
                Descripcion = p.Descripcion,
                Precio = p.Costo,
                CategoriaId = p.CategoriaId,
                Categoria = p.Categoria != null ? new CategoriaDTO
                {
                    Id = p.Categoria.Id,
                    Nombre = p.Categoria.Nombre
                } : null,
                ImagenUrl = p.ImagenUrl // Asumiendo que tienes esta propiedad
            });

            return Ok(prendasDTO);
        }

        // GET: api/Prendas/5
        [HttpGet("{id}")]
        public IActionResult GetPrenda(int id)
        {
            var prenda = _prendaService.GetById(id);
            if (prenda == null)
            {
                return NotFound();
            }

            var prendaDTO = new PrendaDTO
            {
                Id = prenda.Id,
                Nombre = prenda.Nombre,
                Codigo = prenda.Codigo,
                Descripcion = prenda.Descripcion,
                Precio = prenda.Costo,
                CategoriaId = prenda.CategoriaId,
                Categoria = prenda.Categoria != null ? new CategoriaDTO
                {
                    Id = prenda.Categoria.Id,
                    Nombre = prenda.Categoria.Nombre
                } : null,
                ImagenUrl = prenda.ImagenUrl // Asumiendo que tienes esta propiedad
            };

            return Ok(prendaDTO);
        }
    }
}