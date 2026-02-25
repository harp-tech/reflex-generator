using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Harp.Generators.Tests;

[TestClass]
public sealed class InterfaceGeneratorTests
{
    InterfaceGenerator generator;
    DirectoryInfo outputDirectory;
    string payloadExtensions;

    [TestInitialize]
    public async Task Initialize()
    {
        generator = await InterfaceGenerator.Create();
        payloadExtensions = TestHelper.GetManifestResourceText("PayloadMarshal.cs");
        outputDirectory = Directory.CreateDirectory("InterfaceOutput");
        try { Directory.Delete(outputDirectory.FullName, recursive: true); }
        catch { } // best effort
    }

    [DataTestMethod]
    [DataRow("core.yml")]
    [DataRow("device.yml")]
    public void DeviceTemplate_GenerateAndBuildWithoutErrors(string metadataFileName)
    {
        metadataFileName = TestHelper.GetMetadataPath(metadataFileName);
        var implementation = generator.GenerateImplementation(metadataFileName, typeof(InterfaceGeneratorTests).Namespace);
        var outputFileName = $"{Path.GetFileNameWithoutExtension(metadataFileName)}.cs";
        var customImplementation = TestHelper.GetManifestResourceText($"EmbeddedSources.{outputFileName}");
        try
        {
            CompilerTestHelper.CompileFromSource(implementation.Device, implementation.AsyncDevice, payloadExtensions, customImplementation);
            TestHelper.AssertExpectedOutput(implementation.Device, outputFileName);
        }
        catch (AssertFailedException)
        {
            outputDirectory.Create();
            File.WriteAllText(Path.Combine(outputDirectory.FullName, outputFileName), implementation.Device);
            throw;
        }
    }
}
