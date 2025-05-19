using ENTITIES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TryOn.BLL;

namespace GUI
{
    public partial class VentasPage : Page
    {
        private readonly PedidoService _pedidoService;
        private readonly Usuario _usuarioActual;
        private List<Pedido> _pedidos;

        public VentasPage(Usuario usuario)
        {
            InitializeComponent();
            _pedidoService = new PedidoService();
            _usuarioActual = usuario;

            // Seleccionar "Todos" en combo de estados
            cmbEstado.SelectedIndex = 0;

            CargarDatos();
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
            mainWindow.NavegarACatalogo(); // Usar el método público en lugar de btnCatalogo_Click
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

                // Cambiar a la pestaña de detalles
                ((TabControl)this.Parent).SelectedIndex = 1;
            }
        }

        private void btnCambiarEstado_Click(object sender, RoutedEventArgs e)
        {
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
    }
}
