using System;
using System.Globalization;
using System.Windows.Data;

namespace LCrypt.Value_Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class StringToLanguageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return App.LocalizationDictionary[(string) value ?? throw new ArgumentNullException(nameof(value))];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
