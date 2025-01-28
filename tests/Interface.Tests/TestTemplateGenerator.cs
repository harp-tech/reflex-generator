using Mono.TextTemplating;

namespace Interface.Tests;

public class TestTemplateGenerator : TemplateGenerator
{
    protected override bool LoadIncludeText(string requestFileName, out string content, out string location)
    {
        content = TestHelper.GetManifestResourceText(requestFileName);
        location = string.Empty;
        return true;
    }
}
