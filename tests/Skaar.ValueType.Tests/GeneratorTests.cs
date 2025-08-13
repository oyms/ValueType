using System.Text.Json;
using System.Text.RegularExpressions;

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
    [Fact]
    public void ValidateWithRegex()
    {
        var x = GeneratorTestsTargetType1.Parse("a123-b45-c6789");
        Assert.Equal("123-45-6789", x.ToString());
        Assert.Equal("123-45-6789", x);
        Assert.True(x.IsValid);
    }
}

[ValueType]
public partial struct GeneratorTestsTargetType0
{
    private partial ReadOnlySpan<char> Clean(ReadOnlySpan<char> value) => Helper.Clean.Trim(value);
    private partial bool ValueIsValid(ReadOnlySpan<char> value) => Helper.Validate.Default(value);
}

[ValueType]
internal partial struct GeneratorTestsTargetType1
{
    private partial ReadOnlySpan<char> Clean(ReadOnlySpan<char> value) => Helper.Clean.Filter(value, c => !char.IsLetter(c));
    private partial bool ValueIsValid(ReadOnlySpan<char> value) => Helper.Validate.IsMatch(value, MyRegex());
    [GeneratedRegex(@"^\d{3}-\d{2}-\d{4}$")]
    private static partial Regex MyRegex();
}

