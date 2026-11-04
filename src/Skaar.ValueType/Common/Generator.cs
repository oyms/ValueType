using System;
using System.Reflection;

namespace Skaar.ValueType.Common;

/// <remarks>
/// The attribute, interfaces, converters and helpers referenced by generated code
/// live in the Skaar.ValueType.Abstractions assembly (namespace <see cref="Ns"/>),
/// so every consuming assembly shares one definition of them.
/// </remarks>
internal class Generator(string @namespace)
{
    protected string Ns { get; } = @namespace;
    public const string AttributeName = "ValueTypeAttribute";
    public const string BaseInterfaceName = "IValueType";
    string ToolName => Assembly.GetExecutingAssembly().GetName().Name;
    Version ToolVersion => Assembly.GetExecutingAssembly().GetName().Version;

    protected string GeneratedCodeAttribute =>
        $"[System.CodeDom.Compiler.GeneratedCode(\"{ToolName}\", \"{ToolVersion}\")]";

}
