using ENTITIES;
using System;
using System.Windows;
using TryOn.BLL;

namespace GUI
{
    public partial class UsuarioDialog : Window
    {
        private readonly UsuarioService _usuarioService;
        private Usuario _usuario;
        private bool _esEdicion;

        public UsuarioDialog()
        {
            InitializeComponent();
            _usuarioService = new UsuarioService();
            _usuario = new Usuario();
            _esEdicion = false;
        }

        public UsuarioDialog(Usuario usuario)
        {
            InitializeComponent();
            _usuarioService = new UsuarioService();
            _usuario = usuario;
            _esEdicion = true;

            txtTitulo.Text = "Editar Usuario";

            CargarDatosUsuario();
        }

        private void CargarDatosUsuario()
        {
            txtNombre.Text = _usuario.Nombre;
            txtApellido.Text = _usuario.Apellido;
            txtEmail.Text = _usuario.Email;
            txtTelefono.Text = _usuario.Telefono;
            txtDireccion.Text = _usuario.Direccion;
            chkEsAdmin.IsChecked = _usuario.EsAdmin;

            // No mostrar contraseña por seguridad
            txtPassword.Password = "";
            txtConfirmPassword.Password = "";
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar campos
                if (string.IsNullOrEmpty(txtNombre.Text) || string.IsNullOrEmpty(txtApellido.Text) ||
                    string.IsNullOrEmpty(txtEmail.Text))
                {
                    MessageBox.Show("Por favor, complete los campos obligatorios", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Validar contraseñas
                if (!_esEdicion && string.IsNullOrEmpty(txtPassword.Password))
                {
                    MessageBox.Show("La contraseña es obligatoria", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!string.IsNullOrEmpty(txtPassword.Password) && txtPassword.Password != txtConfirmPassword.Password)
                {
                    MessageBox.Show("Las contraseñas no coinciden", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Actualizar datos del usuario
                _usuario.Nombre = txtNombre.Text.Trim();
                _usuario.Apellido = txtApellido.Text.Trim();
                _usuario.Email = txtEmail.Text.Trim();
                _usuario.Telefono = txtTelefono.Text.Trim();
                _usuario.Direccion = txtDireccion.Text.Trim();
                _usuario.EsAdmin = chkEsAdmin.IsChecked ?? false;

                // Actualizar contraseña solo si se proporcionó una nueva
                if (!string.IsNullOrEmpty(txtPassword.Password))
                {
                    _usuario.Password = txtPassword.Password;
                }

                // Guardar usuario
                if (_esEdicion)
                {
                    _usuarioService.Update(_usuario);
                }
                else
                {
                    _usuarioService.Add(_usuario);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
