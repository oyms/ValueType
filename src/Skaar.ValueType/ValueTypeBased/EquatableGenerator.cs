using Microsoft.CodeAnalysis;

namespace Skaar.ValueType.ValueTypeBased;

public class EquatableGenerator(ITypeSymbol type, ITypeSymbol genericType) : InterfaceImplementor
{
    public override string RenderInterfaceName()
    {
        return $"{Ns}.{InterfaceName}<{type.Name}>";
    }

    protected override bool GenericArgumentTypeImplementsInterface() => TypeImplementsGenericInterface(genericType, Ns, InterfaceName, genericType);
    protected override bool WrapperTypeImplementsInterface() => TypeImplementsGenericInterface(type, Ns, InterfaceName, type);
    protected override string InterfaceName => "IEquatable";
    protected override string Ns => "System";

    public override string Render()
    {
        return $$"""
                     
                     ///<inheritdoc/>
                     bool {{RenderInterfaceName()}}.Equals({{type.Name}} other) => _value.Equals(other._value);
                 """;
    }
}

public class ComparableGenerator(ITypeSymbol type, ITypeSymbol genericType) : InterfaceImplementor
{
    protected override string InterfaceName => "IComparable";
    protected override string Ns => "System";
    public override string RenderInterfaceName()
    {
        return $"{Ns}.{InterfaceName}<{type.Name}>";
    }
    
    protected override bool GenericArgumentTypeImplementsInterface() => TypeImplementsGenericInterface(genericType, Ns, InterfaceName, genericType);
    protected override bool WrapperTypeImplementsInterface() => TypeImplementsGenericInterface(type, Ns, InterfaceName, type);
    public override string Render()
    {
        return $$"""
                     
                     ///<inheritdoc/>
                     int {{RenderInterfaceName()}}.CompareTo({{type.Name}} other) => _value.CompareTo(other._value);
                 """;
    }
}