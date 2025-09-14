using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Skaar.ValueType.Tests.StringBased;

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

    [Fact]
    public void CustomConstructor()
    {
        var x = GeneratorTestsTargetType2.Parse("Hello World");
        Assert.True(x.WasSetInCtor);
    }

    [Fact]
    public void DefaultCleanAndValid_WhenMethodsAreNotDefined_TrimsWhitespace()
    {
        const string expected = "Hello World";
        var targetValid = GeneratorTestsTargetType3.Parse($"\t\r {expected} \n");
        GeneratorTestsTargetType3 targetInvalid = default;

        Assert.True(targetValid.IsValid);
        Assert.Equal(expected, targetValid.ToString());
        Assert.False(targetInvalid.IsValid);
        Assert.Empty(targetInvalid.ToString());
    }
}

[ValueType]
public partial struct GeneratorTestsTargetType0
{
    private ReadOnlySpan<char> Clean(ReadOnlySpan<char> value) => Helper.Clean.Trim(value);
    private bool ValueIsValid(ReadOnlySpan<char> value) => Helper.Validate.Default(value);
}

[ValueType]
public partial struct GeneratorTestsTargetType1
{
    private static ReadOnlySpan<char> Clean(ReadOnlySpan<char> value) => Helper.Clean.Filter(value, c => !char.IsLetter(c));
    private static bool ValueIsValid(ReadOnlySpan<char> value) => Helper.Validate.IsMatch(value, MyRegex());
    [GeneratedRegex(@"^\d{3}-\d{2}-\d{4}$")]
    private static partial Regex MyRegex();
}

[ValueType]
[StructLayout(LayoutKind.Auto)]
public partial struct GeneratorTestsTargetType2
{
    private GeneratorTestsTargetType2(ReadOnlySpan<char> value)
    {
        _value = Clean(value).ToArray();
        WasSetInCtor = true;
    }
    private ReadOnlySpan<char> Clean(ReadOnlySpan<char> value) => Helper.Clean.Trim(value);
    private bool ValueIsValid(ReadOnlySpan<char> value) => Helper.Validate.Default(value);
    public bool WasSetInCtor { get; }
}

[ValueType]
public partial struct GeneratorTestsTargetType3;

