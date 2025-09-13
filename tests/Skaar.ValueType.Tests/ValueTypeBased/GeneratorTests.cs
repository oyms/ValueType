using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
        ((IEquatable<GeneratorTestsTargetTypeInt>) t1).Equals(t0).ShouldBeTrue();
    }  
    
    [Fact]
    public void Equals_DifferentInnerValue_ReturnsFalse()
    {
        GeneratorTestsTargetTypeInt t0 = 100;
        GeneratorTestsTargetTypeInt t1 = 17;
        
        t0.Equals(t1).ShouldBeFalse();
        ((IEquatable<GeneratorTestsTargetTypeInt>) t1).Equals(t0).ShouldBeFalse();
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
}

[ValueType<int>]
public partial struct GeneratorTestsTargetTypeInt;

[ValueType<Guid>]
public partial struct GeneratorTestsTargetTypeGuid;