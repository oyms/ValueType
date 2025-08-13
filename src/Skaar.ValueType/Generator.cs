using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Skaar.ValueType;

[Generator]
public class Generator : IIncrementalGenerator
{
    private static readonly string AttributeName = "ValueTypeAttribute";
    private static readonly string InterfaceName = "IValueType";
    private static readonly string TypeConverterName = "ParsableTypeConverter";
    private static readonly string JsonConverterName = "ParsableJsonConverter";
    private static readonly string HelperName = "Helper";
    private static readonly string AttributeNamespace = "Skaar.ValueType";
    string ToolName => Assembly.GetExecutingAssembly().GetName().Name;
    Version ToolVersion => Assembly.GetExecutingAssembly().GetName().Version;
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        GenerateAttributeFiles(context);
        GenerateInterface(context);
        GenerateConverters(context);
        GenerateHelper(context);
        GenerateStructFiles(context);
    }

    private void GenerateStructFiles(IncrementalGeneratorInitializationContext context)
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
                .Any(attr => attr.AttributeClass?.ToDisplayString() == $"{AttributeNamespace}.{AttributeName}")
            )
            .Collect();
        
        context.RegisterSourceOutput(structDeclarations, (productionContext, structSymbols) =>
        {
            foreach (var structSymbol in structSymbols)
            {
                var typeName = structSymbol.Name;
                var ns = structSymbol.ContainingNamespace.ToDisplayString();
                var visibility = structSymbol.DeclaredAccessibility switch
                {
                    Accessibility.Public => "public ",
                    Accessibility.Internal => "internal ",
                    _ => string.Empty
                };
                productionContext.AddSource($"{ns}.{typeName}.g.cs", SourceText.From(StructSource(ns, typeName, visibility), Encoding.UTF8));
            }
        });
    }

    private void GenerateAttributeFiles(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource($"{AttributeName}.g.cs", SourceText.From(AttributeSource(AttributeNamespace, AttributeName), Encoding.UTF8));
        });
    }

    private void GenerateInterface(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource($"{InterfaceName}.g.cs", SourceText.From(InterfaceSource(AttributeNamespace, InterfaceName), Encoding.UTF8));
        });
    }    
    
    private void GenerateHelper(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource($"{HelperName}.g.cs", SourceText.From(HelperSource(AttributeNamespace, HelperName), Encoding.UTF8));
        });
    }
    
    private void GenerateConverters(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource($"TypeConverter.g.cs", SourceText.From(TypeConverterSource(AttributeNamespace, TypeConverterName), Encoding.UTF8));
        });        
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource($"JsonConverter.g.cs", SourceText.From(JsonConverterSource(AttributeNamespace, JsonConverterName), Encoding.UTF8));
        });
    }

    private string StructSource(string @namespace, string structName, string visibility)
    {
        return $$"""
            using System;
            using System.CodeDom.Compiler;
            using System.ComponentModel;
            using System.Diagnostics;
            using System.Diagnostics.CodeAnalysis;
            using System.Numerics;
            using System.Text.Json.Serialization;
            
            #nullable enable
            
            namespace {{@namespace}}
            {
                /// <summary>
                /// A value type wrapping a string value
                /// </summary>
                [GeneratedCode("{{ToolName}}", "{{ToolVersion}}")]
                [JsonConverter(typeof({{AttributeNamespace}}.{{JsonConverterName}}<{{structName}}>))]
                [TypeConverter(typeof({{AttributeNamespace}}.{{TypeConverterName}}<{{structName}}>))]
                {{visibility}}readonly partial struct {{structName}} :
                    {{AttributeNamespace}}.{{InterfaceName}},
                    ISpanParsable<{{structName}}>,
                    IEquatable<{{structName}}>,
                    IEqualityOperators<{{structName}}, {{structName}}, bool>
                {
                    private {{structName}}(ReadOnlySpan<char> value) => _value = Clean(value).ToArray();
                
                    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                    private readonly ReadOnlyMemory<char> _value;
                    
                    ReadOnlySpan<char> {{AttributeNamespace}}.{{InterfaceName}}.Span => _value.Span;
                    
                    #region Clean
                    
                    private partial ReadOnlySpan<char> Clean(ReadOnlySpan<char> value);
                    
                    #endregion
                    
                    #region Validate
                    
                    public int Length => _value.Length;
                    
                    public bool IsValid => ValueIsValid(_value.Span);
                    
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
                }
            }
        """;
    }

    private string AttributeSource(string @namespace, string className) =>
        $$"""
           using System;
           using System.CodeDom.Compiler;
           
           #nullable enable
           #pragma warning disable CS0436 // Type may be defined multiple times
           namespace {{@namespace}};
           /// <summary>
           /// Classes decorated with this attribute will trigger code generation.
           /// A partial part of the struct will be generated in the same namespace.
           /// </summary>
           [GeneratedCode("{{ToolName}}", "{{ToolVersion}}")]
           [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = false)]
           public class {{className}} : System.Attribute;
                
           """;
    
    private string InterfaceSource(string @namespace, string typeName) =>
        $$"""
           using System;
           using System.CodeDom.Compiler;
           
           #nullable enable
           #pragma warning disable CS0436 // Type may be defined multiple times
           namespace {{@namespace}};
           /// <summary>
           /// This is a marker interface for generated value types
           /// </summary>
           [GeneratedCode("{{ToolName}}", "{{ToolVersion}}")]
           public interface {{typeName}}
           {
                bool IsValid { get; }
                int Length { get; }
                ReadOnlySpan<char> Span { get; }
           }
                
           """;

    private string TypeConverterSource(string @namespace, string typeConverterName) =>
        $$"""
           using System;
           using System.CodeDom.Compiler;
           using System.ComponentModel;
           using System.Diagnostics.CodeAnalysis;
           using System.Globalization;
           
           #nullable enable
           #pragma warning disable CS0436 // Type may be defined multiple times
           namespace {{@namespace}};
           /// <summary>
           /// This is a type converter for value types
           /// </summary>
           [GeneratedCode("{{ToolName}}", "{{ToolVersion}}")]
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

    private string JsonConverterSource(string @namespace, string jsonConverterName) =>
        $$"""
          using System;
          using System.Buffers;
          using System.CodeDom.Compiler;
          using System.Text;
          using System.Text.Json;
          using System.Text.Json.Serialization;
          
          #nullable enable
          #pragma warning disable CS0436 // Type may be defined multiple times
          namespace {{@namespace}};
          /// <summary>
          /// This is a json converter for value types
          /// </summary>
          [GeneratedCode("{{ToolName}}", "{{ToolVersion}}")]
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
    
    private string HelperSource(string @namespace, string helperName) =>
        $$"""
           using System;
           using System.CodeDom.Compiler;
           using System.Text.RegularExpressions;
           
           #nullable enable
           #pragma warning disable CS0436 // Type may be defined multiple times
           namespace {{@namespace}};
           /// <summary>
           /// This is a helper class for value types
           /// </summary>
           [GeneratedCode("{{ToolName}}", "{{ToolVersion}}")]
           internal static class {{helperName}}
           {
                public static class Clean
                {
                    public static ReadOnlySpan<char> Default(ReadOnlySpan<char> value) => value;
                    public static ReadOnlySpan<char> RemoveNonDigits(ReadOnlySpan<char> rawValue) => Filter(rawValue, char.IsDigit);     
                    public static ReadOnlySpan<char> RemoveNonLettersOrDigits(ReadOnlySpan<char> rawValue) => Filter(rawValue, char.IsLetterOrDigit);     
                    public static ReadOnlySpan<char> RemoveWhitespace(ReadOnlySpan<char> rawValue) => Filter(rawValue, c => !char.IsWhiteSpace(c));
                    public static ReadOnlySpan<char> Trim(ReadOnlySpan<char> rawValue) => rawValue.Trim();
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
            
                public static class Validate
                {
                    public static bool Default(ReadOnlySpan<char> value) => !value.IsEmpty;
                    public static bool IsDigits(ReadOnlySpan<char> value) => All(value, char.IsDigit);
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
                    public static bool IsMatch(ReadOnlySpan<char> value, Regex regex) => regex.IsMatch(value);
                }
        }
        """;
}