using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bonsai.Harp;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Harp.Generators;

public class DeviceInfo
{
    public string Device;
    public int WhoAmI;
    public HarpVersion FirmwareVersion;
    public HarpVersion HardwareTargets;
    public Dictionary<string, RegisterInfo> Registers = [];
    public Dictionary<string, BitMaskInfo> BitMasks = [];
    public Dictionary<string, GroupMaskInfo> GroupMasks = [];
}

[Flags]
public enum RegisterAccess
{
    Read = 0x1,
    Write = 0x2,
    Event = 0x4
}

public enum RegisterVisibility
{
    Public,
    Private
}

public enum MemberConverter
{
    None,
    Payload,
    RawPayload
}

public class RegisterInfo
{
    public int Address;
    public string Description = "";
    public RegisterAccess Access = RegisterAccess.Read;
    public PayloadType Type;
    public int Length;
    public Dictionary<string, PayloadMemberInfo> PayloadSpec;
    public RegisterVisibility Visibility;
    public bool Volatile;
    public string MaskType;
    public string InterfaceType;
    public MemberConverter Converter;
    public float? MinValue;
    public float? MaxValue;
    public float? DefaultValue;

    public bool HasConverter => Converter > MemberConverter.None;
    public string PayloadInterfaceType => Converter == MemberConverter.RawPayload
        ? "ArraySegment<byte>"
        : TemplateHelper.GetInterfaceType(Type, Length);
}

public class PayloadMemberInfo
{
    public int? Mask;
    public int? Offset;
    public int? Length;
    public string MaskType;
    public string InterfaceType;
    public MemberConverter Converter;
    public string Description = "";
    public float? MinValue;
    public float? MaxValue;
    public float? DefaultValue;

    public bool HasConverter => Converter > MemberConverter.None;
    public string GetConverterInterfaceType(PayloadType payloadType)
    {
        return Converter switch
        {
            MemberConverter.RawPayload => "ArraySegment<byte>",
            MemberConverter.Payload => Length.GetValueOrDefault() > 0
                ? $"ArraySegment<{TemplateHelper.GetInterfaceType(payloadType, 0)}>"
                : TemplateHelper.GetInterfaceType(payloadType, 0),
            _ => TemplateHelper.GetInterfaceType(payloadType, Length.GetValueOrDefault())
        };
    }
}

public class BitMaskInfo
{
    public string Description = "";
    public Dictionary<string, MaskValue> Bits = [];

    public string InterfaceType => TemplateHelper.GetInterfaceType(Bits);
}

public class GroupMaskInfo
{
    public string Description = "";
    public Dictionary<string, MaskValue> Values = [];

    public string InterfaceType => TemplateHelper.GetInterfaceType(Values);
}

public class MaskValue
{
    public int Value;
    public string Description;
}

public static partial class TemplateHelper
{
    public static DeviceInfo ReadDeviceMetadata(string path)
    {
        using var reader = new StreamReader(path);
        var parser = new MergingParser(new Parser(reader));
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(RegisterAccessTypeConverter.Instance)
            .WithTypeConverter(MaskValueTypeConverter.Instance)
            .Build();
        return deserializer.Deserialize<DeviceInfo>(parser);
    }

    public static string GetInterfaceType(string name, RegisterInfo register)
    {
        if (!string.IsNullOrEmpty(register.InterfaceType)) return register.InterfaceType;
        else if (register.PayloadSpec != null) return $"{name}Payload";
        else if (!string.IsNullOrEmpty(register.MaskType)) return register.MaskType;
        else return GetInterfaceType(register.Type, register.Length);
    }

    public static string GetInterfaceType(PayloadMemberInfo member, PayloadType payloadType)
    {
        if (!string.IsNullOrEmpty(member.InterfaceType)) return member.InterfaceType;
        else if (!string.IsNullOrEmpty(member.MaskType)) return member.MaskType;
        else return GetInterfaceType(payloadType, member.Length.GetValueOrDefault());
    }

    public static string GetInterfaceType(PayloadType payloadType)
    {
        return payloadType switch
        {
            PayloadType.U8 => "byte",
            PayloadType.S8 => "sbyte",
            PayloadType.U16 => "ushort",
            PayloadType.S16 => "short",
            PayloadType.U32 => "uint",
            PayloadType.S32 => "int",
            PayloadType.U64 => "ulong",
            PayloadType.S64 => "long",
            PayloadType.Float => "float",
            _ => throw new ArgumentOutOfRangeException(nameof(payloadType)),
        };
    }

    public static string GetInterfaceType(PayloadType payloadType, int payloadLength)
    {
        var baseType = GetInterfaceType(payloadType);
        if (payloadLength > 0) return $"{baseType}[]";
        else return baseType;
    }

    public static string GetInterfaceType(Dictionary<string, MaskValue> maskValues)
    {
        var max = maskValues.Values.Max(field => field.Value);
        if (max <= byte.MaxValue) return "byte";
        if (max <= ushort.MaxValue) return "ushort";
        else return "uint";
    }

    public static bool GetInterfaceTypeSize(string interfaceType, out PayloadType payloadType, out int size)
    {
        payloadType = interfaceType switch
        {
            "byte" => PayloadType.U8,
            "sbyte" => PayloadType.S8,
            "ushort" => PayloadType.U16,
            "short" => PayloadType.S16,
            "uint" => PayloadType.U32,
            "int" => PayloadType.S32,
            "ulong" => PayloadType.U64,
            "long" => PayloadType.S64,
            "float" => PayloadType.Float,
            _ => 0
        };

        size = GetPayloadTypeSize(payloadType);
        return payloadType > 0;
    }

    public static string GetPayloadTypeSuffix(PayloadType payloadType, int payloadLength = 0)
    {
        if (payloadLength > 0)
        {
            var baseType = GetInterfaceType(payloadType);
            return $"Array<{baseType}>";
        }

        return payloadType switch
        {
            PayloadType.U8 => "Byte",
            PayloadType.S8 => "SByte",
            PayloadType.U16 => "UInt16",
            PayloadType.S16 => "Int16",
            PayloadType.U32 => "UInt32",
            PayloadType.S32 => "Int32",
            PayloadType.U64 => "UInt64",
            PayloadType.S64 => "Int64",
            PayloadType.Float => "Single",
            _ => throw new ArgumentOutOfRangeException(nameof(payloadType)),
        };
    }

    public static int GetPayloadTypeSize(PayloadType payloadType)
    {
        return (int)payloadType & 0xF;
    }

    public static string GetRangeAttributeDeclaration(float? minValue, float? maxValue)
    {
        var minValueDeclaration = minValue.HasValue ? minValue.Value.ToString() : "long.MinValue";
        var maxValueDeclaration = maxValue.HasValue ? maxValue.Value.ToString() : "long.MaxValue";
        return $"[Range(min: {minValueDeclaration}, max: {maxValueDeclaration})]";
    }

    public static string GetDefaultValueAssignment(float? defaultValue, float? minValue, PayloadType payloadType)
    {
        defaultValue ??= minValue;
        var suffix = payloadType == PayloadType.Float ? "F" : string.Empty;
        return defaultValue.HasValue? $" = {defaultValue}{suffix};" : string.Empty;
    }

    public static string GetParseConversion(RegisterInfo register, string expression)
    {
        if (register.PayloadSpec != null || register.HasConverter)
            return $"ParsePayload({expression})";
        else if (register.InterfaceType == "string")
            return $"PayloadMarshal.ReadUtf8String({expression})";
        else
            return GetConversionToInterfaceType(register.InterfaceType ?? register.MaskType, expression);
    }

    public static string GetFormatConversion(RegisterInfo register, string expression)
    {
        if (register.PayloadSpec != null || register.InterfaceType == "string" || register.HasConverter)
            return $"FormatPayload({expression})";
        else
            return GetConversionFromInterfaceType(register.InterfaceType ?? register.MaskType, register.PayloadInterfaceType, expression);
    }

    public static string GetConversionToInterfaceType(string interfaceType, string expression)
    {
        if (string.IsNullOrEmpty(interfaceType)) return expression;
        return interfaceType switch
        {
            "bool" => $"{expression} != 0",
            _ => $"({interfaceType}){expression}",
        };
    }

    public static string GetConversionFromInterfaceType(
        string interfaceType,
        string payloadInterfaceType,
        string expression)
    {
        if (!string.IsNullOrEmpty(interfaceType))
        {
            if (interfaceType == "bool") expression = $"({expression} ? 1 : 0)";
            expression = $"({payloadInterfaceType}){expression}";
        }
        return expression;
    }

    static int GetMaskShift(int mask)
    {
        var lsb = mask & (~mask + 1);
        return (int)Math.Log(lsb, 2);
    }

    public static int GetMaskSelect(GroupMaskInfo groupMask)
    {
        var maxValue = groupMask.Values.Max(member => member.Value.Value);
        if (maxValue == 0)
            return 1;

        var msb = 1 << (int)Math.Log(maxValue, 2);
        return (msb << 1) - 1;
    }

    static int GetMemberSize(
        PayloadMemberInfo member,
        RegisterInfo register,
        DeviceInfo deviceMetadata,
        out string interfaceType,
        out PayloadType payloadType)
    {
        interfaceType = GetInterfaceType(member, register.Type);
        if (deviceMetadata.GroupMasks.TryGetValue(interfaceType, out GroupMaskInfo groupMask))
            interfaceType = groupMask.InterfaceType;
        else if (deviceMetadata.BitMasks.TryGetValue(interfaceType, out BitMaskInfo bitMask))
            interfaceType = bitMask.InterfaceType;

        if (GetInterfaceTypeSize(interfaceType, out payloadType, out int size))
            return size;
        else
            return GetPayloadTypeSize(register.Type);
    }

    public static string GetPayloadMemberParser(
        string name,
        PayloadMemberInfo member,
        string expression,
        RegisterInfo register,
        DeviceInfo deviceMetadata)
    {
        var payloadType = register.Type;
        var memberLength = member.Length.GetValueOrDefault();
        var memberOffset = member.Offset.GetValueOrDefault();
        var payloadInterfaceType = GetInterfaceType(payloadType);
        if (memberLength > 0 && member.InterfaceType != "bool")
        {
            if (member.Converter == MemberConverter.RawPayload)
                throw new NotSupportedException("Raw payload converters inside payload spec is not currently supported.");

            GetMemberSize(member, register, deviceMetadata, out var memberInterfaceType, out var memberPayloadType);
            if (memberInterfaceType == "string")
                return $"PayloadMarshal.ReadUtf8String(new ArraySegment<{payloadInterfaceType}>({expression}, {memberOffset}, {memberLength}))";
            else if (member.Converter == MemberConverter.Payload || memberInterfaceType != GetInterfaceType(payloadType, register.Length))
            {
                expression = $"new ArraySegment<{payloadInterfaceType}>({expression}, {memberOffset}, {memberLength})";
                if (member.Converter == MemberConverter.Payload)
                    return $"ParsePayload{name}({expression})";
                else if (memberPayloadType > 0)
                {
                    expression = $"PayloadMarshal.Read{GetPayloadTypeSuffix(memberPayloadType)}({expression})";
                    return GetConversionToInterfaceType(member.MaskType, expression);
                }
                else
                {
                    return $"PayloadMarshal.Read{memberInterfaceType}({expression})";
                }
            }
            else
                expression = $"PayloadMarshal.GetSubArray({expression}, {memberOffset}, {memberLength})";
        }
        else if (member.Offset.HasValue)
        {
            expression = $"{expression}[{member.Offset.GetValueOrDefault()}]";
        }
        if (member.Mask.HasValue)
        {
            var mask = member.Mask.Value;
            var shift = GetMaskShift(mask);
            expression = $"({expression} & 0x{mask:X})";
            if (member.InterfaceType != "bool" || member.HasConverter)
            {
                if (shift > 0)
                {
                    expression = $"({expression} >> {shift})";
                }

                expression = $"({payloadInterfaceType}){expression}";
            }
        }

        if (member.HasConverter)
        {
            return $"ParsePayload{name}({expression})";
        }
        return GetConversionToInterfaceType(member.InterfaceType ?? member.MaskType, expression);
    }

    public static string GetPayloadMemberAssignmentFormatter(
        string name,
        PayloadMemberInfo member,
        string expression,
        RegisterInfo register,
        DeviceInfo deviceMetadata,
        bool assigned)
    {
        var payloadType = register.Type;
        var memberLength = member.Length.GetValueOrDefault();
        var memberOffset = member.Offset.GetValueOrDefault();
        var payloadInterfaceType = GetInterfaceType(payloadType);

        var memberConversion = GetPayloadMemberValueFormatter(
            name,
            member,
            $"value.{name}",
            payloadType);
        if (memberLength > 0 && member.InterfaceType != "bool")
        {
            if (member.Converter == MemberConverter.RawPayload)
                throw new NotSupportedException("Raw payload converters inside payload spec is not currently supported.");
            return $"PayloadMarshal.Write(new ArraySegment<{payloadInterfaceType}>({expression}, {memberOffset}, {memberLength}), {memberConversion})";
        }
        else
        {
            if (member.Mask.HasValue && assigned)
            {
                memberConversion = $" |= {memberConversion}";
            }
            else
                memberConversion = $" = {memberConversion}";
            var memberIndexer = member.Offset.HasValue ? $"[{memberOffset}]" : string.Empty;
            return $"{expression}{memberIndexer}{memberConversion}";
        }
    }


    public static string GetPayloadMemberValueFormatter(
        string name,
        PayloadMemberInfo member,
        string expression,
        PayloadType payloadType)
    {
        if (member.HasConverter)
            expression = $"FormatPayload{name}({expression})";
        
        if (member.Length > 0)
            return expression;

        var isBoolean = member.InterfaceType == "bool" && !member.HasConverter;
        var payloadInterfaceType = GetInterfaceType(payloadType);
        if (!string.IsNullOrEmpty(member.InterfaceType ?? member.MaskType))
        {
            if (isBoolean)
            {
                var bitValue = member.Mask.HasValue ? $"0x{member.Mask.Value:X}" : "1";
                expression = $"({expression} ? {bitValue} : 0)";
            }
            expression = $"({payloadInterfaceType}){expression}";
        }
        if (member.Mask.HasValue && !isBoolean)
        {
            var mask = member.Mask.Value;
            var shift = GetMaskShift(mask);
            if (shift > 0)
            {
                expression = $"({expression} << {shift})";
            }

            expression = $"({payloadInterfaceType})({expression} & 0x{mask:X})";
        }

        return expression;
    }

    public static string RemoveSuffix(string value, string suffix)
    {
        return value.EndsWith(suffix)
            ? value.Substring(0, value.Length - suffix.Length)
            : value;
    }
}

class MaskValueTypeConverter : IYamlTypeConverter
{
    public static MaskValueTypeConverter Instance = new MaskValueTypeConverter();
    static readonly IDeserializer ValueDeserializer = new Deserializer();

    public bool Accepts(Type type)
    {
        return type == typeof(MaskValue);
    }

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        if (parser.TryConsume(out MappingStart _))
        {
            var maskValue = new MaskValue();
            while (!parser.TryConsume(out MappingEnd _))
            {
                var key = parser.Consume<Scalar>();
                var value = parser.Consume<Scalar>();
                if (string.IsNullOrEmpty(value.Value))
                {
                    maskValue.Value = ValueDeserializer.Deserialize<int>(key.Value);
                }
                else if (key.Value.Equals(nameof(maskValue.Value), StringComparison.OrdinalIgnoreCase))
                {
                    maskValue.Value = ValueDeserializer.Deserialize<int>(value.Value);
                }
                else if (key.Value.Equals(nameof(maskValue.Description), StringComparison.OrdinalIgnoreCase))
                {
                    maskValue.Description = value.Value;
                }
            }
            return maskValue;
        }
        else
        {
            var scalar = parser.Consume<Scalar>();
            return new MaskValue { Value = ValueDeserializer.Deserialize<int>(scalar.Value) };
        }
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

class RegisterAccessTypeConverter : IYamlTypeConverter
{
    public static RegisterAccessTypeConverter Instance = new RegisterAccessTypeConverter();

    public bool Accepts(Type type)
    {
        return type == typeof(RegisterAccess);
    }

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        if (parser.TryConsume(out SequenceStart _))
        {
            RegisterAccess value = RegisterAccess.Read;
            while (parser.TryConsume(out Scalar scalar))
            {
                value |= (RegisterAccess)Enum.Parse(typeof(RegisterAccess), scalar.Value);
            }
            parser.Consume<SequenceEnd>();
            return value;
        }
        else
        {
            var scalar = parser.Consume<Scalar>();
            return (RegisterAccess)Enum.Parse(typeof(RegisterAccess), scalar.Value);
        }
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
