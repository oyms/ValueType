using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Skaar.ValueType.ValueTypeBased;

public abstract class InterfaceImplementor
{
    public abstract string RenderInterfaceName();
    public bool ShouldRender => GenericArgumentTypeImplementsInterface();// && !TypeImplementsInterface();
    public abstract string Render();
    protected abstract bool GenericArgumentTypeImplementsInterface();
    protected abstract bool WrapperTypeImplementsInterface();

    protected bool TypeImplementsGenericInterface(ITypeSymbol type, string @namespace, string interfaceName, ITypeSymbol genericArgument)
    {
         return type.AllInterfaces.Any(x =>
             x.Name == interfaceName &&
             x.ContainingNamespace.ToDisplayString() == @namespace &&
             x.IsGenericType &&
             x.TypeArguments.Length == 1 &&
             SymbolEqualityComparer.Default.Equals(x.TypeArguments[0], genericArgument)
        );
    }
}

public class EquatableGenerator(INamedTypeSymbol type, ITypeSymbol genericType) : InterfaceImplementor
{
    private const string InterfaceName = "IEquatable";
    private const string Ns = "System";
    public override string RenderInterfaceName()
    {
        return $"{Ns}.{InterfaceName}<{type.Name}>";
    }

    protected override bool GenericArgumentTypeImplementsInterface() => TypeImplementsGenericInterface(genericType, Ns, InterfaceName, genericType);

    protected override bool WrapperTypeImplementsInterface() => TypeImplementsGenericInterface(type, Ns, InterfaceName, genericType);

    public override string Render()
    {
        return $$"""
                     
                     ///<inheritdoc/>
                     bool {{Ns}}.{{InterfaceName}}<{{type.Name}}>.Equals({{type.Name}} other) => _value.Equals(other._value);
                 """;
    }
}