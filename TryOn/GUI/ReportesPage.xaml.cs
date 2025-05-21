using ENTITIES;
using GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TryOn.BLL;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media;

namespace GUI
{
    public partial class ReportesPage : Page
    {
        private readonly ReportesService _reportesService;
        private readonly TelegramService _telegramService;

        private List<Pedido> _pedidos;
        private List<Prenda> _prendas;
        private List<Inventario> _inventarios;
        private bool _datosInicializados = false;

        #region Propiedades de binding para LiveCharts

        // Productos más vendidos
        public SeriesCollection ProductosVendidosSeries { get; set; }
        public List<string> ProductosVendidosLabels { get; set; }
        public SeriesCollection ProductosVendidosPieSeries { get; set; }

        // Ventas por período
        public SeriesCollection VentasPorPeriodoSeries { get; set; }
        public List<string> VentasPorPeriodoLabels { get; set; }
        public Func<double, string> VentasPorPeriodoYFormatter { get; set; }

        // Inventario bajo stock
        public SeriesCollection InventarioStockSeries { get; set; }
        public List<string> InventarioStockLabels { get; set; }

        // Ventas por categoría
        public SeriesCollection CategoriasPieSeries { get; set; }
        public SeriesCollection CategoriasSeries { get; set; }
        public List<string> CategoriasLabels { get; set; }
        public Func<double, string> CategoriasYFormatter { get; set; }

        #endregion

        public ReportesPage()
        {
            InitializeComponent();
            _reportesService = new ReportesService();

            try
            {
                _telegramService = TelegramService.GetInstance();
            }
            catch (Exception ex)
            {
                // Manejar silenciosamente el error del servicio de Telegram
                Console.WriteLine($"Error al inicializar TelegramService: {ex.Message}");
            }

            // Inicializar fechas
            dpFechaDesde.SelectedDate = DateTime.Now.AddMonths(-1);
            dpFechaHasta.SelectedDate = DateTime.Now;

            // Inicializar valor por defecto para stock mínimo
            txtStockMinimo.Text = "10";

            // Inicializar propiedades de binding
            InicializarPropiedadesBinding();

            // Establecer DataContext
            DataContext = this;

            // Cargar datos antes de seleccionar el tipo de reporte
            CargarDatos();

            // Seleccionar "Último mes" en combos de período
            cmbPeriodoProductos.SelectedIndex = 0;
            cmbPeriodoCategorias.SelectedIndex = 0;

            // Seleccionar primer tipo de reporte después de cargar los datos
            if (_datosInicializados)
            {
                cmbTipoReporte.SelectedIndex = 0;
            }
        }

        private void CargarDatos()
        {
            try
            {
                // No necesitamos cargar todos los datos aquí, ya que el ReportesService
                // se encargará de obtener los datos necesarios para cada reporte
                _datosInicializados = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar reportes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbTipoReporte_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_datosInicializados || cmbTipoReporte.SelectedIndex < 0)
                return;

            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar tipo de reporte: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbPeriodoProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_datosInicializados || cmbPeriodoProductos.SelectedIndex < 0)
                return;

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
                DateTime fechaInicio = _reportesService.ObtenerFechaInicioPeriodo(cmbPeriodoProductos.SelectedIndex);

                // Obtener productos más vendidos desde el servicio
                var resultados = _reportesService.ObtenerProductosMasVendidos(fechaInicio);

                // Mostrar resultados en la tabla
                dgProductosMasVendidos.ItemsSource = resultados;

                // Actualizar gráficos
                ActualizarGraficoProductosMasVendidos(resultados);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte de productos más vendidos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                dgProductosMasVendidos.ItemsSource = new List<object>();
                ActualizarGraficoProductosMasVendidos(new List<ProductoVendidoDTO>());
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

                // Obtener ventas por período desde el servicio
                var ventasPorFecha = _reportesService.ObtenerVentasPorPeriodo(fechaDesde, fechaHasta);
                dgVentasPorPeriodo.ItemsSource = ventasPorFecha;

                // Actualizar gráficos
                ActualizarGraficoVentasPorPeriodo(ventasPorFecha);

                // Obtener y mostrar resumen
                var resumen = _reportesService.ObtenerResumenVentas(fechaDesde, fechaHasta);
                ActualizarResumenVentas(resumen.TotalVentas, resumen.TotalPedidos, resumen.PromedioPedido);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte de ventas por período: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                dgVentasPorPeriodo.ItemsSource = new List<object>();
                ActualizarGraficoVentasPorPeriodo(new List<VentaPorPeriodoDTO>());
                ActualizarResumenVentas(0, 0, 0);
            }
        }

        private void ActualizarResumenVentas(decimal totalVentas, int totalPedidos, decimal promedioPedido)
        {
            // Actualizar tarjetas de resumen
            txtTotalVentas.Text = string.Format("${0:N2}", totalVentas);
            txtTotalPedidos.Text = totalPedidos.ToString();
            txtPromedioPedido.Text = string.Format("${0:N2}", promedioPedido);
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

                // Obtener inventario bajo stock desde el servicio
                var inventarioBajoStock = _reportesService.ObtenerInventarioBajoStock(stockMinimo);
                dgInventarioBajoStock.ItemsSource = inventarioBajoStock;

                // Actualizar gráficos
                ActualizarGraficoInventarioBajoStock(inventarioBajoStock);

                // Obtener y mostrar resumen
                var resumen = _reportesService.ObtenerResumenInventario(stockMinimo);
                ActualizarResumenInventario(resumen.SinStock, resumen.StockCritico, resumen.StockBajo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte de inventario bajo stock: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                dgInventarioBajoStock.ItemsSource = new List<object>();
                ActualizarGraficoInventarioBajoStock(new List<Inventario>());
                ActualizarResumenInventario(0, 0, 0);
            }
        }

        private void cmbPeriodoCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_datosInicializados || cmbPeriodoCategorias.SelectedIndex < 0)
                return;

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
                DateTime fechaInicio = _reportesService.ObtenerFechaInicioPeriodo(cmbPeriodoCategorias.SelectedIndex);

                // Obtener ventas por categoría desde el servicio
                // Usamos el método más detallado para asegurar que tengamos datos completos
                var resultadosDetallados = _reportesService.ObtenerVentasDetalladoPorCategoria(fechaInicio);

                // Convertir a formato para la tabla
                var resultadosTabla = resultadosDetallados.Select(c => new VentaPorCategoriaDTO
                {
                    CategoriaId = c.Id,
                    Nombre = c.Nombre,
                    CantidadProductos = c.TotalProductos,
                    CantidadVendida = c.TotalUnidadesVendidas,
                    TotalVentas = c.TotalVentas
                }).ToList();

                dgVentasPorCategoria.ItemsSource = resultadosTabla;

                // Actualizar gráficos con los datos detallados para asegurar consistencia
                ActualizarGraficoVentasPorCategoria(resultadosTabla);

                // Verificar si tenemos datos
                if (!resultadosTabla.Any())
                {
                    MessageBox.Show("No se encontraron ventas por categoría en el período seleccionado.",
                        "Sin datos", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte de ventas por categoría: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                dgVentasPorCategoria.ItemsSource = new List<object>();
                ActualizarGraficoVentasPorCategoria(new List<VentaPorCategoriaDTO>());
            }
        }

        private void btnEnviarPromocion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EnviarPromocionDialog dialog = new EnviarPromocionDialog();
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir diálogo de promociones: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InicializarPropiedadesBinding()
        {
            // Inicializar colecciones para gráficos
            ProductosVendidosSeries = new SeriesCollection();
            ProductosVendidosLabels = new List<string>();
            ProductosVendidosPieSeries = new SeriesCollection();

            VentasPorPeriodoSeries = new SeriesCollection();
            VentasPorPeriodoLabels = new List<string>();
            VentasPorPeriodoYFormatter = value => string.Format("${0:N0}", value);

            InventarioStockSeries = new SeriesCollection();
            InventarioStockLabels = new List<string>();

            CategoriasPieSeries = new SeriesCollection();
            CategoriasSeries = new SeriesCollection();
            CategoriasLabels = new List<string>();
            CategoriasYFormatter = value => string.Format("${0:N0}", value);
        }

        private void ActualizarGraficoProductosMasVendidos(List<ProductoVendidoDTO> resultados)
        {
            try
            {
                // Verificar si estamos en el hilo de la UI
                if (!System.Windows.Threading.Dispatcher.CurrentDispatcher.CheckAccess())
                {
                    // Estamos en otro hilo, ejecutar en el hilo de la UI
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() =>
                        ActualizarGraficoProductosMasVendidos(resultados));
                    return;
                }

                // Recrear las colecciones completamente para evitar problemas de binding
                ProductosVendidosSeries = new SeriesCollection();
                ProductosVendidosLabels = new List<string>();
                ProductosVendidosPieSeries = new SeriesCollection();

                // Si no hay resultados, no actualizar gráficos
                if (resultados == null || !resultados.Any())
                    return;

                // Limitar a los 5 primeros para el gráfico
                var topProductos = resultados.Take(5).ToList();

                // Preparar datos para gráfico de barras
                var valoresVentas = new ChartValues<double>();
                foreach (var producto in topProductos)
                {
                    ProductosVendidosLabels.Add(producto.Nombre);
                    valoresVentas.Add(producto.CantidadVendida);
                }

                // Crear y agregar la serie
                ProductosVendidosSeries.Add(new ColumnSeries
                {
                    Title = "Cantidad Vendida",
                    Values = valoresVentas,
                    Fill = new SolidColorBrush(Colors.DodgerBlue)
                });

                // Preparar datos para gráfico de pastel
                foreach (var producto in topProductos)
                {
                    ProductosVendidosPieSeries.Add(new PieSeries
                    {
                        Title = producto.Nombre,
                        Values = new ChartValues<double> { (double)producto.TotalVentas },
                        DataLabels = true,
                        LabelPoint = point => string.Format("${0:N0}", point.Y)
                    });
                }

                // Notificar cambios en las propiedades
                NotificarCambiosGraficos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en ActualizarGraficoProductosMasVendidos: {ex.Message}",
                    "Error en gráfico", MessageBoxButton.OK, MessageBoxImage.Error);

                // Inicializar con colecciones vacías en caso de error
                ProductosVendidosSeries = new SeriesCollection();
                ProductosVendidosLabels = new List<string>();
                ProductosVendidosPieSeries = new SeriesCollection();
            }
        }

        private void ActualizarGraficoVentasPorPeriodo(List<VentaPorPeriodoDTO> ventasPorFecha)
        {
            try
            {
                // Limpiar series y etiquetas
                VentasPorPeriodoSeries.Clear();
                VentasPorPeriodoLabels.Clear();

                // Si no hay resultados, no actualizar gráficos
                if (ventasPorFecha == null || !ventasPorFecha.Any())
                    return;

                // Preparar datos para gráfico de línea
                var valoresVentas = new ChartValues<double>();

                foreach (var venta in ventasPorFecha)
                {
                    VentasPorPeriodoLabels.Add(venta.Fecha.ToString("dd/MM"));
                    valoresVentas.Add((double)venta.TotalVentas);
                }

                VentasPorPeriodoSeries.Add(new LineSeries
                {
                    Title = "Total Ventas",
                    Values = valoresVentas,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    LineSmoothness = 0,
                    Stroke = new SolidColorBrush(Colors.ForestGreen),
                    Fill = new SolidColorBrush(Color.FromArgb(50, 34, 139, 34))
                });

                // Notificar cambios
                NotificarCambiosGraficos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en ActualizarGraficoVentasPorPeriodo: {ex.Message}",
                    "Error en gráfico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActualizarGraficoInventarioBajoStock(List<Inventario> inventarioBajoStock)
        {
            try
            {
                // Limpiar series y etiquetas
                InventarioStockSeries.Clear();
                InventarioStockLabels.Clear();

                // Si no hay resultados, no actualizar gráficos
                if (inventarioBajoStock == null || !inventarioBajoStock.Any())
                    return;

                // Tomar los 10 productos con menos stock
                var topProductosBajoStock = inventarioBajoStock.Take(10).ToList();

                // Preparar datos para gráfico de barras horizontales
                var valoresStock = new ChartValues<double>();

                foreach (var inventario in topProductosBajoStock)
                {
                    string nombreProducto = inventario.Prenda?.Nombre ?? "Desconocido";
                    string etiqueta = $"{nombreProducto} ({inventario.Talla}, {inventario.Color})";
                    InventarioStockLabels.Add(etiqueta.Length > 20 ? etiqueta.Substring(0, 20) + "..." : etiqueta);
                    valoresStock.Add(inventario.Cantidad);
                }

                InventarioStockSeries.Add(new RowSeries
                {
                    Title = "Stock",
                    Values = valoresStock,
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString(),
                    Fill = new SolidColorBrush(Colors.OrangeRed)
                });

                // Notificar cambios
                NotificarCambiosGraficos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en ActualizarGraficoInventarioBajoStock: {ex.Message}",
                    "Error en gráfico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActualizarGraficoVentasPorCategoria(List<VentaPorCategoriaDTO> resultados)
        {
            try
            {
                // Limpiar series y etiquetas
                CategoriasPieSeries.Clear();
                CategoriasSeries.Clear();
                CategoriasLabels.Clear();

                // Si no hay resultados, no actualizar gráficos
                if (resultados == null || !resultados.Any())
                    return;

                // Colores para las categorías
                var colores = new List<SolidColorBrush>
                {
                    new SolidColorBrush(Colors.DodgerBlue),
                    new SolidColorBrush(Colors.OrangeRed),
                    new SolidColorBrush(Colors.ForestGreen),
                    new SolidColorBrush(Colors.Purple),
                    new SolidColorBrush(Colors.Gold),
                    new SolidColorBrush(Colors.Crimson),
                    new SolidColorBrush(Colors.Teal)
                };

                // Preparar datos para gráfico de pastel
                int colorIndex = 0;
                foreach (var categoria in resultados)
                {
                    var color = colores[colorIndex % colores.Count];

                    CategoriasPieSeries.Add(new PieSeries
                    {
                        Title = categoria.Nombre,
                        Values = new ChartValues<double> { (double)categoria.TotalVentas },
                        DataLabels = true,
                        LabelPoint = point => string.Format("{0}: ${1:N0}", categoria.Nombre, point.Y),
                        Fill = color
                    });

                    colorIndex++;
                }

                // Preparar datos para gráfico de barras
                var valoresVentas = new ChartValues<double>();

                foreach (var categoria in resultados)
                {
                    CategoriasLabels.Add(categoria.Nombre);
                    valoresVentas.Add((double)categoria.TotalVentas);
                }

                CategoriasSeries.Add(new ColumnSeries
                {
                    Title = "Total Ventas",
                    Values = valoresVentas,
                    DataLabels = true,
                    LabelPoint = point => string.Format("${0:N0}", point.Y),
                    Fill = new SolidColorBrush(Colors.DodgerBlue)
                });

                // Notificar cambios
                NotificarCambiosGraficos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en ActualizarGraficoVentasPorCategoria: {ex.Message}",
                    "Error en gráfico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActualizarResumenInventario(int sinStock, int stockCritico, int stockBajo)
        {
            // Actualizar indicadores de estado de inventario
            txtProductosSinStock.Text = sinStock.ToString();
            txtProductosStockCritico.Text = stockCritico.ToString();
            txtProductosStockBajo.Text = stockBajo.ToString();
        }

        private void NotificarCambiosGraficos()
        {
            this.DataContext = null;
            this.DataContext = this;
        }
    }
}
