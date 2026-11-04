using System.Text.Json;
using System.Text.Json.Serialization;

namespace Skaar.ValueType;

/// <summary>
/// This is a json converter for value types
/// </summary>
public class StructBasedJsonConverter<T, TT> : JsonConverter<T> where T : struct, IStructBasedValueType<TT> where TT : struct
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return default;

        var inner = JsonSerializer.Deserialize<TT>(ref reader, options);
        return (T)T.Create(inner);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
