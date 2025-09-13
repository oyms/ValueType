using Microsoft.CodeAnalysis;

namespace Skaar.ValueType;

[Generator]
public class Generator : IIncrementalGenerator
{
    public static readonly string AttributeNamespace = "Skaar.ValueType";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var stringBasedSource = new StringBased.Generator(AttributeNamespace);
        stringBasedSource.GenerateAttributeFiles(context);
        stringBasedSource.GenerateCommonInterface(context);
        stringBasedSource.GenerateInterface(context);
        stringBasedSource.GenerateConverters(context);
        stringBasedSource.GenerateHelper(context);
        stringBasedSource.GenerateStructFiles(context);

        var valueBasedSource = new ValueTypeBased.Generator(AttributeNamespace);
        valueBasedSource.GenerateInterface(context);
        valueBasedSource.GenerateConverters(context);
        valueBasedSource.GenerateStructFiles(context);
    }
}