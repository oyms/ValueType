using System.Numerics;
using System.Text.Json;
using Shouldly;

namespace Skaar.ValueType.Tests.ValueTypeBased;

public class GeneratorTests
{
    [Fact]
    public void IntBasedType_ToString_ReturnsSameAsInnerValue()
    {
        const int innerValue = 72;
        var target = (GeneratorTestsTargetTypeInt)innerValue;
        target.ToString().ShouldBe(innerValue.ToString());
    }

    [Fact]
    public void Equals_SameInnerValue_ReturnsTrue()
    {
        GeneratorTestsTargetTypeInt t0 = 10;
        GeneratorTestsTargetTypeInt t1 = 10;

        t0.Equals(t1).ShouldBeTrue();
        ((IEquatable<GeneratorTestsTargetTypeInt>)t1).Equals(t0).ShouldBeTrue();
    }

    [Fact]
    public void Equals_DifferentInnerValue_ReturnsFalse()
    {
        GeneratorTestsTargetTypeInt t0 = 100;
        GeneratorTestsTargetTypeInt t1 = 17;

        t0.Equals(t1).ShouldBeFalse();
        ((IEquatable<GeneratorTestsTargetTypeInt>)t1).Equals(t0).ShouldBeFalse();
    }

    [Fact]
    public void ToString_ToString_ReturnsSameAsInnerValue()
    {
        var value = Guid.NewGuid();
        GeneratorTestsTargetTypeGuid target = value;

        ((IFormattable)target).ToString("B", null).ShouldBe(value.ToString("B", null));
    }

    [Fact]
    public void Parse_Parse_ReturnsSameAsInnerValue()
    {
        var value = Guid.NewGuid();
        var result = GeneratorTestsTargetTypeGuid.Parse(value.ToString("B", null));
        result.ShouldBe(value);
    }

    [Fact]
    public void JsonConversion_Roundtrip_ReturnsSameValue()
    {
        var target = new JsonTestTarget(
            255,
            Guid.NewGuid(),
            true,
            DateTimeOffset.Now,
            123.456m,
            789.0123456f
        );

        var json = JsonSerializer.Serialize(target);
        var result = JsonSerializer.Deserialize<JsonTestTarget>(json);

        result.ShouldBe(target);
    }

    [Fact]
    public void ExplicitImplementation_OnType_UsesThatImplementation()
    {
        IFormattable other = (GeneratorTestsTargetTypeFloat)123.456f;
        other.ToString("E", null).ShouldBe(123.456f.ToString("E", null));

        IFormattable target = (GeneratorTestsTargetTypeFloatWithFormattableImplementation)123.456f;
        target.ToString("E", null).ShouldBe("E");
    }
}

[ValueType<int>]
public partial struct GeneratorTestsTargetTypeInt;

[ValueType<Guid>]
public partial struct GeneratorTestsTargetTypeGuid;

[ValueType<bool>]
public partial struct GeneratorTestsTargetTypeBool;

[ValueType<DateTimeOffset>]
public partial struct GeneratorTestsTargetTypeDate;

[ValueType<decimal>]
public partial struct GeneratorTestsTargetTypeDecimal;

[ValueType<float>]
public partial struct GeneratorTestsTargetTypeFloat;

[ValueType<float>]
public partial struct GeneratorTestsTargetTypeFloatWithFormattableImplementation : IFormattable
{
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return format ?? "default";
    }
}

file record JsonTestTarget(
    GeneratorTestsTargetTypeInt IntType,
    GeneratorTestsTargetTypeGuid GuidType,
    GeneratorTestsTargetTypeBool BoolType,
    GeneratorTestsTargetTypeDate DateType,
    GeneratorTestsTargetTypeDecimal DecimalType,
    GeneratorTestsTargetTypeFloat FloatType
);