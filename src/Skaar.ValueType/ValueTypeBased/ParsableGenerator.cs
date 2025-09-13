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
                     public static {{type.ToDisplayString()}} Parse(string s, System.IFormatProvider? provider = null) => new(_parseValue<{{genericType.ToDisplayString()}}>(s, provider));
                     private static T _parseValue<T>(string s, System.IFormatProvider? provider = null) where T : IParsable<T> => T.Parse(s, provider);
                     ///<inheritdoc/>
                     public static bool TryParse(string? s, System.IFormatProvider? provider, out {{type.ToDisplayString()}} result)
                     {
                        if(_tryParseValue<{{genericType.ToDisplayString()}}>(s, provider, out var value))
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
                     private static bool _tryParseValue<T>(string? s, System.IFormatProvider? provider, out T result) where T : IParsable<T> => T.TryParse(s, provider, out result);
                 """;
    }
}