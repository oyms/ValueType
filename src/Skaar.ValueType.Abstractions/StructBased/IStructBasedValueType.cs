namespace Skaar.ValueType;

/// <summary>
/// This is a marker interface for struct based value types
/// </summary>
public interface IStructBasedValueType<T> : IValueType<T> where T : struct
{
    /// <summary>
    /// <c>true</c> if the value is different from default, <c>false</c> otherwise.
    /// </summary>
    bool HasValue { get; }

    /// <summary>
    /// A factory method to create a new instance of the value type.
    /// Can be cast to the concrete type.
    /// </summary>
    static abstract IStructBasedValueType<T> Create(T value);
}
