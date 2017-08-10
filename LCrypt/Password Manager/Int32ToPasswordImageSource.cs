using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace LCrypt.Password_Manager
{
    public class Int32ToPasswordImageSource : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return new BitmapImage(new Uri(
                    $"pack://application:,,/Resources/Password Manager Icons/{value?.ToString()}.png"));
            }
            catch (Exception)
            {
                return new BitmapImage(new Uri(
                    "pack://application:,,/Resources/Password Manager Icons/0.png"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
