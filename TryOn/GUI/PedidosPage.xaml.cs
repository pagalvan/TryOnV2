using ENTITIES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TryOn.BLL;

namespace GUI
{
    public partial class PedidosPage : Page
    {
        private readonly PedidoService _pedidoService;
        private readonly Usuario _usuarioActual;
        private List<Pedido> _pedidos;

        public PedidosPage(Usuario usuario)
        {
            InitializeComponent();
            _pedidoService = new PedidoService();
            _usuarioActual = usuario;

            // Establecer el DataContext para que el binding funcione
            this.DataContext = this;

            // Seleccionar "Todos" en combo de estados
            cmbEstado.SelectedIndex = 0;

            // Configurar visibilidad según tipo de usuario
            ConfigurarVisibilidadSegunUsuario();

            CargarDatos();
        }

        private void ConfigurarVisibilidadSegunUsuario()
        {
            // Si no es administrador, ocultar columna de cliente y información de cliente
            if (!_usuarioActual.EsAdmin)
            {
                colCliente.Visibility = Visibility.Collapsed;
                txtClienteInfo.Visibility = Visibility.Collapsed;
            }
        }

        private void CargarDatos()
        {
            try
            {
                // Cargar pedidos
                _pedidos = _pedidoService.GetAll().ToList();

                // Si no es administrador, filtrar solo los pedidos del usuario actual
                if (!_usuarioActual.EsAdmin)
                {
                    _pedidos = _pedidos.Where(p => p.UsuarioId == _usuarioActual.Id).ToList();
                }

                FiltrarPedidos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FiltrarPedidos()
        {
            try
            {
                var pedidos = _pedidos;

                // Filtrar por estado
                if (cmbEstado.SelectedIndex > 0) // Si no es "Todos"
                {
                    string estado = ((ComboBoxItem)cmbEstado.SelectedItem).Content.ToString();
                    pedidos = pedidos.Where(p => p.Estado == estado).ToList();
                }

                // Filtrar por texto de búsqueda
                if (!string.IsNullOrEmpty(txtBuscarPedido.Text))
                {
                    string busqueda = txtBuscarPedido.Text.ToLower();
                    pedidos = pedidos.Where(p =>
                        p.Id.ToString().Contains(busqueda) ||
                        p.Usuario.NombreCompleto.ToLower().Contains(busqueda) ||
                        p.Estado.ToLower().Contains(busqueda)
                    ).ToList();
                }

                dgPedidos.ItemsSource = pedidos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar pedidos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnNuevoPedido_Click(object sender, RoutedEventArgs e)
        {
            // Redirigir a la página de catálogo
            MainWindow mainWindow = (MainWindow)Window.GetWindow(this);
            mainWindow.NavegarACatalogo();
        }

        private void btnEliminarPedido_Click(object sender, RoutedEventArgs e)
        {
            // Verificar permisos de administrador
            if (!_usuarioActual.EsAdmin)
            {
                MessageBox.Show("No tienes permisos para eliminar pedidos.", "Acceso Denegado",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Button btn = (Button)sender;
            int pedidoId = (int)btn.Tag;

            // Obtener el pedido para mostrar información en la confirmación
            Pedido pedido = _pedidoService.GetById(pedidoId);
            if (pedido == null)
            {
                MessageBox.Show("El pedido no fue encontrado.", "Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Mostrar diálogo de confirmación
            string mensaje = $"¿Estás seguro de que deseas eliminar el pedido #{pedido.Id}?\n\n" +
                            $"Cliente: {pedido.Usuario.NombreCompleto}\n" +
                            $"Fecha: {pedido.FechaPedido:dd/MM/yyyy HH:mm}\n" +
                            $"Total: ${pedido.Total:N2}\n\n" +
                            "Esta acción no se puede deshacer.";

            MessageBoxResult resultado = MessageBox.Show(mensaje, "Confirmar Eliminación",
                                                        MessageBoxButton.YesNo,
                                                        MessageBoxImage.Question,
                                                        MessageBoxResult.No);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    // Llamar al método Delete del servicio
                    _pedidoService.Delete(pedidoId);

                    // Mostrar mensaje de éxito
                    MessageBox.Show("El pedido ha sido eliminado exitosamente.", "Eliminación Exitosa",
                                   MessageBoxButton.OK, MessageBoxImage.Information);

                    // Recargar los datos para actualizar la vista
                    CargarDatos();

                    // Limpiar los detalles del pedido si el pedido eliminado estaba seleccionado
                    if (dgPedidos.SelectedItem != null && ((Pedido)dgPedidos.SelectedItem).Id == pedidoId)
                    {
                        LimpiarDetallesPedido();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar el pedido: {ex.Message}", "Error",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LimpiarDetallesPedido()
        {
            txtPedidoId.Text = "";
            txtPedidoCliente.Text = "";
            txtPedidoFecha.Text = "";
            txtPedidoEstado.Text = "";
            txtPedidoTotal.Text = "";
            txtPedidoDireccion.Text = "";
            dgDetallesPedido.ItemsSource = null;
        }

        private void btnActualizarPedidos_Click(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void cmbEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FiltrarPedidos();
        }

        private void btnBuscarPedido_Click(object sender, RoutedEventArgs e)
        {
            FiltrarPedidos();
        }

        private void dgPedidos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPedidos.SelectedItem != null)
            {
                Pedido pedido = (Pedido)dgPedidos.SelectedItem;
                MostrarDetallesPedido(pedido);
            }
        }

        private void btnVerDetalles_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int pedidoId = (int)btn.Tag;

            Pedido pedido = _pedidoService.GetById(pedidoId);
            if (pedido != null)
            {
                MostrarDetallesPedido(pedido);
            }
        }

        private void btnCambiarEstado_Click(object sender, RoutedEventArgs e)
        {
            // Verificar nuevamente que sea administrador por seguridad
            if (!_usuarioActual.EsAdmin)
            {
                MessageBox.Show("No tienes permisos para cambiar el estado de pedidos.", "Acceso Denegado",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Button btn = (Button)sender;
            int pedidoId = (int)btn.Tag;

            Pedido pedido = _pedidoService.GetById(pedidoId);
            if (pedido != null)
            {
                // Mostrar diálogo para cambiar estado
                CambiarEstadoDialog dialog = new CambiarEstadoDialog(pedido);
                if (dialog.ShowDialog() == true)
                {
                    CargarDatos();

                    // Si el pedido seleccionado es el que se modificó, actualizar detalles
                    if (dgPedidos.SelectedItem != null && ((Pedido)dgPedidos.SelectedItem).Id == pedidoId)
                    {
                        MostrarDetallesPedido(_pedidoService.GetById(pedidoId));
                    }
                }
            }
        }

        private void MostrarDetallesPedido(Pedido pedido)
        {
            // Mostrar información del pedido
            txtPedidoId.Text = pedido.Id.ToString();
            txtPedidoCliente.Text = pedido.Usuario.NombreCompleto;
            txtPedidoFecha.Text = pedido.FechaPedido.ToString("dd/MM/yyyy HH:mm");
            txtPedidoEstado.Text = pedido.Estado;
            txtPedidoTotal.Text = $"${pedido.Total:N2}";
            txtPedidoDireccion.Text = pedido.DireccionEnvio ?? "No especificada";

            // Mostrar detalles del pedido
            dgDetallesPedido.ItemsSource = pedido.Detalles;
        }

        // Propiedad para binding de visibilidad del botón cambiar estado
        public bool EsAdmin => _usuarioActual.EsAdmin;
    }
}