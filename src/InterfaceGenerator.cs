using System.IO;
using System.Threading.Tasks;
using Mono.TextTemplating;

namespace Harp.Generators;

public sealed class InterfaceGenerator
{
    readonly EmbeddedTemplateGenerator _generator;
    readonly CompiledTemplate _deviceTemplate;
    readonly CompiledTemplate _asyncDeviceTemplate;

    private InterfaceGenerator(EmbeddedTemplateGenerator generator, CompiledTemplate deviceTemplate, CompiledTemplate asyncDeviceTemplate)
    {
        _generator = generator;
        _deviceTemplate = deviceTemplate;
        _asyncDeviceTemplate = asyncDeviceTemplate;
    }

    public static async Task<InterfaceGenerator> Create()
    {
        var generator = new EmbeddedTemplateGenerator();
        var deviceTemplateContents = EmbeddedTemplateGenerator.GetManifestResourceText("Device.tt");
        var asyncDeviceTemplateContents = EmbeddedTemplateGenerator.GetManifestResourceText("AsyncDevice.tt");
        var deviceTemplate = await generator.CompileTemplateAsync(deviceTemplateContents);
        generator.ThrowExceptionForGeneratorError();

        var asyncDeviceTemplate = await generator.CompileTemplateAsync(asyncDeviceTemplateContents);
        generator.ThrowExceptionForGeneratorError();

        return new InterfaceGenerator(generator, deviceTemplate, asyncDeviceTemplate);
    }

    static string ProcessTemplate(EmbeddedTemplateGenerator generator, CompiledTemplate template, string metadataFileName, string ns)
    {
        var session = generator.GetOrCreateSession();
        session["Namespace"] = ns;
        session["MetadataPath"] = Path.GetFullPath(Path.Combine("Metadata", metadataFileName));
        var code = template.Process();
        generator.ThrowExceptionForGeneratorError();
        return code;
    }

    public InterfaceImplementation GenerateImplementation(string metadataFileName, string ns) =>
        new(Device: ProcessTemplate(_generator, _deviceTemplate, metadataFileName, ns),
            AsyncDevice: ProcessTemplate(_generator, _asyncDeviceTemplate, metadataFileName, ns));
}

public record struct InterfaceImplementation(string Device, string AsyncDevice)
{
    public const string DeviceFileName = "Device.Generated.cs";
    public const string AsyncDeviceFileName = "AsyncDevice.Generated.cs";
}
