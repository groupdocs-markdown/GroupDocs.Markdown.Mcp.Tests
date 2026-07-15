---
id: 002
date: 2026-07-15
package-under-test: 26.7.0
type: chore
---

# Track GroupDocs.Markdown.Mcp 26.7.0 + add Cursor how-to

## What changed
- Default package-under-test bumped `26.5.0 → 26.7.0`:
  `Directory.Build.props <McpPackageVersion>`, both defaults in
  `.github/workflows/integration.yml`, and all `@<version>` / `:tag` doc pins
  across `how-to/*`, `examples/*`, `docker-scripts/README.md`, `AGENTS.md`,
  `README.md`, and `llms.txt`.
- New `how-to/07-use-with-cursor.md` — connects the server to Cursor's Agent
  (uses the `mcpServers` key; documents the Windows `dnx` SSL/timeout workaround
  via a full `dotnet.exe` path + cached DLL, and the Docker route).
- New `examples/cursor-mcp.json` — copy-paste Cursor config pinned to `@26.7.0`.
- `ToolDiscoveryTests` still asserts exactly **2** tools (`convert_to_markdown`,
  `get_document_info`) — the surface is unchanged; only the pinned version moved.

## Why
Keeps the integration suite pointed at the current release (26.7.0) and adds
Cursor to the documented client matrix, matching the cross-product MCP standard.

## Migration / impact
None — same two tools, same fixtures. Override the version on the CLI
(`dotnet test -p:McpPackageVersion=<v>`) to validate a different published build.
