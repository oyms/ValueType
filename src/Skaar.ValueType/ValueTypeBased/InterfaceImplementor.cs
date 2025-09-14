using System.Linq;
using Microsoft.CodeAnalysis;

namespace Skaar.ValueType.ValueTypeBased;

public abstract class InterfaceImplementor
{
    public abstract string RenderInterfaceName();
    public bool ShouldRender => GenericArgumentTypeImplementsInterface() && !WrapperTypeImplementsInterface();
    public abstract string Render();
    protected abstract bool GenericArgumentTypeImplementsInterface();
    protected abstract bool WrapperTypeImplementsInterface();
    protected abstract string InterfaceName { get; }
    protected abstract string Ns { get; }

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
    protected bool TypeImplementsGenericInterface(ITypeSymbol type, string @namespace, string interfaceName, params ITypeSymbol[] genericArguments)
    {
        if (genericArguments.Length == 0)
        {
            return TypeImplementsInterface(type, @namespace, interfaceName);
        }
        return type.AllInterfaces.Any(x =>
            x.Name == interfaceName &&
            x.ContainingNamespace.ToDisplayString() == @namespace &&
            x.IsGenericType &&
            x.TypeArguments.Length == genericArguments.Length &&
            x.TypeArguments.Zip(genericArguments, (a, b) => SymbolEqualityComparer.Default.Equals(a, b)).All(b => b)
        );
    }
    protected bool TypeImplementsInterface(ITypeSymbol type, string @namespace, string interfaceName)
    {
        return type.AllInterfaces.Any(x =>
            x.Name == interfaceName &&
            x.ContainingNamespace.ToDisplayString() == @namespace
        );
    }
    
    protected string RenderInterfaceName(ITypeSymbol genericParameter)
    {
        return $"{Ns}.{InterfaceName}<{genericParameter.Name}>";
    }
}