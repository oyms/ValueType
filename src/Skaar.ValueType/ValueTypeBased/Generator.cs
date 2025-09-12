using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Skaar.ValueType.ValueTypeBased;

internal class Generator(string @namespace) : Common.Generator(@namespace)
{
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
                var typeName = structSymbol!.Name;
                var ns = structSymbol.ContainingNamespace.ToDisplayString();
                var hasConstructorDefined = HasConstructorDefined(structSymbol as INamedTypeSymbol);
                productionContext.AddSource($"{ns}.{typeName}.g.cs",
                    SourceText.From(StructSource(ns, typeName, valueType.ToDisplayString(), !hasConstructorDefined), Encoding.UTF8));
            }
        });
    }

    private string StructSource(string structNamespace, string structName, string valueType, bool renderCtor)
    {
        var ctor = renderCtor
            ? $"private {structName}({valueType} value) => _value = value;"
            : "";
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
                 readonly partial struct {{structName}} 
                 {
                    [System.Diagnostics.DebuggerBrowsable(DebuggerBrowsableState.Never)]
                    private readonly {{valueType}} _value; 
                    {{ctor}}
                    public static implicit operator {{structName}}({{valueType}} value) => new(value);
                    public static explicit operator {{valueType}}({{structName}} value) => value._value;
                    public override string ToString() => _value.ToString();
                    public override int GetHashCode() => _value.GetHashCode();
                 }
                 """;
    }
}