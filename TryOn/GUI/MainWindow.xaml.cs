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
            ConfigurarVisibilidadSegunRol();

            // Cargar página inicial según el rol del usuario
            if (_usuarioActual.EsAdmin)
            {
                // Para administradores, cargar la página de inventario
                MainFrame.Navigate(new InventarioPage());
                txtTituloPagina.Text = "Gestión de Inventario";
            }
            else
            {
                // Para usuarios normales, cargar directamente el catálogo
                MainFrame.Navigate(new CatalogoPage(_usuarioActual));
                txtTituloPagina.Text = "Catálogo de Productos";
            }
        }

        private void ConfigurarVisibilidadSegunRol()
        {
            if (_usuarioActual.EsAdmin)
            {
                // Administrador: mostrar todas las opciones
                btnInventario.Visibility = Visibility.Visible;
                btnCatalogo.Visibility = Visibility.Visible;
                btnVentas.Visibility = Visibility.Visible;
                btnVentas.Content = "Pedidos"; // Texto para admin
                btnReportes.Visibility = Visibility.Visible;
                btnUsuarios.Visibility = Visibility.Visible;
            }
            else
            {
                // Usuario normal: mostrar catálogo y sus pedidos
                btnInventario.Visibility = Visibility.Collapsed;
                btnCatalogo.Visibility = Visibility.Visible;
                btnVentas.Visibility = Visibility.Visible;
                btnVentas.Content = "Mis Pedidos"; // Texto para usuario normal
                btnReportes.Visibility = Visibility.Collapsed;
                btnUsuarios.Visibility = Visibility.Collapsed;
            }
        }

        public void NavegarACatalogo()
        {
            MainFrame.Navigate(new CatalogoPage(_usuarioActual));
            txtTituloPagina.Text = "Catálogo de Productos";
        }

        private void btnInventario_Click(object sender, RoutedEventArgs e)
        {
            if (!_usuarioActual.EsAdmin)
            {
                MessageBox.Show("No tienes permisos para acceder a esta sección", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MainFrame.Navigate(new InventarioPage());
            txtTituloPagina.Text = "Gestión de Inventario";
        }

        public void btnCatalogo_Click(object sender, RoutedEventArgs e)
        {
            NavegarACatalogo();
        }

        private void btnVentas_Click(object sender, RoutedEventArgs e)
        {
            // Navegar a PedidosPage pasando el usuario actual
            MainFrame.Navigate(new PedidosPage(_usuarioActual));


            if (_usuarioActual.EsAdmin)
            {
                txtTituloPagina.Text = "Pedidos";
            }
            else
            {
                txtTituloPagina.Text = "Mis Pedidos";
            }
        }

        private void btnReportes_Click(object sender, RoutedEventArgs e)
        {
            if (!_usuarioActual.EsAdmin)
            {
                MessageBox.Show("No tienes permisos para acceder a esta sección", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MainFrame.Navigate(new ReportesPage());
            txtTituloPagina.Text = "Reportes";
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            if (!_usuarioActual.EsAdmin)
            {
                MessageBox.Show("No tienes permisos para acceder a esta sección", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MainFrame.Navigate(new UsuariosPage());
            txtTituloPagina.Text = "Gestión de Usuarios";
        }

        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}