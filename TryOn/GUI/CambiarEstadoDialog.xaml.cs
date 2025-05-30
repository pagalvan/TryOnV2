using ENTITIES;
using System;
using System.Windows;
using System.Windows.Controls;
using TryOn.BLL;

namespace GUI
{
    public partial class CambiarEstadoDialog : Window
    {
        private readonly PedidoService _pedidoService;
        private readonly Pedido _pedido;

        public CambiarEstadoDialog(Pedido pedido)
        {
            InitializeComponent();
            _pedidoService = new PedidoService();
            _pedido = pedido;

            CargarDatosPedido();
        }

        private void CargarDatosPedido()
        {
            txtPedidoId.Text = _pedido.Id.ToString();
            txtCliente.Text = _pedido.Usuario.NombreCompleto;
            txtEstadoActual.Text = _pedido.Estado;

            // Seleccionar estado actual en el combo
            foreach (ComboBoxItem item in cmbNuevoEstado.Items)
            {
                if (item.Content.ToString() == _pedido.Estado)
                {
                    cmbNuevoEstado.SelectedItem = item;
                    break;
                }
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar selección
                if (cmbNuevoEstado.SelectedItem == null)
                {
                    MessageBox.Show("Por favor, seleccione un estado", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string nuevoEstado = ((ComboBoxItem)cmbNuevoEstado.SelectedItem).Content.ToString();

                // Cambiar estado
                _pedidoService.CambiarEstado(_pedido.Id, nuevoEstado);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar estado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
