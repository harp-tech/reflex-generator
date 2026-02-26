using System.Collections.Generic;
using System.IO;

namespace Harp.Generators;

public class FirmwareGenerator
{
    readonly App _appTemplate = new();
    readonly AppImpl _appImplTemplate = new();
    readonly AppFuncs _appFuncsTemplate = new();
    readonly AppFuncsImpl _appFuncsImplTemplate = new();
    readonly AppRegs _appRegsTemplate = new();
    readonly AppRegsImpl _appRegsImplTemplate = new();
    readonly Interrupts _interruptsTemplate = new();

    public FirmwareGenerator(string registerMetadataFileName, string iosMetadataFileName)
    {
        var session = new Dictionary<string, object>
        {
            { "RegisterMetadataPath", Path.GetFullPath(registerMetadataFileName) },
            { "IOMetadataPath", Path.GetFullPath(iosMetadataFileName) }
        };
        _appTemplate.Session = session;
        _appImplTemplate.Session = session;
        _appFuncsTemplate.Session = session;
        _appFuncsImplTemplate.Session = session;
        _appRegsTemplate.Session = session;
        _appRegsImplTemplate.Session = session;
        _interruptsTemplate.Session = session;
        _appTemplate.Initialize();
        _appImplTemplate.Initialize();
        _appFuncsTemplate.Initialize();
        _appFuncsImplTemplate.Initialize();
        _appRegsTemplate.Initialize();
        _appRegsImplTemplate.Initialize();
        _interruptsTemplate.Initialize();
    }

    public FirmwareHeaders GenerateHeaders() =>
        new(App: _appTemplate.TransformText(),
            AppFuncs: _appFuncsTemplate.TransformText(),
            AppRegs: _appRegsTemplate.TransformText());

    public FirmwareImplementation GenerateImplementation() =>
        new(App: _appImplTemplate.TransformText(),
            AppFuncs: _appFuncsImplTemplate.TransformText(),
            AppRegs: _appRegsImplTemplate.TransformText(),
            Interrupts: _interruptsTemplate.TransformText());
}

public record struct FirmwareHeaders(string App, string AppFuncs, string AppRegs)
{
    public const string AppFileName = "app.h";
    public const string AppFuncsFileName = "app_funcs.h";
    public const string AppRegsFileName = "app_ios_and_regs.h";

    public readonly IEnumerable<KeyValuePair<string, string>> GetGeneratedFileContents()
    {
        yield return new(AppFileName, App);
        yield return new(AppFuncsFileName, AppFuncs);
        yield return new(AppRegsFileName, AppRegs);
    }
}

public record struct FirmwareImplementation(string App, string AppFuncs, string AppRegs, string Interrupts)
{
    public const string AppFileName = "app.c";
    public const string AppFuncsFileName = "app_funcs.c";
    public const string AppRegsFileName = "app_ios_and_regs.c";
    public const string InterruptsFileName = "interrupts.c";

    public readonly IEnumerable<KeyValuePair<string, string>> GetGeneratedFileContents()
    {
        yield return new(AppFileName, App);
        yield return new(AppFuncsFileName, AppFuncs);
        yield return new(AppRegsFileName, AppRegs);
        yield return new(InterruptsFileName, Interrupts);
    }
}
