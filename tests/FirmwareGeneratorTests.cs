using Harp.Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Interface.Tests;

[TestClass]
public sealed class FirmwareGeneratorTests
{
    FirmwareGenerator generator;
    DirectoryInfo outputDirectory;

    [TestInitialize]
    public async Task Initialize()
    {
        generator = await FirmwareGenerator.Create();
        outputDirectory = Directory.CreateDirectory("FirmwareOutput");
        try { Directory.Delete(outputDirectory.FullName, recursive: true); }
        catch { } // best effort
    }

    [DataTestMethod]
    [DataRow("device.yml")]
    public void FirmwareTemplate_GenerateAndBuildWithoutErrors(string metadataFileName)
    {
        var iosMetadataFileName = Path.ChangeExtension(metadataFileName, ".ios.yml");
        var headers = generator.GenerateHeaders(metadataFileName, iosMetadataFileName);
        var implementation = generator.GenerateImplementation(metadataFileName, iosMetadataFileName);

        var outputFileName = Path.GetFileNameWithoutExtension(metadataFileName);
        var appOutputFileName = $"{outputFileName}.{FirmwareHeaders.AppFileName}";
        var appImplOutputFileName = $"{outputFileName}.{FirmwareImplementation.AppFileName}";
        var appFuncsOutputFileName = $"{outputFileName}.{FirmwareHeaders.AppFuncsFileName}";
        var appFuncsImplOutputFileName = $"{outputFileName}.{FirmwareImplementation.AppFuncsFileName}";
        var appRegsOutputFileName = $"{outputFileName}.{FirmwareHeaders.AppRegsFileName}";
        var appRegsImplOutputFileName = $"{outputFileName}.{FirmwareImplementation.AppRegsFileName}";
        var interruptsOutputFileName = $"{outputFileName}.{FirmwareImplementation.InterruptsFileName}";
        try
        {
            TestHelper.AssertExpectedOutput(headers.App, appOutputFileName);
            TestHelper.AssertExpectedOutput(implementation.App, appImplOutputFileName);
            TestHelper.AssertExpectedOutput(headers.AppFuncs, appFuncsOutputFileName);
            TestHelper.AssertExpectedOutput(implementation.AppFuncs, appFuncsImplOutputFileName);
            TestHelper.AssertExpectedOutput(headers.AppRegs, appRegsOutputFileName);
            TestHelper.AssertExpectedOutput(implementation.AppRegs, appRegsImplOutputFileName);
            TestHelper.AssertExpectedOutput(implementation.Interrupts, interruptsOutputFileName);
        }
        catch (AssertFailedException)
        {
            outputDirectory.Create();
            File.WriteAllText(Path.Combine(outputDirectory.FullName, appOutputFileName), headers.App);
            File.WriteAllText(Path.Combine(outputDirectory.FullName, appImplOutputFileName), implementation.App);
            File.WriteAllText(Path.Combine(outputDirectory.FullName, appFuncsOutputFileName), headers.AppFuncs);
            File.WriteAllText(Path.Combine(outputDirectory.FullName, appFuncsImplOutputFileName), implementation.AppFuncs);
            File.WriteAllText(Path.Combine(outputDirectory.FullName, appRegsOutputFileName), headers.AppRegs);
            File.WriteAllText(Path.Combine(outputDirectory.FullName, appRegsImplOutputFileName), implementation.AppRegs);
            File.WriteAllText(Path.Combine(outputDirectory.FullName, interruptsOutputFileName), implementation.Interrupts);
            throw;
        }
    }
}
