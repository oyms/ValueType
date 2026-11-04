namespace Skaar.ValueType;

/// <summary>
/// Classes decorated with this attribute will trigger code generation.
/// A partial part of the struct will be generated in the same namespace.
/// </summary>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
public class ValueTypeAttribute : Attribute;

/// <summary>
/// Classes decorated with this attribute will trigger code generation.
/// A partial part of the struct will be generated in the same namespace.
/// </summary>
/// <typeparam name="T">The type of the inner value this type wraps.</typeparam>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
public class ValueTypeAttribute<T> : Attribute where T : struct;
