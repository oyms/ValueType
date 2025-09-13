using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Skaar.ValueType.ValueTypeBased;

#nullable enable

internal class Generator(string @namespace) : Common.Generator(@namespace)
{
    private static readonly string InterfaceName = "IStructBasedValueType";
    public void GenerateStructFiles(IncrementalGeneratorInitializationContext context)
    {
        var structDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                (node, _) => node is StructDeclarationSyntax syntax && syntax.AttributeLists.Any(),
                (ctx, _) =>
                {
                    var structSyntax = (StructDeclarationSyntax)ctx.Node;
                    var symbol = ctx.SemanticModel.GetDeclaredSymbol(structSyntax);
                    return symbol;
                })
            .Where(s => s!.ContainingType is null)
            .Select((s, _) =>
            {
                var genericAttr = s!.GetAttributes()
                    .FirstOrDefault(attr =>
                        attr.AttributeClass is not null &&
                        attr.AttributeClass.IsGenericType &&
                        attr.AttributeClass.Name == AttributeName && attr.AttributeClass.Arity == 1
                        && attr.AttributeClass.ContainingNamespace.ToDisplayString() == Ns
                    );
                return (Struct: s, GenericAttr: genericAttr?.AttributeClass?.TypeArguments.First());
            })
            .Where(s => s.GenericAttr is not null)
            .Collect();

        context.RegisterSourceOutput(structDeclarations, (productionContext, structSymbols) =>
        {
            foreach (var (structSymbol, valueType) in structSymbols.Where(s => s.Struct is not null))
            {
                var targetSymbol = (ITypeSymbol)structSymbol;
                InterfaceImplementor[] interfaces = [
                    new EquatableGenerator(targetSymbol, valueType),
                    new ComparableGenerator(targetSymbol, valueType),
                    new ConvertibleGenerator(targetSymbol, valueType),
                    new FormattableGenerator(targetSymbol, valueType)
                ];
                var typeName = structSymbol!.Name;
                var ns = structSymbol.ContainingNamespace.ToDisplayString();
                var hasConstructorDefined = HasConstructorDefined(structSymbol as INamedTypeSymbol, valueType);
                productionContext.AddSource($"{ns}.{typeName}.g.cs",
                    SourceText.From(
                        StructSource(ns, typeName, valueType.ToDisplayString(), !hasConstructorDefined, interfaces),
                        Encoding.UTF8));
            }
        });
    }
    
    public void GenerateInterface(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource($"{InterfaceName}.g.cs", SourceText.From(InterfaceSource(InterfaceName), Encoding.UTF8));
        });
    } 

    private bool HasConstructorDefined(INamedTypeSymbol? symbol, ITypeSymbol parameterType)
    {
        if (symbol is null) return false;
        return symbol.InstanceConstructors.Any(ctor =>
        {
            if (ctor.IsStatic) return false;
            if (ctor.Parameters.Length != 1) return false;
            var pType = ctor.Parameters[0].Type;
            return SymbolEqualityComparer.Default.Equals(pType, parameterType);
        });
    }

    private string StructSource(string structNamespace, string structName, string valueType, bool renderCtor,
        params InterfaceImplementor[] interfaces)
    {
        var activeInterfaces = interfaces.Where(i => i.ShouldRender).ToArray();
        var ctor = renderCtor
            ? $"private {structName}({valueType} value) => _value = value;"
            : "";
        var interfaceList = activeInterfaces.Any()
            ? $", {string.Join(", ", activeInterfaces.Select(i => i.RenderInterfaceName()))}"
            : string.Empty;
        var interfaceImplementations = string.Join("\n", activeInterfaces.Select(i => i.Render()));
        var valueTypeInterfaceName = $"{Ns}.{InterfaceName}<{valueType}>";
        return $$"""
                 using System;
                 using System.ComponentModel;
                 using System.Diagnostics;
                 using System.Diagnostics.CodeAnalysis;
                 using System.Numerics;
                 using System.Text.Json.Serialization;

                 #nullable enable

                 namespace {{structNamespace}};

                 /// <summary>
                 /// A value type wrapping a {{valueType}} value
                 /// </summary>
                 {{GeneratedCodeAttribute}}
                 [System.Diagnostics.DebuggerDisplay("{_value}")]
                 readonly partial struct {{structName}} : {{valueTypeInterfaceName}}{{interfaceList}}
                 {
                    [System.Diagnostics.DebuggerBrowsable(DebuggerBrowsableState.Never)]
                    private readonly {{valueType}} _value; 
                    {{ctor}}
                    public static implicit operator {{structName}}({{valueType}} value) => new(value);
                    public static explicit operator {{valueType}}({{structName}} value) => value._value;
                    public override string ToString() => _value.ToString();
                    public override int GetHashCode() => _value.GetHashCode();
                    
                    bool {{valueTypeInterfaceName}}.HasValue => !Equals(_value, default);
                    {{valueType}} {{valueTypeInterfaceName}}.Value => _value;
                    
                    {{interfaceImplementations}}
                 }
                 """;
    }
    private string InterfaceSource(string typeName) =>
        $$"""
          using System;

          #nullable enable
          #pragma warning disable CS0436 // Type may be defined multiple times
          namespace {{Ns}};
          /// <summary>
          /// This is a marker interface for struct based value types
          /// </summary>
          {{GeneratedCodeAttribute}}
          public interface {{typeName}}<T> where T: struct
          {
               /// <summary>
               /// <c>true</c> if the value is different from default, <c>false</c> otherwise.
               /// </summary>
               bool HasValue { get; }
               /// <summary>
               /// The inner value.
               /// </summary>
               T Value { get; }
          }
               
          """;
}