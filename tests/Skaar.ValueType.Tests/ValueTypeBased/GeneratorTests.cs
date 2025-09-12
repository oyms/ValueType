using System.Diagnostics;
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
}

[ValueType<int>]
public partial struct GeneratorTestsTargetTypeInt;

[System.Diagnostics.DebuggerDisplay("{_innerValue}")]
file struct Kladd
{
    [System.Diagnostics.DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly int _innerValue;
    public Kladd(int innerValue) => _innerValue = innerValue;
    
    public static implicit operator Kladd(int innerValue) => new(innerValue);

}