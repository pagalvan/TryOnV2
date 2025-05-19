using System;
using System.Windows;
using TryOn.BLL;
using ENTITIES;

namespace GUI
{
    public partial class LoginWindow : Window
    {
        private readonly UsuarioService _usuarioService;

        public LoginWindow()
        {
            InitializeComponent();
            _usuarioService = new UsuarioService();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = txtEmail.Text.Trim();
                string password = txtPassword.Password;

                // Validar campos
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Por favor, complete todos los campos", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Validar credenciales
                if (_usuarioService.ValidateLogin(email, password))
                {
                    Usuario usuario = _usuarioService.GetByEmail(email);

                    // Abrir ventana principal
                    MainWindow mainWindow = new MainWindow(usuario);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Credenciales incorrectas", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar sesión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lnkRegistrar_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }
    }
}

