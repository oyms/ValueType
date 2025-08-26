Value type
===

<img alt="icon" style="width: 200px;" src="./resources/logo.svg" />

```csharp
[ValueType]
public partial struct MyValueType
{
    private partial ReadOnlySpan<char> Clean(ReadOnlySpan<char> value) => Helper.Clean.Trim(value);
    private partial bool ValueIsValid(ReadOnlySpan<char> value) => Helper.Validate.Default(value);
}
```

Code generation to generate structs wrapping string values.
This avoids writing boilerplate code for value types wrapping strings.

The structs are optimized for memory usage and performance.
They implement `IEquatable<T>`, `IEqualityOperators<T,T,bool>` and `ISpanParsable<T>` interfaces.
They have both type converters and json converters (`System.Text.Json`).

Convert from `string` (or `ReadOnlySpan<char>`) using `Parse` or `TryParse` methods,
or with explicit conversion.

Convert the other way using `ToString()` method or conversion.

[![NuGet Version](https://img.shields.io/nuget/v/Skaar.ValueType.svg)](https://www.nuget.org/packages/Skaar.ValueType)

## Installation

```bash
dotnet add package Skaar.ValueType
```

## Usage

Create partial structs. Decorate with the `ValueType` attribute.
Implement partial methods to clean and validate the value.

`record struct` is not supported.
It cannot be a nested type.

### Clean

```csharp
private partial ReadOnlySpan<char> Clean(ReadOnlySpan<char> value);
```

This method is called when parsing/creating the value.
It can be implemented with a helper method from `Skaar.ValueType.Helper.Clean` class.

### Validate

```csharp
private partial bool ValueIsValid(ReadOnlySpan<char> value);
``` 

This method is called to validate the value.
It is called from the `IsValid` property and is used in parsing methods.

It can be implemented with a helper method from `Skaar.ValueType.Helper.Validate` class.

### Example

```csharp

using Skaar.ValueType;

[ValueType]
public partial struct MyValueType
{
    private partial ReadOnlySpan<char> Clean(ReadOnlySpan<char> value) => Helper.Clean.RemoveNonDigits(value);
    private partial bool ValueIsValid(ReadOnlySpan<char> value) => Helper.Validate.IsMatch(value, new Regex(@"^\d{3}$"));
} 

var value = MyValueType.Parse("123a");
Console.WriteLine(value); // 123
value.IsValid; // true
var sameValue = MyValueType.Parse("123b");
Console.WriteLine(value == sameValue); // true
var stringValue = (string) value; // Or value.ToString();
```

### Custom constructor

If a custom constructor is required (for instance, to set other properties),
this can be defined, and the partial generated part will omit the constructor.

The constructor must have the same signature as the generated one:

`GeneratorTestsTargetType2(ReadOnlySpan<char> value)`, and it should set 
the field `_value`. It can be private or public.

```C#
[ValueType]
public partial struct GeneratorTestsTargetType2
{
    private GeneratorTestsTargetType2(ReadOnlySpan<char> value)
    {
        _value = Clean(value).ToArray();
        WasSetInCtor = true;
    }
    private partial ReadOnlySpan<char> Clean(ReadOnlySpan<char> value) => Helper.Clean.Trim(value);
    private partial bool ValueIsValid(ReadOnlySpan<char> value) => Helper.Validate.Default(value);
    public bool WasSetInCtor { get; }
}
```