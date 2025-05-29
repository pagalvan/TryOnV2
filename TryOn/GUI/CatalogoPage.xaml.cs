using ENTITIES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TryOn.BLL;

namespace GUI
{
    public partial class CatalogoPage : Page
    {
        private readonly PrendaService _prendaService;
        private readonly InventarioService _inventarioService;
        private readonly PedidoService _pedidoService;
        private readonly CategoriaService _categoriaService; // Aseguramos que esté declarado
        private readonly Usuario _usuarioActual;

        private List<Prenda> _prendas;
        private List<Inventario> _inventarios;
        private List<DetallePedido> _carrito = new List<DetallePedido>();

        public CatalogoPage(Usuario usuario)
        {
            InitializeComponent();
            _prendaService = new PrendaService();
            _inventarioService = new InventarioService();
            _pedidoService = new PedidoService();
            _categoriaService = new CategoriaService(); // Inicializamos el servicio de categorías
            _usuarioActual = usuario;

            CargarDatos();
        }

        private void CargarDatos()
        {
            try
            {
                // Cargar prendas
                _prendas = _prendaService.GetAll().ToList();

                // Cargar inventario
                _inventarios = _inventarioService.GetAll().ToList();

                // Cargar combos
                CargarCombos();

                // Mostrar productos
                MostrarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarCombos()
        {
            try
            {
                // Cargar categorías directamente desde el servicio de categorías
                var categorias = _categoriaService.GetAll().ToList();

                if (categorias == null || !categorias.Any())
                {
                    MessageBox.Show("No se encontraron categorías en la base de datos.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                cmbCategoria.ItemsSource = categorias;
                cmbCategoria.DisplayMemberPath = "Nombre";
                cmbCategoria.SelectedValuePath = "Id";
                cmbCategoria.SelectedIndex = -1;

                // Seleccionar "Todas" en combo de tallas
                cmbTalla.SelectedIndex = 0;

                // Cargar combo de colores (obtener colores únicos del inventario)
                var colores = _inventarios.Select(i => i.Color).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
                cmbColor.ItemsSource = colores;
                cmbColor.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar combos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MostrarProductos()
        {
            try
            {
                pnlProductos.Children.Clear();

                // Filtrar inventario según selecciones
                var inventariosFiltrados = _inventarios;

                // Filtrar por categoría
                if (cmbCategoria.SelectedItem != null)
                {
                    int categoriaId = ((Categoria)cmbCategoria.SelectedItem).Id;

                    // Filtrar prendas por categoría y luego obtener inventarios de esas prendas
                    var prendasFiltradas = _prendas.Where(p => p.CategoriaId == categoriaId).Select(p => p.Id).ToList();
                    inventariosFiltrados = inventariosFiltrados.Where(i => prendasFiltradas.Contains(i.PrendaId)).ToList();
                }

                // Filtrar por talla
                if (cmbTalla.SelectedIndex > 0) // Si no es "Todas"
                {
                    string talla = ((ComboBoxItem)cmbTalla.SelectedItem).Content.ToString();
                    inventariosFiltrados = inventariosFiltrados.Where(i => i.Talla == talla).ToList();
                }

                // Filtrar por color
                if (cmbColor.SelectedItem != null)
                {
                    string color = cmbColor.SelectedItem.ToString();
                    inventariosFiltrados = inventariosFiltrados.Where(i => i.Color == color).ToList();
                }

                // Agrupar por prenda para mostrar una tarjeta por prenda
                var prendasAgrupadas = inventariosFiltrados.GroupBy(i => i.PrendaId).Select(g => g.First()).ToList();

                if (prendasAgrupadas.Count == 0)
                {
                    TextBlock noResults = new TextBlock
                    {
                        Text = "No se encontraron productos con los filtros seleccionados",
                        FontSize = 16,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 50, 0, 0)
                    };
                    pnlProductos.Children.Add(noResults);
                    return;
                }

                foreach (var inventario in prendasAgrupadas)
                {
                    // Obtener la prenda completa desde la colección de prendas
                    var prenda = _prendas.FirstOrDefault(p => p.Id == inventario.PrendaId);
                    if (prenda == null) continue;

                    // Crear tarjeta de producto
                    Border tarjeta = new Border
                    {
                        Width = 220,
                        Margin = new Thickness(10),
                        BorderBrush = new SolidColorBrush(Colors.LightGray),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(5),
                        Background = new SolidColorBrush(Colors.White)
                    };

                    // Contenido de la tarjeta
                    StackPanel contenido = new StackPanel
                    {
                        Margin = new Thickness(10)
                    };

                    // Imagen del producto
                    Image imagen = new Image
                    {
                        Height = 150,
                        Stretch = Stretch.Uniform,
                        Margin = new Thickness(0, 0, 0, 10)
                    };

                    // Si hay imagen, mostrarla, sino mostrar imagen por defecto
                    if (!string.IsNullOrEmpty(prenda.ImagenUrl))
                    {
                        try
                        {
                            imagen.Source = new BitmapImage(new Uri(prenda.ImagenUrl));
                        }
                        catch
                        {
                            imagen.Source = new BitmapImage(new Uri("pack://application:,,,/TryOn;component/Resources/no-image.png"));
                        }
                    }
                    else
                    {
                        imagen.Source = new BitmapImage(new Uri("pack://application:,,,/TryOn;component/Resources/no-image.png"));
                    }

                    // Nombre del producto
                    TextBlock nombre = new TextBlock
                    {
                        Text = prenda.Nombre,
                        FontWeight = FontWeights.SemiBold,
                        FontSize = 16,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 0, 0, 5)
                    };

                    // Categoría del producto
                    TextBlock categoria = new TextBlock
                    {
                        Text = prenda.Categoria?.Nombre ?? "Sin categoría",
                        FontSize = 12,
                        Foreground = new SolidColorBrush(Colors.Gray),
                        Margin = new Thickness(0, 0, 0, 5)
                    };

                    // Precio del producto
                    TextBlock precio = new TextBlock
                    {
                        Text = $"${prenda.PrecioVenta:N2}",
                        FontSize = 14,
                        Margin = new Thickness(0, 0, 0, 10)
                    };

                    // Botón para agregar al carrito
                    Button btnAgregar = new Button
                    {
                        Content = "Agregar al Carrito",
                        Tag = inventario.Id,
                        Margin = new Thickness(0, 10, 0, 0)
                    };
                    btnAgregar.Click += btnAgregarAlCarrito_Click;

                    // Agregar elementos a la tarjeta
                    contenido.Children.Add(imagen);
                    contenido.Children.Add(nombre);
                    contenido.Children.Add(categoria);
                    contenido.Children.Add(precio);
                    contenido.Children.Add(btnAgregar);

                    tarjeta.Child = contenido;
                    pnlProductos.Children.Add(tarjeta);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al mostrar productos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAgregarAlCarrito_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                int inventarioId = (int)btn.Tag;

                // Obtener inventario
                var inventario = _inventarioService.GetById(inventarioId);
                if (inventario == null)
                {
                    MessageBox.Show("Producto no encontrado", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Obtener la prenda completa
                var prenda = _prendas.FirstOrDefault(p => p.Id == inventario.PrendaId);
                if (prenda == null)
                {
                    MessageBox.Show("Información del producto no encontrada", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Verificar si ya está en el carrito
                var itemExistente = _carrito.FirstOrDefault(d => d.InventarioId == inventarioId);
                if (itemExistente != null)
                {
                    // Incrementar cantidad
                    itemExistente.Cantidad++;
                    itemExistente.Subtotal = itemExistente.PrecioUnitario * itemExistente.Cantidad;
                }
                else
                {
                    // Agregar nuevo item al carrito
                    var detalle = new DetallePedido
                    {
                        InventarioId = inventarioId,
                        Inventario = inventario,
                        Cantidad = 1,
                        PrecioUnitario = prenda.PrecioVenta,
                        Subtotal = prenda.PrecioVenta
                    };
                    _carrito.Add(detalle);
                }

                // Actualizar contador del carrito
                ActualizarContadorCarrito();

                MessageBox.Show("Producto agregado al carrito", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar al carrito: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActualizarContadorCarrito()
        {
            int totalItems = _carrito.Sum(d => d.Cantidad);
            btnVerCarrito.Content = $"Ver Carrito ({totalItems})";
        }

        private void ActualizarCarrito()
        {
            lstCarrito.ItemsSource = null;
            lstCarrito.ItemsSource = _carrito;

            decimal total = _carrito.Sum(d => d.Subtotal);
            txtTotal.Text = $"${total:N2}";
        }

        private void btnVerCarrito_Click(object sender, RoutedEventArgs e)
        {
            ActualizarCarrito();
            popupCarrito.IsOpen = true;
        }

        private void btnCerrarCarrito_Click(object sender, RoutedEventArgs e)
        {
            popupCarrito.IsOpen = false;
        }

        private void btnDisminuirCantidad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                int inventarioId = (int)btn.Tag;

                var item = _carrito.FirstOrDefault(d => d.InventarioId == inventarioId);
                if (item != null)
                {
                    if (item.Cantidad > 1)
                    {
                        item.Cantidad--;
                        item.Subtotal = item.PrecioUnitario * item.Cantidad;
                        ActualizarCarrito();
                        ActualizarContadorCarrito();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al disminuir cantidad: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAumentarCantidad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                int inventarioId = (int)btn.Tag;

                var item = _carrito.FirstOrDefault(d => d.InventarioId == inventarioId);
                if (item != null)
                {
                    // Verificar stock disponible
                    var inventario = _inventarioService.GetById(inventarioId);
                    if (inventario != null && item.Cantidad < inventario.Cantidad)
                    {
                        item.Cantidad++;
                        item.Subtotal = item.PrecioUnitario * item.Cantidad;
                        ActualizarCarrito();
                        ActualizarContadorCarrito();
                    }
                    else
                    {
                        MessageBox.Show("No hay suficiente stock disponible", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al aumentar cantidad: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEliminarDelCarrito_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                int inventarioId = (int)btn.Tag;

                var item = _carrito.FirstOrDefault(d => d.InventarioId == inventarioId);
                if (item != null)
                {
                    _carrito.Remove(item);
                    ActualizarCarrito();
                    ActualizarContadorCarrito();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar del carrito: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRealizarPedido_Click(object sender, RoutedEventArgs e)
        {
            if (_carrito.Count == 0)
            {
                MessageBox.Show("El carrito está vacío", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Crear pedido
                Pedido pedido = new Pedido
                {
                    UsuarioId = _usuarioActual.Id,
                    Estado = "Pendiente",
                    Detalles = _carrito,
                    DireccionEnvio = _usuarioActual.Direccion,
                    FechaPedido = DateTime.Now
                };

                // Guardar pedido
                _pedidoService.Add(pedido);

                MessageBox.Show("Pedido realizado con éxito", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiar carrito
                _carrito.Clear();
                ActualizarCarrito();
                ActualizarContadorCarrito();
                popupCarrito.IsOpen = false;

                // Recargar datos
                CargarDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al realizar pedido: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbCategoria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MostrarProductos();
        }

        private void cmbTalla_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MostrarProductos();
        }

        private void cmbColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MostrarProductos();
        }

        // Método para recargar los datos y refrescar la interfaz
        public void RefrescarDatos()
        {
            CargarDatos();
        }
    }
}