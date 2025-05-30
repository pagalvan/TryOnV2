using ENTITIES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TryOn.BLL;

namespace GUI
{
    public partial class InventarioPage : Page
    {
        private readonly PrendaService _prendaService;
        private readonly InventarioService _inventarioService;
        private readonly CategoriaService _categoryService;
        private List<Prenda> _prendas;
        private List<Inventario> _inventarios;
        private List<Categoria> _categorias;

        public InventarioPage()
        {
            InitializeComponent();
            _prendaService = new PrendaService();
            _inventarioService = new InventarioService();
            _categoryService = new CategoriaService();

            CargarDatos();
        }

        private void CargarDatos()
        {
            try
            {
                // Cargar prendas
                _prendas = _prendaService.GetAll().ToList();
                dgPrendas.ItemsSource = _prendas;

                // Cargar inventario
                _inventarios = _inventarioService.GetAll().ToList();
                dgInventario.ItemsSource = _inventarios;

                // Cargar categorías desde el servicio
                _categorias = _categoryService.GetAll().ToList();

                // Cargar combos
                CargarCombos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarCombos()
        {
            // Cargar combo de prendas
            cmbPrenda.ItemsSource = _prendas;
            cmbPrenda.DisplayMemberPath = "Nombre";
            cmbPrenda.SelectedValuePath = "Id";
            cmbPrenda.SelectedIndex = -1;

            // Cargar combo de categorías desde el servicio
            var categoriasCombo = new List<Categoria>();
            categoriasCombo.Add(new Categoria { Id = 0, Nombre = "Todas las categorías" });
            categoriasCombo.AddRange(_categorias);
            
            cmbCategoria.ItemsSource = categoriasCombo;
            cmbCategoria.DisplayMemberPath = "Nombre";
            cmbCategoria.SelectedValuePath = "Id";
            cmbCategoria.SelectedIndex = 0; // Seleccionar "Todas las categorías"

            // Seleccionar "Todas" en combo de tallas
            cmbTalla.SelectedIndex = 0;
        }

        private void btnAgregarPrenda_Click(object sender, RoutedEventArgs e)
        {
            PrendaDialog dialog = new PrendaDialog();
            if (dialog.ShowDialog() == true)
            {
                CargarDatos();
            }
        }

        private void btnEditarPrenda_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int prendaId = (int)btn.Tag;

            Prenda prenda = _prendaService.GetById(prendaId);
            if (prenda != null)
            {
                PrendaDialog dialog = new PrendaDialog(prenda);
                if (dialog.ShowDialog() == true)
                {
                    CargarDatos();
                }
            }
        }

        private void btnEliminarPrenda_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int prendaId = (int)btn.Tag;

            var result = MessageBox.Show(
                "¿Está seguro de eliminar esta prenda?\n\n" +
                "Esto también eliminará todos los registros relacionados (inventario, detalles de pedidos, etc.)\n\n" +
                "Esta acción no se puede deshacer.",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _prendaService.Delete(prendaId);
                    MessageBox.Show("Prenda eliminada correctamente.", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarDatos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar prenda: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnAgregarStock_Click(object sender, RoutedEventArgs e)
        {
            InventarioDialog dialog = new InventarioDialog();
            if (dialog.ShowDialog() == true)
            {
                CargarDatos();
            }
        }

        private void btnEditarInventario_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int inventarioId = (int)btn.Tag;

            Inventario inventario = _inventarioService.GetById(inventarioId);
            if (inventario != null)
            {
                InventarioDialog dialog = new InventarioDialog(inventario);
                if (dialog.ShowDialog() == true)
                {
                    CargarDatos();
                }
            }
        }

        private void btnEliminarInventario_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int inventarioId = (int)btn.Tag;

            var result = MessageBox.Show(
                "¿Está seguro de eliminar este registro de inventario?\n\n" +
                "Esto también eliminará los detalles de pedidos relacionados.\n\n" +
                "Esta acción no se puede deshacer.",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _inventarioService.Delete(inventarioId);
                    MessageBox.Show("Inventario eliminado correctamente.", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarDatos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar inventario: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void btnAjustarStock_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int inventarioId = (int)btn.Tag;

            Inventario inventario = _inventarioService.GetById(inventarioId);
            if (inventario != null)
            {
                AjustarStockDialog dialog = new AjustarStockDialog(inventario);
                if (dialog.ShowDialog() == true)
                {
                    CargarDatos();
                }
            }
        }

        private void btnActualizarInventario_Click(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void cmbCategoria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FiltrarPrendas();
        }

        private void btnBuscarPrenda_Click(object sender, RoutedEventArgs e)
        {
            FiltrarPrendas();
        }

        private void FiltrarPrendas()
        {
            try
            {
                var prendas = _prendas;

                // Filtrar por categoría
                if (cmbCategoria.SelectedItem != null)
                {
                    var categoriaSeleccionada = (Categoria)cmbCategoria.SelectedItem;
                    if (categoriaSeleccionada.Id > 0) // No es "Todas las categorías"
                    {
                        prendas = prendas.Where(p => p.CategoriaId == categoriaSeleccionada.Id).ToList();
                    }
                }

                // Filtrar por texto de búsqueda
                if (!string.IsNullOrEmpty(txtBuscarPrenda.Text))
                {
                    string busqueda = txtBuscarPrenda.Text.ToLower();
                    prendas = prendas.Where(p =>
                        p.Nombre.ToLower().Contains(busqueda) ||
                        p.Codigo.ToLower().Contains(busqueda) ||
                        (p.Descripcion != null && p.Descripcion.ToLower().Contains(busqueda))
                    ).ToList();
                }

                dgPrendas.ItemsSource = prendas;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar prendas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbPrenda_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FiltrarInventario();
        }

        private void cmbTalla_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FiltrarInventario();
        }

        private void FiltrarInventario()
        {
            try
            {
                var inventarios = _inventarios;

                // Filtrar por prenda
                if (cmbPrenda.SelectedItem != null)
                {
                    int prendaId = ((Prenda)cmbPrenda.SelectedItem).Id;
                    inventarios = inventarios.Where(i => i.PrendaId == prendaId).ToList();
                }

                // Filtrar por talla
                if (cmbTalla.SelectedIndex > 0) // Si no es "Todas"
                {
                    string talla = ((ComboBoxItem)cmbTalla.SelectedItem).Content.ToString();
                    inventarios = inventarios.Where(i => i.Talla == talla).ToList();
                }

                dgInventario.ItemsSource = inventarios;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar inventario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgPrendas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPrendas.SelectedItem != null)
            {
                Prenda prenda = (Prenda)dgPrendas.SelectedItem;

                // Filtrar inventario por la prenda seleccionada
                var inventariosFiltrados = _inventarios.Where(i => i.PrendaId == prenda.Id).ToList();
                dgInventario.ItemsSource = inventariosFiltrados;
            }
        }
    }
}
