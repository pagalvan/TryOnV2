using ENTITIES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TryOn.BLL;


namespace GUI
{
    public partial class EnviarPromocionDialog : Window
    {
        private readonly PromocionService _promocionService;
        private readonly PrendaService _prendaService;
        private readonly TelegramService _telegramService;
        private List<Promocion> _promociones;
        private List<Prenda> _prendas;
        private List<Categoria> _categorias;

        public EnviarPromocionDialog()
        {
            InitializeComponent();
            _promocionService = new PromocionService();
            _prendaService = new PrendaService();
            _telegramService = TelegramService.GetInstance();

            // Inicializar fechas
            dpFechaInicio.SelectedDate = DateTime.Now;
            dpFechaFin.SelectedDate = DateTime.Now.AddDays(7);

            // Generar código aleatorio
            txtCodigoPromocion.Text = GenerarCodigoAleatorio();

            // Mostrar número de usuarios registrados
            ActualizarContadorUsuarios();

            CargarDatos();
        }

        private void ActualizarContadorUsuarios()
        {
            int usuariosRegistrados = _telegramService.GetRegisteredUsersCount();
            txtUsuariosRegistrados.Text = $"Usuarios registrados en Telegram: {usuariosRegistrados}";
        }

        private string GenerarCodigoAleatorio()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void CargarDatos()
        {
            try
            {
                // Cargar promociones
                _promociones = _promocionService.GetAll().ToList();
                cmbPromociones.ItemsSource = _promociones;
                cmbPromociones.DisplayMemberPath = "Titulo";
                cmbPromociones.SelectedValuePath = "Id";
                cmbPromociones.SelectedIndex = -1;

                // Cargar prendas
                _prendas = _prendaService.GetAll().ToList();
                cmbPrenda.ItemsSource = _prendas;
                cmbPrenda.DisplayMemberPath = "Nombre";
                cmbPrenda.SelectedValuePath = "Id";

                // Cargar categorías
                _categorias = _prendas.Select(p => p.Categoria).Where(c => c != null).GroupBy(c => c.Id).Select(g => g.First()).ToList();
                cmbCategoria.ItemsSource = _categorias;
                cmbCategoria.DisplayMemberPath = "Nombre";
                cmbCategoria.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbPromociones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPromociones.SelectedItem != null)
            {
                Promocion promocion = (Promocion)cmbPromociones.SelectedItem;

                // Cargar datos de la promoción seleccionada
                txtTitulo.Text = promocion.Titulo;
                txtDescripcion.Text = promocion.Descripcion;
                txtPorcentajeDescuento.Text = promocion.PorcentajeDescuento.ToString();
                txtCodigoPromocion.Text = promocion.CodigoPromocion;
                dpFechaInicio.SelectedDate = promocion.FechaInicio;
                dpFechaFin.SelectedDate = promocion.FechaFin;

                // Seleccionar tipo de aplicación
                if (promocion.PrendaId.HasValue)
                {
                    rbPrenda.IsChecked = true;
                    pnlPrenda.Visibility = Visibility.Visible;
                    pnlCategoria.Visibility = Visibility.Collapsed;
                    cmbPrenda.SelectedValue = promocion.PrendaId;
                }
                else if (promocion.CategoriaId.HasValue)
                {
                    rbCategoria.IsChecked = true;
                    pnlCategoria.Visibility = Visibility.Visible;
                    pnlPrenda.Visibility = Visibility.Collapsed;
                    cmbCategoria.SelectedValue = promocion.CategoriaId;
                }
                else
                {
                    rbTodas.IsChecked = true;
                    pnlCategoria.Visibility = Visibility.Collapsed;
                    pnlPrenda.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void rbCategoria_Checked(object sender, RoutedEventArgs e)
        {
            pnlCategoria.Visibility = Visibility.Visible;
            pnlPrenda.Visibility = Visibility.Collapsed;
        }

        private void rbPrenda_Checked(object sender, RoutedEventArgs e)
        {
            pnlPrenda.Visibility = Visibility.Visible;
            pnlCategoria.Visibility = Visibility.Collapsed;
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void btnEnviar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar campos
                if (string.IsNullOrEmpty(txtTitulo.Text))
                {
                    MessageBox.Show("El título es obligatorio", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrEmpty(txtPorcentajeDescuento.Text))
                {
                    MessageBox.Show("El porcentaje de descuento es obligatorio", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                decimal porcentajeDescuento;
                if (!decimal.TryParse(txtPorcentajeDescuento.Text, out porcentajeDescuento) || porcentajeDescuento <= 0 || porcentajeDescuento > 100)
                {
                    MessageBox.Show("El porcentaje de descuento debe ser un número entre 1 y 100", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (dpFechaInicio.SelectedDate == null || dpFechaFin.SelectedDate == null)
                {
                    MessageBox.Show("Las fechas son obligatorias", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (dpFechaInicio.SelectedDate > dpFechaFin.SelectedDate)
                {
                    MessageBox.Show("La fecha de inicio debe ser anterior a la fecha de fin", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (rbCategoria.IsChecked == true && cmbCategoria.SelectedItem == null)
                {
                    MessageBox.Show("Debe seleccionar una categoría", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (rbPrenda.IsChecked == true && cmbPrenda.SelectedItem == null)
                {
                    MessageBox.Show("Debe seleccionar una prenda", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Crear promoción
                Promocion promocion = new Promocion
                {
                    Titulo = txtTitulo.Text.Trim(),
                    Descripcion = txtDescripcion.Text.Trim(),
                    PorcentajeDescuento = porcentajeDescuento,
                    FechaInicio = dpFechaInicio.SelectedDate.Value,
                    FechaFin = dpFechaFin.SelectedDate.Value,
                    CodigoPromocion = txtCodigoPromocion.Text.Trim(),
                    Activa = true
                };

                // Asignar categoría o prenda según selección
                if (rbCategoria.IsChecked == true)
                {
                    promocion.CategoriaId = (int)cmbCategoria.SelectedValue;
                    promocion.Categoria = (Categoria)cmbCategoria.SelectedItem;
                }
                else if (rbPrenda.IsChecked == true)
                {
                    promocion.PrendaId = (int)cmbPrenda.SelectedValue;
                    promocion.Prenda = (Prenda)cmbPrenda.SelectedItem;
                }

                // Guardar promoción
                _promocionService.Add(promocion);

                // Enviar promoción por Telegram
                btnEnviar.IsEnabled = false;
                btnEnviar.Content = "Enviando...";

                bool enviado = await _telegramService.EnviarPromocion(promocion, txtMensajeAdicional.Text.Trim());

                if (enviado)
                {
                    MessageBox.Show("Promoción enviada correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("No hay usuarios registrados en Telegram para enviar la promoción", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    btnEnviar.IsEnabled = true;
                    btnEnviar.Content = "Enviar Promoción";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar promoción: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                btnEnviar.IsEnabled = true;
                btnEnviar.Content = "Enviar Promoción";
            }
        }
    }
}