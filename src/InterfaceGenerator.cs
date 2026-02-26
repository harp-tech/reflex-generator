using System.Collections.Generic;
using System.IO;

namespace Harp.Generators;

public sealed class InterfaceGenerator
{
    readonly Device _deviceTemplate = new();
    readonly AsyncDevice _asyncDeviceTemplate = new();

    public InterfaceGenerator(string metadataFileName, string ns)
    {
        var session = new Dictionary<string, object>
        {
            { "Namespace", ns },
            { "MetadataPath", Path.GetFullPath(metadataFileName) }
        };
        _deviceTemplate.Session = session;
        _asyncDeviceTemplate.Session = session;
        _deviceTemplate.Initialize();
        _asyncDeviceTemplate.Initialize();
    }

    public InterfaceImplementation GenerateImplementation() =>
        new(Device: _deviceTemplate.TransformText(),
            AsyncDevice: _asyncDeviceTemplate.TransformText());
}

public record struct InterfaceImplementation(string Device, string AsyncDevice)
{
    public const string DeviceFileName = "Device.Generated.cs";
    public const string AsyncDeviceFileName = "AsyncDevice.Generated.cs";

    public readonly IEnumerable<KeyValuePair<string, string>> GetGeneratedFileContents()
    {
        yield return new(DeviceFileName, Device);
        yield return new(AsyncDeviceFileName, AsyncDevice);
    }
}
