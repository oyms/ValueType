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
                var typeName = structSymbol!.Name;
                var ns = structSymbol.ContainingNamespace.ToDisplayString();
                var hasConstructorDefined = HasConstructorDefined(structSymbol as INamedTypeSymbol);
                productionContext.AddSource($"{ns}.{typeName}.g.cs", SourceText.From(StructSource(ns, typeName, !hasConstructorDefined), Encoding.UTF8));
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
    
    public void GenerateHelper(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource($"{HelperName}.g.cs", SourceText.From(HelperSource(HelperName), Encoding.UTF8));
        });
    }
    
    public void GenerateConverters(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource($"TypeConverter.g.cs", SourceText.From(TypeConverterSource(TypeConverterName), Encoding.UTF8));
        });        
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource($"JsonConverter.g.cs", SourceText.From(JsonConverterSource(JsonConverterName), Encoding.UTF8));
        });
    }

    private bool HasConstructorDefined(INamedTypeSymbol? symbol)
    {
        if (symbol is null) return false;
        return symbol.InstanceConstructors.Any(ctor =>
        {
            if (ctor.IsStatic) return false;
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

    private string StructSource(string structNamespace, string structName, bool renderCtor)
    {
        var ctor = renderCtor
            ? $"private {structName}(ReadOnlySpan<char> value) => _value = Clean(value).ToArray();"
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
        /// A value type wrapping a string value
        /// </summary>
        {{GeneratedCodeAttribute}}
        [JsonConverter(typeof({{Ns}}.{{JsonConverterName}}<{{structName}}>))]
        [TypeConverter(typeof({{Ns}}.{{TypeConverterName}}<{{structName}}>))]
        readonly partial struct {{structName}} :
            {{Ns}}.{{InterfaceName}},
            ISpanParsable<{{structName}}>,
            IEquatable<{{structName}}>,
            IEqualityOperators<{{structName}}, {{structName}}, bool>
        {
            {{ctor}}
        
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly ReadOnlyMemory<char> _value;
            
            ReadOnlySpan<char> {{Ns}}.{{InterfaceName}}.Span => _value.Span;
            
            #region Clean
            
            /// <summary>
            /// Cleans the input value according to the specified cleaning rules.
            /// </summary>
            private partial ReadOnlySpan<char> Clean(ReadOnlySpan<char> value);
            
            #endregion
            
            #region Validate
            
            public int Length => _value.Length;
            
            public bool IsValid => ValueIsValid(_value.Span);
            
            /// <summary>
            /// Checks if the value is valid according to the specified validation rules.
            /// </summary>
            private partial bool ValueIsValid(ReadOnlySpan<char> value);
            
            
            #endregion
            
            #region Parse
            
            public static {{structName}} Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
            {
                if (!TryParse(s, provider, out var result) && !result.IsValid)
                {
                    throw new FormatException("String is not a valid {{structName}}.");
                }
                
                return result;
            }
            
            public static {{structName}} Parse(string? s, IFormatProvider? provider = null)
            {
                if (!TryParse(s, provider, out var result) && !result.IsValid)
                {
                    throw new FormatException("String is not a valid {{structName}}.");
                }
                
                return result;
            }
            
            public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out {{structName}} result)
            {
                result = new {{structName}}(s);
                return result.IsValid;
            }
            
            public static bool TryParse(string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out {{structName}} result)
            {
                result = new {{structName}}(s);
                return result.IsValid;
            }
            
            #endregion
            
            #region equality
            
            public override int GetHashCode() => _value.GetHashCode();
            public bool Equals({{structName}} other) => _value.Span.SequenceEqual(other._value.Span);
            public override bool Equals(object? obj) => obj is {{structName}} other && Equals(other);
            public static bool operator ==({{structName}} left, {{structName}} right) => left.Equals(right);
            public static bool operator !=({{structName}} left, {{structName}} right) => !left.Equals(right);
            
            #endregion
            
            #region conversion
            
            /// <summary>
            /// Returns the underlying value as a string.
            /// </summary>
            public override string ToString() => _value.ToString();
            
            public static explicit operator string({{structName}} value) => value.ToString();
            public static explicit operator {{structName}}(string value) => new {{structName}}(value);
            public static implicit operator ReadOnlySpan<char>({{structName}} value) => value._value.Span;
            public static explicit operator {{structName}}(ReadOnlySpan<char> value) => new {{structName}}(value);
            
            #endregion
            
            #region Base interface
            
            string {{Ns}}.{{BaseInterfaceName}}<string>.Value => _value.ToString();
            
            #endregion
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
           /// This is a marker interface for generated value types
           /// </summary>
           {{GeneratedCodeAttribute}}
           public interface {{typeName}} : {{Ns}}.{{BaseInterfaceName}}<string>
           {
                /// <summary>
                /// <c>true</c> if the value is valid, <c>false</c> otherwise.
                /// </summary>
                bool IsValid { get; }
                /// <summary>
                /// The length of the value.
                /// </summary>
                int Length { get; }
                /// <summary>
                /// The value as a <see cref="ReadOnlySpan{char}"/>.
                /// </summary>
                ReadOnlySpan<char> Span { get; }
           }
                
           """;

    private string TypeConverterSource(string typeConverterName) =>
        $$"""
           using System;
           using System.ComponentModel;
           using System.Diagnostics.CodeAnalysis;
           using System.Globalization;
           
           #nullable enable
           #pragma warning disable CS0436 // Type may be defined multiple times
           namespace {{Ns}};
           /// <summary>
           /// This is a type converter for value types
           /// </summary>
           {{GeneratedCodeAttribute}}
           public class {{typeConverterName}}<T> : TypeConverter where T: IParsable<T>
           {
                public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string);
                public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType) => destinationType == typeof(string);
           
               public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
               {
                   if(value is string stringValue) return T.Parse(stringValue, culture);
                   return base.ConvertFrom(context, culture, value);
               }
               
               public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
               {
                   if (value is null) return null;
                   if(value is T typedValue) return typedValue.ToString();
                   return base.ConvertTo(context, culture, value, destinationType);
               }
           }
                
           """;

    private string JsonConverterSource(string jsonConverterName) =>
        $$"""
          using System;
          using System.Buffers;
          using System.Text;
          using System.Text.Json;
          using System.Text.Json.Serialization;
          
          #nullable enable
          #pragma warning disable CS0436 // Type may be defined multiple times
          namespace {{Ns}};
          /// <summary>
          /// This is a json converter for value types
          /// </summary>
          {{GeneratedCodeAttribute}}
          public class {{jsonConverterName}}<T> : JsonConverter<T> where T : ISpanParsable<T>, {{InterfaceName}}
          {
              public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
              {
                  ReadOnlySpan<byte> utf8Span = reader.HasValueSequence 
                      ? reader.ValueSequence.ToArray() 
                      : reader.ValueSpan;
              
                  var maxChars = Encoding.UTF8.GetMaxCharCount(utf8Span.Length);
              
                  var charBuffer = maxChars <= 256
                      ? stackalloc char[256]  // Small buffer on stack
                      : new char[maxChars];   // Fallback to heap if too big
              
                  int actualChars = Encoding.UTF8.GetChars(utf8Span, charBuffer);
              
                  ReadOnlySpan<char> decodedChars = charBuffer.Slice(0, actualChars);
              
                  if (T.TryParse(decodedChars, null, out var result))
                  {
                      return result;
                  }
              
                  throw new JsonException($"Invalid value for {typeof(T).Name}");
              }
              
              public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
              {
                  ArgumentNullException.ThrowIfNull(writer);
                  Span<char> buffer = stackalloc char[value.Length];
                  if (value is ISpanFormattable formattable && formattable.TryFormat(buffer, out int charsWritten, default, null))
                  {
                      ReadOnlySpan<char> slice = buffer.Slice(0, charsWritten);
                      writer.WriteStringValue(slice);
                  }
                  else
                  {
                      writer.WriteStringValue(value.Span);
                  }
              }
          }
               
          """;
    
    private string HelperSource(string helperName) =>
        $$"""
           using System;
           using System.Text.RegularExpressions;
           
           #nullable enable
           #pragma warning disable CS0436 // Type may be defined multiple times
           namespace {{Ns}};
           /// <summary>
           /// This is a helper class for value types
           /// </summary>
           {{GeneratedCodeAttribute}}
           internal static class {{helperName}}
           {
                /// <summary>
                /// Provides methods for cleaning.
                /// </summary>
                public static class Clean
                {
                    /// <summary>
                    /// A trivial implementation that returns the value unchanged.
                    /// </summary>
                    public static ReadOnlySpan<char> Default(ReadOnlySpan<char> value) => value;
                    /// <summary>
                    /// Removes all non-digit characters from the input value.
                    /// </summary>
                    public static ReadOnlySpan<char> RemoveNonDigits(ReadOnlySpan<char> rawValue) => Filter(rawValue, char.IsDigit); 
                    /// <summary>   
                    /// Removes all non-letter and non-digit characters from the input value.
                    /// </summary>
                    public static ReadOnlySpan<char> RemoveNonLettersOrDigits(ReadOnlySpan<char> rawValue) => Filter(rawValue, char.IsLetterOrDigit);     
                    /// <summary>
                    /// Removes all whitespace characters from the input value.
                    /// </summary>
                    public static ReadOnlySpan<char> RemoveWhitespace(ReadOnlySpan<char> rawValue) => Filter(rawValue, c => !char.IsWhiteSpace(c));
                    /// <summary>
                    /// Trims whitespace from the start and end of the input value.
                    /// </summary>
                    public static ReadOnlySpan<char> Trim(ReadOnlySpan<char> rawValue) => rawValue.Trim();
                    /// <summary>
                    /// Filters the input value based on a predicate.
                    /// </summary>
                    public static ReadOnlySpan<char> Filter(ReadOnlySpan<char> rawValue, Predicate<char> predicate)
                    {
                        var buffer = rawValue.Length <= 256 ? stackalloc char[rawValue.Length] : new char[rawValue.Length];
            
                        int j = 0;
                        for (int i = 0; i < rawValue.Length; i++)
                        {
                            if (predicate(rawValue[i]))
                            {
                                buffer[j++] = rawValue[i];
                            }
                        }
                        var result = new char[j];
                        buffer[..j].CopyTo(result);
                        return result;
                    }
                }
            
                /// <summary>
                /// Provides methods for validation.
                /// </summary>
                public static class Validate
                {
                    /// <summary>
                    /// The value is valid if it is not empty.
                    /// </summary>
                    public static bool Default(ReadOnlySpan<char> value) => !value.IsEmpty;
                    /// <summary>
                    /// Checks if the value contains only digits.
                    /// </summary>
                    public static bool IsDigits(ReadOnlySpan<char> value) => All(value, char.IsDigit);
                    /// <summary>
                    /// Validates all characters based on a predicate.
                    /// </summary>
                    public static bool All(ReadOnlySpan<char> value, Predicate<char> predicate)
                    {
                        foreach (var c in value)
                        {
                            if (!predicate(c))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    /// <summary>
                    /// Validates if the value matches a regular expression.
                    /// </summary>
                    public static bool IsMatch(ReadOnlySpan<char> value, Regex regex) => regex.IsMatch(value);
                }
        }
        """;
}