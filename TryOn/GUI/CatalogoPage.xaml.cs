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
            // Cargar combo de categorías (obtener categorías únicas de las prendas)
            var categorias = _prendas.Select(p => p.Categoria).Where(c => c != null).GroupBy(c => c.Id).Select(g => g.First()).ToList();
            cmbCategoria.ItemsSource = categorias;
            cmbCategoria.DisplayMemberPath = "Nombre";
            cmbCategoria.SelectedValuePath = "Id";
            cmbCategoria.SelectedIndex = -1;

            // Seleccionar "Todas" en combo de tallas
            cmbTalla.SelectedIndex = 0;

            // Cargar combo de colores (obtener colores únicos del inventario)
            var colores = _inventarios.Select(i => i.Color).Distinct().ToList();
            cmbColor.ItemsSource = colores;
            cmbColor.SelectedIndex = -1;
        }

        private void MostrarProductos()
        {
            pnlProductos.Children.Clear();

            // Filtrar inventario según selecciones
            var inventariosFiltrados = _inventarios;

            // Filtrar por categoría
            if (cmbCategoria.SelectedItem != null)
            {
                int categoriaId = ((Categoria)cmbCategoria.SelectedItem).Id;
                inventariosFiltrados = inventariosFiltrados.Where(i => i.Prenda.CategoriaId == categoriaId).ToList();
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

            foreach (var inventario in prendasAgrupadas)
            {
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
                if (!string.IsNullOrEmpty(inventario.Prenda.ImagenUrl))
                {
                    try
                    {
                        imagen.Source = new BitmapImage(new Uri(inventario.Prenda.ImagenUrl));
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
                    Text = inventario.Prenda.Nombre,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 16,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                // Precio del producto
                TextBlock precio = new TextBlock
                {
                    Text = $"${inventario.Prenda.PrecioVenta:N2}",
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
                contenido.Children.Add(precio);
                contenido.Children.Add(btnAgregar);

                tarjeta.Child = contenido;
                pnlProductos.Children.Add(tarjeta);
            }
        }

        private void btnAgregarAlCarrito_Click(object sender, RoutedEventArgs e)
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
                    PrecioUnitario = inventario.Prenda.PrecioVenta,
                    Subtotal = inventario.Prenda.PrecioVenta
                };
                _carrito.Add(detalle);
            }

            // Actualizar contador del carrito
            ActualizarContadorCarrito();

            MessageBox.Show("Producto agregado al carrito", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void btnAumentarCantidad_Click(object sender, RoutedEventArgs e)
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

        private void btnEliminarDelCarrito_Click(object sender, RoutedEventArgs e)
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
                    DireccionEnvio = _usuarioActual.Direccion
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
    }
}