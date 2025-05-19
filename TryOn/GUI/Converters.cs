using System;
using System.Globalization;
using System.Windows.Data;

namespace GUI
{
    public class BoolToAdminConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool)value ? "Administrador" : "Cliente";
            }
            return "Cliente";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString() == "Administrador";
        }
    }
}
