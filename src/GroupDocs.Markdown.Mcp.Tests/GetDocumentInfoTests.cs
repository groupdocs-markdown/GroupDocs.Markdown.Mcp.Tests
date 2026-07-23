using GroupDocs.Markdown.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Markdown.Mcp.IntegrationTests;

/// GetDocumentInfo returns a JSON object — { fileName, fileFormat, pageCount,
/// title, author, isEncrypted }. It only inspects the document (no conversion),
/// so it runs cleanly unlicensed.
public class GetDocumentInfoTests : IClassFixture<McpServerFixture>
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public GetDocumentInfoTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task GetDocumentInfo_BusinessPlanPdf_ReturnsJsonWithFormatAndPageCount()
    {
        if (!FixtureExists(SampleDocuments.BusinessPlanPdf)) return;

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.GetDocumentInfo.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.BusinessPlanPdf },
            });

        Assert.False(response.IsError ?? false, $"Tool reported an error: {ToolResponse.Text(response)}");

        var json = ToolResponse.Json(response);
        _output.WriteLine(json.ToString());

        Assert.Equal("Pdf", json.GetProperty("fileFormat").GetString(), ignoreCase: true);
        Assert.True(json.GetProperty("pageCount").GetInt32() >= 1, "Expected at least one page.");
        Assert.True(json.TryGetProperty("isEncrypted", out _), "Missing 'isEncrypted' field.");
    }

    public static IEnumerable<object[]> RealSampleData() => new[]
    {
        new object[] { SampleDocuments.BusinessPlanDocx, "Docx" },
        new object[] { SampleDocuments.BusinessPlanPdf,  "Pdf" },
        new object[] { SampleDocuments.BusinessPlanEpub, "Epub" },
        new object[] { SampleDocuments.CostAnalysisXlsx, "Xlsx" },
    };

    [Theory]
    [MemberData(nameof(RealSampleData))]
    public async Task GetDocumentInfo_RealSample_ReportsExpectedFormat(string fileName, string expectedFormat)
    {
        if (!FixtureExists(fileName)) return;

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.GetDocumentInfo.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = fileName },
            });

        Assert.False(response.IsError ?? false,
            $"Tool reported an error for '{fileName}': {ToolResponse.Text(response)}");

        var json = ToolResponse.Json(response);
        _output.WriteLine(json.ToString());

        Assert.Equal(expectedFormat, json.GetProperty("fileFormat").GetString(), ignoreCase: true);
        Assert.Equal(fileName, json.GetProperty("fileName").GetString());
    }

    [Fact]
    public async Task GetDocumentInfo_ProtectedDocx_WithPassword_Succeeds()
    {
        if (!FixtureExists(SampleDocuments.ProtectedDocx)) return;

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.GetDocumentInfo.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.ProtectedDocx },
                ["password"] = SampleDocuments.ProtectedDocumentPassword,
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.False(response.IsError ?? false, $"Tool reported an error: {body}");
        Assert.DoesNotContain("Document-info lookup failed for", body);
    }

    private bool FixtureExists(string fileName)
    {
        var present = File.Exists(Path.Combine(_fixture.StoragePath, fileName));
        if (!present)
            _output.WriteLine($"Fixture '{fileName}' not present in storage — skipping.");
        return present;
    }
}
