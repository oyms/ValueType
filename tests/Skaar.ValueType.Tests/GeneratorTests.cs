using System.Text.Json;

namespace Skaar.ValueType.Tests;

public class GeneratorTests
{
    [Fact]
    public void Parse()
    {
        var x = GeneratorTestsTargetType0.Parse("Hello World");
        Assert.Equal("Hello World", x.ToString());
        Assert.Equal("Hello World", x);
        Assert.True(x.IsValid);
    }
    
    [Fact]
    public void Equality()
    {
        var x = GeneratorTestsTargetType0.Parse("Hello World");
        var y = GeneratorTestsTargetType0.Parse("Hello World");
        Assert.True(x.Equals(y));
        Assert.Equal(x, y);
        Assert.True(x == y);
    }
    
    [Fact]
    public void JsonSerialization()
    {
        var x = GeneratorTestsTargetType0.Parse("Hello World");
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        
        var json = JsonSerializer.Serialize(x, options);
        Assert.Equal("\"Hello World\"", json);
        
        var deserialized = JsonSerializer.Deserialize<GeneratorTestsTargetType0>(json, options);
        Assert.Equal(x, deserialized);
    }

}
[ValueType]
internal partial struct GeneratorTestsTargetType0
{
    private partial ReadOnlySpan<char> Clean(ReadOnlySpan<char> value) => Helper.Clean.Trim(value);
    private partial bool ValueIsValid(ReadOnlySpan<char> value) => Helper.Validate.Default(value);
}