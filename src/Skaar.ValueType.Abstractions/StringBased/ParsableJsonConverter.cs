using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Skaar.ValueType;

/// <summary>
/// This is a json converter for value types
/// </summary>
public class ParsableJsonConverter<T> : JsonConverter<T> where T : ISpanParsable<T>, IStringBasedValueType
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
