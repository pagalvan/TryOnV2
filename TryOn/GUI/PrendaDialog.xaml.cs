using ENTITIES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TryOn.BLL;

namespace GUI
{
    public partial class PrendaDialog : Window
    {
        private readonly PrendaService _prendaService;
        private readonly CategoriaService _categoryService;
        private Prenda _prenda;
        private bool _isEditing;

        public PrendaDialog()
        {
            InitializeComponent();
            _prendaService = new PrendaService();
            _categoryService = new CategoriaService();
            _isEditing = false;
            txtTitulo.Text = "Agregar Prenda";
        }

        public PrendaDialog(Prenda prenda) : this()
        {
            _prenda = prenda;
            _isEditing = true;
            txtTitulo.Text = "Editar Prenda";
            CargarDatos();
        }

        private void CargarDatos()
        {
            if (_prenda != null)
            {
                txtCodigo.Text = _prenda.Codigo;
                txtNombre.Text = _prenda.Nombre;
                txtDescripcion.Text = _prenda.Descripcion;
                txtCategoria.Text = _prenda.Categoria?.Nombre ?? "";
                txtPrecioVenta.Text = _prenda.PrecioVenta.ToString("F2");
                txtCosto.Text = _prenda.Costo.ToString("F2");
                txtImagenUrl.Text = _prenda.ImagenUrl;
            }
        }

        private void btnSugerirCategoria_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var categorias = _categoryService.GetAll().ToList();
                if (categorias.Any())
                {
                    CategorySuggestionDialog dialog = new CategorySuggestionDialog(categorias);
                    if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.SelectedCategoryName))
                    {
                        txtCategoria.Text = dialog.SelectedCategoryName;
                    }
                }
                else
                {
                    MessageBox.Show("No hay categorías existentes.", "Información",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar categorías: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidarDatos())
                    return;

                // Crear o obtener la categoría
                Categoria categoria = null;
                if (!string.IsNullOrWhiteSpace(txtCategoria.Text))
                {
                    categoria = _categoryService.CreateOrGetCategory(txtCategoria.Text.Trim());
                }

                if (_isEditing)
                {
                    _prenda.Codigo = txtCodigo.Text.Trim();
                    _prenda.Nombre = txtNombre.Text.Trim();
                    _prenda.Descripcion = txtDescripcion.Text.Trim();
                    _prenda.CategoriaId = categoria?.Id ?? 0;
                    _prenda.Categoria = categoria;
                    _prenda.PrecioVenta = decimal.Parse(txtPrecioVenta.Text);
                    _prenda.Costo = decimal.Parse(txtCosto.Text);
                    _prenda.ImagenUrl = txtImagenUrl.Text.Trim();

                    _prendaService.Update(_prenda);
                    MessageBox.Show("Prenda actualizada correctamente.", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var nuevaPrenda = new Prenda
                    {
                        Codigo = txtCodigo.Text.Trim(),
                        Nombre = txtNombre.Text.Trim(),
                        Descripcion = txtDescripcion.Text.Trim(),
                        CategoriaId = categoria?.Id ?? 0,
                        Categoria = categoria,
                        PrecioVenta = decimal.Parse(txtPrecioVenta.Text),
                        Costo = decimal.Parse(txtCosto.Text),
                        ImagenUrl = txtImagenUrl.Text.Trim()
                    };

                    _prendaService.Add(nuevaPrenda);
                    MessageBox.Show("Prenda agregada correctamente.", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la prenda: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(txtCodigo.Text))
            {
                MessageBox.Show("El código es requerido.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCodigo.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es requerido.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return false;
            }

            if (!decimal.TryParse(txtPrecioVenta.Text, out decimal precioVenta) || precioVenta < 0)
            {
                MessageBox.Show("El precio de venta debe ser un número válido mayor o igual a 0.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrecioVenta.Focus();
                return false;
            }

            if (!decimal.TryParse(txtCosto.Text, out decimal costo) || costo < 0)
            {
                MessageBox.Show("El costo debe ser un número válido mayor o igual a 0.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCosto.Focus();
                return false;
            }

            return true;
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}