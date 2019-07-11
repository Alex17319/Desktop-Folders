using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DesktopFolders
{
	public class RectConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			double x = 0, y = 0, width = 0, height = 0;
			if (values.Length < 2) {
				//Leave all as 0
				Console.WriteLine("#1");
			} else if (values.Length < 4) {
				try { width  = (double)values[0]; } catch (InvalidCastException) { } catch (NullReferenceException) { }
				try { height = (double)values[1]; } catch (InvalidCastException) { } catch (NullReferenceException) { }
				Console.WriteLine("#2");
			} else {
				try { x      = (double)values[0]; } catch (InvalidCastException) { } catch (NullReferenceException) { }
				try { y      = (double)values[1]; } catch (InvalidCastException) { } catch (NullReferenceException) { }
				try { width  = (double)values[2]; } catch (InvalidCastException) { } catch (NullReferenceException) { }
				try { height = (double)values[3]; } catch (InvalidCastException) { } catch (NullReferenceException) { }
				Console.WriteLine("#3");
			}
			//?//	x      = double.IsNaN(x     ) ? 0 : x;
			//?//	y      = double.IsNaN(y     ) ? 0 : y;
			//?//	width  = double.IsNaN(width ) ? 0 : width;
			//?//	height = double.IsNaN(height) ? 0 : height;
			Console.WriteLine("#4: " + x + ", " + y + ", " + width + ", " + height);
			return new Rect(x, y, width, height);
		}
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
