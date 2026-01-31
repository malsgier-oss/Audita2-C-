using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WorkAudit.UI.Converters;

public class BoolToGridLengthConverter : IValueConverter
{
    public GridLength TrueValue { get; set; } = new GridLength(350, GridUnitType.Pixel);
    public GridLength FalseValue { get; set; } = GridLength.Auto;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? TrueValue : new GridLength(0);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
