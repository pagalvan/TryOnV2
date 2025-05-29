using ENTITIES;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GUI
{
    public partial class CategorySuggestionDialog : Window
    {
        public string SelectedCategoryName { get; private set; }

        public CategorySuggestionDialog(List<Categoria> categorias)
        {
            InitializeComponent();
            lstCategorias.ItemsSource = categorias;
            lstCategorias.DisplayMemberPath = "Nombre";
        }

        private void btnSeleccionar_Click(object sender, RoutedEventArgs e)
        {
            if (lstCategorias.SelectedItem != null)
            {
                SelectedCategoryName = ((Categoria)lstCategorias.SelectedItem).Nombre;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Seleccione una categoría.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void lstCategorias_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstCategorias.SelectedItem != null)
            {
                btnSeleccionar_Click(sender, null);
            }
        }
    }
}