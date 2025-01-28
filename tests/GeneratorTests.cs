using Mono.TextTemplating;

namespace Interface.Tests;

[TestClass]
public sealed class GeneratorTests
{
    TemplateGenerator generator;
    CompiledTemplate deviceTemplate;
    CompiledTemplate asyncDeviceTemplate;

    [TestInitialize]
    public async Task Initialize()
    {
        generator = new TestTemplateGenerator();
        var deviceTemplateContents = TestHelper.GetManifestResourceText("Device.tt");
        var asyncDeviceTemplateContents = TestHelper.GetManifestResourceText("AsyncDevice.tt");
        deviceTemplate = await generator.CompileTemplateAsync(deviceTemplateContents);
        TestHelper.AssertNoGeneratorErrors(generator);

        asyncDeviceTemplate = await generator.CompileTemplateAsync(asyncDeviceTemplateContents);
        TestHelper.AssertNoGeneratorErrors(generator);
    }

    private string ProcessTemplate(CompiledTemplate template, string metadataFileName)
    {
        var session = generator.GetOrCreateSession();
        session["Namespace"] = typeof(GeneratorTests).Namespace;
        session["MetadataPath"] = Path.GetFullPath(Path.Combine("Metadata", metadataFileName));
        return template.Process();
    }

    [DataTestMethod]
    [DataRow("device.yml")]
    public void DeviceTemplate_GenerateNoErrors(string metadataFileName)
    {
        ProcessTemplate(deviceTemplate, metadataFileName);
        TestHelper.AssertNoGeneratorErrors(generator);
    }

    [DataTestMethod]
    [DataRow("device.yml")]
    public void AsyncDeviceTemplate_GenerateNoErrors(string metadataFileName)
    {
        ProcessTemplate(asyncDeviceTemplate, metadataFileName);
        TestHelper.AssertNoGeneratorErrors(generator);
    }
}
