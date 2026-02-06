using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.TextTemplating;

namespace Interface.Tests;

[TestClass]
public sealed class FirmwareGeneratorTests
{
    TemplateGenerator generator;
    CompiledTemplate appTemplate;
    CompiledTemplate appFuncsTemplate;
    CompiledTemplate appFuncsImplTemplate;
    CompiledTemplate appRegsTemplate;
    CompiledTemplate appRegsImplTemplate;
    DirectoryInfo outputDirectory;

    [TestInitialize]
    public async Task Initialize()
    {
        generator = new TestTemplateGenerator();
        var appTemplateContents = TestHelper.GetManifestResourceText("App.tt");
        var appFuncsTemplateContents = TestHelper.GetManifestResourceText("AppFuncs.tt");
        var appFuncsImplTemplateContents = TestHelper.GetManifestResourceText("AppFuncsImpl.tt");
        var appRegsTemplateContents = TestHelper.GetManifestResourceText("AppRegs.tt");
        var appRegsImplTemplateContents = TestHelper.GetManifestResourceText("AppRegsImpl.tt");
        appTemplate = await generator.CompileTemplateAsync(appTemplateContents);
        TestHelper.AssertNoGeneratorErrors(generator);

        appFuncsTemplate = await generator.CompileTemplateAsync(appFuncsTemplateContents);
        TestHelper.AssertNoGeneratorErrors(generator);

        appFuncsImplTemplate = await generator.CompileTemplateAsync(appFuncsImplTemplateContents);
        TestHelper.AssertNoGeneratorErrors(generator);

        appRegsTemplate = await generator.CompileTemplateAsync(appRegsTemplateContents);
        TestHelper.AssertNoGeneratorErrors(generator);

        appRegsImplTemplate = await generator.CompileTemplateAsync(appRegsImplTemplateContents);
        TestHelper.AssertNoGeneratorErrors(generator);

        outputDirectory = Directory.CreateDirectory("ActualOutput");
        try { Directory.Delete(outputDirectory.FullName, recursive: true); }
        catch { } // best effort
    }

    private string ProcessTemplate(CompiledTemplate template, string registerMetadataFileName, string iosMetadataFileName = default)
    {
        var session = generator.GetOrCreateSession();
        session["RegisterMetadataPath"] = Path.GetFullPath(Path.Combine("Metadata", registerMetadataFileName));
        if (!string.IsNullOrEmpty(iosMetadataFileName))
            session["IOMetadataPath"] = Path.GetFullPath(Path.Combine("Metadata", iosMetadataFileName));
        return template.Process();
    }

    [DataTestMethod]
    [DataRow("device.yml")]
    public void FirmwareTemplate_GenerateAndBuildWithoutErrors(string metadataFileName)
    {
        var appCode = ProcessTemplate(appTemplate, metadataFileName);
        TestHelper.AssertNoGeneratorErrors(generator);

        var appFuncsCode = ProcessTemplate(appFuncsTemplate, metadataFileName);
        TestHelper.AssertNoGeneratorErrors(generator);

        var appFuncsImplCode = ProcessTemplate(appFuncsImplTemplate, metadataFileName);
        TestHelper.AssertNoGeneratorErrors(generator);

        var iosMetadataFileName = Path.ChangeExtension(metadataFileName, ".ios.yml");
        var appRegsCode = ProcessTemplate(appRegsTemplate, metadataFileName, iosMetadataFileName);
        TestHelper.AssertNoGeneratorErrors(generator);

        var appRegsImplCode = ProcessTemplate(appRegsImplTemplate, metadataFileName, iosMetadataFileName);
        TestHelper.AssertNoGeneratorErrors(generator);

        var outputFileName = Path.GetFileNameWithoutExtension(metadataFileName);
        var appOutputFileName = $"{outputFileName}.app.h";
        var appFuncsOutputFileName = $"{outputFileName}.app_funcs.h";
        var appFuncsImplOutputFileName = $"{outputFileName}.app_funcs.c";
        var appRegsOutputFileName = $"{outputFileName}.app_ios_and_regs.h";
        var appRegsImplOutputFileName = $"{outputFileName}.app_ios_and_regs.c";
        try
        {
            TestHelper.AssertExpectedOutput(appCode, appOutputFileName);
            TestHelper.AssertExpectedOutput(appFuncsCode, appFuncsOutputFileName);
            TestHelper.AssertExpectedOutput(appFuncsImplCode, appFuncsImplOutputFileName);
            TestHelper.AssertExpectedOutput(appRegsCode, appRegsOutputFileName);
            TestHelper.AssertExpectedOutput(appRegsImplCode, appRegsImplOutputFileName);
        }
        catch (AssertFailedException)
        {
            outputDirectory.Create();
            File.WriteAllText(Path.Combine(outputDirectory.FullName, appOutputFileName), appCode);
            File.WriteAllText(Path.Combine(outputDirectory.FullName, appFuncsOutputFileName), appFuncsCode);
            File.WriteAllText(Path.Combine(outputDirectory.FullName, appFuncsImplOutputFileName), appFuncsImplCode);
            File.WriteAllText(Path.Combine(outputDirectory.FullName, appRegsOutputFileName), appRegsCode);
            File.WriteAllText(Path.Combine(outputDirectory.FullName, appRegsImplOutputFileName), appRegsImplCode);
            throw;
        }
    }
}
