using System;
using System.Collections.Generic;
using System.Linq;

namespace ENTITIES
{
    // DTOs para Reportes

    public class ProductoVendidoDTO
    {
        public int PrendaId { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
    }

    public class VentaPorPeriodoDTO
    {
        public DateTime Fecha { get; set; }
        public int CantidadPedidos { get; set; }
        public decimal TotalVentas { get; set; }
    }

    public class ResumenVentasDTO
    {
        public decimal TotalVentas { get; set; }
        public int TotalPedidos { get; set; }
        public decimal PromedioPedido { get; set; }
    }

    public class ResumenInventarioDTO
    {
        public int SinStock { get; set; }
        public int StockCritico { get; set; }
        public int StockBajo { get; set; }
    }

    public class VentaPorCategoriaDTO
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; }
        public int CantidadProductos { get; set; }
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
    }

    // Nueva clase para categorías con sus ventas
    public class CategoriaVentaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int TotalProductos { get; set; }
        public List<PrendaVentaDTO> Prendas { get; set; } = new List<PrendaVentaDTO>();

        public int TotalUnidadesVendidas => Prendas.Sum(p => p.CantidadVendida);
        public decimal TotalVentas => Prendas.Sum(p => p.TotalVentas);
    }

    public class PrendaVentaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
    }
}

