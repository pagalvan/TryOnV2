using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITIES
{
    public class Pedido
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaPedido { get; set; }
        public string Estado { get; set; }
        public decimal Total { get; set; }
        public string DireccionEnvio { get; set; }
        public string MetodoPago { get; set; }

        // Propiedades de navegación
        public Usuario Usuario { get; set; }
        public List<DetallePedido> Detalles { get; set; }

        public Pedido()
        {
            FechaPedido = DateTime.Now;
            Estado = "Pendiente";
            Detalles = new List<DetallePedido>();
        }
    }
}
