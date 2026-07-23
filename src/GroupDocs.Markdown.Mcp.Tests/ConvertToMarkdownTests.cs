using GroupDocs.Markdown.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Markdown.Mcp.IntegrationTests;

/// ConvertToMarkdown runs unlicensed in CI by design. GroupDocs.Markdown's
/// Convert() succeeds in evaluation mode — output may be limited and carries an
/// evaluation notice, but the call does not throw. Tests therefore assert the
/// response is not a tool-level failure (the tool prefixes real failures with
/// "Conversion to Markdown failed for") rather than asserting exact content.
public class ConvertToMarkdownTests : IClassFixture<McpServerFixture>
{
    private const string FailurePrefix = "Conversion to Markdown failed for";

    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public ConvertToMarkdownTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task ConvertToMarkdown_BusinessPlanDocx_ProducesMarkdownFile()
    {
        if (!FixtureExists(SampleDocuments.BusinessPlanDocx)) return;

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.ConvertToMarkdown.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BusinessPlanDocx },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false, $"Tool reported an error: {body}");
        Assert.DoesNotContain(FailurePrefix, body);

        var savedMd = Path.Combine(_fixture.StoragePath, "business-plan.md");
        Assert.True(File.Exists(savedMd), $"Expected converted Markdown at '{savedMd}'.");
        Assert.True(new FileInfo(savedMd).Length > 0, "Converted Markdown file is empty.");
    }

    public static IEnumerable<object[]> RealSampleData() => new[]
    {
        new object[] { SampleDocuments.BusinessPlanDocx },
        new object[] { SampleDocuments.BusinessPlanPdf },
        new object[] { SampleDocuments.BusinessPlanEpub },
        new object[] { SampleDocuments.CostAnalysisXlsx },
    };

    [Theory]
    [MemberData(nameof(RealSampleData))]
    public async Task ConvertToMarkdown_RealSample_DoesNotReportFailure(string fileName)
    {
        if (!FixtureExists(fileName)) return;

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.ConvertToMarkdown.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = fileName },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false, $"Tool reported an error converting '{fileName}': {body}");
        Assert.DoesNotContain(FailurePrefix, body);
        Assert.False(string.IsNullOrWhiteSpace(body));
    }

    [Fact]
    public async Task ConvertToMarkdown_SkipImages_OmitsBase64DataUris()
    {
        if (!FixtureExists(SampleDocuments.BusinessPlanPdf)) return;

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.ConvertToMarkdown.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BusinessPlanPdf },
                ["images"] = "skip",
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false, $"Tool reported an error: {body}");
        Assert.DoesNotContain(FailurePrefix, body);
        Assert.DoesNotContain("data:image", body);
    }

    [Fact]
    public async Task ConvertToMarkdown_ProtectedDocx_WithPassword_Succeeds()
    {
        if (!FixtureExists(SampleDocuments.ProtectedDocx)) return;

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.ConvertToMarkdown.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.ProtectedDocx },
                ["password"] = SampleDocuments.ProtectedDocumentPassword,
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false, $"Tool reported an error: {body}");
        Assert.DoesNotContain(FailurePrefix, body);
    }

    [Fact]
    public async Task ConvertToMarkdown_ProtectedDocx_WithoutPassword_ReturnsGracefulError()
    {
        if (!FixtureExists(SampleDocuments.ProtectedDocx)) return;

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.ConvertToMarkdown.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.ProtectedDocx },
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        // The tool catches the engine's protected-document exception and surfaces
        // it as a descriptive string (Pitfall #18) — the server must stay up.
        Assert.False(string.IsNullOrWhiteSpace(body));
        var listAfter = await _fixture.Client.ListToolsAsync();
        Assert.NotEmpty(listAfter);
    }

    private bool FixtureExists(string fileName)
    {
        var present = File.Exists(Path.Combine(_fixture.StoragePath, fileName));
        if (!present)
            _output.WriteLine($"Fixture '{fileName}' not present in storage — skipping.");
        return present;
    }
}
