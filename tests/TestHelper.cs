using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.TextTemplating;

namespace Interface.Tests;

static class TestHelper
{
    public static Stream GetManifestResourceStream(string name)
    {
        var qualifierType = typeof(TestHelper);
        var embeddedWorkflowStream = qualifierType.Namespace + "." + name;
        return qualifierType.Assembly.GetManifestResourceStream(embeddedWorkflowStream)!;
    }

    public static string GetManifestResourceText(string name)
    {
        using var resourceStream = GetManifestResourceStream(name);
        if (resourceStream is null)
            return string.Empty;

        using var resourceReader = new StreamReader(resourceStream);
        return resourceReader.ReadToEnd();
    }

    public static void AssertNoGeneratorErrors(TemplateGenerator generator)
    {
        if (generator.Errors.HasErrors)
        {
            Assert.Fail(GetGeneratorErrorMessage(generator));
        }
    }

    public static void AssertExpectedOutput(string actual, string outputFileName)
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

    public static string GetGeneratorErrorMessage(TemplateGenerator generator)
    {
        if (!generator.Errors.HasErrors)
            return string.Empty;

        var stringBuilder = new StringBuilder();
        for (int i = 0; i < generator.Errors.Count; i++)
        {
            var error = generator.Errors[i];
            stringBuilder.AppendLine(
                $"({error.ErrorNumber}) {error.ErrorText} in {error.FileName}:line {error.Line}"
            );
        }

        return stringBuilder.ToString();
    }
}
