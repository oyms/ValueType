Value type model code generator
===

This library generates code for value types that wrap string values or structs/primitives.

When you want strong types for your value objects or identifiers, this library can help you.

```csharp
[ValueType]
public partial struct MyValueTypeWrappingStrings;

[ValueType<Guid>]
public partial struct MyValueTypeWrappingGuids;

MyValueTypeWrappingGuids myInstance = Guid.NewGuid();
Console.WriteLine(myInstance); // prints the Guid value
```

The code generated assumes a fairly recent version of C#.

[Documentation on GitHub](https://github.com/oyms/ValueType/blob/main/README.md)

![Icon](https://raw.githubusercontent.com/oyms/ValueType/refs/heads/main/resources/logo.svg)