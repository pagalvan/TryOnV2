using ENTITIES;
using System;
using System.Windows;
using TryOn.BLL;

namespace GUI
{
    public partial class AjustarStockDialog : Window
    {
        private readonly InventarioService _inventarioService;
        private readonly Inventario _inventario;

        public AjustarStockDialog(Inventario inventario)
        {
            InitializeComponent();
            _inventarioService = new InventarioService();
            _inventario = inventario;

            CargarDatosInventario();
        }

        private void CargarDatosInventario()
        {
            txtProducto.Text = _inventario.Prenda.Nombre;
            txtTalla.Text = _inventario.Talla;
            txtColor.Text = _inventario.Color;
            txtStockActual.Text = _inventario.Cantidad.ToString();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar cantidad
                int cantidad;
                if (!int.TryParse(txtCantidad.Text, out cantidad))
                {
                    MessageBox.Show("La cantidad debe ser un número entero", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Verificar que el stock no quede negativo
                if (_inventario.Cantidad + cantidad < 0)
                {
                    MessageBox.Show("El stock no puede quedar negativo", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Ajustar stock
                _inventarioService.AjustarStock(_inventario.Id, cantidad);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al ajustar stock: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}