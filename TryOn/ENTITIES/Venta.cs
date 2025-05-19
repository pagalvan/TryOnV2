using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITIES
{
    public class Venta
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public DateTime FechaVenta { get; set; }
        public decimal Total { get; set; }
        public string MetodoPago { get; set; }
        public int UsuarioId { get; set; }

        // Propiedades de navegación
        public Pedido Pedido { get; set; }
        public Usuario Usuario { get; set; }

        public Venta()
        {
            FechaVenta = DateTime.Now;
        }
    }
}
