using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using LCrypt.Enumerations;

namespace LCrypt.Password_Manager
{
    public class SearchScopeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is SearchScope)) return 0;
            return (int)((SearchScope)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int)) return SearchScope.Everything;
            switch ((int)value)
            {
                case 0:
                    return SearchScope.Everything;
                case 1:
                    return SearchScope.Name;
                case 2:
                    return SearchScope.Username;
                case 3:
                    return SearchScope.Email;
                case 4:
                    return SearchScope.Comment;
                default:
                    return SearchScope.Everything;
            }
        }
    }
}
