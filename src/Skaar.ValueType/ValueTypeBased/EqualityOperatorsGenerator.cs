using System.Linq;
using Microsoft.CodeAnalysis;

namespace Skaar.ValueType.ValueTypeBased;

public class EqualityOperatorsGenerator(ITypeSymbol type, ITypeSymbol genericType) : InterfaceImplementor
{
    protected override string InterfaceName => "IEqualityOperators";
    protected override string Ns => "System.Numerics";
    public override string RenderInterfaceName()
    {
        return $"{Ns}.{InterfaceName}<{type.Name}, {type.Name}, bool>";
    }

    protected override bool GenericArgumentTypeImplementsInterface() => ImplementsInterface(genericType);
    protected override bool WrapperTypeImplementsInterface() => ImplementsInterface(type);
    
    private bool ImplementsInterface(ITypeSymbol type)
    {
        return type.AllInterfaces.Any(x =>
            x.Name == InterfaceName &&
            x.ContainingNamespace.ToDisplayString() == Ns &&
            x.IsGenericType &&
            x.TypeArguments.Length == 3 &&
            SymbolEqualityComparer.Default.Equals(x.TypeArguments[0], type) &&
            SymbolEqualityComparer.Default.Equals(x.TypeArguments[1], type) &&
            x.TypeArguments[2].SpecialType == SpecialType.System_Boolean
        );
    }
    
    public override string Render()
    {
        return $$"""
                     
             ///<inheritdoc/>
             public static bool operator ==({{type.Name}} left, {{type.Name}} right) => left._value == right._value;
             ///<inheritdoc/>
             public static bool operator !=({{type.Name}} left, {{type.Name}} right) => left._value != right._value;
         """;
    }
}