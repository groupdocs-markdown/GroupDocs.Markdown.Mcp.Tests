namespace GroupDocs.Markdown.Mcp.IntegrationTests.Fixtures;

/// Stages fixture documents into the MCP server's storage folder.
///
/// Real document fixtures live under the repo's `Files/` folder (sourced from
/// the upstream GroupDocs.Markdown-for-.NET examples repo — see Files/README.md)
/// and are copied into the test storage directory at fixture startup.
internal static class SampleDocuments
{
    // Real document fixtures committed under Files/ — copied from the source
    // folder (env var or csproj-staged copy under bin/) into test storage.
    public const string BusinessPlanDocx = "business-plan.docx";
    public const string BusinessPlanPdf = "business-plan.pdf";
    public const string BusinessPlanEpub = "business-plan.epub";
    public const string CostAnalysisXlsx = "cost-analysis.xlsx";
    public const string ProtectedDocx = "protected.docx";

    /// Password for the protected.docx fixture (from the upstream
    /// LoadAPasswordProtectedDocument example).
    public const string ProtectedDocumentPassword = "secret";

    public static IReadOnlyList<string> RealSamples { get; } = new[]
    {
        BusinessPlanDocx, BusinessPlanPdf, BusinessPlanEpub, CostAnalysisXlsx, ProtectedDocx,
    };

    /// Reserved for future synthetic fixtures. Currently a no-op — all
    /// integration tests use the real documents staged from Files/.
    public static void WriteAll(string directory)
    {
        Directory.CreateDirectory(directory);
    }

    /// Copies real sample files (those in RealSamples) from the resolved source
    /// directory into the test storage directory. Files not present in the source
    /// are skipped — the corresponding tests detect absence and skip themselves.
    public static void CopyRealSamples(string targetDirectory, string? sourceDirectory)
    {
        if (string.IsNullOrEmpty(sourceDirectory) || !Directory.Exists(sourceDirectory))
            return;

        Directory.CreateDirectory(targetDirectory);
        foreach (var name in RealSamples)
        {
            var src = Path.Combine(sourceDirectory, name);
            if (File.Exists(src))
                File.Copy(src, Path.Combine(targetDirectory, name), overwrite: true);
        }
    }

    /// Resolves the source folder containing real sample files. Order:
    ///   1. GROUPDOCS_MCP_SAMPLE_DOCS env var (set by docker-compose mount).
    ///   2. `Files/` next to the test assembly — populated by the csproj
    ///      `<None Include="..\..\Files\**\*">` copy item.
    ///   3. Walk up from the assembly to find the repo's `Files/`.
    public static string? ResolveSourceSampleDocs()
    {
        var env = Environment.GetEnvironmentVariable("GROUPDOCS_MCP_SAMPLE_DOCS");
        if (!string.IsNullOrEmpty(env) && Directory.Exists(env))
            return env;

        var staged = Path.Combine(AppContext.BaseDirectory, "Files");
        if (Directory.Exists(staged))
            return staged;

        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 10 && !string.IsNullOrEmpty(dir); i++)
        {
            var candidate = Path.Combine(dir, "Files");
            if (Directory.Exists(candidate))
                return candidate;
            dir = Path.GetDirectoryName(dir);
        }

        return null;
    }
}
