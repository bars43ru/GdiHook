using System;
using System.Windows.Data;
using Newtonsoft.Json;

namespace GdiHook.View.ValueConverters
{
    public class ControleElementJsonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return JsonConvert.SerializeObject(value, Formatting.Indented);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
