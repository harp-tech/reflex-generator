using System.IO;
using System.Threading.Tasks;
using Mono.TextTemplating;

namespace Harp.Generators;

public class FirmwareGenerator
{
    readonly EmbeddedTemplateGenerator _generator;
    readonly CompiledTemplate _appTemplate;
    readonly CompiledTemplate _appImplTemplate;
    readonly CompiledTemplate _appFuncsTemplate;
    readonly CompiledTemplate _appFuncsImplTemplate;
    readonly CompiledTemplate _appRegsTemplate;
    readonly CompiledTemplate _appRegsImplTemplate;
    readonly CompiledTemplate _interruptsTemplate;

    private FirmwareGenerator(
        EmbeddedTemplateGenerator generator,
        CompiledTemplate appTemplate,
        CompiledTemplate appImplTemplate,
        CompiledTemplate appFuncsTemplate,
        CompiledTemplate appFuncsImplTemplate,
        CompiledTemplate appRegsTemplate,
        CompiledTemplate appRegsImplTemplate,
        CompiledTemplate interruptsTemplate)
    {
        _generator = generator;
        _appTemplate = appTemplate;
        _appImplTemplate = appImplTemplate;
        _appFuncsTemplate = appFuncsTemplate;
        _appFuncsImplTemplate = appFuncsImplTemplate;
        _appRegsTemplate = appRegsTemplate;
        _appRegsImplTemplate = appRegsImplTemplate;
        _interruptsTemplate = interruptsTemplate;
    }

    public static async Task<FirmwareGenerator> Create()
    {
        var generator = new EmbeddedTemplateGenerator();
        var appTemplateContents = EmbeddedTemplateGenerator.GetManifestResourceText("App.tt");
        var appImplTemplateContents = EmbeddedTemplateGenerator.GetManifestResourceText("AppImpl.tt");
        var appFuncsTemplateContents = EmbeddedTemplateGenerator.GetManifestResourceText("AppFuncs.tt");
        var appFuncsImplTemplateContents = EmbeddedTemplateGenerator.GetManifestResourceText("AppFuncsImpl.tt");
        var appRegsTemplateContents = EmbeddedTemplateGenerator.GetManifestResourceText("AppRegs.tt");
        var appRegsImplTemplateContents = EmbeddedTemplateGenerator.GetManifestResourceText("AppRegsImpl.tt");
        var interruptsTemplateContents = EmbeddedTemplateGenerator.GetManifestResourceText("Interrupts.tt");

        var appTemplate = await generator.CompileTemplateAsync(appTemplateContents);
        generator.ThrowExceptionForGeneratorError();

        var appImplTemplate = await generator.CompileTemplateAsync(appImplTemplateContents);
        generator.ThrowExceptionForGeneratorError();

        var appFuncsTemplate = await generator.CompileTemplateAsync(appFuncsTemplateContents);
        generator.ThrowExceptionForGeneratorError();

        var appFuncsImplTemplate = await generator.CompileTemplateAsync(appFuncsImplTemplateContents);
        generator.ThrowExceptionForGeneratorError();

        var appRegsTemplate = await generator.CompileTemplateAsync(appRegsTemplateContents);
        generator.ThrowExceptionForGeneratorError();

        var appRegsImplTemplate = await generator.CompileTemplateAsync(appRegsImplTemplateContents);
        generator.ThrowExceptionForGeneratorError();

        var interruptsTemplate = await generator.CompileTemplateAsync(interruptsTemplateContents);
        generator.ThrowExceptionForGeneratorError();

        return new FirmwareGenerator(
            generator,
            appTemplate,
            appImplTemplate,
            appFuncsTemplate,
            appFuncsImplTemplate,
            appRegsTemplate,
            appRegsImplTemplate,
            interruptsTemplate);
    }

    static string ProcessTemplate(
        EmbeddedTemplateGenerator generator,
        CompiledTemplate template,
        string registerMetadataFileName,
        string iosMetadataFileName = default)
    {
        var session = generator.GetOrCreateSession();
        session["RegisterMetadataPath"] = Path.GetFullPath(Path.Combine("Metadata", registerMetadataFileName));
        if (!string.IsNullOrEmpty(iosMetadataFileName))
            session["IOMetadataPath"] = Path.GetFullPath(Path.Combine("Metadata", iosMetadataFileName));
        return template.Process();
    }

    public FirmwareHeaders GenerateHeaders(string registerMetadataFileName, string iosMetadataFileName) =>
        new(App: ProcessTemplate(_generator, _appTemplate, registerMetadataFileName, iosMetadataFileName),
            AppFuncs: ProcessTemplate(_generator, _appFuncsTemplate, registerMetadataFileName, iosMetadataFileName),
            AppRegs: ProcessTemplate(_generator, _appRegsTemplate, registerMetadataFileName, iosMetadataFileName));

    public FirmwareImplementation GenerateImplementation(string registerMetadataFileName, string iosMetadataFileName) =>
        new(App: ProcessTemplate(_generator, _appImplTemplate, registerMetadataFileName, iosMetadataFileName),
            AppFuncs: ProcessTemplate(_generator, _appFuncsImplTemplate, registerMetadataFileName, iosMetadataFileName),
            AppRegs: ProcessTemplate(_generator, _appRegsImplTemplate, registerMetadataFileName, iosMetadataFileName),
            Interrupts: ProcessTemplate(_generator, _interruptsTemplate, registerMetadataFileName, iosMetadataFileName));
}

public record struct FirmwareHeaders(string App, string AppFuncs, string AppRegs)
{
    public const string AppFileName = "app.h";
    public const string AppFuncsFileName = "app_funcs.h";
    public const string AppRegsFileName = "app_ios_and_regs.h";
}

public record struct FirmwareImplementation(string App, string AppFuncs, string AppRegs, string Interrupts)
{
    public const string AppFileName = "app.c";
    public const string AppFuncsFileName = "app_funcs.c";
    public const string AppRegsFileName = "app_ios_and_regs.c";
    public const string InterruptsFileName = "interrupts.c";
}
