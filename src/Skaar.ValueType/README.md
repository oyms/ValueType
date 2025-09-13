Value type model code generator
===

This library generates code for value types that wrap string values or structs/primitives.

```csharp
[ValueType]
public partial struct MyValueType
{
    private partial ReadOnlySpan<char> Clean(ReadOnlySpan<char> value) => Helper.Clean.Trim(value);
    private partial bool ValueIsValid(ReadOnlySpan<char> value) => Helper.Validate.Default(value);
}
```
```csharp
[ValueType<Guid>]
public partial struct MyValueTypeWrappingGuids;

MyValueTypeWrappingGuids myInstance = Guid.NewGuid();
Console.WriteLine(myInstance); // prints the Guid value
```

[Documentation on GitHub](https://github.com/oyms/ValueType/blob/main/README.md)

![Icon](https://raw.githubusercontent.com/oyms/ValueType/refs/heads/main/resources/logo.svg)