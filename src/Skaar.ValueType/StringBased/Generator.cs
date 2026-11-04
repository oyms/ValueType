using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

#nullable enable

namespace Skaar.ValueType.StringBased;

internal class Generator(string @namespace) : Common.Generator(@namespace)
{
    private static readonly string InterfaceName = "IStringBasedValueType";
    private static readonly string TypeConverterName = "ParsableTypeConverter";
    private static readonly string JsonConverterName = "ParsableJsonConverter";
    private static readonly string HelperName = "Helper";
    
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
            .Where(s => s is not null && s.GetAttributes()
                .Any(attr => attr.AttributeClass?.ToDisplayString() == $"{Ns}.{AttributeName}")
            )
            .Where(s => s!.ContainingType is null)
            .Collect();
        
        context.RegisterSourceOutput(structDeclarations, (productionContext, structSymbols) =>
        {
            foreach (var structSymbol in structSymbols.Where(s => s is not null))
            {
                var namedSymbol = structSymbol as INamedTypeSymbol;
                var typeName = structSymbol!.Name;
                var ns = structSymbol.ContainingNamespace.ToDisplayString();
                var setup = new StructParameters(
                    ns,typeName, 
                    !HasConstructorDefined(namedSymbol), 
                    !HasCleanMethodDefined(namedSymbol), 
                    !HasValidateMethodDefined(namedSymbol));
                productionContext.AddSource($"{ns}.{typeName}.g.cs", SourceText.From(StructSource(setup), Encoding.UTF8));
            }
        });
    }

    private bool HasConstructorDefined(INamedTypeSymbol? symbol)
    {
        if (symbol is null) return false;
        return symbol.InstanceConstructors.Any(ctor =>
        {
            if (ctor.IsStatic) return false;
            if (ctor.Parameters.Length != 1) return false;
            return IsReadOnlySpanOfChar(ctor.Parameters[0].Type);
        });
    }

    private bool HasCleanMethodDefined(INamedTypeSymbol? symbol)
    {
        if (symbol is null) return false;
        foreach (var method in symbol.GetMembers("Clean").OfType<IMethodSymbol>())
        {
            if (method.Parameters.Length != 1) continue;
            if (!IsReadOnlySpanOfChar(method.Parameters[0].Type)) continue;
            if (!IsReadOnlySpanOfChar(method.ReturnType)) continue;
            if (method is { IsPartialDefinition: true, PartialImplementationPart: null }) continue;
            return true;
        }
        return false;
    }

    private bool HasValidateMethodDefined(INamedTypeSymbol? symbol)
    {
        if (symbol is null) return false;
        foreach (var method in symbol.GetMembers("ValueIsValid").OfType<IMethodSymbol>())
        {
            if (method.Parameters.Length != 1) continue;
            if (!IsReadOnlySpanOfChar(method.Parameters[0].Type)) continue;
            if (method is { IsPartialDefinition: true, PartialImplementationPart: null }) continue;
            return true;
        }
        return false;
    }

    private bool IsReadOnlySpanOfChar(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol named &&
            named.IsGenericType &&
            named.Name == nameof(ReadOnlySpan<char>) &&
            named.ContainingNamespace.ToDisplayString() == "System" &&
            named.TypeArguments.Length == 1 &&
            named.TypeArguments[0].SpecialType == SpecialType.System_Char)
        {
            return true;
        }
        return false;
    }

    private record StructParameters(string Namespace, string TypeName, bool RenderCreator, bool RenderClean, bool RenderValidate)
    {
        public string Namespace { get; } = Namespace;
        public string TypeName { get; } = TypeName;
        public bool RenderCreator { get; } = RenderCreator;
        public bool RenderClean { get; } = RenderClean;
        public bool RenderValidate { get; } = RenderValidate;
    }

    private string StructSource(StructParameters setup)
    {
        var ctor = setup.RenderCreator
            ? $"private {setup.TypeName}(ReadOnlySpan<char> value) => _value = Clean(value).ToArray();"
            : "";
        var cleanMethod = setup.RenderClean
            ? $"""
               
                   #region Clean
                   
                   /// <summary>
                   /// Cleans the input value according to the specified cleaning rules.
                   /// </summary>
                   /// <remarks>Implement this in the other partial part to suppress the generation.</remarks>
                   private ReadOnlySpan<char> Clean(ReadOnlySpan<char> value) => {Ns}.{HelperName}.Clean.Trim(value);
                   
                   #endregion
               """
            : "";
        var validateMethod = setup.RenderValidate
            ? $"""
                   /// <summary>
                   /// Checks if the value is valid according to the specified validation rules.
                   /// </summary>
                   /// <remarks>Implement this in the other partial part to suppress the generation.</remarks>
                   private bool ValueIsValid(ReadOnlySpan<char> value) => {Ns}.{HelperName}.Validate.Default(value);
               """
            : "";
            
        return $$"""
        using System;
        using System.ComponentModel;
        using System.Diagnostics;
        using System.Diagnostics.CodeAnalysis;
        using System.Numerics;
        using System.Text.Json.Serialization;
        
        #nullable enable
        
        namespace {{setup.Namespace}};
        
        /// <summary>
        /// A value type wrapping a string value
        /// </summary>
        {{GeneratedCodeAttribute}}
        [JsonConverter(typeof({{Ns}}.{{JsonConverterName}}<{{setup.TypeName}}>))]
        [TypeConverter(typeof({{Ns}}.{{TypeConverterName}}<{{setup.TypeName}}>))]
        readonly partial struct {{setup.TypeName}} :
            {{Ns}}.{{InterfaceName}},
            ISpanParsable<{{setup.TypeName}}>,
            IEquatable<{{setup.TypeName}}>,
            IEqualityOperators<{{setup.TypeName}}, {{setup.TypeName}}, bool>
        {
            {{ctor}}
        
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly ReadOnlyMemory<char> _value;
            
            ReadOnlySpan<char> {{Ns}}.{{InterfaceName}}.Span => _value.Span;
            
            {{cleanMethod}}
            
            #region Validate
            
            public int Length => _value.Length;
            
            public bool IsValid => ValueIsValid(_value.Span);
            
           {{validateMethod}}
           
            #endregion
            
            #region Parse
            
            public static {{setup.TypeName}} Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
            {
                if (!TryParse(s, provider, out var result) && !result.IsValid)
                {
                    throw new FormatException("String is not a valid {{setup.TypeName}}.");
                }
                
                return result;
            }
            
            public static {{setup.TypeName}} Parse(string? s, IFormatProvider? provider = null)
            {
                if (!TryParse(s, provider, out var result) && !result.IsValid)
                {
                    throw new FormatException("String is not a valid {{setup.TypeName}}.");
                }
                
                return result;
            }
            
            public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out {{setup.TypeName}} result)
            {
                result = new {{setup.TypeName}}(s);
                return result.IsValid;
            }
            
            public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out {{setup.TypeName}} result)
            {
                result = new {{setup.TypeName}}(s);
                return result.IsValid;
            }
            
            #endregion
            
            #region equality
            
            public override int GetHashCode() {
                var span = _value.Span;
                var hc = new System.HashCode();
                for (int i = 0; i < span.Length; i++) hc.Add(span[i]);
                return hc.ToHashCode();
            }
            public bool Equals({{setup.TypeName}} other) => _value.Span.SequenceEqual(other._value.Span);
            public override bool Equals(object? obj) => obj is {{setup.TypeName}} other && Equals(other);
            public static bool operator ==({{setup.TypeName}} left, {{setup.TypeName}} right) => left.Equals(right);
            public static bool operator !=({{setup.TypeName}} left, {{setup.TypeName}} right) => !left.Equals(right);
            
            #endregion
            
            #region conversion
            
            /// <summary>
            /// Returns the underlying value as a string.
            /// </summary>
            public override string ToString() => _value.ToString();
            
            public static explicit operator string({{setup.TypeName}} value) => value.ToString();
            public static explicit operator {{setup.TypeName}}(string value) => new {{setup.TypeName}}(value);
            public static implicit operator ReadOnlySpan<char>({{setup.TypeName}} value) => value._value.Span;
            public static explicit operator {{setup.TypeName}}(ReadOnlySpan<char> value) => new {{setup.TypeName}}(value);
            
            #endregion
            
            #region Base interface
            
            string {{Ns}}.{{BaseInterfaceName}}<string>.Value => _value.ToString();
            
            #endregion
        }
        """;
    }
}