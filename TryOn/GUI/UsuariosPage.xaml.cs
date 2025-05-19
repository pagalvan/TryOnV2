using ENTITIES;
using GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TryOn.BLL;

namespace GUI
{
    public partial class UsuariosPage : Page
    {
        private readonly UsuarioService _usuarioService;
        private List<Usuario> _usuarios;

        public UsuariosPage()
        {
            InitializeComponent();
            _usuarioService = new UsuarioService();

            // Ya no es necesario registrar el convertidor aquí
            // Resources.Add("BoolToAdminConverter", new BoolToAdminConverter());

            CargarDatos();
        }

        private void CargarDatos()
        {
            try
            {
                // Cargar usuarios
                _usuarios = _usuarioService.GetAll().ToList();
                dgUsuarios.ItemsSource = _usuarios;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAgregarUsuario_Click(object sender, RoutedEventArgs e)
        {
            UsuarioDialog dialog = new UsuarioDialog();
            if (dialog.ShowDialog() == true)
            {
                CargarDatos();
            }
        }

        private void btnEditarUsuario_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int usuarioId = (int)btn.Tag;

            Usuario usuario = _usuarioService.GetById(usuarioId);
            if (usuario != null)
            {
                UsuarioDialog dialog = new UsuarioDialog(usuario);
                if (dialog.ShowDialog() == true)
                {
                    CargarDatos();
                }
            }
        }

        private void btnEliminarUsuario_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int usuarioId = (int)btn.Tag;

            if (MessageBox.Show("¿Está seguro de eliminar este usuario?", "Confirmar eliminación",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    _usuarioService.Delete(usuarioId);
                    CargarDatos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnActualizarUsuarios_Click(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void btnBuscarUsuario_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string busqueda = txtBuscarUsuario.Text.ToLower();

                if (string.IsNullOrEmpty(busqueda))
                {
                    dgUsuarios.ItemsSource = _usuarios;
                    return;
                }

                var usuariosFiltrados = _usuarios.Where(u =>
                    u.Nombre.ToLower().Contains(busqueda) ||
                    u.Apellido.ToLower().Contains(busqueda) ||
                    u.Email.ToLower().Contains(busqueda) ||
                    (u.Telefono != null && u.Telefono.ToLower().Contains(busqueda))
                ).ToList();

                dgUsuarios.ItemsSource = usuariosFiltrados;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar usuarios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}