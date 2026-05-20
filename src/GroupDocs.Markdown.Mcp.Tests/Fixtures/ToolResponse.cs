using System.Text.Json;
using ModelContextProtocol.Protocol;

namespace GroupDocs.Markdown.Mcp.IntegrationTests.Fixtures;

/// Helpers for reading `CallToolResult` contents. GroupDocs.Markdown.Mcp tools
/// return a single `TextContentBlock`, sometimes carrying a JSON document
/// (GetDocumentInfo) and sometimes a plain message (ConvertToMarkdown).
internal static class ToolResponse
{
    public static string Text(CallToolResult result)
    {
        var block = result.Content
            .OfType<TextContentBlock>()
            .FirstOrDefault()
            ?? throw new InvalidOperationException(
                "Tool response contained no TextContentBlock.");

        return block.Text ?? string.Empty;
    }

    public static JsonElement Json(CallToolResult result)
    {
        var text = Text(result);
        try
        {
            return JsonDocument.Parse(text).RootElement.Clone();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Tool response was not valid JSON. Body:\n{text}", ex);
        }
    }
}
