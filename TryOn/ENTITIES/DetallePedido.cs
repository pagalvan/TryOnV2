using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITIES
{
    public class DetallePedido
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int InventarioId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }

        // Propiedades de navegación
        public Pedido Pedido { get; set; }
        public Inventario Inventario { get; set; }
    }
}
