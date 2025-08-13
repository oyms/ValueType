Value type model code generator
===

This library generates code for value types that wrap string values.

```csharp
[ValueType]
public partial struct MyValueType
{
    private partial ReadOnlySpan<char> Clean(ReadOnlySpan<char> value) => Helper.Clean.Trim(value);
    private partial bool ValueIsValid(ReadOnlySpan<char> value) => Helper.Validate.Default(value);
}
```

[Documentation on GitHub](https://github.com/oyms/ValueType/blob/main/README.md)

![Icon](https://raw.githubusercontent.com/oyms/ValueType/refs/heads/main/.idea/.idea.ValueType/.idea/icon.svg)