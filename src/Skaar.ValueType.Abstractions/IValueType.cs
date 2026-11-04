namespace Skaar.ValueType;

/// <summary>
/// This is a marker interface for generated value types
/// </summary>
public interface IValueType<T>
{
    /// <summary>
    /// The inner value
    /// </summary>
    T Value { get; }
}
