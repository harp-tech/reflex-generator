using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Bonsai.Harp;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Harp.Generators;

public enum PinDirection
{
    Input,
    Output
}

public enum InputPinMode
{
    PullUp,
    PullDown,
    TriState,
    BusHolder
}

public enum TriggerMode
{
    None,
    Rising,
    Falling,
    Toggle,
    Low
}

public enum InterruptPriority
{
    Off,
    Low,
    Medium,
    High
}

public enum OutputPinMode
{
    Digital,
    WiredOr,
    WiredAnd,
    WiredOrPull,
    WiredAndPull
}

public enum LogicState
{
    Low,
    High
}

public class PortPinInfo
{
    public string Port;
    public int PinNumber;
    public PinDirection Direction;
    public string Description = "";
}

public class InputPinInfo : PortPinInfo
{
    public InputPinMode PinMode;
    public TriggerMode TriggerMode;
    public InterruptPriority InterruptPriority;
    public int InterruptNumber;
}

public class OutputPinInfo : PortPinInfo
{
    public bool AllowRead;
    public OutputPinMode PinMode;
    public LogicState InitialState;
    public bool Invert;
}

public static partial class TemplateHelper
{
    public static Dictionary<string, PortPinInfo> ReadPortPinMetadata(string path)
    {
        using var reader = new StreamReader(path);
        var deserializerBuilder = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance);
        var portPinConverter = new PortPinInfoTypeConverter(deserializerBuilder.Build());
        var deserializer = deserializerBuilder.WithTypeConverter(portPinConverter).Build();
        return deserializer.Deserialize<Dictionary<string, PortPinInfo>>(reader);
    }

    public static IEnumerable<KeyValuePair<string, T>> GetPortPinsOfType<T>(IDictionary<string, PortPinInfo> portPins) where T : PortPinInfo
    {
        return from item in portPins
               where item.Value is T
               select new KeyValuePair<string, T>(item.Key, (T)item.Value);
    }

    public static string GetFirmwareType(PayloadType payloadType)
    {
        return payloadType switch
        {
            PayloadType.U8 => "uint8_t",
            PayloadType.S8 => "int8_t",
            PayloadType.U16 => "uint16_t",
            PayloadType.S16 => "int16_t",
            PayloadType.U32 => "uint32_t",
            PayloadType.S32 => "int32_t",
            PayloadType.U64 => "uint64_t",
            PayloadType.S64 => "int64_t",
            PayloadType.Float => "float_t",
            _ => throw new ArgumentOutOfRangeException(nameof(payloadType)),
        };
    }

    public static string GetFirmwareRegisterType(PayloadType payloadType)
    {
        return payloadType switch
        {
            PayloadType.U8 => "U8",
            PayloadType.S8 => "I8",
            PayloadType.U16 => "U16",
            PayloadType.S16 => "I16",
            PayloadType.U32 => "U32",
            PayloadType.S32 => "I32",
            PayloadType.U64 => "U64",
            PayloadType.S64 => "I64",
            PayloadType.Float => "FLOAT",
            _ => throw new ArgumentOutOfRangeException(nameof(payloadType)),
        };
    }

    public static string GetFirmwareSenseMode(TriggerMode trigger)
    {
        return trigger switch
        {
            TriggerMode.Rising => "SENSE_IO_EDGE_RISING",
            TriggerMode.Falling => "SENSE_IO_EDGE_FALLING",
            TriggerMode.Toggle => "SENSE_IO_EDGES_BOTH",
            TriggerMode.Low => "SENSE_IO_LOW_LEVEL",
            _ => throw new ArgumentOutOfRangeException(nameof(trigger))
        };
    }

    public static string GetFirmwarePullMode(InputPinMode pinMode)
    {
        return pinMode switch
        {
            InputPinMode.PullUp => "PULL_IO_UP",
            InputPinMode.PullDown => "PULL_IO_DOWN",
            InputPinMode.TriState => "PULL_IO_TRISTATE",
            InputPinMode.BusHolder => "PULL_IO_BUSHOLDER",
            _ => throw new ArgumentOutOfRangeException(nameof(pinMode))
        };
    }

    public static string GetFirmwareOutputIO(OutputPinMode pinMode)
    {
        return pinMode switch
        {
            OutputPinMode.Digital => "OUT_IO_DIGITAL",
            OutputPinMode.WiredOr => "OUT_IO_WIREDOR",
            OutputPinMode.WiredAnd => "OUT_IO_WIREDAND",
            OutputPinMode.WiredOrPull => "OUT_IO_WIREDORPULL",
            OutputPinMode.WiredAndPull => "OUT_IO_WIREDANDPULL",
            _ => throw new ArgumentOutOfRangeException(nameof(pinMode))
        };
    }

    public static string GetFirmwareInterruptPriority(InterruptPriority priority)
    {
        return priority switch
        {
            InterruptPriority.Off => "INT_LEVEL_OFF",
            InterruptPriority.Low => "INT_LEVEL_LOW",
            InterruptPriority.Medium => "INT_LEVEL_MED",
            InterruptPriority.High => "INT_LEVEL_HIGH",
            _ => throw new ArgumentOutOfRangeException(nameof(priority))
        };
    }
}

class PortPinInfoTypeConverter(IDeserializer deserializer) : IYamlTypeConverter
{
    static readonly string DirectionProperty = CamelCaseNamingConvention.Instance.Apply(nameof(PortPinInfo.Direction));
    static readonly ISerializer DefaultSerializer = new Serializer();

    public IDeserializer Deserializer { get; } = deserializer ?? throw new ArgumentNullException(nameof(deserializer));

    public bool Accepts(Type type)
    {
        return type == typeof(PortPinInfo);
    }

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var portPin = Deserializer.Deserialize<Dictionary<object, object>>(parser);
        if (portPin.TryGetValue(DirectionProperty, out object value))
        {
            var pinDirection = PascalCaseNamingConvention.Instance.Apply((string)value);
            Type portPinType = pinDirection switch
            {
                nameof(PinDirection.Input) => typeof(InputPinInfo),
                nameof(PinDirection.Output) => typeof(OutputPinInfo),
                _ => throw new YamlException($"Invalid value for '{DirectionProperty}' when deserializing type '{typeof(PortPinInfo)}'"),
            };
            var yaml = DefaultSerializer.Serialize(portPin);
            return Deserializer.Deserialize(yaml, portPinType);
        }

        throw new YamlException($"Required property '{DirectionProperty}' not found when deserializing type '{typeof(PortPinInfo)}'.");
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

public class FirmwareNamingConvention : INamingConvention
{
    public static FirmwareNamingConvention Instance = new FirmwareNamingConvention();

    public string Apply(string value)
    {
        const string Separator = "_";
        var startIndex = 0;
        while (startIndex < value.Length && (char.IsUpper(value[startIndex]) || !char.IsLetter(value[startIndex])))
        {
            startIndex++;
        }

        value = value.Substring(0, startIndex).ToLowerInvariant() + value.Substring(startIndex);
        var previousMatch = 0;
        var previousLength = 0;
        value = Regex.Replace(value, "(?<sep>[_\\-])?(?<char>[A-Z])", match =>
        {
            var length = match.Index - previousMatch;
            previousMatch = match.Index;
            var character = match.Groups["char"].Value.ToLowerInvariant();
            var separate = length != 1 || (match.Index + 1) < value.Length && char.IsLower(value[match.Index + 1]);
            previousLength = length;
            return separate ? Separator + character : character;
        });
        return value.ToUpperInvariant();
    }

    public string Reverse(string value)
    {
        throw new NotImplementedException();
    }
}

public class FirmwareGroupMaskNamingConvention : INamingConvention
{
    public static FirmwareGroupMaskNamingConvention Instance = new FirmwareGroupMaskNamingConvention();
    static readonly string[] TrimSuffixes = new[] { "StateConfiguration", "Configuration" };
    static readonly Dictionary<string, string> TokenSubstitutions = new Dictionary<string, string>
    {
        { "DIGITAL_INPUT", "DI" },
        { "DIGITAL_OUTPUT", "DO" }
    };

    public string Apply(string value)
    {
        for (int i = 0; i < TrimSuffixes.Length; i++)
        {
            var trimIndex = value.LastIndexOf(TrimSuffixes[i]);
            if (trimIndex >= 0)
            {
                value = value.Substring(0, trimIndex);
                break;
            }
        }

        var result = FirmwareNamingConvention.Instance.Apply(value);
        foreach (var substitution in TokenSubstitutions)
        {
            result = result.Replace(substitution.Key, substitution.Value);
        }
        return result;
    }

    public string Reverse(string value)
    {
        throw new NotImplementedException();
    }
}
