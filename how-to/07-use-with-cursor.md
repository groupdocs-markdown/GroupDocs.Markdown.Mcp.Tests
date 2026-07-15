# Use with Cursor

Connect the MCP server to [Cursor](https://cursor.com) so you can ask its Agent
to convert documents to Markdown or inspect document info.

## Prerequisites

- Cursor installed and updated (MCP support is in **Settings → Tools & MCP**).
- One of:
  - [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (for the `dnx` route — recommended), or
  - [Docker](https://www.docker.com/products/docker-desktop) (for the container route).

## Config file location

Cursor uses the **`mcpServers`** key (like Claude Desktop) — **not** `servers`
as in VS Code. Two scopes:

| Scope | Path |
|---|---|
| Global (all projects) | `~/.cursor/mcp.json` (macOS/Linux) · `%USERPROFILE%\.cursor\mcp.json` (Windows) |
| Project-only | `.cursor/mcp.json` in the workspace root |

Create the file if it doesn't exist.

## Option A — dnx (recommended)

```json
{
  "mcpServers": {
    "groupdocs-markdown": {
      "command": "dnx",
      "args": ["GroupDocs.Markdown.Mcp@26.7.0", "--yes"],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "/Users/you/Documents"
      }
    }
  }
}
```

- Replace the storage path with an **absolute path** to the folder Cursor should
  operate on. On Windows use `"C:\\Users\\you\\Documents"` (double-escaped) or
  forward slashes.
- Omit `@26.7.0` to always pull the latest stable.
- Add `"GROUPDOCS_LICENSE_PATH": "…/GroupDocs.Total.lic"` to `env` to remove the
  evaluation-mode limits/notice from converted Markdown. Both tools still run in
  evaluation mode — `GetDocumentInfo` is unaffected, and `ConvertToMarkdown`
  produces output (only limited / annotated without a license).

Copy-paste starter: [examples/cursor-mcp.json](../examples/cursor-mcp.json).

## Option B — Windows: full path to `dotnet.exe` (SSL / timeout workaround)

On Windows, Cursor launching `dnx` can fail with an **SSL / ~30 s timeout** on
the first package probe. Bypass `dnx` by running the already-cached tool DLL
directly with `dotnet.exe`:

```json
{
  "mcpServers": {
    "groupdocs-markdown": {
      "command": "C:\\Program Files\\dotnet\\dotnet.exe",
      "args": [
        "C:\\Users\\you\\.nuget\\packages\\groupdocs.markdown.mcp\\26.7.0\\tools\\net10.0\\any\\GroupDocs.Markdown.Mcp.dll"
      ],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "C:\\Users\\you\\Documents"
      }
    }
  }
}
```

Populate the cache first by running `dnx GroupDocs.Markdown.Mcp@26.7.0 --yes` once
in a terminal, then point `args[0]` at the resulting
`…\.nuget\packages\groupdocs.markdown.mcp\<version>\tools\net10.0\any\GroupDocs.Markdown.Mcp.dll`.

## Option C — Docker

```json
{
  "mcpServers": {
    "groupdocs-markdown": {
      "command": "docker",
      "args": [
        "run", "--rm", "-i",
        "-v", "/Users/you/Documents:/data",
        "ghcr.io/groupdocs-markdown/markdown-net-mcp:26.7.0"
      ]
    }
  }
}
```

## Reload and verify

1. Save `mcp.json`.
2. **Settings → Tools & MCP** → find `groupdocs-markdown` → toggle it on (or hit
   the reload icon). A green dot means it connected.
3. Expand it — you should see `convert_to_markdown` and `get_document_info`.

## Example prompts (Agent mode)

```
Convert report.pdf to Markdown.

Export business-plan.docx as Markdown with images embedded as base64.

Convert pages 1-3 of contract.pdf to Markdown, text only — skip images.

How many pages does business-plan.pdf have?
```

The Agent will call `convert_to_markdown` / `get_document_info` and compose its
answer from the results.

## Troubleshooting

| Symptom | Fix |
|---|---|
| Server greyed out / won't start on Windows | `dnx` SSL/timeout — use **Option B** (full `dotnet.exe` path + cached DLL). |
| Server not listed | JSON typo — Cursor silently drops unparseable entries. Validate with `jq . mcp.json`. Confirm the key is `mcpServers`, not `servers`. |
| Converted Markdown carries an evaluation notice / looks truncated | Expected in evaluation mode. Add `GROUPDOCS_LICENSE_PATH` to `env` for unrestricted output. |
| No extra native packages needed | GroupDocs.Markdown renders image-bearing documents through a self-contained SkiaSharp native asset — no `libgdiplus`/`apt`/`brew` setup required on any platform. |

## Next steps

- [04 — Use with Claude Desktop](04-use-with-claude-desktop.md)
- [05 — Use with VS Code / Copilot](05-use-with-vscode-copilot.md)
