using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITIES
{
    public class Prenda
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal Costo { get; set; }
        public int CategoriaId { get; set; }
        public string ImagenUrl { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Propiedades de navegación
        public Categoria Categoria { get; set; }

        public Prenda()
        {
            FechaCreacion = DateTime.Now;
        }
    }
}
