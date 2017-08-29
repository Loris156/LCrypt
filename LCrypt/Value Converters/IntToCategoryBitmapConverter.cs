using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace LCrypt.Value_Converters
{
    public class IntToCategoryBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return new BitmapImage(new Uri(
                    $"pack://application:,,/Resources/Password Manager Icons/Category/{value}.png"));
            }
            catch (Exception)
            {
                return new BitmapImage(new Uri(
                    "pack://application:,,/Resources/Password Manager Icons/Category/0.png"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}