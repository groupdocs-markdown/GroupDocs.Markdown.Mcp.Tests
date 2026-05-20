using ModelContextProtocol.Client;

namespace GroupDocs.Markdown.Mcp.IntegrationTests.Fixtures;

/// Resolves tool names by keyword. The server-side [McpServerTool] attribute
/// derives the wire name from the C# method name converted to snake_case
/// (ConvertToMarkdown → convert_to_markdown, GetDocumentInfo → get_document_info).
/// Keyword-based resolution keeps tests robust against future renames / casing
/// convention changes. Each keyword below MUST be a snake_case substring of the
/// matching tool's wire name.
internal sealed class ToolCatalog
{
    private readonly IReadOnlyList<McpClientTool> _tools;

    private ToolCatalog(IReadOnlyList<McpClientTool> tools) => _tools = tools;

    public static async Task<ToolCatalog> LoadAsync(McpClient client, CancellationToken ct = default)
    {
        var tools = await client.ListToolsAsync(cancellationToken: ct);
        return new ToolCatalog(tools.ToList());
    }

    public IReadOnlyList<McpClientTool> All => _tools;

    public McpClientTool ConvertToMarkdown => Resolve("convert");
    public McpClientTool GetDocumentInfo => Resolve("document_info");

    private McpClientTool Resolve(string keyword) =>
        _tools.FirstOrDefault(t => t.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                $"No tool with name containing '{keyword}'. Found: {string.Join(", ", _tools.Select(t => t.Name))}");
}
