using ENTITIES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TryOn.BLL;

namespace GUI
{
    public partial class PrendaDialog : Window
    {
        private readonly PrendaService _prendaService;
        private Prenda _prenda;
        private bool _esEdicion;

        public PrendaDialog()
        {
            InitializeComponent();
            _prendaService = new PrendaService();
            _prenda = new Prenda();
            _esEdicion = false;

            CargarCategorias();
        }

        public PrendaDialog(Prenda prenda)
        {
            InitializeComponent();
            _prendaService = new PrendaService();
            _prenda = prenda;
            _esEdicion = true;

            txtTitulo.Text = "Editar Prenda";

            CargarCategorias();
            CargarDatosPrenda();
        }

        private void CargarCategorias()
        {
            try
            {
                // Obtener categorías desde la base de datos
                // En un caso real, esto debería venir de un servicio de categorías
                var categorias = new List<Categoria>
                {
                    new Categoria { Id = 1, Nombre = "Camisetas" },
                    new Categoria { Id = 2, Nombre = "Pantalones" },
                    new Categoria { Id = 3, Nombre = "Vestidos" },
                    new Categoria { Id = 4, Nombre = "Calzado" },
                    new Categoria { Id = 5, Nombre = "Accesorios" }
                };

                cmbCategoria.ItemsSource = categorias;
                cmbCategoria.DisplayMemberPath = "Nombre";
                cmbCategoria.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar categorías: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarDatosPrenda()
        {
            txtCodigo.Text = _prenda.Codigo;
            txtNombre.Text = _prenda.Nombre;
            txtDescripcion.Text = _prenda.Descripcion;
            cmbCategoria.SelectedValue = _prenda.CategoriaId;
            txtPrecioVenta.Text = _prenda.PrecioVenta.ToString();
            txtCosto.Text = _prenda.Costo.ToString();
            txtImagenUrl.Text = _prenda.ImagenUrl;
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar campos
                if (string.IsNullOrEmpty(txtCodigo.Text) || string.IsNullOrEmpty(txtNombre.Text) ||
                    cmbCategoria.SelectedItem == null || string.IsNullOrEmpty(txtPrecioVenta.Text) ||
                    string.IsNullOrEmpty(txtCosto.Text))
                {
                    MessageBox.Show("Por favor, complete los campos obligatorios", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Validar valores numéricos
                decimal precioVenta, costo;
                if (!decimal.TryParse(txtPrecioVenta.Text, out precioVenta) || precioVenta <= 0)
                {
                    MessageBox.Show("El precio de venta debe ser un número mayor a cero", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!decimal.TryParse(txtCosto.Text, out costo) || costo <= 0)
                {
                    MessageBox.Show("El costo debe ser un número mayor a cero", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Actualizar datos de la prenda
                _prenda.Codigo = txtCodigo.Text.Trim();
                _prenda.Nombre = txtNombre.Text.Trim();
                _prenda.Descripcion = txtDescripcion.Text.Trim();
                _prenda.CategoriaId = (int)cmbCategoria.SelectedValue;
                _prenda.PrecioVenta = precioVenta;
                _prenda.Costo = costo;
                _prenda.ImagenUrl = txtImagenUrl.Text.Trim();

                // Guardar prenda
                if (_esEdicion)
                {
                    _prendaService.Update(_prenda);
                }
                else
                {
                    _prendaService.Add(_prenda);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar prenda: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
