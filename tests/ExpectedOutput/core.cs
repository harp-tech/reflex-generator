using Bonsai;
using Bonsai.Harp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Interface.Tests
{
    /// <summary>
    /// Generates events and processes commands for the Tests device connected
    /// at the specified serial port.
    /// </summary>
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Generates events and processes commands for the Tests device.")]
    public partial class Device : Bonsai.Harp.Device, INamedElement
    {
        /// <summary>
        /// Represents the unique identity class of the <see cref="Tests"/> device.
        /// This field is constant.
        /// </summary>
        public const int WhoAmI = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class.
        /// </summary>
        public Device() : base(WhoAmI) { }

        string INamedElement.Name => nameof(Tests);

        /// <summary>
        /// Gets a read-only mapping from address to register type.
        /// </summary>
        public static new IReadOnlyDictionary<int, Type> RegisterMap { get; } = new Dictionary<int, Type>
            (Bonsai.Harp.Device.RegisterMap.ToDictionary(entry => entry.Key, entry => entry.Value))
        {
            { 0, typeof(WhoAmI) },
            { 1, typeof(HardwareVersionHigh) },
            { 2, typeof(HardwareVersionLow) },
            { 3, typeof(AssemblyVersion) },
            { 4, typeof(CoreVersionHigh) },
            { 5, typeof(CoreVersionLow) },
            { 6, typeof(FirmwareVersionHigh) },
            { 7, typeof(FirmwareVersionLow) },
            { 8, typeof(TimestampSeconds) },
            { 9, typeof(TimestampMicroseconds) },
            { 10, typeof(OperationControl) },
            { 11, typeof(ResetDevice) },
            { 12, typeof(DeviceName) },
            { 13, typeof(SerialNumber) },
            { 14, typeof(ClockConfiguration) }
        };

        /// <summary>
        /// Gets the contents of the metadata file describing the <see cref="Tests"/>
        /// device registers.
        /// </summary>
        public static readonly string Metadata = GetDeviceMetadata();

        static string GetDeviceMetadata()
        {
            var deviceType = typeof(Device);
            using var metadataStream = deviceType.Assembly.GetManifestResourceStream($"{deviceType.Namespace}.device.yml");
            using var streamReader = new System.IO.StreamReader(metadataStream);
            return streamReader.ReadToEnd();
        }
    }

    /// <summary>
    /// Represents an operator that returns the contents of the metadata file
    /// describing the <see cref="Tests"/> device registers.
    /// </summary>
    [Description("Returns the contents of the metadata file describing the Tests device registers.")]
    public partial class GetDeviceMetadata : Source<string>
    {
        /// <summary>
        /// Returns an observable sequence with the contents of the metadata file
        /// describing the <see cref="Tests"/> device registers.
        /// </summary>
        /// <returns>
        /// A sequence with a single <see cref="string"/> object representing the
        /// contents of the metadata file.
        /// </returns>
        public override IObservable<string> Generate()
        {
            return Observable.Return(Device.Metadata);
        }
    }

    /// <summary>
    /// Represents an operator that groups the sequence of <see cref="Tests"/>" messages by register type.
    /// </summary>
    [Description("Groups the sequence of Tests messages by register type.")]
    public partial class GroupByRegister : Combinator<HarpMessage, IGroupedObservable<Type, HarpMessage>>
    {
        /// <summary>
        /// Groups an observable sequence of <see cref="Tests"/> messages
        /// by register type.
        /// </summary>
        /// <param name="source">The sequence of Harp device messages.</param>
        /// <returns>
        /// A sequence of observable groups, each of which corresponds to a unique
        /// <see cref="Tests"/> register.
        /// </returns>
        public override IObservable<IGroupedObservable<Type, HarpMessage>> Process(IObservable<HarpMessage> source)
        {
            return source.GroupBy(message => Device.RegisterMap[message.Address]);
        }
    }

    /// <summary>
    /// Represents an operator that writes the sequence of <see cref="Tests"/>" messages
    /// to the standard Harp storage format.
    /// </summary>
    [DefaultProperty(nameof(Path))]
    [Description("Writes the sequence of Tests messages to the standard Harp storage format.")]
    public partial class DeviceDataWriter : Sink<HarpMessage>, INamedElement
    {
        const string BinaryExtension = ".bin";
        const string MetadataFileName = "device.yml";
        readonly Bonsai.Harp.MessageWriter writer = new();

        string INamedElement.Name => nameof(Tests) + "DataWriter";

        /// <summary>
        /// Gets or sets the relative or absolute path on which to save the message data.
        /// </summary>
        [Description("The relative or absolute path of the directory on which to save the message data.")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string Path
        {
            get => System.IO.Path.GetDirectoryName(writer.FileName);
            set => writer.FileName = System.IO.Path.Combine(value, nameof(Tests) + BinaryExtension);
        }

        /// <summary>
        /// Gets or sets a value indicating whether element writing should be buffered. If <see langword="true"/>,
        /// the write commands will be queued in memory as fast as possible and will be processed
        /// by the writer in a different thread. Otherwise, writing will be done in the same
        /// thread in which notifications arrive.
        /// </summary>
        [Description("Indicates whether writing should be buffered.")]
        public bool Buffered
        {
            get => writer.Buffered;
            set => writer.Buffered = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite the output file if it already exists.
        /// </summary>
        [Description("Indicates whether to overwrite the output file if it already exists.")]
        public bool Overwrite
        {
            get => writer.Overwrite;
            set => writer.Overwrite = value;
        }

        /// <summary>
        /// Gets or sets a value specifying how the message filter will use the matching criteria.
        /// </summary>
        [Description("Specifies how the message filter will use the matching criteria.")]
        public FilterType FilterType
        {
            get => writer.FilterType;
            set => writer.FilterType = value;
        }

        /// <summary>
        /// Gets or sets a value specifying the expected message type. If no value is
        /// specified, all messages will be accepted.
        /// </summary>
        [Description("Specifies the expected message type. If no value is specified, all messages will be accepted.")]
        public MessageType? MessageType
        {
            get => writer.MessageType;
            set => writer.MessageType = value;
        }

        private IObservable<TSource> WriteDeviceMetadata<TSource>(IObservable<TSource> source)
        {
            var basePath = Path;
            if (string.IsNullOrEmpty(basePath))
                return source;

            var metadataPath = System.IO.Path.Combine(basePath, MetadataFileName);
            return Observable.Create<TSource>(observer =>
            {
                Bonsai.IO.PathHelper.EnsureDirectory(metadataPath);
                if (System.IO.File.Exists(metadataPath) && !Overwrite)
                {
                    throw new System.IO.IOException(string.Format("The file '{0}' already exists.", metadataPath));
                }

                System.IO.File.WriteAllText(metadataPath, Device.Metadata);
                return source.SubscribeSafe(observer);
            });
        }

        /// <summary>
        /// Writes each Harp message in the sequence to the specified binary file, and the
        /// contents of the device metadata file to a separate text file.
        /// </summary>
        /// <param name="source">The sequence of messages to write to the file.</param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of writing the
        /// messages to a raw binary file, and the contents of the device metadata file
        /// to a separate text file.
        /// </returns>
        public override IObservable<HarpMessage> Process(IObservable<HarpMessage> source)
        {
            return source.Publish(ps => ps.Merge(
                WriteDeviceMetadata(writer.Process(ps.GroupBy(message => message.Address)))
                .IgnoreElements()
                .Cast<HarpMessage>()));
        }

        /// <summary>
        /// Writes each Harp message in the sequence of observable groups to the
        /// corresponding binary file, where the name of each file is generated from
        /// the common group register address. The contents of the device metadata file are
        /// written to a separate text file.
        /// </summary>
        /// <param name="source">
        /// A sequence of observable groups, each of which corresponds to a unique register
        /// address.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of writing the Harp
        /// messages in each group to the corresponding file, and the contents of the device
        /// metadata file to a separate text file.
        /// </returns>
        public IObservable<IGroupedObservable<int, HarpMessage>> Process(IObservable<IGroupedObservable<int, HarpMessage>> source)
        {
            return WriteDeviceMetadata(writer.Process(source));
        }

        /// <summary>
        /// Writes each Harp message in the sequence of observable groups to the
        /// corresponding binary file, where the name of each file is generated from
        /// the common group register name. The contents of the device metadata file are
        /// written to a separate text file.
        /// </summary>
        /// <param name="source">
        /// A sequence of observable groups, each of which corresponds to a unique register
        /// type.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of writing the Harp
        /// messages in each group to the corresponding file, and the contents of the device
        /// metadata file to a separate text file.
        /// </returns>
        public IObservable<IGroupedObservable<Type, HarpMessage>> Process(IObservable<IGroupedObservable<Type, HarpMessage>> source)
        {
            return WriteDeviceMetadata(writer.Process(source));
        }
    }

    /// <summary>
    /// Represents an operator that filters register-specific messages
    /// reported by the <see cref="Tests"/> device.
    /// </summary>
    /// <seealso cref="WhoAmI"/>
    /// <seealso cref="HardwareVersionHigh"/>
    /// <seealso cref="HardwareVersionLow"/>
    /// <seealso cref="AssemblyVersion"/>
    /// <seealso cref="CoreVersionHigh"/>
    /// <seealso cref="CoreVersionLow"/>
    /// <seealso cref="FirmwareVersionHigh"/>
    /// <seealso cref="FirmwareVersionLow"/>
    /// <seealso cref="TimestampSeconds"/>
    /// <seealso cref="TimestampMicroseconds"/>
    /// <seealso cref="OperationControl"/>
    /// <seealso cref="ResetDevice"/>
    /// <seealso cref="DeviceName"/>
    /// <seealso cref="SerialNumber"/>
    /// <seealso cref="ClockConfiguration"/>
    [XmlInclude(typeof(WhoAmI))]
    [XmlInclude(typeof(HardwareVersionHigh))]
    [XmlInclude(typeof(HardwareVersionLow))]
    [XmlInclude(typeof(AssemblyVersion))]
    [XmlInclude(typeof(CoreVersionHigh))]
    [XmlInclude(typeof(CoreVersionLow))]
    [XmlInclude(typeof(FirmwareVersionHigh))]
    [XmlInclude(typeof(FirmwareVersionLow))]
    [XmlInclude(typeof(TimestampSeconds))]
    [XmlInclude(typeof(TimestampMicroseconds))]
    [XmlInclude(typeof(OperationControl))]
    [XmlInclude(typeof(ResetDevice))]
    [XmlInclude(typeof(DeviceName))]
    [XmlInclude(typeof(SerialNumber))]
    [XmlInclude(typeof(ClockConfiguration))]
    [Description("Filters register-specific messages reported by the Tests device.")]
    public class FilterRegister : FilterRegisterBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterRegister"/> class.
        /// </summary>
        public FilterRegister()
        {
            Register = new WhoAmI();
        }

        string INamedElement.Name
        {
            get => $"{nameof(Tests)}.{GetElementDisplayName(Register)}";
        }
    }

    /// <summary>
    /// Represents an operator which filters and selects specific messages
    /// reported by the Tests device.
    /// </summary>
    /// <seealso cref="WhoAmI"/>
    /// <seealso cref="HardwareVersionHigh"/>
    /// <seealso cref="HardwareVersionLow"/>
    /// <seealso cref="AssemblyVersion"/>
    /// <seealso cref="CoreVersionHigh"/>
    /// <seealso cref="CoreVersionLow"/>
    /// <seealso cref="FirmwareVersionHigh"/>
    /// <seealso cref="FirmwareVersionLow"/>
    /// <seealso cref="TimestampSeconds"/>
    /// <seealso cref="TimestampMicroseconds"/>
    /// <seealso cref="OperationControl"/>
    /// <seealso cref="ResetDevice"/>
    /// <seealso cref="DeviceName"/>
    /// <seealso cref="SerialNumber"/>
    /// <seealso cref="ClockConfiguration"/>
    [XmlInclude(typeof(WhoAmI))]
    [XmlInclude(typeof(HardwareVersionHigh))]
    [XmlInclude(typeof(HardwareVersionLow))]
    [XmlInclude(typeof(AssemblyVersion))]
    [XmlInclude(typeof(CoreVersionHigh))]
    [XmlInclude(typeof(CoreVersionLow))]
    [XmlInclude(typeof(FirmwareVersionHigh))]
    [XmlInclude(typeof(FirmwareVersionLow))]
    [XmlInclude(typeof(TimestampSeconds))]
    [XmlInclude(typeof(TimestampMicroseconds))]
    [XmlInclude(typeof(OperationControl))]
    [XmlInclude(typeof(ResetDevice))]
    [XmlInclude(typeof(DeviceName))]
    [XmlInclude(typeof(SerialNumber))]
    [XmlInclude(typeof(ClockConfiguration))]
    [XmlInclude(typeof(TimestampedWhoAmI))]
    [XmlInclude(typeof(TimestampedHardwareVersionHigh))]
    [XmlInclude(typeof(TimestampedHardwareVersionLow))]
    [XmlInclude(typeof(TimestampedAssemblyVersion))]
    [XmlInclude(typeof(TimestampedCoreVersionHigh))]
    [XmlInclude(typeof(TimestampedCoreVersionLow))]
    [XmlInclude(typeof(TimestampedFirmwareVersionHigh))]
    [XmlInclude(typeof(TimestampedFirmwareVersionLow))]
    [XmlInclude(typeof(TimestampedTimestampSeconds))]
    [XmlInclude(typeof(TimestampedTimestampMicroseconds))]
    [XmlInclude(typeof(TimestampedOperationControl))]
    [XmlInclude(typeof(TimestampedResetDevice))]
    [XmlInclude(typeof(TimestampedDeviceName))]
    [XmlInclude(typeof(TimestampedSerialNumber))]
    [XmlInclude(typeof(TimestampedClockConfiguration))]
    [Description("Filters and selects specific messages reported by the Tests device.")]
    public partial class Parse : ParseBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Parse"/> class.
        /// </summary>
        public Parse()
        {
            Register = new WhoAmI();
        }

        string INamedElement.Name => $"{nameof(Tests)}.{GetElementDisplayName(Register)}";
    }

    /// <summary>
    /// Represents an operator which formats a sequence of values as specific
    /// Tests register messages.
    /// </summary>
    /// <seealso cref="WhoAmI"/>
    /// <seealso cref="HardwareVersionHigh"/>
    /// <seealso cref="HardwareVersionLow"/>
    /// <seealso cref="AssemblyVersion"/>
    /// <seealso cref="CoreVersionHigh"/>
    /// <seealso cref="CoreVersionLow"/>
    /// <seealso cref="FirmwareVersionHigh"/>
    /// <seealso cref="FirmwareVersionLow"/>
    /// <seealso cref="TimestampSeconds"/>
    /// <seealso cref="TimestampMicroseconds"/>
    /// <seealso cref="OperationControl"/>
    /// <seealso cref="ResetDevice"/>
    /// <seealso cref="DeviceName"/>
    /// <seealso cref="SerialNumber"/>
    /// <seealso cref="ClockConfiguration"/>
    [XmlInclude(typeof(WhoAmI))]
    [XmlInclude(typeof(HardwareVersionHigh))]
    [XmlInclude(typeof(HardwareVersionLow))]
    [XmlInclude(typeof(AssemblyVersion))]
    [XmlInclude(typeof(CoreVersionHigh))]
    [XmlInclude(typeof(CoreVersionLow))]
    [XmlInclude(typeof(FirmwareVersionHigh))]
    [XmlInclude(typeof(FirmwareVersionLow))]
    [XmlInclude(typeof(TimestampSeconds))]
    [XmlInclude(typeof(TimestampMicroseconds))]
    [XmlInclude(typeof(OperationControl))]
    [XmlInclude(typeof(ResetDevice))]
    [XmlInclude(typeof(DeviceName))]
    [XmlInclude(typeof(SerialNumber))]
    [XmlInclude(typeof(ClockConfiguration))]
    [Description("Formats a sequence of values as specific Tests register messages.")]
    public partial class Format : FormatBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Format"/> class.
        /// </summary>
        public Format()
        {
            Register = new WhoAmI();
        }

        string INamedElement.Name => $"{nameof(Tests)}.{GetElementDisplayName(Register)}";
    }

    /// <summary>
    /// Represents a register that specifies the identity class of the device.
    /// </summary>
    [Description("Specifies the identity class of the device.")]
    public partial class WhoAmI
    {
        /// <summary>
        /// Represents the address of the <see cref="WhoAmI"/> register. This field is constant.
        /// </summary>
        public const int Address = 0;

        /// <summary>
        /// Represents the payload type of the <see cref="WhoAmI"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U16;

        /// <summary>
        /// Represents the length of the <see cref="WhoAmI"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="WhoAmI"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static ushort GetPayload(HarpMessage message)
        {
            return message.GetPayloadUInt16();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="WhoAmI"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadUInt16();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="WhoAmI"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="WhoAmI"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="WhoAmI"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="WhoAmI"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// WhoAmI register.
    /// </summary>
    /// <seealso cref="WhoAmI"/>
    [Description("Filters and selects timestamped messages from the WhoAmI register.")]
    public partial class TimestampedWhoAmI
    {
        /// <summary>
        /// Represents the address of the <see cref="WhoAmI"/> register. This field is constant.
        /// </summary>
        public const int Address = WhoAmI.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="WhoAmI"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetPayload(HarpMessage message)
        {
            return WhoAmI.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that specifies the major hardware version of the device.
    /// </summary>
    [Description("Specifies the major hardware version of the device.")]
    public partial class HardwareVersionHigh
    {
        /// <summary>
        /// Represents the address of the <see cref="HardwareVersionHigh"/> register. This field is constant.
        /// </summary>
        public const int Address = 1;

        /// <summary>
        /// Represents the payload type of the <see cref="HardwareVersionHigh"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="HardwareVersionHigh"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="HardwareVersionHigh"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte GetPayload(HarpMessage message)
        {
            return message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="HardwareVersionHigh"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadByte();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="HardwareVersionHigh"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="HardwareVersionHigh"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="HardwareVersionHigh"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="HardwareVersionHigh"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// HardwareVersionHigh register.
    /// </summary>
    /// <seealso cref="HardwareVersionHigh"/>
    [Description("Filters and selects timestamped messages from the HardwareVersionHigh register.")]
    public partial class TimestampedHardwareVersionHigh
    {
        /// <summary>
        /// Represents the address of the <see cref="HardwareVersionHigh"/> register. This field is constant.
        /// </summary>
        public const int Address = HardwareVersionHigh.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="HardwareVersionHigh"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetPayload(HarpMessage message)
        {
            return HardwareVersionHigh.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that specifies the minor hardware version of the device.
    /// </summary>
    [Description("Specifies the minor hardware version of the device.")]
    public partial class HardwareVersionLow
    {
        /// <summary>
        /// Represents the address of the <see cref="HardwareVersionLow"/> register. This field is constant.
        /// </summary>
        public const int Address = 2;

        /// <summary>
        /// Represents the payload type of the <see cref="HardwareVersionLow"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="HardwareVersionLow"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="HardwareVersionLow"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte GetPayload(HarpMessage message)
        {
            return message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="HardwareVersionLow"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadByte();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="HardwareVersionLow"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="HardwareVersionLow"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="HardwareVersionLow"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="HardwareVersionLow"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// HardwareVersionLow register.
    /// </summary>
    /// <seealso cref="HardwareVersionLow"/>
    [Description("Filters and selects timestamped messages from the HardwareVersionLow register.")]
    public partial class TimestampedHardwareVersionLow
    {
        /// <summary>
        /// Represents the address of the <see cref="HardwareVersionLow"/> register. This field is constant.
        /// </summary>
        public const int Address = HardwareVersionLow.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="HardwareVersionLow"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetPayload(HarpMessage message)
        {
            return HardwareVersionLow.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that specifies the version of the assembled components in the device.
    /// </summary>
    [Description("Specifies the version of the assembled components in the device.")]
    public partial class AssemblyVersion
    {
        /// <summary>
        /// Represents the address of the <see cref="AssemblyVersion"/> register. This field is constant.
        /// </summary>
        public const int Address = 3;

        /// <summary>
        /// Represents the payload type of the <see cref="AssemblyVersion"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="AssemblyVersion"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="AssemblyVersion"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte GetPayload(HarpMessage message)
        {
            return message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="AssemblyVersion"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadByte();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="AssemblyVersion"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="AssemblyVersion"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="AssemblyVersion"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="AssemblyVersion"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// AssemblyVersion register.
    /// </summary>
    /// <seealso cref="AssemblyVersion"/>
    [Description("Filters and selects timestamped messages from the AssemblyVersion register.")]
    public partial class TimestampedAssemblyVersion
    {
        /// <summary>
        /// Represents the address of the <see cref="AssemblyVersion"/> register. This field is constant.
        /// </summary>
        public const int Address = AssemblyVersion.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="AssemblyVersion"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetPayload(HarpMessage message)
        {
            return AssemblyVersion.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that specifies the major version of the Harp core implemented by the device.
    /// </summary>
    [Description("Specifies the major version of the Harp core implemented by the device.")]
    public partial class CoreVersionHigh
    {
        /// <summary>
        /// Represents the address of the <see cref="CoreVersionHigh"/> register. This field is constant.
        /// </summary>
        public const int Address = 4;

        /// <summary>
        /// Represents the payload type of the <see cref="CoreVersionHigh"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="CoreVersionHigh"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="CoreVersionHigh"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte GetPayload(HarpMessage message)
        {
            return message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="CoreVersionHigh"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadByte();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="CoreVersionHigh"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="CoreVersionHigh"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="CoreVersionHigh"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="CoreVersionHigh"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// CoreVersionHigh register.
    /// </summary>
    /// <seealso cref="CoreVersionHigh"/>
    [Description("Filters and selects timestamped messages from the CoreVersionHigh register.")]
    public partial class TimestampedCoreVersionHigh
    {
        /// <summary>
        /// Represents the address of the <see cref="CoreVersionHigh"/> register. This field is constant.
        /// </summary>
        public const int Address = CoreVersionHigh.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="CoreVersionHigh"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetPayload(HarpMessage message)
        {
            return CoreVersionHigh.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that specifies the minor version of the Harp core implemented by the device.
    /// </summary>
    [Description("Specifies the minor version of the Harp core implemented by the device.")]
    public partial class CoreVersionLow
    {
        /// <summary>
        /// Represents the address of the <see cref="CoreVersionLow"/> register. This field is constant.
        /// </summary>
        public const int Address = 5;

        /// <summary>
        /// Represents the payload type of the <see cref="CoreVersionLow"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="CoreVersionLow"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="CoreVersionLow"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte GetPayload(HarpMessage message)
        {
            return message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="CoreVersionLow"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadByte();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="CoreVersionLow"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="CoreVersionLow"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="CoreVersionLow"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="CoreVersionLow"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// CoreVersionLow register.
    /// </summary>
    /// <seealso cref="CoreVersionLow"/>
    [Description("Filters and selects timestamped messages from the CoreVersionLow register.")]
    public partial class TimestampedCoreVersionLow
    {
        /// <summary>
        /// Represents the address of the <see cref="CoreVersionLow"/> register. This field is constant.
        /// </summary>
        public const int Address = CoreVersionLow.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="CoreVersionLow"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetPayload(HarpMessage message)
        {
            return CoreVersionLow.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that specifies the major version of the Harp core implemented by the device.
    /// </summary>
    [Description("Specifies the major version of the Harp core implemented by the device.")]
    public partial class FirmwareVersionHigh
    {
        /// <summary>
        /// Represents the address of the <see cref="FirmwareVersionHigh"/> register. This field is constant.
        /// </summary>
        public const int Address = 6;

        /// <summary>
        /// Represents the payload type of the <see cref="FirmwareVersionHigh"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="FirmwareVersionHigh"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="FirmwareVersionHigh"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte GetPayload(HarpMessage message)
        {
            return message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="FirmwareVersionHigh"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadByte();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="FirmwareVersionHigh"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="FirmwareVersionHigh"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="FirmwareVersionHigh"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="FirmwareVersionHigh"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// FirmwareVersionHigh register.
    /// </summary>
    /// <seealso cref="FirmwareVersionHigh"/>
    [Description("Filters and selects timestamped messages from the FirmwareVersionHigh register.")]
    public partial class TimestampedFirmwareVersionHigh
    {
        /// <summary>
        /// Represents the address of the <see cref="FirmwareVersionHigh"/> register. This field is constant.
        /// </summary>
        public const int Address = FirmwareVersionHigh.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="FirmwareVersionHigh"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetPayload(HarpMessage message)
        {
            return FirmwareVersionHigh.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that specifies the minor version of the Harp core implemented by the device.
    /// </summary>
    [Description("Specifies the minor version of the Harp core implemented by the device.")]
    public partial class FirmwareVersionLow
    {
        /// <summary>
        /// Represents the address of the <see cref="FirmwareVersionLow"/> register. This field is constant.
        /// </summary>
        public const int Address = 7;

        /// <summary>
        /// Represents the payload type of the <see cref="FirmwareVersionLow"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="FirmwareVersionLow"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="FirmwareVersionLow"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte GetPayload(HarpMessage message)
        {
            return message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="FirmwareVersionLow"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadByte();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="FirmwareVersionLow"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="FirmwareVersionLow"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="FirmwareVersionLow"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="FirmwareVersionLow"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// FirmwareVersionLow register.
    /// </summary>
    /// <seealso cref="FirmwareVersionLow"/>
    [Description("Filters and selects timestamped messages from the FirmwareVersionLow register.")]
    public partial class TimestampedFirmwareVersionLow
    {
        /// <summary>
        /// Represents the address of the <see cref="FirmwareVersionLow"/> register. This field is constant.
        /// </summary>
        public const int Address = FirmwareVersionLow.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="FirmwareVersionLow"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetPayload(HarpMessage message)
        {
            return FirmwareVersionLow.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that stores the integral part of the system timestamp, in seconds.
    /// </summary>
    [Description("Stores the integral part of the system timestamp, in seconds.")]
    public partial class TimestampSeconds
    {
        /// <summary>
        /// Represents the address of the <see cref="TimestampSeconds"/> register. This field is constant.
        /// </summary>
        public const int Address = 8;

        /// <summary>
        /// Represents the payload type of the <see cref="TimestampSeconds"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U32;

        /// <summary>
        /// Represents the length of the <see cref="TimestampSeconds"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="TimestampSeconds"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static uint GetPayload(HarpMessage message)
        {
            return message.GetPayloadUInt32();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="TimestampSeconds"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<uint> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadUInt32();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="TimestampSeconds"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="TimestampSeconds"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, uint value)
        {
            return HarpMessage.FromUInt32(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="TimestampSeconds"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="TimestampSeconds"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, uint value)
        {
            return HarpMessage.FromUInt32(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// TimestampSeconds register.
    /// </summary>
    /// <seealso cref="TimestampSeconds"/>
    [Description("Filters and selects timestamped messages from the TimestampSeconds register.")]
    public partial class TimestampedTimestampSeconds
    {
        /// <summary>
        /// Represents the address of the <see cref="TimestampSeconds"/> register. This field is constant.
        /// </summary>
        public const int Address = TimestampSeconds.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="TimestampSeconds"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<uint> GetPayload(HarpMessage message)
        {
            return TimestampSeconds.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that stores the fractional part of the system timestamp, in microseconds.
    /// </summary>
    [Description("Stores the fractional part of the system timestamp, in microseconds.")]
    public partial class TimestampMicroseconds
    {
        /// <summary>
        /// Represents the address of the <see cref="TimestampMicroseconds"/> register. This field is constant.
        /// </summary>
        public const int Address = 9;

        /// <summary>
        /// Represents the payload type of the <see cref="TimestampMicroseconds"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U16;

        /// <summary>
        /// Represents the length of the <see cref="TimestampMicroseconds"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="TimestampMicroseconds"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static ushort GetPayload(HarpMessage message)
        {
            return message.GetPayloadUInt16();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="TimestampMicroseconds"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadUInt16();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="TimestampMicroseconds"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="TimestampMicroseconds"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="TimestampMicroseconds"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="TimestampMicroseconds"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// TimestampMicroseconds register.
    /// </summary>
    /// <seealso cref="TimestampMicroseconds"/>
    [Description("Filters and selects timestamped messages from the TimestampMicroseconds register.")]
    public partial class TimestampedTimestampMicroseconds
    {
        /// <summary>
        /// Represents the address of the <see cref="TimestampMicroseconds"/> register. This field is constant.
        /// </summary>
        public const int Address = TimestampMicroseconds.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="TimestampMicroseconds"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetPayload(HarpMessage message)
        {
            return TimestampMicroseconds.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that stores the configuration mode of the device.
    /// </summary>
    [Description("Stores the configuration mode of the device.")]
    public partial class OperationControl
    {
        /// <summary>
        /// Represents the address of the <see cref="OperationControl"/> register. This field is constant.
        /// </summary>
        public const int Address = 10;

        /// <summary>
        /// Represents the payload type of the <see cref="OperationControl"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="OperationControl"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        static OperationControlPayload ParsePayload(byte payload)
        {
            OperationControlPayload result;
            result.OperationMode = (OperationMode)(byte)(payload & 0x3);
            result.DumpRegisters = (payload & 0x8) != 0;
            result.MuteReplies = (payload & 0x10) != 0;
            result.VisualIndicators = (EnableFlag)(byte)((payload & 0x20) >> 5);
            result.OperationLed = (EnableFlag)(byte)((payload & 0x40) >> 6);
            result.Heartbeat = (EnableFlag)(byte)((payload & 0x80) >> 7);
            return result;
        }

        static byte FormatPayload(OperationControlPayload value)
        {
            byte result;
            result = (byte)((byte)value.OperationMode & 0x3);
            result |= (byte)(value.DumpRegisters ? 0x8 : 0);
            result |= (byte)(value.MuteReplies ? 0x10 : 0);
            result |= (byte)(((byte)value.VisualIndicators << 5) & 0x20);
            result |= (byte)(((byte)value.OperationLed << 6) & 0x40);
            result |= (byte)(((byte)value.Heartbeat << 7) & 0x80);
            return result;
        }

        /// <summary>
        /// Returns the payload data for <see cref="OperationControl"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static OperationControlPayload GetPayload(HarpMessage message)
        {
            return ParsePayload(message.GetPayloadByte());
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="OperationControl"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<OperationControlPayload> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create(ParsePayload(payload.Value), payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="OperationControl"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="OperationControl"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, OperationControlPayload value)
        {
            return HarpMessage.FromByte(Address, messageType, FormatPayload(value));
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="OperationControl"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="OperationControl"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, OperationControlPayload value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, FormatPayload(value));
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// OperationControl register.
    /// </summary>
    /// <seealso cref="OperationControl"/>
    [Description("Filters and selects timestamped messages from the OperationControl register.")]
    public partial class TimestampedOperationControl
    {
        /// <summary>
        /// Represents the address of the <see cref="OperationControl"/> register. This field is constant.
        /// </summary>
        public const int Address = OperationControl.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="OperationControl"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<OperationControlPayload> GetPayload(HarpMessage message)
        {
            return OperationControl.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that resets the device and saves non-volatile registers.
    /// </summary>
    [Description("Resets the device and saves non-volatile registers.")]
    public partial class ResetDevice
    {
        /// <summary>
        /// Represents the address of the <see cref="ResetDevice"/> register. This field is constant.
        /// </summary>
        public const int Address = 11;

        /// <summary>
        /// Represents the payload type of the <see cref="ResetDevice"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="ResetDevice"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="ResetDevice"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static ResetFlags GetPayload(HarpMessage message)
        {
            return (ResetFlags)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="ResetDevice"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ResetFlags> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((ResetFlags)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="ResetDevice"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="ResetDevice"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, ResetFlags value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="ResetDevice"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="ResetDevice"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, ResetFlags value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// ResetDevice register.
    /// </summary>
    /// <seealso cref="ResetDevice"/>
    [Description("Filters and selects timestamped messages from the ResetDevice register.")]
    public partial class TimestampedResetDevice
    {
        /// <summary>
        /// Represents the address of the <see cref="ResetDevice"/> register. This field is constant.
        /// </summary>
        public const int Address = ResetDevice.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="ResetDevice"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ResetFlags> GetPayload(HarpMessage message)
        {
            return ResetDevice.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that stores the user-specified device name.
    /// </summary>
    [Description("Stores the user-specified device name.")]
    public partial class DeviceName
    {
        /// <summary>
        /// Represents the address of the <see cref="DeviceName"/> register. This field is constant.
        /// </summary>
        public const int Address = 12;

        /// <summary>
        /// Represents the payload type of the <see cref="DeviceName"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="DeviceName"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 25;

        static byte[] FormatPayload(string value)
        {
            var result = new byte[RegisterLength];
            PayloadMarshal.Write(new ArraySegment<byte>(result), value);
            return result;
        }

        /// <summary>
        /// Returns the payload data for <see cref="DeviceName"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static string GetPayload(HarpMessage message)
        {
            return PayloadMarshal.ReadUtf8String(message.GetPayload());
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="DeviceName"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<string> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayload();
            return Timestamped.Create(PayloadMarshal.ReadUtf8String(payload.Value), payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="DeviceName"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="DeviceName"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, string value)
        {
            return HarpMessage.FromPayload(Address, messageType, PayloadType.U8, FormatPayload(value));
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="DeviceName"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="DeviceName"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, string value)
        {
            return HarpMessage.FromPayload(Address, timestamp, messageType, PayloadType.U8, FormatPayload(value));
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// DeviceName register.
    /// </summary>
    /// <seealso cref="DeviceName"/>
    [Description("Filters and selects timestamped messages from the DeviceName register.")]
    public partial class TimestampedDeviceName
    {
        /// <summary>
        /// Represents the address of the <see cref="DeviceName"/> register. This field is constant.
        /// </summary>
        public const int Address = DeviceName.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="DeviceName"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<string> GetPayload(HarpMessage message)
        {
            return DeviceName.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that specifies the unique serial number of the device.
    /// </summary>
    [Description("Specifies the unique serial number of the device.")]
    public partial class SerialNumber
    {
        /// <summary>
        /// Represents the address of the <see cref="SerialNumber"/> register. This field is constant.
        /// </summary>
        public const int Address = 13;

        /// <summary>
        /// Represents the payload type of the <see cref="SerialNumber"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U16;

        /// <summary>
        /// Represents the length of the <see cref="SerialNumber"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="SerialNumber"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static ushort GetPayload(HarpMessage message)
        {
            return message.GetPayloadUInt16();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="SerialNumber"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadUInt16();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="SerialNumber"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="SerialNumber"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="SerialNumber"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="SerialNumber"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// SerialNumber register.
    /// </summary>
    /// <seealso cref="SerialNumber"/>
    [Description("Filters and selects timestamped messages from the SerialNumber register.")]
    public partial class TimestampedSerialNumber
    {
        /// <summary>
        /// Represents the address of the <see cref="SerialNumber"/> register. This field is constant.
        /// </summary>
        public const int Address = SerialNumber.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="SerialNumber"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetPayload(HarpMessage message)
        {
            return SerialNumber.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that specifies the configuration for the device synchronization clock.
    /// </summary>
    [Description("Specifies the configuration for the device synchronization clock.")]
    public partial class ClockConfiguration
    {
        /// <summary>
        /// Represents the address of the <see cref="ClockConfiguration"/> register. This field is constant.
        /// </summary>
        public const int Address = 14;

        /// <summary>
        /// Represents the payload type of the <see cref="ClockConfiguration"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="ClockConfiguration"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="ClockConfiguration"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static ClockConfigurationFlags GetPayload(HarpMessage message)
        {
            return (ClockConfigurationFlags)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="ClockConfiguration"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ClockConfigurationFlags> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((ClockConfigurationFlags)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="ClockConfiguration"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="ClockConfiguration"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, ClockConfigurationFlags value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="ClockConfiguration"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="ClockConfiguration"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, ClockConfigurationFlags value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// ClockConfiguration register.
    /// </summary>
    /// <seealso cref="ClockConfiguration"/>
    [Description("Filters and selects timestamped messages from the ClockConfiguration register.")]
    public partial class TimestampedClockConfiguration
    {
        /// <summary>
        /// Represents the address of the <see cref="ClockConfiguration"/> register. This field is constant.
        /// </summary>
        public const int Address = ClockConfiguration.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="ClockConfiguration"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ClockConfigurationFlags> GetPayload(HarpMessage message)
        {
            return ClockConfiguration.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents an operator which creates standard message payloads for the
    /// Tests device.
    /// </summary>
    /// <seealso cref="CreateWhoAmIPayload"/>
    /// <seealso cref="CreateHardwareVersionHighPayload"/>
    /// <seealso cref="CreateHardwareVersionLowPayload"/>
    /// <seealso cref="CreateAssemblyVersionPayload"/>
    /// <seealso cref="CreateCoreVersionHighPayload"/>
    /// <seealso cref="CreateCoreVersionLowPayload"/>
    /// <seealso cref="CreateFirmwareVersionHighPayload"/>
    /// <seealso cref="CreateFirmwareVersionLowPayload"/>
    /// <seealso cref="CreateTimestampSecondsPayload"/>
    /// <seealso cref="CreateTimestampMicrosecondsPayload"/>
    /// <seealso cref="CreateOperationControlPayload"/>
    /// <seealso cref="CreateResetDevicePayload"/>
    /// <seealso cref="CreateDeviceNamePayload"/>
    /// <seealso cref="CreateSerialNumberPayload"/>
    /// <seealso cref="CreateClockConfigurationPayload"/>
    [XmlInclude(typeof(CreateWhoAmIPayload))]
    [XmlInclude(typeof(CreateHardwareVersionHighPayload))]
    [XmlInclude(typeof(CreateHardwareVersionLowPayload))]
    [XmlInclude(typeof(CreateAssemblyVersionPayload))]
    [XmlInclude(typeof(CreateCoreVersionHighPayload))]
    [XmlInclude(typeof(CreateCoreVersionLowPayload))]
    [XmlInclude(typeof(CreateFirmwareVersionHighPayload))]
    [XmlInclude(typeof(CreateFirmwareVersionLowPayload))]
    [XmlInclude(typeof(CreateTimestampSecondsPayload))]
    [XmlInclude(typeof(CreateTimestampMicrosecondsPayload))]
    [XmlInclude(typeof(CreateOperationControlPayload))]
    [XmlInclude(typeof(CreateResetDevicePayload))]
    [XmlInclude(typeof(CreateDeviceNamePayload))]
    [XmlInclude(typeof(CreateSerialNumberPayload))]
    [XmlInclude(typeof(CreateClockConfigurationPayload))]
    [XmlInclude(typeof(CreateTimestampedWhoAmIPayload))]
    [XmlInclude(typeof(CreateTimestampedHardwareVersionHighPayload))]
    [XmlInclude(typeof(CreateTimestampedHardwareVersionLowPayload))]
    [XmlInclude(typeof(CreateTimestampedAssemblyVersionPayload))]
    [XmlInclude(typeof(CreateTimestampedCoreVersionHighPayload))]
    [XmlInclude(typeof(CreateTimestampedCoreVersionLowPayload))]
    [XmlInclude(typeof(CreateTimestampedFirmwareVersionHighPayload))]
    [XmlInclude(typeof(CreateTimestampedFirmwareVersionLowPayload))]
    [XmlInclude(typeof(CreateTimestampedTimestampSecondsPayload))]
    [XmlInclude(typeof(CreateTimestampedTimestampMicrosecondsPayload))]
    [XmlInclude(typeof(CreateTimestampedOperationControlPayload))]
    [XmlInclude(typeof(CreateTimestampedResetDevicePayload))]
    [XmlInclude(typeof(CreateTimestampedDeviceNamePayload))]
    [XmlInclude(typeof(CreateTimestampedSerialNumberPayload))]
    [XmlInclude(typeof(CreateTimestampedClockConfigurationPayload))]
    [Description("Creates standard message payloads for the Tests device.")]
    public partial class CreateMessage : CreateMessageBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateMessage"/> class.
        /// </summary>
        public CreateMessage()
        {
            Payload = new CreateWhoAmIPayload();
        }

        string INamedElement.Name => $"{nameof(Tests)}.{GetElementDisplayName(Payload)}";
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that specifies the identity class of the device.
    /// </summary>
    [DisplayName("WhoAmIPayload")]
    [Description("Creates a message payload that specifies the identity class of the device.")]
    public partial class CreateWhoAmIPayload
    {
        /// <summary>
        /// Gets or sets the value that specifies the identity class of the device.
        /// </summary>
        [Description("The value that specifies the identity class of the device.")]
        public ushort WhoAmI { get; set; }

        /// <summary>
        /// Creates a message payload for the WhoAmI register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public ushort GetPayload()
        {
            return WhoAmI;
        }

        /// <summary>
        /// Creates a message that specifies the identity class of the device.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the WhoAmI register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Interface.Tests.WhoAmI.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that specifies the identity class of the device.
    /// </summary>
    [DisplayName("TimestampedWhoAmIPayload")]
    [Description("Creates a timestamped message payload that specifies the identity class of the device.")]
    public partial class CreateTimestampedWhoAmIPayload : CreateWhoAmIPayload
    {
        /// <summary>
        /// Creates a timestamped message that specifies the identity class of the device.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the WhoAmI register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Interface.Tests.WhoAmI.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that specifies the major hardware version of the device.
    /// </summary>
    [DisplayName("HardwareVersionHighPayload")]
    [Description("Creates a message payload that specifies the major hardware version of the device.")]
    public partial class CreateHardwareVersionHighPayload
    {
        /// <summary>
        /// Gets or sets the value that specifies the major hardware version of the device.
        /// </summary>
        [Description("The value that specifies the major hardware version of the device.")]
        public byte HardwareVersionHigh { get; set; }

        /// <summary>
        /// Creates a message payload for the HardwareVersionHigh register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte GetPayload()
        {
            return HardwareVersionHigh;
        }

        /// <summary>
        /// Creates a message that specifies the major hardware version of the device.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the HardwareVersionHigh register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Interface.Tests.HardwareVersionHigh.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that specifies the major hardware version of the device.
    /// </summary>
    [DisplayName("TimestampedHardwareVersionHighPayload")]
    [Description("Creates a timestamped message payload that specifies the major hardware version of the device.")]
    public partial class CreateTimestampedHardwareVersionHighPayload : CreateHardwareVersionHighPayload
    {
        /// <summary>
        /// Creates a timestamped message that specifies the major hardware version of the device.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the HardwareVersionHigh register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Interface.Tests.HardwareVersionHigh.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that specifies the minor hardware version of the device.
    /// </summary>
    [DisplayName("HardwareVersionLowPayload")]
    [Description("Creates a message payload that specifies the minor hardware version of the device.")]
    public partial class CreateHardwareVersionLowPayload
    {
        /// <summary>
        /// Gets or sets the value that specifies the minor hardware version of the device.
        /// </summary>
        [Description("The value that specifies the minor hardware version of the device.")]
        public byte HardwareVersionLow { get; set; }

        /// <summary>
        /// Creates a message payload for the HardwareVersionLow register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte GetPayload()
        {
            return HardwareVersionLow;
        }

        /// <summary>
        /// Creates a message that specifies the minor hardware version of the device.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the HardwareVersionLow register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Interface.Tests.HardwareVersionLow.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that specifies the minor hardware version of the device.
    /// </summary>
    [DisplayName("TimestampedHardwareVersionLowPayload")]
    [Description("Creates a timestamped message payload that specifies the minor hardware version of the device.")]
    public partial class CreateTimestampedHardwareVersionLowPayload : CreateHardwareVersionLowPayload
    {
        /// <summary>
        /// Creates a timestamped message that specifies the minor hardware version of the device.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the HardwareVersionLow register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Interface.Tests.HardwareVersionLow.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that specifies the version of the assembled components in the device.
    /// </summary>
    [DisplayName("AssemblyVersionPayload")]
    [Description("Creates a message payload that specifies the version of the assembled components in the device.")]
    public partial class CreateAssemblyVersionPayload
    {
        /// <summary>
        /// Gets or sets the value that specifies the version of the assembled components in the device.
        /// </summary>
        [Description("The value that specifies the version of the assembled components in the device.")]
        public byte AssemblyVersion { get; set; }

        /// <summary>
        /// Creates a message payload for the AssemblyVersion register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte GetPayload()
        {
            return AssemblyVersion;
        }

        /// <summary>
        /// Creates a message that specifies the version of the assembled components in the device.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the AssemblyVersion register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Interface.Tests.AssemblyVersion.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that specifies the version of the assembled components in the device.
    /// </summary>
    [DisplayName("TimestampedAssemblyVersionPayload")]
    [Description("Creates a timestamped message payload that specifies the version of the assembled components in the device.")]
    public partial class CreateTimestampedAssemblyVersionPayload : CreateAssemblyVersionPayload
    {
        /// <summary>
        /// Creates a timestamped message that specifies the version of the assembled components in the device.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the AssemblyVersion register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Interface.Tests.AssemblyVersion.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that specifies the major version of the Harp core implemented by the device.
    /// </summary>
    [DisplayName("CoreVersionHighPayload")]
    [Description("Creates a message payload that specifies the major version of the Harp core implemented by the device.")]
    public partial class CreateCoreVersionHighPayload
    {
        /// <summary>
        /// Gets or sets the value that specifies the major version of the Harp core implemented by the device.
        /// </summary>
        [Description("The value that specifies the major version of the Harp core implemented by the device.")]
        public byte CoreVersionHigh { get; set; }

        /// <summary>
        /// Creates a message payload for the CoreVersionHigh register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte GetPayload()
        {
            return CoreVersionHigh;
        }

        /// <summary>
        /// Creates a message that specifies the major version of the Harp core implemented by the device.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the CoreVersionHigh register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Interface.Tests.CoreVersionHigh.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that specifies the major version of the Harp core implemented by the device.
    /// </summary>
    [DisplayName("TimestampedCoreVersionHighPayload")]
    [Description("Creates a timestamped message payload that specifies the major version of the Harp core implemented by the device.")]
    public partial class CreateTimestampedCoreVersionHighPayload : CreateCoreVersionHighPayload
    {
        /// <summary>
        /// Creates a timestamped message that specifies the major version of the Harp core implemented by the device.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the CoreVersionHigh register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Interface.Tests.CoreVersionHigh.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that specifies the minor version of the Harp core implemented by the device.
    /// </summary>
    [DisplayName("CoreVersionLowPayload")]
    [Description("Creates a message payload that specifies the minor version of the Harp core implemented by the device.")]
    public partial class CreateCoreVersionLowPayload
    {
        /// <summary>
        /// Gets or sets the value that specifies the minor version of the Harp core implemented by the device.
        /// </summary>
        [Description("The value that specifies the minor version of the Harp core implemented by the device.")]
        public byte CoreVersionLow { get; set; }

        /// <summary>
        /// Creates a message payload for the CoreVersionLow register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte GetPayload()
        {
            return CoreVersionLow;
        }

        /// <summary>
        /// Creates a message that specifies the minor version of the Harp core implemented by the device.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the CoreVersionLow register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Interface.Tests.CoreVersionLow.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that specifies the minor version of the Harp core implemented by the device.
    /// </summary>
    [DisplayName("TimestampedCoreVersionLowPayload")]
    [Description("Creates a timestamped message payload that specifies the minor version of the Harp core implemented by the device.")]
    public partial class CreateTimestampedCoreVersionLowPayload : CreateCoreVersionLowPayload
    {
        /// <summary>
        /// Creates a timestamped message that specifies the minor version of the Harp core implemented by the device.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the CoreVersionLow register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Interface.Tests.CoreVersionLow.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that specifies the major version of the Harp core implemented by the device.
    /// </summary>
    [DisplayName("FirmwareVersionHighPayload")]
    [Description("Creates a message payload that specifies the major version of the Harp core implemented by the device.")]
    public partial class CreateFirmwareVersionHighPayload
    {
        /// <summary>
        /// Gets or sets the value that specifies the major version of the Harp core implemented by the device.
        /// </summary>
        [Description("The value that specifies the major version of the Harp core implemented by the device.")]
        public byte FirmwareVersionHigh { get; set; }

        /// <summary>
        /// Creates a message payload for the FirmwareVersionHigh register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte GetPayload()
        {
            return FirmwareVersionHigh;
        }

        /// <summary>
        /// Creates a message that specifies the major version of the Harp core implemented by the device.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the FirmwareVersionHigh register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Interface.Tests.FirmwareVersionHigh.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that specifies the major version of the Harp core implemented by the device.
    /// </summary>
    [DisplayName("TimestampedFirmwareVersionHighPayload")]
    [Description("Creates a timestamped message payload that specifies the major version of the Harp core implemented by the device.")]
    public partial class CreateTimestampedFirmwareVersionHighPayload : CreateFirmwareVersionHighPayload
    {
        /// <summary>
        /// Creates a timestamped message that specifies the major version of the Harp core implemented by the device.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the FirmwareVersionHigh register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Interface.Tests.FirmwareVersionHigh.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that specifies the minor version of the Harp core implemented by the device.
    /// </summary>
    [DisplayName("FirmwareVersionLowPayload")]
    [Description("Creates a message payload that specifies the minor version of the Harp core implemented by the device.")]
    public partial class CreateFirmwareVersionLowPayload
    {
        /// <summary>
        /// Gets or sets the value that specifies the minor version of the Harp core implemented by the device.
        /// </summary>
        [Description("The value that specifies the minor version of the Harp core implemented by the device.")]
        public byte FirmwareVersionLow { get; set; }

        /// <summary>
        /// Creates a message payload for the FirmwareVersionLow register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte GetPayload()
        {
            return FirmwareVersionLow;
        }

        /// <summary>
        /// Creates a message that specifies the minor version of the Harp core implemented by the device.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the FirmwareVersionLow register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Interface.Tests.FirmwareVersionLow.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that specifies the minor version of the Harp core implemented by the device.
    /// </summary>
    [DisplayName("TimestampedFirmwareVersionLowPayload")]
    [Description("Creates a timestamped message payload that specifies the minor version of the Harp core implemented by the device.")]
    public partial class CreateTimestampedFirmwareVersionLowPayload : CreateFirmwareVersionLowPayload
    {
        /// <summary>
        /// Creates a timestamped message that specifies the minor version of the Harp core implemented by the device.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the FirmwareVersionLow register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Interface.Tests.FirmwareVersionLow.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that stores the integral part of the system timestamp, in seconds.
    /// </summary>
    [DisplayName("TimestampSecondsPayload")]
    [Description("Creates a message payload that stores the integral part of the system timestamp, in seconds.")]
    public partial class CreateTimestampSecondsPayload
    {
        /// <summary>
        /// Gets or sets the value that stores the integral part of the system timestamp, in seconds.
        /// </summary>
        [Description("The value that stores the integral part of the system timestamp, in seconds.")]
        public uint TimestampSeconds { get; set; }

        /// <summary>
        /// Creates a message payload for the TimestampSeconds register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public uint GetPayload()
        {
            return TimestampSeconds;
        }

        /// <summary>
        /// Creates a message that stores the integral part of the system timestamp, in seconds.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the TimestampSeconds register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Interface.Tests.TimestampSeconds.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that stores the integral part of the system timestamp, in seconds.
    /// </summary>
    [DisplayName("TimestampedTimestampSecondsPayload")]
    [Description("Creates a timestamped message payload that stores the integral part of the system timestamp, in seconds.")]
    public partial class CreateTimestampedTimestampSecondsPayload : CreateTimestampSecondsPayload
    {
        /// <summary>
        /// Creates a timestamped message that stores the integral part of the system timestamp, in seconds.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the TimestampSeconds register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Interface.Tests.TimestampSeconds.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that stores the fractional part of the system timestamp, in microseconds.
    /// </summary>
    [DisplayName("TimestampMicrosecondsPayload")]
    [Description("Creates a message payload that stores the fractional part of the system timestamp, in microseconds.")]
    public partial class CreateTimestampMicrosecondsPayload
    {
        /// <summary>
        /// Gets or sets the value that stores the fractional part of the system timestamp, in microseconds.
        /// </summary>
        [Description("The value that stores the fractional part of the system timestamp, in microseconds.")]
        public ushort TimestampMicroseconds { get; set; }

        /// <summary>
        /// Creates a message payload for the TimestampMicroseconds register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public ushort GetPayload()
        {
            return TimestampMicroseconds;
        }

        /// <summary>
        /// Creates a message that stores the fractional part of the system timestamp, in microseconds.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the TimestampMicroseconds register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Interface.Tests.TimestampMicroseconds.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that stores the fractional part of the system timestamp, in microseconds.
    /// </summary>
    [DisplayName("TimestampedTimestampMicrosecondsPayload")]
    [Description("Creates a timestamped message payload that stores the fractional part of the system timestamp, in microseconds.")]
    public partial class CreateTimestampedTimestampMicrosecondsPayload : CreateTimestampMicrosecondsPayload
    {
        /// <summary>
        /// Creates a timestamped message that stores the fractional part of the system timestamp, in microseconds.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the TimestampMicroseconds register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Interface.Tests.TimestampMicroseconds.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that stores the configuration mode of the device.
    /// </summary>
    [DisplayName("OperationControlPayload")]
    [Description("Creates a message payload that stores the configuration mode of the device.")]
    public partial class CreateOperationControlPayload
    {
        /// <summary>
        /// Gets or sets a value that specifies the operation mode of the device.
        /// </summary>
        [Description("Specifies the operation mode of the device.")]
        public OperationMode OperationMode { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the device should report the content of all registers on initialization.
        /// </summary>
        [Description("Specifies whether the device should report the content of all registers on initialization.")]
        public bool DumpRegisters { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the replies to all commands will be muted, i.e. not sent by the device.
        /// </summary>
        [Description("Specifies whether the replies to all commands will be muted, i.e. not sent by the device.")]
        public bool MuteReplies { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the state of all visual indicators on the device.
        /// </summary>
        [Description("Specifies the state of all visual indicators on the device.")]
        public EnableFlag VisualIndicators { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the device state LED should report the operation mode of the device.
        /// </summary>
        [Description("Specifies whether the device state LED should report the operation mode of the device.")]
        public EnableFlag OperationLed { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the device should report the content of the seconds register each second.
        /// </summary>
        [Description("Specifies whether the device should report the content of the seconds register each second.")]
        public EnableFlag Heartbeat { get; set; }

        /// <summary>
        /// Creates a message payload for the OperationControl register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public OperationControlPayload GetPayload()
        {
            OperationControlPayload value;
            value.OperationMode = OperationMode;
            value.DumpRegisters = DumpRegisters;
            value.MuteReplies = MuteReplies;
            value.VisualIndicators = VisualIndicators;
            value.OperationLed = OperationLed;
            value.Heartbeat = Heartbeat;
            return value;
        }

        /// <summary>
        /// Creates a message that stores the configuration mode of the device.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the OperationControl register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Interface.Tests.OperationControl.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that stores the configuration mode of the device.
    /// </summary>
    [DisplayName("TimestampedOperationControlPayload")]
    [Description("Creates a timestamped message payload that stores the configuration mode of the device.")]
    public partial class CreateTimestampedOperationControlPayload : CreateOperationControlPayload
    {
        /// <summary>
        /// Creates a timestamped message that stores the configuration mode of the device.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the OperationControl register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Interface.Tests.OperationControl.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that resets the device and saves non-volatile registers.
    /// </summary>
    [DisplayName("ResetDevicePayload")]
    [Description("Creates a message payload that resets the device and saves non-volatile registers.")]
    public partial class CreateResetDevicePayload
    {
        /// <summary>
        /// Gets or sets the value that resets the device and saves non-volatile registers.
        /// </summary>
        [Description("The value that resets the device and saves non-volatile registers.")]
        public ResetFlags ResetDevice { get; set; }

        /// <summary>
        /// Creates a message payload for the ResetDevice register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public ResetFlags GetPayload()
        {
            return ResetDevice;
        }

        /// <summary>
        /// Creates a message that resets the device and saves non-volatile registers.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the ResetDevice register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Interface.Tests.ResetDevice.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that resets the device and saves non-volatile registers.
    /// </summary>
    [DisplayName("TimestampedResetDevicePayload")]
    [Description("Creates a timestamped message payload that resets the device and saves non-volatile registers.")]
    public partial class CreateTimestampedResetDevicePayload : CreateResetDevicePayload
    {
        /// <summary>
        /// Creates a timestamped message that resets the device and saves non-volatile registers.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the ResetDevice register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Interface.Tests.ResetDevice.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that stores the user-specified device name.
    /// </summary>
    [DisplayName("DeviceNamePayload")]
    [Description("Creates a message payload that stores the user-specified device name.")]
    public partial class CreateDeviceNamePayload
    {
        /// <summary>
        /// Gets or sets the value that stores the user-specified device name.
        /// </summary>
        [Description("The value that stores the user-specified device name.")]
        public string DeviceName { get; set; }

        /// <summary>
        /// Creates a message payload for the DeviceName register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public string GetPayload()
        {
            return DeviceName;
        }

        /// <summary>
        /// Creates a message that stores the user-specified device name.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the DeviceName register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Interface.Tests.DeviceName.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that stores the user-specified device name.
    /// </summary>
    [DisplayName("TimestampedDeviceNamePayload")]
    [Description("Creates a timestamped message payload that stores the user-specified device name.")]
    public partial class CreateTimestampedDeviceNamePayload : CreateDeviceNamePayload
    {
        /// <summary>
        /// Creates a timestamped message that stores the user-specified device name.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the DeviceName register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Interface.Tests.DeviceName.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that specifies the unique serial number of the device.
    /// </summary>
    [DisplayName("SerialNumberPayload")]
    [Description("Creates a message payload that specifies the unique serial number of the device.")]
    public partial class CreateSerialNumberPayload
    {
        /// <summary>
        /// Gets or sets the value that specifies the unique serial number of the device.
        /// </summary>
        [Description("The value that specifies the unique serial number of the device.")]
        public ushort SerialNumber { get; set; }

        /// <summary>
        /// Creates a message payload for the SerialNumber register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public ushort GetPayload()
        {
            return SerialNumber;
        }

        /// <summary>
        /// Creates a message that specifies the unique serial number of the device.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the SerialNumber register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Interface.Tests.SerialNumber.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that specifies the unique serial number of the device.
    /// </summary>
    [DisplayName("TimestampedSerialNumberPayload")]
    [Description("Creates a timestamped message payload that specifies the unique serial number of the device.")]
    public partial class CreateTimestampedSerialNumberPayload : CreateSerialNumberPayload
    {
        /// <summary>
        /// Creates a timestamped message that specifies the unique serial number of the device.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the SerialNumber register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Interface.Tests.SerialNumber.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that specifies the configuration for the device synchronization clock.
    /// </summary>
    [DisplayName("ClockConfigurationPayload")]
    [Description("Creates a message payload that specifies the configuration for the device synchronization clock.")]
    public partial class CreateClockConfigurationPayload
    {
        /// <summary>
        /// Gets or sets the value that specifies the configuration for the device synchronization clock.
        /// </summary>
        [Description("The value that specifies the configuration for the device synchronization clock.")]
        public ClockConfigurationFlags ClockConfiguration { get; set; }

        /// <summary>
        /// Creates a message payload for the ClockConfiguration register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public ClockConfigurationFlags GetPayload()
        {
            return ClockConfiguration;
        }

        /// <summary>
        /// Creates a message that specifies the configuration for the device synchronization clock.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the ClockConfiguration register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Interface.Tests.ClockConfiguration.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that specifies the configuration for the device synchronization clock.
    /// </summary>
    [DisplayName("TimestampedClockConfigurationPayload")]
    [Description("Creates a timestamped message payload that specifies the configuration for the device synchronization clock.")]
    public partial class CreateTimestampedClockConfigurationPayload : CreateClockConfigurationPayload
    {
        /// <summary>
        /// Creates a timestamped message that specifies the configuration for the device synchronization clock.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the ClockConfiguration register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Interface.Tests.ClockConfiguration.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents the payload of the OperationControl register.
    /// </summary>
    public struct OperationControlPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationControlPayload"/> structure.
        /// </summary>
        /// <param name="operationMode">Specifies the operation mode of the device.</param>
        /// <param name="dumpRegisters">Specifies whether the device should report the content of all registers on initialization.</param>
        /// <param name="muteReplies">Specifies whether the replies to all commands will be muted, i.e. not sent by the device.</param>
        /// <param name="visualIndicators">Specifies the state of all visual indicators on the device.</param>
        /// <param name="operationLed">Specifies whether the device state LED should report the operation mode of the device.</param>
        /// <param name="heartbeat">Specifies whether the device should report the content of the seconds register each second.</param>
        public OperationControlPayload(
            OperationMode operationMode,
            bool dumpRegisters,
            bool muteReplies,
            EnableFlag visualIndicators,
            EnableFlag operationLed,
            EnableFlag heartbeat)
        {
            OperationMode = operationMode;
            DumpRegisters = dumpRegisters;
            MuteReplies = muteReplies;
            VisualIndicators = visualIndicators;
            OperationLed = operationLed;
            Heartbeat = heartbeat;
        }

        /// <summary>
        /// Specifies the operation mode of the device.
        /// </summary>
        public OperationMode OperationMode;

        /// <summary>
        /// Specifies whether the device should report the content of all registers on initialization.
        /// </summary>
        public bool DumpRegisters;

        /// <summary>
        /// Specifies whether the replies to all commands will be muted, i.e. not sent by the device.
        /// </summary>
        public bool MuteReplies;

        /// <summary>
        /// Specifies the state of all visual indicators on the device.
        /// </summary>
        public EnableFlag VisualIndicators;

        /// <summary>
        /// Specifies whether the device state LED should report the operation mode of the device.
        /// </summary>
        public EnableFlag OperationLed;

        /// <summary>
        /// Specifies whether the device should report the content of the seconds register each second.
        /// </summary>
        public EnableFlag Heartbeat;

        /// <summary>
        /// Returns a <see cref="string"/> that represents the payload of
        /// the OperationControl register.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents the payload of the
        /// OperationControl register.
        /// </returns>
        public override string ToString()
        {
            return "OperationControlPayload { " +
                "OperationMode = " + OperationMode + ", " +
                "DumpRegisters = " + DumpRegisters + ", " +
                "MuteReplies = " + MuteReplies + ", " +
                "VisualIndicators = " + VisualIndicators + ", " +
                "OperationLed = " + OperationLed + ", " +
                "Heartbeat = " + Heartbeat + " " +
            "}";
        }
    }

    /// <summary>
    /// Specifies the behavior of the non-volatile registers when resetting the device.
    /// </summary>
    [Flags]
    public enum ResetFlags : byte
    {
        /// <summary>
        /// All reset flags are cleared.
        /// </summary>
        [Description("All reset flags are cleared.")]
        None = 0x0,

        /// <summary>
        /// The device will boot with all the registers reset to their default factory values.
        /// </summary>
        [Description("The device will boot with all the registers reset to their default factory values.")]
        RestoreDefault = 0x1,

        /// <summary>
        /// The device will boot and restore all the registers to the values stored in non-volatile memory.
        /// </summary>
        [Description("The device will boot and restore all the registers to the values stored in non-volatile memory.")]
        RestoreEeprom = 0x2,

        /// <summary>
        /// The device will boot and save all the current register values to non-volatile memory.
        /// </summary>
        [Description("The device will boot and save all the current register values to non-volatile memory.")]
        Save = 0x4,

        /// <summary>
        /// The device will boot with the default device name.
        /// </summary>
        [Description("The device will boot with the default device name.")]
        RestoreName = 0x8,

        /// <summary>
        /// The device will enter firmware update mode.
        /// </summary>
        [Description("The device will enter firmware update mode.")]
        UpdateFirmware = 0x20,

        /// <summary>
        /// Specifies that the device has booted from default factory values.
        /// </summary>
        [Description("Specifies that the device has booted from default factory values.")]
        BootFromDefault = 0x40,

        /// <summary>
        /// Specifies that the device has booted from non-volatile values stored in EEPROM.
        /// </summary>
        [Description("Specifies that the device has booted from non-volatile values stored in EEPROM.")]
        BootFromEeprom = 0x80
    }

    /// <summary>
    /// Specifies configuration flags for the device synchronization clock.
    /// </summary>
    [Flags]
    public enum ClockConfigurationFlags : byte
    {
        /// <summary>
        /// All clock configuration flags are cleared.
        /// </summary>
        [Description("All clock configuration flags are cleared.")]
        None = 0x0,

        /// <summary>
        /// The device will repeat the clock synchronization signal to the clock output connector, if available.
        /// </summary>
        [Description("The device will repeat the clock synchronization signal to the clock output connector, if available.")]
        ClockRepeater = 0x1,

        /// <summary>
        /// The device resets and generates the clock synchronization signal on the clock output connector, if available.
        /// </summary>
        [Description("The device resets and generates the clock synchronization signal on the clock output connector, if available.")]
        ClockGenerator = 0x2,

        /// <summary>
        /// Specifies the device has the capability to repeat the clock synchronization signal to the clock output connector.
        /// </summary>
        [Description("Specifies the device has the capability to repeat the clock synchronization signal to the clock output connector.")]
        RepeaterCapability = 0x8,

        /// <summary>
        /// Specifies the device has the capability to generate the clock synchronization signal to the clock output connector.
        /// </summary>
        [Description("Specifies the device has the capability to generate the clock synchronization signal to the clock output connector.")]
        GeneratorCapability = 0x10,

        /// <summary>
        /// The device will unlock the timestamp register counter and will accept commands to set new timestamp values.
        /// </summary>
        [Description("The device will unlock the timestamp register counter and will accept commands to set new timestamp values.")]
        ClockUnlock = 0x40,

        /// <summary>
        /// The device will lock the timestamp register counter and will not accept commands to set new timestamp values.
        /// </summary>
        [Description("The device will lock the timestamp register counter and will not accept commands to set new timestamp values.")]
        ClockLock = 0x80
    }

    /// <summary>
    /// Specifies the operation mode of the device.
    /// </summary>
    public enum OperationMode : byte
    {
        /// <summary>
        /// Disable all event reporting on the device.
        /// </summary>
        [Description("Disable all event reporting on the device.")]
        Standby = 0,

        /// <summary>
        /// Event detection is enabled. Only enabled events are reported by the device.
        /// </summary>
        [Description("Event detection is enabled. Only enabled events are reported by the device.")]
        Active = 1,

        /// <summary>
        /// The device enters speed mode.
        /// </summary>
        [Description("The device enters speed mode.")]
        Speed = 3
    }

    /// <summary>
    /// Specifies whether a specific register flag is enabled or disabled.
    /// </summary>
    public enum EnableFlag : byte
    {
        /// <summary>
        /// Specifies that the flag is disabled.
        /// </summary>
        [Description("Specifies that the flag is disabled.")]
        Disabled = 0,

        /// <summary>
        /// Specifies that the flag is enabled.
        /// </summary>
        [Description("Specifies that the flag is enabled.")]
        Enabled = 1
    }

    internal static partial class PayloadMarshal
    {
        internal static T[] GetSubArray<T>(T[] array, int offset, int count)
        {
            var result = new T[count];
            Array.Copy(array, offset, result, 0, count);
            return result;
        }

        internal static byte ReadByte(ArraySegment<byte> segment) => segment.Array[segment.Offset];

        internal static sbyte ReadSByte(ArraySegment<byte> segment) => (sbyte)segment.Array[segment.Offset];

        internal static ushort ReadUInt16(ArraySegment<byte> segment) => BitConverter.ToUInt16(segment.Array, segment.Offset);

        internal static short ReadInt16(ArraySegment<byte> segment) => BitConverter.ToInt16(segment.Array, segment.Offset);

        internal static uint ReadUInt32(ArraySegment<byte> segment) => BitConverter.ToUInt32(segment.Array, segment.Offset);

        internal static int ReadInt32(ArraySegment<byte> segment) => BitConverter.ToInt32(segment.Array, segment.Offset);

        internal static ulong ReadUInt64(ArraySegment<byte> segment) => BitConverter.ToUInt64(segment.Array, segment.Offset);

        internal static long ReadInt64(ArraySegment<byte> segment) => BitConverter.ToInt64(segment.Array, segment.Offset);

        internal static float ReadSingle(ArraySegment<byte> segment) => BitConverter.ToSingle(segment.Array, segment.Offset);

        internal static string ReadUtf8String(ArraySegment<byte> segment)
        {
            var count = Array.IndexOf(segment.Array, (byte)0, segment.Offset, segment.Count) - segment.Offset;
            return System.Text.Encoding.UTF8.GetString(segment.Array, segment.Offset, count < 0 ? segment.Count : count);
        }

        internal static void Write(ArraySegment<byte> segment, byte value) => segment.Array[segment.Offset] = value;

        internal static void Write(ArraySegment<byte> segment, sbyte value) => segment.Array[segment.Offset] = (byte)value;

        internal static void Write(ArraySegment<byte> segment, ushort value)
        {
            segment.Array[segment.Offset] = (byte)value;
            segment.Array[segment.Offset + 1] = (byte)(value >> 8);
        }

        internal static void Write(ArraySegment<byte> segment, short value)
        {
            segment.Array[segment.Offset] = (byte)value;
            segment.Array[segment.Offset + 1] = (byte)(value >> 8);
        }

        internal static void Write(ArraySegment<byte> segment, uint value)
        {
            segment.Array[segment.Offset] = (byte)value;
            segment.Array[segment.Offset + 1] = (byte)(value >> 8);
            segment.Array[segment.Offset + 2] = (byte)(value >> 16);
            segment.Array[segment.Offset + 3] = (byte)(value >> 24);
        }

        internal static void Write(ArraySegment<byte> segment, int value)
        {
            segment.Array[segment.Offset] = (byte)value;
            segment.Array[segment.Offset + 1] = (byte)(value >> 8);
            segment.Array[segment.Offset + 2] = (byte)(value >> 16);
            segment.Array[segment.Offset + 3] = (byte)(value >> 24);
        }

        internal static void Write(ArraySegment<byte> segment, ulong value)
        {
            segment.Array[segment.Offset] = (byte)value;
            segment.Array[segment.Offset + 1] = (byte)(value >> 8);
            segment.Array[segment.Offset + 2] = (byte)(value >> 16);
            segment.Array[segment.Offset + 3] = (byte)(value >> 24);
            segment.Array[segment.Offset + 4] = (byte)(value >> 32);
            segment.Array[segment.Offset + 5] = (byte)(value >> 40);
            segment.Array[segment.Offset + 6] = (byte)(value >> 48);
            segment.Array[segment.Offset + 7] = (byte)(value >> 56);
        }

        internal static void Write(ArraySegment<byte> segment, long value)
        {
            segment.Array[segment.Offset] = (byte)value;
            segment.Array[segment.Offset + 1] = (byte)(value >> 8);
            segment.Array[segment.Offset + 2] = (byte)(value >> 16);
            segment.Array[segment.Offset + 3] = (byte)(value >> 24);
            segment.Array[segment.Offset + 4] = (byte)(value >> 32);
            segment.Array[segment.Offset + 5] = (byte)(value >> 40);
            segment.Array[segment.Offset + 6] = (byte)(value >> 48);
            segment.Array[segment.Offset + 7] = (byte)(value >> 56);
        }

        internal static unsafe void Write(ArraySegment<byte> segment, float value) => Write(segment, *(int*)&value);

        internal static unsafe void Write(ArraySegment<byte> segment, string value) =>
            System.Text.Encoding.UTF8.GetBytes(value, 0, Math.Min(value.Length, segment.Count), segment.Array, segment.Offset);

        internal static void Write<T>(ArraySegment<byte> segment, T[] values) where T : unmanaged
        {
            Buffer.BlockCopy(values, 0, segment.Array, segment.Offset, segment.Count);
        }

        internal static void Write<T>(ArraySegment<T> segment, T[] values)
        {
            Array.Copy(values, 0, segment.Array, segment.Offset, segment.Count);
        }
    }
}
