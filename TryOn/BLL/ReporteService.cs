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

        #region Productos Más Pedidos

        public List<ProductoPedidoDTO> ObtenerProductosMasPedidos(DateTime fechaInicio)
        {
            try
            {
                // Obtener todos los pedidos desde la fecha de inicio
                var pedidos = _pedidoRepository.GetAll().Where(p => p.FechaPedido >= fechaInicio).ToList();

                if (!pedidos.Any() || pedidos.All(p => p.Detalles == null || !p.Detalles.Any()))
                {
                    return new List<ProductoPedidoDTO>();
                }

                // Obtener todas las prendas para evitar múltiples consultas a la base de datos
                var prendas = _prendaRepository.GetAll().ToList();

                // Agrupar por prenda y calcular totales
                var productosMasPedidos = pedidos
                    .Where(p => p.Detalles != null && p.Detalles.Any())
                    .SelectMany(p => p.Detalles)
                    .Where(d => d.Inventario != null && d.Inventario.PrendaId > 0)
                    .GroupBy(d => d.Inventario.PrendaId)
                    .Select(g => new
                    {
                        PrendaId = g.Key,
                        CantidadPedida = g.Sum(d => d.Cantidad),
                        TotalPosiblesGanancias = g.Sum(d => d.Subtotal)
                    })
                    .OrderByDescending(x => x.CantidadPedida)
                    .Take(10)
                    .ToList();

                // Mapear a DTO con información completa
                var resultado = productosMasPedidos.Select(p =>
                {
                    var prenda = prendas.FirstOrDefault(pr => pr.Id == p.PrendaId);
                    return new ProductoPedidoDTO
                    {
                        PrendaId = p.PrendaId,
                        Nombre = prenda?.Nombre ?? "Desconocido",
                        Categoria = prenda?.Categoria?.Nombre ?? "Desconocida",
                        CantidadPedida = p.CantidadPedida,
                        TotalPosiblesGanancias = p.TotalPosiblesGanancias
                    };
                }).ToList();

                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener productos más pedidos: {ex.Message}", ex);
            }
        }

        #endregion

        #region Posibles Ganancias Por Período

        public List<PosibleGananciaPorPeriodoDTO> ObtenerPosiblesGananciasPorPeriodo(DateTime fechaDesde, DateTime fechaHasta)
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
                    return new List<PosibleGananciaPorPeriodoDTO>();
                }

                // Agrupar por fecha y calcular totales
                var posiblesGananciasPorFecha = pedidos
                    .GroupBy(p => p.FechaPedido.Date)
                    .Select(g => new PosibleGananciaPorPeriodoDTO
                    {
                        Fecha = g.Key,
                        CantidadPedidos = g.Count(),
                        TotalPosiblesGanancias = g.Sum(p => p.Total)
                    })
                    .OrderBy(x => x.Fecha)
                    .ToList();

                return posiblesGananciasPorFecha;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener posibles ganancias por período: {ex.Message}", ex);
            }
        }

        public ResumenPosiblesGananciasDTO ObtenerResumenPosiblesGanancias(DateTime fechaDesde, DateTime fechaHasta)
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
                    return new ResumenPosiblesGananciasDTO();
                }

                // Calcular totales
                decimal totalPosiblesGanancias = pedidos.Sum(p => p.Total);
                int totalPedidos = pedidos.Count;
                decimal promedioPedido = totalPedidos > 0 ? totalPosiblesGanancias / totalPedidos : 0;

                return new ResumenPosiblesGananciasDTO
                {
                    TotalPosiblesGanancias = totalPosiblesGanancias,
                    TotalPedidos = totalPedidos,
                    PromedioPedido = promedioPedido
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener resumen de posibles ganancias: {ex.Message}", ex);
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

        #region Posibles Ganancias Por Categoría

        public List<PosibleGananciaPorCategoriaDTO> ObtenerPosiblesGananciasPorCategoria(DateTime fechaInicio)
        {
            try
            {
                // Primero obtenemos todas las categorías a través de las prendas
                var prendas = _prendaRepository.GetAll().ToList();

                // Obtener todos los pedidos desde la fecha de inicio
                var pedidos = _pedidoRepository.GetAll().Where(p => p.FechaPedido >= fechaInicio).ToList();

                if (!pedidos.Any() || !prendas.Any())
                {
                    return new List<PosibleGananciaPorCategoriaDTO>();
                }

                // Obtener pedidos por prenda
                var pedidosPorPrenda = pedidos
                    .Where(p => p.Detalles != null && p.Detalles.Any())
                    .SelectMany(p => p.Detalles)
                    .Where(d => d.Inventario != null && d.Inventario.PrendaId > 0)
                    .GroupBy(d => d.Inventario.PrendaId)
                    .Select(g => new
                    {
                        PrendaId = g.Key,
                        CantidadPedida = g.Sum(d => d.Cantidad),
                        TotalPosiblesGanancias = g.Sum(d => d.Subtotal)
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
                    return new List<PosibleGananciaPorCategoriaDTO>();
                }

                // Agrupar pedidos por categoría
                var resultado = new List<PosibleGananciaPorCategoriaDTO>();

                foreach (var categoria in categorias)
                {
                    // Obtener todas las prendas de esta categoría
                    var prendasCategoria = prendas.Where(p => p.CategoriaId == categoria.Id).ToList();

                    // Calcular posibles ganancias totales para esta categoría
                    int cantidadPedida = 0;
                    decimal totalPosiblesGanancias = 0;

                    foreach (var prenda in prendasCategoria)
                    {
                        var pedidoPrenda = pedidosPorPrenda.FirstOrDefault(v => v.PrendaId == prenda.Id);
                        if (pedidoPrenda != null)
                        {
                            cantidadPedida += pedidoPrenda.CantidadPedida;
                            totalPosiblesGanancias += pedidoPrenda.TotalPosiblesGanancias;
                        }
                    }

                    // Solo agregar categorías con pedidos
                    if (cantidadPedida > 0)
                    {
                        resultado.Add(new PosibleGananciaPorCategoriaDTO
                        {
                            CategoriaId = categoria.Id,
                            Nombre = categoria.Nombre,
                            CantidadProductos = prendasCategoria.Count,
                            CantidadPedida = cantidadPedida,
                            TotalPosiblesGanancias = totalPosiblesGanancias
                        });
                    }
                }

                // Ordenar por total de posibles ganancias
                return resultado.OrderByDescending(c => c.TotalPosiblesGanancias).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener posibles ganancias por categoría: {ex.Message}", ex);
            }
        }

        // Método alternativo que obtiene información más detallada sobre cada categoría
        public List<CategoriaPosibleGananciaDTO> ObtenerPosiblesGananciasDetalladoPorCategoria(DateTime fechaInicio)
        {
            try
            {
                // Obtener todas las prendas con sus categorías
                var prendas = _prendaRepository.GetAll().ToList();

                // Obtener todos los pedidos desde la fecha de inicio
                var pedidos = _pedidoRepository.GetAll().Where(p => p.FechaPedido >= fechaInicio).ToList();

                if (!pedidos.Any() || !prendas.Any())
                {
                    return new List<CategoriaPosibleGananciaDTO>();
                }

                // Obtener pedidos por prenda
                var pedidosPorPrenda = pedidos
                    .Where(p => p.Detalles != null && p.Detalles.Any())
                    .SelectMany(p => p.Detalles)
                    .Where(d => d.Inventario != null && d.Inventario.PrendaId > 0)
                    .GroupBy(d => d.Inventario.PrendaId)
                    .ToDictionary(
                        g => g.Key,
                        g => new PrendaPedidoDTO
                        {
                            Id = g.Key,
                            CantidadPedida = g.Sum(d => d.Cantidad),
                            TotalPosiblesGanancias = g.Sum(d => d.Subtotal)
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
                var resultado = new List<CategoriaPosibleGananciaDTO>();

                foreach (var categoria in categorias)
                {
                    // Verificar si hay prendas para esta categoría
                    if (!prendasPorCategoria.ContainsKey(categoria.Id))
                        continue;

                    var categoriaDTO = new CategoriaPosibleGananciaDTO
                    {
                        Id = categoria.Id,
                        Nombre = categoria.Nombre,
                        TotalProductos = prendasPorCategoria[categoria.Id].Count
                    };

                    // Agregar los pedidos de cada prenda en esta categoría
                    foreach (var prenda in prendasPorCategoria[categoria.Id])
                    {
                        if (pedidosPorPrenda.ContainsKey(prenda.Id))
                        {
                            var pedidoPrenda = pedidosPorPrenda[prenda.Id];
                            pedidoPrenda.Nombre = prenda.Nombre; // Asignar el nombre de la prenda
                            categoriaDTO.Prendas.Add(pedidoPrenda);
                        }
                    }

                    // Solo agregar categorías con pedidos
                    if (categoriaDTO.Prendas.Any())
                    {
                        resultado.Add(categoriaDTO);
                    }
                }

                // Ordenar por total de posibles ganancias
                return resultado.OrderByDescending(c => c.TotalPosiblesGanancias).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener posibles ganancias detalladas por categoría: {ex.Message}", ex);
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