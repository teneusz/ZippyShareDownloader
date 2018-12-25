using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TenekDownloader.view.converter
{
	 public class AnythingToVsibleConverter : IValueConverter
	 {
		 public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		 {
			 if (value is bool b)
			 {
				 return ConvertFromBool(b);
			 }

			 try
			 {
				 return ConvertFromBool(bool.Parse((string) value));
			 }
			 catch
			 {
				 return ConvertFromObject(value);
			 }
		 }

		 private object ConvertFromObject(object value)
		 {
			 return ConvertFromBool(value != null);
		 }

		 public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		 {
			 throw new NotImplementedException();
		 }

		 public Visibility ConvertFromBool(bool value)
		 {
			 return value ? Visibility.Visible : Visibility.Collapsed;
		 }
	 }
}
