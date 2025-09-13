using Microsoft.CodeAnalysis;

namespace Skaar.ValueType.ValueTypeBased;

public class ConvertibleGenerator(ITypeSymbol type, ITypeSymbol genericType) : InterfaceImplementor
{
    protected override string InterfaceName => "IConvertible";
    protected override string Ns => "System";

    public override string RenderInterfaceName()
    {
        return $"{Ns}.{InterfaceName}";
    }

    protected override bool GenericArgumentTypeImplementsInterface() =>
        TypeImplementsInterface(genericType, Ns, InterfaceName);

    protected override bool WrapperTypeImplementsInterface() => TypeImplementsInterface(type, Ns, InterfaceName);

    public override string Render()
    {
        return $$"""
                     
                     ///<inheritdoc/>
                     System.TypeCode {{RenderInterfaceName()}}.GetTypeCode() => (({{RenderInterfaceName()}})_value).GetTypeCode();
                     ///<inheritdoc/>
                     bool {{RenderInterfaceName()}}.ToBoolean(IFormatProvider? provider) => (({{RenderInterfaceName()}})_value).ToBoolean(provider);
                     ///<inheritdoc/> 
                     char {{RenderInterfaceName()}}.ToChar(IFormatProvider? provider) => (({{RenderInterfaceName()}})_value).ToChar(provider);
                     ///<inheritdoc/> 
                     sbyte {{RenderInterfaceName()}}.ToSByte(IFormatProvider? provider) => (({{RenderInterfaceName()}})_value).ToSByte(provider);
                     ///<inheritdoc/> 
                     byte {{RenderInterfaceName()}}.ToByte(IFormatProvider? provider) => (({{RenderInterfaceName()}})_value).ToByte(provider);
                     ///<inheritdoc/> 
                     short {{RenderInterfaceName()}}.ToInt16(IFormatProvider? provider) => (({{RenderInterfaceName()}})_value).ToInt16(provider);
                     ///<inheritdoc/> 
                     ushort {{RenderInterfaceName()}}.ToUInt16(IFormatProvider? provider) => (({{RenderInterfaceName()}})_value).ToUInt16(provider);
                     ///<inheritdoc/> 
                     int {{RenderInterfaceName()}}.ToInt32(IFormatProvider? provider) => (({{RenderInterfaceName()}})_value).ToInt32(provider);
                     ///<inheritdoc/> 
                     uint {{RenderInterfaceName()}}.ToUInt32(IFormatProvider? provider) => (({{RenderInterfaceName()}})_value).ToUInt32(provider);
                     ///<inheritdoc/> 
                     long {{RenderInterfaceName()}}.ToInt64(IFormatProvider? provider) => (({{RenderInterfaceName()}})_value).ToInt64(provider);
                     ///<inheritdoc/> 
                     ulong {{RenderInterfaceName()}}.ToUInt64(IFormatProvider? provider) => (({{RenderInterfaceName()}})_value).ToUInt64(provider);
                     ///<inheritdoc/> 
                     float {{RenderInterfaceName()}}.ToSingle(IFormatProvider? provider) => (({{RenderInterfaceName()}})_value).ToSingle(provider);
                     ///<inheritdoc/> 
                     double {{RenderInterfaceName()}}.ToDouble(IFormatProvider? provider) => (({{RenderInterfaceName()}})_value).ToDouble(provider);
                     ///<inheritdoc/> 
                     decimal {{RenderInterfaceName()}}.ToDecimal(IFormatProvider? provider) => (({{RenderInterfaceName()}})_value).ToDecimal(provider);
                     ///<inheritdoc/> 
                     DateTime {{RenderInterfaceName()}}.ToDateTime(IFormatProvider? provider) => (({{RenderInterfaceName()}})_value).ToDateTime(provider);
                     ///<inheritdoc/> 
                     string {{RenderInterfaceName()}}.ToString(IFormatProvider? provider) => (({{RenderInterfaceName()}})_value).ToString(provider);
                     ///<inheritdoc/>
                     object {{RenderInterfaceName()}}.ToType(Type conversionType, IFormatProvider? provider) => (({{RenderInterfaceName()}})_value).ToType(conversionType, provider);
                 """;
    }
}