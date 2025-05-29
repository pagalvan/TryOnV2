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

        // Productos más pedidos
        public SeriesCollection ProductosPedidosSeries { get; set; }
        public List<string> ProductosPedidosLabels { get; set; }
        public SeriesCollection ProductosPedidosPieSeries { get; set; }

        // Posibles ganancias por período
        public SeriesCollection PosiblesGananciasPorPeriodoSeries { get; set; }
        public List<string> PosiblesGananciasPorPeriodoLabels { get; set; }
        public Func<double, string> PosiblesGananciasPorPeriodoYFormatter { get; set; }

        // Inventario bajo stock
        public SeriesCollection InventarioStockSeries { get; set; }
        public List<string> InventarioStockLabels { get; set; }

        // Posibles ganancias por categoría
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
                    case 0: // Productos más pedidos
                        GenerarReporteProductosMasPedidos();
                        break;
                    case 1: // Posibles ganancias por período
                        GenerarReportePosiblesGananciasPorPeriodo();
                        break;
                    case 2: // Inventario bajo stock
                        GenerarReporteInventarioBajoStock();
                        break;
                    case 3: // Posibles ganancias por categoría
                        GenerarReportePosiblesGananciasPorCategoria();
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

            GenerarReporteProductosMasPedidos();
        }

        private void btnGenerarReporteProductos_Click(object sender, RoutedEventArgs e)
        {
            GenerarReporteProductosMasPedidos();
        }

        private void GenerarReporteProductosMasPedidos()
        {
            try
            {
                // Obtener fecha de inicio según el período seleccionado
                DateTime fechaInicio = _reportesService.ObtenerFechaInicioPeriodo(cmbPeriodoProductos.SelectedIndex);

                // Obtener productos más pedidos desde el servicio
                var resultados = _reportesService.ObtenerProductosMasPedidos(fechaInicio);

                // Mostrar resultados en la tabla
                dgProductosMasPedidos.ItemsSource = resultados;

                // Actualizar gráficos
                ActualizarGraficoProductosMasPedidos(resultados);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte de productos más pedidos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                dgProductosMasPedidos.ItemsSource = new List<object>();
                ActualizarGraficoProductosMasPedidos(new List<ProductoPedidoDTO>());
            }
        }

        private void btnGenerarReportePosiblesGanancias_Click(object sender, RoutedEventArgs e)
        {
            GenerarReportePosiblesGananciasPorPeriodo();
        }

        private void GenerarReportePosiblesGananciasPorPeriodo()
        {
            try
            {
                // Obtener fechas seleccionadas
                DateTime fechaDesde = dpFechaDesde.SelectedDate ?? DateTime.Now.AddMonths(-1);
                DateTime fechaHasta = dpFechaHasta.SelectedDate ?? DateTime.Now;

                // Obtener posibles ganancias por período desde el servicio
                var posiblesGananciasPorFecha = _reportesService.ObtenerPosiblesGananciasPorPeriodo(fechaDesde, fechaHasta);
                dgPosiblesGananciasPorPeriodo.ItemsSource = posiblesGananciasPorFecha;

                // Actualizar gráficos
                ActualizarGraficoPosiblesGananciasPorPeriodo(posiblesGananciasPorFecha);

                // Obtener y mostrar resumen
                var resumen = _reportesService.ObtenerResumenPosiblesGanancias(fechaDesde, fechaHasta);
                ActualizarResumenPosiblesGanancias(resumen.TotalPosiblesGanancias, resumen.TotalPedidos, resumen.PromedioPedido);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte de posibles ganancias por período: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                dgPosiblesGananciasPorPeriodo.ItemsSource = new List<object>();
                ActualizarGraficoPosiblesGananciasPorPeriodo(new List<PosibleGananciaPorPeriodoDTO>());
                ActualizarResumenPosiblesGanancias(0, 0, 0);
            }
        }

        private void ActualizarResumenPosiblesGanancias(decimal totalPosiblesGanancias, int totalPedidos, decimal promedioPedido)
        {
            // Actualizar tarjetas de resumen
            txtTotalPosiblesGanancias.Text = string.Format("${0:N2}", totalPosiblesGanancias);
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

            GenerarReportePosiblesGananciasPorCategoria();
        }

        private void btnGenerarReporteCategorias_Click(object sender, RoutedEventArgs e)
        {
            GenerarReportePosiblesGananciasPorCategoria();
        }

        private void GenerarReportePosiblesGananciasPorCategoria()
        {
            try
            {
                // Obtener fecha de inicio según el período seleccionado
                DateTime fechaInicio = _reportesService.ObtenerFechaInicioPeriodo(cmbPeriodoCategorias.SelectedIndex);

                // Obtener posibles ganancias por categoría desde el servicio
                var resultadosDetallados = _reportesService.ObtenerPosiblesGananciasDetalladoPorCategoria(fechaInicio);

                // Convertir a formato para la tabla
                var resultadosTabla = resultadosDetallados.Select(c => new PosibleGananciaPorCategoriaDTO
                {
                    CategoriaId = c.Id,
                    Nombre = c.Nombre,
                    CantidadProductos = c.TotalProductos,
                    CantidadPedida = c.TotalUnidadesPedidas,
                    TotalPosiblesGanancias = c.TotalPosiblesGanancias
                }).ToList();

                dgPosiblesGananciasPorCategoria.ItemsSource = resultadosTabla;

                // Actualizar gráficos con los datos detallados para asegurar consistencia
                ActualizarGraficoPosiblesGananciasPorCategoria(resultadosTabla);

                // Verificar si tenemos datos
                if (!resultadosTabla.Any())
                {
                    MessageBox.Show("No se encontraron pedidos por categoría en el período seleccionado.",
                        "Sin datos", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte de posibles ganancias por categoría: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                dgPosiblesGananciasPorCategoria.ItemsSource = new List<object>();
                ActualizarGraficoPosiblesGananciasPorCategoria(new List<PosibleGananciaPorCategoriaDTO>());
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
            ProductosPedidosSeries = new SeriesCollection();
            ProductosPedidosLabels = new List<string>();
            ProductosPedidosPieSeries = new SeriesCollection();

            PosiblesGananciasPorPeriodoSeries = new SeriesCollection();
            PosiblesGananciasPorPeriodoLabels = new List<string>();
            PosiblesGananciasPorPeriodoYFormatter = value => string.Format("${0:N0}", value);

            InventarioStockSeries = new SeriesCollection();
            InventarioStockLabels = new List<string>();

            CategoriasPieSeries = new SeriesCollection();
            CategoriasSeries = new SeriesCollection();
            CategoriasLabels = new List<string>();
            CategoriasYFormatter = value => string.Format("${0:N0}", value);
        }

        private void ActualizarGraficoProductosMasPedidos(List<ProductoPedidoDTO> resultados)
        {
            try
            {
                // Verificar si estamos en el hilo de la UI
                if (!System.Windows.Threading.Dispatcher.CurrentDispatcher.CheckAccess())
                {
                    // Estamos en otro hilo, ejecutar en el hilo de la UI
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() =>
                        ActualizarGraficoProductosMasPedidos(resultados));
                    return;
                }

                // Recrear las colecciones completamente para evitar problemas de binding
                ProductosPedidosSeries = new SeriesCollection();
                ProductosPedidosLabels = new List<string>();
                ProductosPedidosPieSeries = new SeriesCollection();

                // Si no hay resultados, no actualizar gráficos
                if (resultados == null || !resultados.Any())
                    return;

                // Limitar a los 5 primeros para el gráfico
                var topProductos = resultados.Take(5).ToList();

                // Preparar datos para gráfico de barras
                var valoresPedidos = new ChartValues<double>();
                foreach (var producto in topProductos)
                {
                    ProductosPedidosLabels.Add(producto.Nombre);
                    valoresPedidos.Add(producto.CantidadPedida);
                }

                // Crear y agregar la serie
                ProductosPedidosSeries.Add(new ColumnSeries
                {
                    Title = "Cantidad Pedida",
                    Values = valoresPedidos,
                    Fill = new SolidColorBrush(Colors.DodgerBlue)
                });

                // Preparar datos para gráfico de pastel
                foreach (var producto in topProductos)
                {
                    ProductosPedidosPieSeries.Add(new PieSeries
                    {
                        Title = producto.Nombre,
                        Values = new ChartValues<double> { (double)producto.TotalPosiblesGanancias },
                        DataLabels = true,
                        LabelPoint = point => string.Format("${0:N0}", point.Y)
                    });
                }

                // Notificar cambios en las propiedades
                NotificarCambiosGraficos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en ActualizarGraficoProductosMasPedidos: {ex.Message}",
                    "Error en gráfico", MessageBoxButton.OK, MessageBoxImage.Error);

                // Inicializar con colecciones vacías en caso de error
                ProductosPedidosSeries = new SeriesCollection();
                ProductosPedidosLabels = new List<string>();
                ProductosPedidosPieSeries = new SeriesCollection();
            }
        }

        private void ActualizarGraficoPosiblesGananciasPorPeriodo(List<PosibleGananciaPorPeriodoDTO> posiblesGananciasPorFecha)
        {
            try
            {
                // Limpiar series y etiquetas
                PosiblesGananciasPorPeriodoSeries.Clear();
                PosiblesGananciasPorPeriodoLabels.Clear();

                // Si no hay resultados, no actualizar gráficos
                if (posiblesGananciasPorFecha == null || !posiblesGananciasPorFecha.Any())
                    return;

                // Preparar datos para gráfico de línea
                var valoresPosiblesGanancias = new ChartValues<double>();

                foreach (var posibleGanancia in posiblesGananciasPorFecha)
                {
                    PosiblesGananciasPorPeriodoLabels.Add(posibleGanancia.Fecha.ToString("dd/MM"));
                    valoresPosiblesGanancias.Add((double)posibleGanancia.TotalPosiblesGanancias);
                }

                PosiblesGananciasPorPeriodoSeries.Add(new LineSeries
                {
                    Title = "Posibles Ganancias",
                    Values = valoresPosiblesGanancias,
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
                MessageBox.Show($"Error en ActualizarGraficoPosiblesGananciasPorPeriodo: {ex.Message}",
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

        private void ActualizarGraficoPosiblesGananciasPorCategoria(List<PosibleGananciaPorCategoriaDTO> resultados)
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
                        Values = new ChartValues<double> { (double)categoria.TotalPosiblesGanancias },
                        DataLabels = true,
                        LabelPoint = point => string.Format("{0}: ${1:N0}", categoria.Nombre, point.Y),
                        Fill = color
                    });

                    colorIndex++;
                }

                // Preparar datos para gráfico de barras
                var valoresPosiblesGanancias = new ChartValues<double>();

                foreach (var categoria in resultados)
                {
                    CategoriasLabels.Add(categoria.Nombre);
                    valoresPosiblesGanancias.Add((double)categoria.TotalPosiblesGanancias);
                }

                CategoriasSeries.Add(new ColumnSeries
                {
                    Title = "Posibles Ganancias",
                    Values = valoresPosiblesGanancias,
                    DataLabels = true,
                    LabelPoint = point => string.Format("${0:N0}", point.Y),
                    Fill = new SolidColorBrush(Colors.DodgerBlue)
                });

                // Notificar cambios
                NotificarCambiosGraficos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en ActualizarGraficoPosiblesGananciasPorCategoria: {ex.Message}",
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