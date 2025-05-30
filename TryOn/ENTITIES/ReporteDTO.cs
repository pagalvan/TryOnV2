using System;
using System.Collections.Generic;
using System.Linq;

namespace ENTITIES
{
    // DTOs para Reportes

    public class ProductoPedidoDTO
    {
        public int PrendaId { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public int CantidadPedida { get; set; }
        public decimal TotalPosiblesGanancias { get; set; }
    }

    public class PosibleGananciaPorPeriodoDTO
    {
        public DateTime Fecha { get; set; }
        public int CantidadPedidos { get; set; }
        public decimal TotalPosiblesGanancias { get; set; }
    }

    public class ResumenPosiblesGananciasDTO
    {
        public decimal TotalPosiblesGanancias { get; set; }
        public int TotalPedidos { get; set; }
        public decimal PromedioPedido { get; set; }
    }

    public class ResumenInventarioDTO
    {
        public int SinStock { get; set; }
        public int StockCritico { get; set; }
        public int StockBajo { get; set; }
    }

    public class PosibleGananciaPorCategoriaDTO
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; }
        public int CantidadProductos { get; set; }
        public int CantidadPedida { get; set; }
        public decimal TotalPosiblesGanancias { get; set; }
    }

    // Nueva clase para categorías con sus posibles ganancias
    public class CategoriaPosibleGananciaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int TotalProductos { get; set; }
        public List<PrendaPedidoDTO> Prendas { get; set; } = new List<PrendaPedidoDTO>();

        public int TotalUnidadesPedidas => Prendas.Sum(p => p.CantidadPedida);
        public decimal TotalPosiblesGanancias => Prendas.Sum(p => p.TotalPosiblesGanancias);
    }

    public class PrendaPedidoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int CantidadPedida { get; set; }
        public decimal TotalPosiblesGanancias { get; set; }
    }
}