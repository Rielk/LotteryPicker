using System.Windows.Data;

namespace LotteryPicker;
[ValueConversion(typeof(bool), typeof(bool))]
public class InverseBooleanConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		return DoConvert(value, targetType);
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		return DoConvert(value, targetType);
	}

	private static object DoConvert(object value, Type targetType)
	{
		if (targetType != typeof(bool))
			throw new InvalidOperationException("The target must be a boolean");

		return !(bool)value;
	}
}