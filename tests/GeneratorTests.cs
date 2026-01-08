using Mono.TextTemplating;

namespace Interface.Tests;

[TestClass]
public sealed class GeneratorTests
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
        payloadExtensions = TestHelper.GetManifestResourceText("PayloadExtensions.cs");
        deviceTemplate = await generator.CompileTemplateAsync(deviceTemplateContents);
        TestHelper.AssertNoGeneratorErrors(generator);

        asyncDeviceTemplate = await generator.CompileTemplateAsync(asyncDeviceTemplateContents);
        TestHelper.AssertNoGeneratorErrors(generator);

        outputDirectory = Directory.CreateDirectory("ActualOutput");
        try { Directory.Delete(outputDirectory.FullName, recursive: true); }
        catch { } // best effort
    }

    private string ProcessTemplate(CompiledTemplate template, string metadataFileName)
    {
        var session = generator.GetOrCreateSession();
        session["Namespace"] = typeof(GeneratorTests).Namespace;
        session["MetadataPath"] = Path.GetFullPath(Path.Combine("Metadata", metadataFileName));
        return template.Process();
    }

    private void AssertExpectedOutput(string actual, string outputFileName)
    {
        var expectedFileName = Path.Combine("ExpectedOutput", outputFileName);
        if (File.Exists(expectedFileName))
        {
            var expected = File.ReadAllText(expectedFileName);
            if (!string.Equals(actual, expected, StringComparison.InvariantCulture))
            {
                Assert.Fail($"The generated output has diverged from the reference: {outputFileName}");
            }
        }
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
        try
        {
            CompilerTestHelper.CompileFromSource(deviceCode, asyncDeviceCode, payloadExtensions);
            AssertExpectedOutput(deviceCode, outputFileName);
        }
        catch (AssertFailedException)
        {
            outputDirectory.Create();
            File.WriteAllText(Path.Combine(outputDirectory.FullName, outputFileName), deviceCode);
            throw;
        }
    }
}
