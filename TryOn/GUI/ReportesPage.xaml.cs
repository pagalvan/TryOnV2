using ENTITIES;
using GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TryOn.BLL;


namespace GUI
{
    public partial class ReportesPage : Page
    {
        private readonly PedidoService _pedidoService;
        private readonly PrendaService _prendaService;
        private readonly InventarioService _inventarioService;
        private readonly TelegramService _telegramService;

        private List<Pedido> _pedidos;
        private List<Prenda> _prendas;
        private List<Inventario> _inventarios;

        public ReportesPage()
        {
            InitializeComponent();
            _pedidoService = new PedidoService();
            _prendaService = new PrendaService();
            _inventarioService = new InventarioService();
            _telegramService = TelegramService.GetInstance();

            // Inicializar fechas
            dpFechaDesde.SelectedDate = DateTime.Now.AddMonths(-1);
            dpFechaHasta.SelectedDate = DateTime.Now;

            // Seleccionar primer tipo de reporte
            cmbTipoReporte.SelectedIndex = 0;

            // Seleccionar "Último mes" en combos de período
            cmbPeriodoProductos.SelectedIndex = 0;
            cmbPeriodoCategorias.SelectedIndex = 0;

            CargarDatos();
        }

        private void CargarDatos()
        {
            try
            {
                // Cargar pedidos
                _pedidos = _pedidoService.GetAll().ToList();

                // Cargar prendas
                _prendas = _prendaService.GetAll().ToList();

                // Cargar inventario
                _inventarios = _inventarioService.GetAll().ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbTipoReporte_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ocultar todas las pestañas
            foreach (TabItem tab in tabReportes.Items)
            {
                tab.Visibility = Visibility.Collapsed;
            }

            // Mostrar la pestaña seleccionada
            ((TabItem)tabReportes.Items[cmbTipoReporte.SelectedIndex]).Visibility = Visibility.Visible;
            tabReportes.SelectedIndex = cmbTipoReporte.SelectedIndex;

            // Generar reporte según el tipo seleccionado
            switch (cmbTipoReporte.SelectedIndex)
            {
                case 0: // Productos más vendidos
                    GenerarReporteProductosMasVendidos();
                    break;
                case 1: // Ventas por período
                    GenerarReporteVentasPorPeriodo();
                    break;
                case 2: // Inventario bajo stock
                    GenerarReporteInventarioBajoStock();
                    break;
                case 3: // Ventas por categoría
                    GenerarReporteVentasPorCategoria();
                    break;
            }
        }

        private void cmbPeriodoProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GenerarReporteProductosMasVendidos();
        }

        private void btnGenerarReporteProductos_Click(object sender, RoutedEventArgs e)
        {
            GenerarReporteProductosMasVendidos();
        }

        private void GenerarReporteProductosMasVendidos()
        {
            try
            {
                // Obtener fecha de inicio según el período seleccionado
                DateTime fechaInicio = ObtenerFechaInicioPeriodo(cmbPeriodoProductos.SelectedIndex);

                // Filtrar pedidos por fecha
                var pedidosFiltrados = _pedidos.Where(p => p.FechaPedido >= fechaInicio).ToList();

                // Obtener productos más vendidos
                var productosMasVendidos = pedidosFiltrados
                    .SelectMany(p => p.Detalles)
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

                // Crear lista de resultados
                var resultados = productosMasVendidos.Select(p =>
                {
                    var prenda = _prendas.FirstOrDefault(pr => pr.Id == p.PrendaId);
                    return new
                    {
                        Nombre = prenda?.Nombre ?? "Desconocido",
                        Categoria = prenda?.Categoria?.Nombre ?? "Desconocida",
                        CantidadVendida = p.CantidadVendida,
                        TotalVentas = p.TotalVentas
                    };
                }).ToList();

                dgProductosMasVendidos.ItemsSource = resultados;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnGenerarReporteVentas_Click(object sender, RoutedEventArgs e)
        {
            GenerarReporteVentasPorPeriodo();
        }

        private void GenerarReporteVentasPorPeriodo()
        {
            try
            {
                // Obtener fechas seleccionadas
                DateTime fechaDesde = dpFechaDesde.SelectedDate ?? DateTime.Now.AddMonths(-1);
                DateTime fechaHasta = dpFechaHasta.SelectedDate ?? DateTime.Now;

                // Asegurar que la fecha hasta incluya todo el día
                fechaHasta = fechaHasta.AddDays(1).AddSeconds(-1);

                // Filtrar pedidos por fecha
                var pedidosFiltrados = _pedidos.Where(p => p.FechaPedido >= fechaDesde && p.FechaPedido <= fechaHasta).ToList();

                // Agrupar por fecha
                var ventasPorFecha = pedidosFiltrados
                    .GroupBy(p => p.FechaPedido.Date)
                    .Select(g => new
                    {
                        Fecha = g.Key,
                        CantidadPedidos = g.Count(),
                        TotalVentas = g.Sum(p => p.Total)
                    })
                    .OrderBy(x => x.Fecha)
                    .ToList();

                dgVentasPorPeriodo.ItemsSource = ventasPorFecha;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnGenerarReporteStock_Click(object sender, RoutedEventArgs e)
        {
            GenerarReporteInventarioBajoStock();
        }

        private void GenerarReporteInventarioBajoStock()
        {
            try
            {
                // Obtener stock mínimo
                int stockMinimo;
                if (!int.TryParse(txtStockMinimo.Text, out stockMinimo))
                {
                    stockMinimo = 10; // Valor por defecto
                }

                // Filtrar inventario por stock mínimo
                var inventarioBajoStock = _inventarios.Where(i => i.Cantidad <= stockMinimo).OrderBy(i => i.Cantidad).ToList();

                dgInventarioBajoStock.ItemsSource = inventarioBajoStock;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbPeriodoCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GenerarReporteVentasPorCategoria();
        }

        private void btnGenerarReporteCategorias_Click(object sender, RoutedEventArgs e)
        {
            GenerarReporteVentasPorCategoria();
        }

        private void GenerarReporteVentasPorCategoria()
        {
            try
            {
                // Obtener fecha de inicio según el período seleccionado
                DateTime fechaInicio = ObtenerFechaInicioPeriodo(cmbPeriodoCategorias.SelectedIndex);

                // Filtrar pedidos por fecha
                var pedidosFiltrados = _pedidos.Where(p => p.FechaPedido >= fechaInicio).ToList();

                // Obtener ventas por categoría
                var ventasPorCategoria = pedidosFiltrados
                    .SelectMany(p => p.Detalles)
                    .GroupBy(d => d.Inventario.Prenda.CategoriaId)
                    .Select(g => new
                    {
                        CategoriaId = g.Key,
                        CantidadVendida = g.Sum(d => d.Cantidad),
                        TotalVentas = g.Sum(d => d.Subtotal)
                    })
                    .OrderByDescending(x => x.TotalVentas)
                    .ToList();

                // Crear lista de resultados
                var resultados = ventasPorCategoria.Select(c =>
                {
                    var categoria = _prendas.FirstOrDefault(p => p.CategoriaId == c.CategoriaId)?.Categoria;
                    var cantidadProductos = _prendas.Count(p => p.CategoriaId == c.CategoriaId);

                    return new
                    {
                        Nombre = categoria?.Nombre ?? "Desconocida",
                        CantidadProductos = cantidadProductos,
                        CantidadVendida = c.CantidadVendida,
                        TotalVentas = c.TotalVentas
                    };
                }).ToList();

                dgVentasPorCategoria.ItemsSource = resultados;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DateTime ObtenerFechaInicioPeriodo(int indice)
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

        private void btnEnviarPromocion_Click(object sender, RoutedEventArgs e)
        {
            EnviarPromocionDialog dialog = new EnviarPromocionDialog();
            dialog.ShowDialog();
        }
    }
}