using ENTITIES;
using System;
using System.Windows;
using TryOn.BLL;

namespace GUI
{
    public partial class RegisterWindow : Window
    {
        private readonly UsuarioService _usuarioService;

        public RegisterWindow()
        {
            InitializeComponent();
            _usuarioService = new UsuarioService();
        }

        private void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar campos
                if (string.IsNullOrEmpty(txtNombre.Text) || string.IsNullOrEmpty(txtApellido.Text) ||
                    string.IsNullOrEmpty(txtEmail.Text) || string.IsNullOrEmpty(txtPassword.Password))
                {
                    MessageBox.Show("Por favor, complete los campos obligatorios", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Validar contraseñas
                if (txtPassword.Password != txtConfirmPassword.Password)
                {
                    MessageBox.Show("Las contraseñas no coinciden", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Crear usuario
                Usuario usuario = new Usuario
                {
                    Nombre = txtNombre.Text.Trim(),
                    Apellido = txtApellido.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Password = txtPassword.Password,
                    Telefono = txtTelefono.Text.Trim(),
                    Direccion = txtDireccion.Text.Trim(),
                    EsAdmin = false // Por defecto, los usuarios registrados no son administradores
                };

                _usuarioService.Add(usuario);

                MessageBox.Show("Usuario registrado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Redirigir a la ventana de inicio de sesión
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lnkLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
