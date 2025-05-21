using ENTITIES;
using System;
using System.Collections.Generic;
using System.Linq;
using TryOn.DAL;

namespace TryOn.BLL
{
    public class ReportesService
    {
        private readonly PedidoRepository _pedidoRepository;
        private readonly PrendaRepository _prendaRepository;
        private readonly InventarioRepository _inventarioRepository;

        public ReportesService()
        {
            _pedidoRepository = new PedidoRepository();
            _prendaRepository = new PrendaRepository();
            _inventarioRepository = new InventarioRepository();
        }

        #region Productos Más Vendidos

        public List<ProductoVendidoDTO> ObtenerProductosMasVendidos(DateTime fechaInicio)
        {
            try
            {
                // Obtener todos los pedidos desde la fecha de inicio
                var pedidos = _pedidoRepository.GetAll().Where(p => p.FechaPedido >= fechaInicio).ToList();

                if (!pedidos.Any() || pedidos.All(p => p.Detalles == null || !p.Detalles.Any()))
                {
                    return new List<ProductoVendidoDTO>();
                }

                // Obtener todas las prendas para evitar múltiples consultas a la base de datos
                var prendas = _prendaRepository.GetAll().ToList();

                // Agrupar por prenda y calcular totales
                var productosMasVendidos = pedidos
                    .Where(p => p.Detalles != null && p.Detalles.Any())
                    .SelectMany(p => p.Detalles)
                    .Where(d => d.Inventario != null && d.Inventario.PrendaId > 0)
                    .GroupBy(d => d.Inventario.PrendaId)
                    .Select(g => new
                    {
                        PrendaId = g.Key,
                        CantidadVendida = g.Sum(d => d.Cantidad),
                        TotalVentas = g.Sum(d => d.Subtotal)
                    })
                    .OrderByDescending(x => x.CantidadVendida)
                    .Take(10)
                    .ToList();

                // Mapear a DTO con información completa
                var resultado = productosMasVendidos.Select(p =>
                {
                    var prenda = prendas.FirstOrDefault(pr => pr.Id == p.PrendaId);
                    return new ProductoVendidoDTO
                    {
                        PrendaId = p.PrendaId,
                        Nombre = prenda?.Nombre ?? "Desconocido",
                        Categoria = prenda?.Categoria?.Nombre ?? "Desconocida",
                        CantidadVendida = p.CantidadVendida,
                        TotalVentas = p.TotalVentas
                    };
                }).ToList();

                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener productos más vendidos: {ex.Message}", ex);
            }
        }

        #endregion

        #region Ventas Por Período

        public List<VentaPorPeriodoDTO> ObtenerVentasPorPeriodo(DateTime fechaDesde, DateTime fechaHasta)
        {
            try
            {
                // Asegurar que la fecha hasta incluya todo el día
                fechaHasta = fechaHasta.AddDays(1).AddSeconds(-1);

                // Obtener todos los pedidos en el rango de fechas
                var pedidos = _pedidoRepository.GetAll()
                    .Where(p => p.FechaPedido >= fechaDesde && p.FechaPedido <= fechaHasta)
                    .ToList();

                if (!pedidos.Any())
                {
                    return new List<VentaPorPeriodoDTO>();
                }

                // Agrupar por fecha y calcular totales
                var ventasPorFecha = pedidos
                    .GroupBy(p => p.FechaPedido.Date)
                    .Select(g => new VentaPorPeriodoDTO
                    {
                        Fecha = g.Key,
                        CantidadPedidos = g.Count(),
                        TotalVentas = g.Sum(p => p.Total)
                    })
                    .OrderBy(x => x.Fecha)
                    .ToList();

                return ventasPorFecha;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener ventas por período: {ex.Message}", ex);
            }
        }

        public ResumenVentasDTO ObtenerResumenVentas(DateTime fechaDesde, DateTime fechaHasta)
        {
            try
            {
                // Asegurar que la fecha hasta incluya todo el día
                fechaHasta = fechaHasta.AddDays(1).AddSeconds(-1);

                // Obtener todos los pedidos en el rango de fechas
                var pedidos = _pedidoRepository.GetAll()
                    .Where(p => p.FechaPedido >= fechaDesde && p.FechaPedido <= fechaHasta)
                    .ToList();

                if (!pedidos.Any())
                {
                    return new ResumenVentasDTO();
                }

                // Calcular totales
                decimal totalVentas = pedidos.Sum(p => p.Total);
                int totalPedidos = pedidos.Count;
                decimal promedioPedido = totalPedidos > 0 ? totalVentas / totalPedidos : 0;

                return new ResumenVentasDTO
                {
                    TotalVentas = totalVentas,
                    TotalPedidos = totalPedidos,
                    PromedioPedido = promedioPedido
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener resumen de ventas: {ex.Message}", ex);
            }
        }

        #endregion

        #region Inventario Bajo Stock

        public List<Inventario> ObtenerInventarioBajoStock(int stockMinimo)
        {
            try
            {
                // Obtener todo el inventario
                var inventarios = _inventarioRepository.GetAll().ToList();

                if (!inventarios.Any())
                {
                    return new List<Inventario>();
                }

                // Filtrar por stock mínimo
                var inventarioBajoStock = inventarios
                    .Where(i => i.Cantidad <= stockMinimo)
                    .OrderBy(i => i.Cantidad)
                    .ToList();

                return inventarioBajoStock;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener inventario bajo stock: {ex.Message}", ex);
            }
        }

        public ResumenInventarioDTO ObtenerResumenInventario(int stockMinimo)
        {
            try
            {
                // Obtener inventario bajo stock
                var inventarioBajoStock = ObtenerInventarioBajoStock(stockMinimo);

                if (!inventarioBajoStock.Any())
                {
                    return new ResumenInventarioDTO();
                }

                // Calcular estadísticas
                int sinStock = inventarioBajoStock.Count(i => i.Cantidad == 0);
                int stockCritico = inventarioBajoStock.Count(i => i.Cantidad > 0 && i.Cantidad <= stockMinimo / 2);
                int stockBajo = inventarioBajoStock.Count(i => i.Cantidad > stockMinimo / 2 && i.Cantidad <= stockMinimo);

                return new ResumenInventarioDTO
                {
                    SinStock = sinStock,
                    StockCritico = stockCritico,
                    StockBajo = stockBajo
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener resumen de inventario: {ex.Message}", ex);
            }
        }

        #endregion

        #region Ventas Por Categoría

        public List<VentaPorCategoriaDTO> ObtenerVentasPorCategoria(DateTime fechaInicio)
        {
            try
            {
                // Primero obtenemos todas las categorías a través de las prendas
                var prendas = _prendaRepository.GetAll().ToList();

                // Obtener todos los pedidos desde la fecha de inicio
                var pedidos = _pedidoRepository.GetAll().Where(p => p.FechaPedido >= fechaInicio).ToList();

                if (!pedidos.Any() || !prendas.Any())
                {
                    return new List<VentaPorCategoriaDTO>();
                }

                // Obtener ventas por prenda
                var ventasPorPrenda = pedidos
                    .Where(p => p.Detalles != null && p.Detalles.Any())
                    .SelectMany(p => p.Detalles)
                    .Where(d => d.Inventario != null && d.Inventario.PrendaId > 0)
                    .GroupBy(d => d.Inventario.PrendaId)
                    .Select(g => new
                    {
                        PrendaId = g.Key,
                        CantidadVendida = g.Sum(d => d.Cantidad),
                        TotalVentas = g.Sum(d => d.Subtotal)
                    })
                    .ToList();

                // Extraer todas las categorías únicas con sus IDs
                var categorias = prendas
                    .Where(p => p.Categoria != null)
                    .Select(p => p.Categoria)
                    .GroupBy(c => c.Id)
                    .Select(g => g.First())
                    .ToList();

                // Si no hay categorías, retornar lista vacía
                if (!categorias.Any())
                {
                    return new List<VentaPorCategoriaDTO>();
                }

                // Agrupar ventas por categoría
                var resultado = new List<VentaPorCategoriaDTO>();

                foreach (var categoria in categorias)
                {
                    // Obtener todas las prendas de esta categoría
                    var prendasCategoria = prendas.Where(p => p.CategoriaId == categoria.Id).ToList();

                    // Calcular ventas totales para esta categoría
                    int cantidadVendida = 0;
                    decimal totalVentas = 0;

                    foreach (var prenda in prendasCategoria)
                    {
                        var ventaPrenda = ventasPorPrenda.FirstOrDefault(v => v.PrendaId == prenda.Id);
                        if (ventaPrenda != null)
                        {
                            cantidadVendida += ventaPrenda.CantidadVendida;
                            totalVentas += ventaPrenda.TotalVentas;
                        }
                    }

                    // Solo agregar categorías con ventas
                    if (cantidadVendida > 0)
                    {
                        resultado.Add(new VentaPorCategoriaDTO
                        {
                            CategoriaId = categoria.Id,
                            Nombre = categoria.Nombre,
                            CantidadProductos = prendasCategoria.Count,
                            CantidadVendida = cantidadVendida,
                            TotalVentas = totalVentas
                        });
                    }
                }

                // Ordenar por total de ventas
                return resultado.OrderByDescending(c => c.TotalVentas).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener ventas por categoría: {ex.Message}", ex);
            }
        }

        // Método alternativo que obtiene información más detallada sobre cada categoría
        public List<CategoriaVentaDTO> ObtenerVentasDetalladoPorCategoria(DateTime fechaInicio)
        {
            try
            {
                // Obtener todas las prendas con sus categorías
                var prendas = _prendaRepository.GetAll().ToList();

                // Obtener todos los pedidos desde la fecha de inicio
                var pedidos = _pedidoRepository.GetAll().Where(p => p.FechaPedido >= fechaInicio).ToList();

                if (!pedidos.Any() || !prendas.Any())
                {
                    return new List<CategoriaVentaDTO>();
                }

                // Obtener ventas por prenda
                var ventasPorPrenda = pedidos
                    .Where(p => p.Detalles != null && p.Detalles.Any())
                    .SelectMany(p => p.Detalles)
                    .Where(d => d.Inventario != null && d.Inventario.PrendaId > 0)
                    .GroupBy(d => d.Inventario.PrendaId)
                    .ToDictionary(
                        g => g.Key,
                        g => new PrendaVentaDTO
                        {
                            Id = g.Key,
                            CantidadVendida = g.Sum(d => d.Cantidad),
                            TotalVentas = g.Sum(d => d.Subtotal)
                        }
                    );

                // Agrupar prendas por categoría
                var prendasPorCategoria = prendas
                    .Where(p => p.Categoria != null)
                    .GroupBy(p => p.CategoriaId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.ToList()
                    );

                // Extraer todas las categorías únicas
                var categorias = prendas
                    .Where(p => p.Categoria != null)
                    .Select(p => p.Categoria)
                    .GroupBy(c => c.Id)
                    .Select(g => g.First())
                    .ToList();

                // Crear el resultado final
                var resultado = new List<CategoriaVentaDTO>();

                foreach (var categoria in categorias)
                {
                    // Verificar si hay prendas para esta categoría
                    if (!prendasPorCategoria.ContainsKey(categoria.Id))
                        continue;

                    var categoriaDTO = new CategoriaVentaDTO
                    {
                        Id = categoria.Id,
                        Nombre = categoria.Nombre,
                        TotalProductos = prendasPorCategoria[categoria.Id].Count
                    };

                    // Agregar las ventas de cada prenda en esta categoría
                    foreach (var prenda in prendasPorCategoria[categoria.Id])
                    {
                        if (ventasPorPrenda.ContainsKey(prenda.Id))
                        {
                            var ventaPrenda = ventasPorPrenda[prenda.Id];
                            ventaPrenda.Nombre = prenda.Nombre; // Asignar el nombre de la prenda
                            categoriaDTO.Prendas.Add(ventaPrenda);
                        }
                    }

                    // Solo agregar categorías con ventas
                    if (categoriaDTO.Prendas.Any())
                    {
                        resultado.Add(categoriaDTO);
                    }
                }

                // Ordenar por total de ventas
                return resultado.OrderByDescending(c => c.TotalVentas).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener ventas detalladas por categoría: {ex.Message}", ex);
            }
        }

        #endregion

        public DateTime ObtenerFechaInicioPeriodo(int indice)
        {
            DateTime fechaActual = DateTime.Now;

            switch (indice)
            {
                case 0: // Último mes
                    return fechaActual.AddMonths(-1);
                case 1: // Últimos 3 meses
                    return fechaActual.AddMonths(-3);
                case 2: // Último año
                    return fechaActual.AddYears(-1);
                case 3: // Todo el tiempo
                    return DateTime.MinValue;
                default:
                    return fechaActual.AddMonths(-1);
            }
        }
    }
}
