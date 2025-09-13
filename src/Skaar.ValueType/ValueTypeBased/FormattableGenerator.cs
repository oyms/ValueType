using Microsoft.CodeAnalysis;

namespace Skaar.ValueType.ValueTypeBased;

public class FormattableGenerator(ITypeSymbol type, ITypeSymbol genericType) : InterfaceImplementor
{
    protected override string InterfaceName => "IFormattable";
    protected override string Ns => "System";

    public override string RenderInterfaceName()
    {
        return $"{Ns}.{InterfaceName}";
    }

    protected override bool GenericArgumentTypeImplementsInterface() =>
        TypeImplementsInterface(genericType, Ns, InterfaceName);

    protected override bool WrapperTypeImplementsInterface() =>
        TypeImplementsInterface(type, Ns, InterfaceName);

    public override string Render()
    {        
        return $$"""
                                     
                     ///<inheritdoc src="{{genericType}}.ToString(string?, System.IFormatProvider?)"/>
                     string {{RenderInterfaceName()}}.ToString(string? format, System.IFormatProvider? formatProvider) => (({{RenderInterfaceName()}})_value).ToString(format, formatProvider);
                 """;

    }
}