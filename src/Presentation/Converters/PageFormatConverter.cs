using System.Globalization;
using System.Windows.Data;

namespace ProcessingApp.Presentation.Converters;

public sealed class PageFormatConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
        {
            return string.Empty;
        }

        var page = values[0];
        var format = values[1] as string;

        if (page is null)
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(format))
        {
            return System.Convert.ToString(page, culture) ?? string.Empty;
        }

        return string.Format(culture, format, page);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
