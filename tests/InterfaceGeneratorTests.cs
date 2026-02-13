using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.TextTemplating;

namespace Interface.Tests;

[TestClass]
public sealed class InterfaceGeneratorTests
{
    TemplateGenerator generator;
    CompiledTemplate deviceTemplate;
    CompiledTemplate asyncDeviceTemplate;
    DirectoryInfo outputDirectory;
    string payloadExtensions;

    [TestInitialize]
    public async Task Initialize()
    {
        generator = new TestTemplateGenerator();
        var deviceTemplateContents = TestHelper.GetManifestResourceText("Device.tt");
        var asyncDeviceTemplateContents = TestHelper.GetManifestResourceText("AsyncDevice.tt");
        payloadExtensions = TestHelper.GetManifestResourceText("PayloadMarshal.cs");
        deviceTemplate = await generator.CompileTemplateAsync(deviceTemplateContents);
        TestHelper.AssertNoGeneratorErrors(generator);

        asyncDeviceTemplate = await generator.CompileTemplateAsync(asyncDeviceTemplateContents);
        TestHelper.AssertNoGeneratorErrors(generator);

        outputDirectory = Directory.CreateDirectory("InterfaceOutput");
        try { Directory.Delete(outputDirectory.FullName, recursive: true); }
        catch { } // best effort
    }

    private string ProcessTemplate(CompiledTemplate template, string metadataFileName)
    {
        var session = generator.GetOrCreateSession();
        session["Namespace"] = typeof(InterfaceGeneratorTests).Namespace;
        session["MetadataPath"] = Path.GetFullPath(Path.Combine("Metadata", metadataFileName));
        return template.Process();
    }

    [DataTestMethod]
    [DataRow("core.yml")]
    [DataRow("device.yml")]
    public void DeviceTemplate_GenerateAndBuildWithoutErrors(string metadataFileName)
    {
        var deviceCode = ProcessTemplate(deviceTemplate, metadataFileName);
        TestHelper.AssertNoGeneratorErrors(generator);

        var asyncDeviceCode = ProcessTemplate(asyncDeviceTemplate, metadataFileName);
        TestHelper.AssertNoGeneratorErrors(generator);

        var outputFileName = $"{Path.GetFileNameWithoutExtension(metadataFileName)}.cs";
        var customImplementation = TestHelper.GetManifestResourceText($"EmbeddedSources.{outputFileName}");
        try
        {
            CompilerTestHelper.CompileFromSource(deviceCode, asyncDeviceCode, payloadExtensions, customImplementation);
            TestHelper.AssertExpectedOutput(deviceCode, outputFileName);
        }
        catch (AssertFailedException)
        {
            outputDirectory.Create();
            File.WriteAllText(Path.Combine(outputDirectory.FullName, outputFileName), deviceCode);
            throw;
        }
    }
}
