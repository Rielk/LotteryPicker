using System.Windows.Data;

namespace LotteryPicker;
[ValueConversion(typeof(bool), typeof(bool))]
public class InverseBooleanConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		return doConvert(value, targetType);
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		return doConvert(value, targetType);
	}

	private object doConvert(object value, Type targetType)
	{
		if (targetType != typeof(bool))
			throw new InvalidOperationException("The target must be a boolean");

		return !(bool)value;
	}
}