using System;
using System.IO;
using System.Text;
using Mono.TextTemplating;

namespace Harp.Generators;

internal class EmbeddedTemplateGenerator : TemplateGenerator
{
    public EmbeddedTemplateGenerator()
    {
        ReferencePaths.Add(Path.GetDirectoryName(GetType().Assembly.Location));
    }

    static Stream GetManifestResourceStream(string name)
    {
        var qualifierType = typeof(EmbeddedTemplateGenerator);
        var embeddedWorkflowStream = qualifierType.Namespace + "." + name;
        return qualifierType.Assembly.GetManifestResourceStream(embeddedWorkflowStream)!;
    }

    internal static string GetManifestResourceText(string name)
    {
        using var resourceStream = GetManifestResourceStream(name);
        if (resourceStream is null)
            throw new ArgumentException($"The specified manifest resource '{name} could not be found.", name);

        using var resourceReader = new StreamReader(resourceStream);
        return resourceReader.ReadToEnd();
    }

    internal void ThrowExceptionForGeneratorError()
    {
        if (!Errors.HasErrors)
            return;

        var stringBuilder = new StringBuilder();
        for (int i = 0; i < Errors.Count; i++)
        {
            var error = Errors[i];
            stringBuilder.AppendLine(
                $"({error.ErrorNumber}) {error.ErrorText} in {error.FileName}:line {error.Line}"
            );
        }

        var errorMessage = stringBuilder.ToString();
        throw new InvalidOperationException(errorMessage);
    }

    protected override bool LoadIncludeText(string requestFileName, out string content, out string location)
    {
        content = GetManifestResourceText(requestFileName);
        location = string.Empty;
        return true;
    }
}
