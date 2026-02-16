using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Shouldly;

namespace Skaar.ValueType.Tests.ValueTypeBased;

public class CustomStructsTests
{
    [Fact]
    public void Equals_SameInnerValue_ReturnsTrue()
    {
        GeneratorTestTargetCustomType0 t0 = new CustomValueType0(1);
        GeneratorTestTargetCustomType0 t1 = new CustomValueType0(1);

        t0.Equals(t1).ShouldBeTrue();
    }

    public class RecursiveType(ITestOutputHelper @out)
    {
        [Fact]
        public void Equals_SameInnerValue_ReturnsTrue()
        {
            GeneratorTestTargetCustomType1 t0 = (CustomValueType1) 42d;
            GeneratorTestTargetCustomType1 t1 = (CustomValueType1) 42;

            t0.Equals(t1).ShouldBeTrue();
        }
        
        [Fact]
        public void JsonConversion_Roundtrip_ReturnsSameValue()
        {
            GeneratorTestTargetCustomType1 value = (CustomValueType1) 42d;
            var target = new
            {
                Value = value
            };

            var json = JsonSerializer.Serialize(target);
            @out.WriteLine(json);
            
            var result = JsonSerializer.Deserialize(json, target.GetType());

            result.ShouldBe(target);
        }
    }
}

//TODO: Fix json for structs (not base types)
//TODO: Equality, parsing and conversion for recursive types

[ValueType<CustomValueType0>]
public partial struct GeneratorTestTargetCustomType0;

public record struct CustomValueType0(int Value);

[ValueType<CustomValueType1>]
public partial struct GeneratorTestTargetCustomType1;

[ValueType<double>]
public partial struct CustomValueType1
{
}

public struct CustomValueType2(int Value);

[ValueType<CustomValueType2>]
public partial struct CustomValueType3;

[ValueType<CustomValueType3>]
public partial struct GeneratorTestTargetCustomType2;