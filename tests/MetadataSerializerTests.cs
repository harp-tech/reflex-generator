using Microsoft.VisualStudio.TestTools.UnitTesting;
using YamlDotNet.Core;

namespace Harp.Generators.Tests;

[TestClass]
public sealed class MetadataSerializerTests
{
    DirectoryInfo outputDirectory;

    [TestInitialize]
    public void Initialize()
    {
        outputDirectory = Directory.CreateDirectory("MetadataOutput");
        try { Directory.Delete(outputDirectory.FullName, recursive: true); }
        catch { } // best effort
    }

    private static string NormalizeYaml(string contents)
    {
        using var reader = new StringReader(contents);
        using var writer = new StringWriter();
        var parser = new MergingParser(new Parser(reader));
        var emitterSettings = EmitterSettings.Default;
        var emitter = new Emitter(writer, new EmitterSettings(
            bestIndent: emitterSettings.BestIndent,
            bestWidth: emitterSettings.BestWidth,
            isCanonical: emitterSettings.IsCanonical,
            maxSimpleKeyLength: emitterSettings.MaxSimpleKeyLength,
            skipAnchorName: true));
        var stream = new YamlStream(parser);
        stream.Save(emitter);
        return writer.ToString();
    }

    [DataTestMethod]
    [DataRow("core.yml")]
    [DataRow("device.yml")]
    public void DeviceMetadata_RoundTripSerializes(string metadataFileName)
    {
        metadataFileName = TestHelper.GetMetadataPath(metadataFileName);
        var metadataContents = File.ReadAllText(metadataFileName);
        metadataContents = NormalizeYaml(metadataContents);

        var deviceMetadata = MetadataDeserializer.Instance.Deserialize<DeviceInfo>(metadataContents);
        var roundTripContents = MetadataSerializer.Instance.Serialize(deviceMetadata);
        try
        {
            Assert.AreEqual(metadataContents, roundTripContents);
        }
        catch (AssertFailedException)
        {
            outputDirectory.Create();
            File.WriteAllText(Path.Combine(outputDirectory.FullName, Path.GetFileName(metadataFileName)), roundTripContents);
            File.WriteAllText(Path.Combine(outputDirectory.FullName, $"{Path.GetFileNameWithoutExtension(metadataFileName)}_orig.yml"), metadataContents);
            throw;
        }
    }
}
