using Microsoft.CodeAnalysis;

namespace Skaar.ValueType.ValueTypeBased;

public class ParsableGenerator(ITypeSymbol type, ITypeSymbol genericType) : InterfaceImplementor
{
    public override string RenderInterfaceName()
    {
        return $"{Ns}.{InterfaceName}<{type.Name}>";
    }

    protected override bool GenericArgumentTypeImplementsInterface() =>
        TypeImplementsGenericInterface(genericType, Ns, InterfaceName, genericType);

    protected override bool WrapperTypeImplementsInterface() =>
        TypeImplementsInterface(type, Ns, InterfaceName);

    protected override string InterfaceName => "IParsable";
    protected override string Ns => "System";

    public override string Render()
    {
        return $$"""
                     
                     ///<inheritdoc/>
                     public static {{type.ToDisplayString()}} Parse(string s, System.IFormatProvider? provider = null) => new({{genericType.ToDisplayString()}}.Parse(s, provider));
                     ///<inheritdoc/>
                     public static bool TryParse(string? s, System.IFormatProvider? provider, out {{type.ToDisplayString()}} result)
                     {
                        if({{genericType.ToDisplayString()}}.TryParse(s, provider, out var value))
                        {
                            result = new(value);
                            return true;
                        }
                        else
                        {
                            result = default;
                            return false;
                        }
                     }
                 """;
    }
}