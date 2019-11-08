using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TenekDownloader.view.converter
{
    public class AnythingToVsibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return ConvertFromBool(b);

            try
            {
                return ConvertFromBool(bool.Parse((string)value));
            }
            catch
            {
                return ConvertFromObject(value);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private object ConvertFromObject(object value)
        {
            return ConvertFromBool(value != null);
        }

        public Visibility ConvertFromBool(bool value)
        {
            return value ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}