using System;
using System.Globalization;
using System.Windows.Data;

namespace LCrypt.Value_Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class BoolInverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b ? !b : throw new ArgumentException("Type of value was not bool!", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
