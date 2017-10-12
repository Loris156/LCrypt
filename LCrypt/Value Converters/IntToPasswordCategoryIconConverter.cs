using System;
using System.Globalization;
using System.Windows.Data;

namespace LCrypt.Value_Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    class IntToPasswordCategoryIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null
                ? string.Empty
                : $"pack://application:,,/Resources/PasswordManagerIcons/Category/{value}.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
