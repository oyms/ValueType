namespace Skaar.ValueType;

/// <summary>
/// This is a marker interface for generated value types
/// </summary>
public interface IStringBasedValueType : IValueType<string>
{
    /// <summary>
    /// <c>true</c> if the value is valid, <c>false</c> otherwise.
    /// </summary>
    bool IsValid { get; }
    /// <summary>
    /// The length of the value.
    /// </summary>
    int Length { get; }
    /// <summary>
    /// The value as a <c>ReadOnlySpan{char}</c>.
    /// </summary>
    ReadOnlySpan<char> Span { get; }
}
