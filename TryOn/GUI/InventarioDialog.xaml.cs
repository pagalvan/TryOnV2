using ENTITIES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TryOn.BLL;

namespace GUI
{
    public partial class InventarioDialog : Window
    {
        private readonly InventarioService _inventarioService;
        private readonly PrendaService _prendaService;
        private Inventario _inventario;
        private bool _esEdicion;

        public InventarioDialog()
        {
            InitializeComponent();
            _inventarioService = new InventarioService();
            _prendaService = new PrendaService();
            _inventario = new Inventario();
            _esEdicion = false;

            CargarPrendas();
            cmbTalla.SelectedIndex = 2; // Seleccionar "M" por defecto
        }

        public InventarioDialog(Inventario inventario)
        {
            InitializeComponent();
            _inventarioService = new InventarioService();
            _prendaService = new PrendaService();
            _inventario = inventario;
            _esEdicion = true;

            txtTitulo.Text = "Editar Inventario";

            CargarPrendas();
            CargarDatosInventario();
        }

        private void CargarPrendas()
        {
            try
            {
                var prendas = _prendaService.GetAll().ToList();
                cmbPrenda.ItemsSource = prendas;
                cmbPrenda.DisplayMemberPath = "Nombre";
                cmbPrenda.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar prendas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarDatosInventario()
        {
            cmbPrenda.SelectedValue = _inventario.PrendaId;

            // Seleccionar talla
            foreach (ComboBoxItem item in cmbTalla.Items)
            {
                if (item.Content.ToString() == _inventario.Talla)
                {
                    cmbTalla.SelectedItem = item;
                    break;
                }
            }

            txtColor.Text = _inventario.Color;
            txtCantidad.Text = _inventario.Cantidad.ToString();
            txtUbicacion.Text = _inventario.Ubicacion;
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar campos
                if (cmbPrenda.SelectedItem == null || cmbTalla.SelectedItem == null ||
                    string.IsNullOrEmpty(txtColor.Text) || string.IsNullOrEmpty(txtCantidad.Text))
                {
                    MessageBox.Show("Por favor, complete los campos obligatorios", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Validar valores numéricos
                int cantidad;
                if (!int.TryParse(txtCantidad.Text, out cantidad) || cantidad < 0)
                {
                    MessageBox.Show("La cantidad debe ser un número entero no negativo", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Actualizar datos del inventario
                _inventario.PrendaId = (int)cmbPrenda.SelectedValue;
                _inventario.Talla = ((ComboBoxItem)cmbTalla.SelectedItem).Content.ToString();
                _inventario.Color = txtColor.Text.Trim();
                _inventario.Cantidad = cantidad;
                _inventario.Ubicacion = txtUbicacion.Text.Trim();

                // Guardar inventario
                if (_esEdicion)
                {
                    _inventarioService.Update(_inventario);
                }
                else
                {
                    _inventarioService.Add(_inventario);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar inventario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
