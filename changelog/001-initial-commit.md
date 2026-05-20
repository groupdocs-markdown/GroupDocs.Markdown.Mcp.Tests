---
id: 001
date: 2026-05-19
package-under-test: 26.5.0
type: feature
---

# Initial integration test suite for GroupDocs.Markdown.Mcp

## What changed

- xUnit test project targeting `net10.0`, referencing only the published
  `ModelContextProtocol` 1.1.0 NuGet — no project reference to the server source.
- `McpServerFixture` launches the published `GroupDocs.Markdown.Mcp@26.5.0`
  package via `dnx` as a child process, wires an MCP stdio client, and seeds a
  temporary storage folder with sample documents.
- `SampleDocuments` stages five real document fixtures from `Files/` (sourced
  from the upstream GroupDocs.Markdown-for-.NET examples repo — see
  `Files/README.md`):
  - `business-plan.docx`, `business-plan.pdf`, `business-plan.epub`,
    `cost-analysis.xlsx` — real conversion inputs.
  - `protected.docx` — password-protected (`secret`) for the `password` parameter.
- Four test classes, ~20 tests total:
  - `ToolDiscoveryTests` — server info, `tools/list` exposes exactly two tools
    (`convert_to_markdown`, `get_document_info`), input-schema validation.
  - `ConvertToMarkdownTests` — converts DOCX / PDF / EPUB / XLSX, verifies a
    saved `.md`, `images=skip` omits base64 data URIs, password-protected DOCX
    with/without password.
  - `GetDocumentInfoTests` — JSON shape (`fileFormat`, `pageCount`,
    `isEncrypted`, …), per-format expectations, password-protected DOCX.
  - `ErrorHandlingTests` — unknown file, corrupted bytes, password parameter.
- GitHub Actions workflow `.github/workflows/integration.yml`:
  - Matrix: `ubuntu-latest`, `windows-latest`, `macos-latest`.
  - Triggers: push, PR, nightly cron, `workflow_dispatch` (with `package_version`
    input), `repository_dispatch` (`nuget-published` event for release smoke).
  - No native-dependency install step — GroupDocs.Markdown uses a self-contained
    SkiaSharp native asset and no System.Drawing, so the bare runners need
    nothing extra.
  - Optional `GROUPDOCS_LICENSE` repo secret auto-decoded into `$RUNNER_TEMP` —
    the suite is designed to pass fully unlicensed; a license only removes the
    evaluation notice / output limits.
- `examples/` — ready-to-use `claude-desktop.json`, `vscode-mcp.json`,
  `docker-compose.yml` copy-paste configs.
- `AGENTS.md` + `llms.txt` for AI coding agent orientation.
- `how-to/` guides covering every deployment channel (NuGet via dnx / dotnet
  tool, Docker, MCP registry, Claude Desktop, VS Code / GitHub Copilot, plus
  running this test suite).

## Why

Closes the release-validation gap: the main repo's unit tests mock
`IFileResolver` / `ILicenseManager` and validate tool logic, but nothing
previously exercised the **shipped** NuGet end-to-end. Every release now has a
cross-platform smoke check against live nuget.org before users hit it.

## Tools NOT exposed (and why)

The main server intentionally does NOT expose a `compose_from_markdown` tool —
`MarkdownConverter.FromMarkdownString` throws `NotImplementedException` in
GroupDocs.Markdown 26.3.0 (the latest stable on NuGet). Rather than ship a tool
whose only response is an error, the surface is reduced to two tools. This
Tests repo therefore has no `ComposeFromMarkdownTests` class.

When the engine ships reverse conversion, add `ComposeFromMarkdownTests`,
re-add `ComposeFromMarkdown => Resolve("compose")` to `ToolCatalog`, and bump
the `ListTools_*` assertion in `ToolDiscoveryTests` from 2 to 3.

## Migration / impact

First release of this repository — no migration.
