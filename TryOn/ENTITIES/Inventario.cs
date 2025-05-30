using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITIES
{
    public class Inventario
    {
        public int Id { get; set; }
        public int PrendaId { get; set; }
        public string Talla { get; set; }
        public string Color { get; set; }
        public int Cantidad { get; set; }
        public string Ubicacion { get; set; }

        // Propiedades de navegación
        public Prenda Prenda { get; set; }
    }
}
