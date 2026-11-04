using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Skaar.ValueType;

/// <summary>
/// This is a type converter for value types
/// </summary>
public class ParsableTypeConverter<T> : TypeConverter where T : IParsable<T>
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string);
    public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType) => destinationType == typeof(string);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string stringValue) return T.Parse(stringValue, culture);
        return base.ConvertFrom(context, culture, value);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value is null) return null;
        if (value is T typedValue) return typedValue.ToString();
        return base.ConvertTo(context, culture, value, destinationType);
    }
}
