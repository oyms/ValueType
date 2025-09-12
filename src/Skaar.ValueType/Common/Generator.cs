using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Skaar.ValueType.Common;

internal class Generator(string @namespace)
{
    protected string Ns { get; } = @namespace;
    public const string AttributeName = "ValueTypeAttribute";
    string ToolName => Assembly.GetExecutingAssembly().GetName().Name;
    Version ToolVersion => Assembly.GetExecutingAssembly().GetName().Version;

    public void GenerateAttributeFiles(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource($"{AttributeName}.g.cs", SourceText.From(AttributeSource(AttributeName), Encoding.UTF8));
        });
    }
    private string AttributeSource(string className) =>
        $$"""
           using System;
           
           #nullable enable
           #pragma warning disable CS0436 // Type may be defined multiple times
           namespace {{Ns}};
           /// <summary>
           /// Classes decorated with this attribute will trigger code generation.
           /// A partial part of the struct will be generated in the same namespace.
           /// </summary>
           {{GeneratedCodeAttribute}}
           [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = false)]
           public class {{className}} : System.Attribute;
           
           /// <summary>
           /// Classes decorated with this attribute will trigger code generation.
           /// A partial part of the struct will be generated in the same namespace.
           /// </summary>
           /// <typeparam name="T">The type of the inner value this type wraps.</typeparam>
           {{GeneratedCodeAttribute}}
           [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = false)]
           public class {{className}}<T> : System.Attribute where T: struct;
           
           
                
           """;

    protected string GeneratedCodeAttribute =>
        $"[System.CodeDom.Compiler.GeneratedCode(\"{ToolName}\", \"{ToolVersion}\")]";
    
    protected bool HasConstructorDefined(INamedTypeSymbol? symbol)
    {
        if(symbol is null) return false;
        return symbol.InstanceConstructors.Any(ctor => 
        {
            if(ctor.IsStatic) return false;
            if (ctor.Parameters.Length != 1) return false;
            var pType = ctor.Parameters[0].Type;
            if (pType is INamedTypeSymbol named &&
                named.IsGenericType &&
                named.Name == nameof(ReadOnlySpan<char>) &&
                named.ContainingNamespace.ToDisplayString() == "System" &&
                named.TypeArguments.Length == 1 &&
                named.TypeArguments[0].SpecialType == SpecialType.System_Char)
            {
                return true;
            }
            return false;

        });
    }
}