using ENTITIES;
using GUI;
using System;
using System.Windows;

namespace GUI
{
    public partial class MainWindow : Window
    {
        private readonly Usuario _usuarioActual;

        public MainWindow(Usuario usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;

            // Configurar nombre de usuario
            txtUsuarioNombre.Text = $"Bienvenido, {_usuarioActual.NombreCompleto}";

            // Configurar visibilidad de opciones según rol
            btnUsuarios.Visibility = _usuarioActual.EsAdmin ? Visibility.Visible : Visibility.Collapsed;

            // Cargar página inicial
            MainFrame.Navigate(new InventarioPage());
            txtTituloPagina.Text = "Gestión de Inventario";
        }

        public void NavegarACatalogo()
        {
            MainFrame.Navigate(new CatalogoPage(_usuarioActual));
            txtTituloPagina.Text = "Catálogo de Productos";
        }

        private void btnInventario_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new InventarioPage());
            txtTituloPagina.Text = "Gestión de Inventario";
        }

        public void btnCatalogo_Click(object sender, RoutedEventArgs e)
        {
            NavegarACatalogo();
        }

        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new VentasPage(_usuarioActual));
            txtTituloPagina.Text = "Ventas y Pedidos";
        }

        private void btnReportes_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ReportesPage());
            txtTituloPagina.Text = "Reportes";
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            if (_usuarioActual.EsAdmin)
            {
                MainFrame.Navigate(new UsuariosPage());
                txtTituloPagina.Text = "Gestión de Usuarios";
            }
            else
            {
                MessageBox.Show("No tienes permisos para acceder a esta sección", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
