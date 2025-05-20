using System;
using System.Windows;
using TryOn.BLL;

namespace GUI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Inicializar el servicio de Telegram
                TelegramService.GetInstance();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar el servicio de Telegram: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}