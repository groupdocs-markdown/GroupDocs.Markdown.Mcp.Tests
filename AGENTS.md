# AGENTS.md — Guide for AI coding agents

Brief orientation for AI coding agents (Claude Code, Copilot, Cursor, Aider, Amp, Codex) working in this repository.

## What this repo is

**Integration tests** for the [`GroupDocs.Markdown.Mcp`](https://www.nuget.org/packages/GroupDocs.Markdown.Mcp) NuGet package — an MCP server that exposes GroupDocs.Markdown for .NET as AI-callable tools for converting documents to Markdown.

This repo is **not** the server itself. The server lives at [groupdocs-markdown/GroupDocs.Markdown.Mcp](https://github.com/groupdocs-markdown/GroupDocs.Markdown.Mcp). This repo:

1. Consumes only the **published** NuGet artifact (no project references).
2. Launches the server via `dnx`, connects as an MCP stdio client, and exercises every advertised tool.
3. Doubles as a copy-pasteable set of example configs and how-to guides for all deployment channels (NuGet, Docker, MCP registry, Claude Desktop, VS Code).

## Folder layout

```
src/GroupDocs.Markdown.Mcp.Tests/
  Fixtures/
    McpServerFixture.cs          ← launches dnx child process, wires stdio MCP client
    SampleDocuments.cs           ← stages real Files/ fixtures + builds a synthetic sample.md
    ToolCatalog.cs               ← keyword-based tool name resolution (convert/compose/document_info)
    ToolResponse.cs              ← CallToolResult text/JSON extraction
    CommandResolver.cs           ← cross-platform dnx.cmd resolution on Windows
    PackageVersion.cs            ← pulls version from env / assembly metadata / default
  ToolDiscoveryTests.cs          ← handshake, tools/list (exactly 2), schema validation
  ConvertToMarkdownTests.cs      ← DOCX / PDF / EPUB / XLSX conversion, image modes, passwords
  GetDocumentInfoTests.cs        ← JSON shape, per-format expectations
  ErrorHandlingTests.cs          ← unknown file, corrupted bytes, password parameter
  GroupDocs.Markdown.Mcp.Tests.csproj
.github/workflows/integration.yml  ← matrix × 3 OS, nightly cron, release-smoke dispatch
changelog/                         ← one MD file per change (NNN-slug.md)
how-to/                            ← user-facing guides for every deployment channel
examples/                          ← claude-desktop.json, vscode-mcp.json, docker-compose.yml
Files/                             ← real document fixtures (see Files/README.md); copied to test output
Directory.Build.props              ← McpPackageVersion property (overridable)
global.json                        ← pinned to .NET 10.0.100
```

## What gets tested

| Area | Covered by |
|---|---|
| Package installs and starts via `dnx` | `McpServerFixture` |
| MCP handshake, server info, exactly 2 tools | `ToolDiscoveryTests` |
| `convert_to_markdown` — DOCX / PDF / EPUB / XLSX, image modes, passwords | `ConvertToMarkdownTests` |
| `get_document_info` — JSON shape, per-format expectations | `GetDocumentInfoTests` |
| Unknown / corrupted files, password parameter | `ErrorHandlingTests` |

## Commands you can run

```bash
# Restore + build
dotnet restore
dotnet build -c Release

# Run all tests against the default package version (26.7.0)
dotnet test -c Release

# Run against a specific published version
dotnet test -c Release -p:McpPackageVersion=26.7.0
# or
MCP_PACKAGE_VERSION=26.7.0 dotnet test -c Release

# Run just the discovery suite (fastest — no tool invocations)
dotnet test -c Release --filter "FullyQualifiedName~ToolDiscovery"
```

## Key design decisions

1. **Keyword-based tool resolution.** `ToolCatalog.Resolve("convert")` picks the tool whose name contains "convert" (case-insensitive). The MCP C# SDK converts `[McpServerTool]` method names to `snake_case` — so the wire names are `convert_to_markdown` and `get_document_info`. Each resolver keyword is a snake_case substring of its wire name. Tests stay robust if that convention changes.

2. **Real fixtures only.** `Files/` holds five real documents (DOCX / PDF / EPUB / XLSX / password-protected DOCX) sourced from the upstream examples repo — the csproj auto-copies them to the test output. No synthetic fixtures: every assertion exercises a real document the engine actually parses.

3. **Unlicensed by design.** GroupDocs.Markdown's `Convert()` succeeds in evaluation mode (limited output + an evaluation notice, but no exception). The whole suite passes unlicensed; a `GROUPDOCS_LICENSE_PATH` only removes the limits. Never add a `GROUPDOCS_LICENSE` requirement to make CI green.

4. **No `compose_from_markdown` tool.** Reverse Markdown→document composition is not yet implemented in the GroupDocs.Markdown engine (`MarkdownConverter.FromMarkdownString` throws `NotImplementedException` in 26.3.0). The server does not advertise the reverse direction; this Tests repo therefore has no `ComposeFromMarkdownTests`. When the engine ships reverse conversion, add the test class and bump the count in `ToolDiscoveryTests` (2 → 3).

5. **No project references to the server.** The csproj only references `ModelContextProtocol` 1.1.0. If the server source breaks in the sibling repo, these tests still pass — they validate the shipped NuGet artifact.

## House rules

1. **Changelog entries required** — any PR that changes behaviour adds `changelog/NNN-slug.md` (schema in `changelog/README.md`).
2. **How-to guides track deployment reality** — if the main repo publishes a new channel (e.g. new Docker registry), add a guide under `how-to/` *and* update `README.md`.
3. **Version bumps flow through `Directory.Build.props`** — `<McpPackageVersion>` is the single source of truth for "what version are we testing." CI overrides it via env var / workflow input.
4. **Tests must not require the main repo's source.** If a test needs a server-side change, file an issue there — don't work around it here.
5. **Target framework is `net10.0` only** — required by `dnx` and the MCP SDK.

## Release smoke hook

The main repo's `publish_prod.yml` should fire a `repository_dispatch` with `event_type=nuget-published` after `dotnet nuget push` succeeds. The workflow in `.github/workflows/integration.yml` consumes `client_payload.package_version` and runs the matrix against the just-published version. This closes the loop: publish → smoke-test live nuget.org → fail loud if broken.

## What NOT to change

- Do not add a `ProjectReference` to the main repo's `GroupDocs.Markdown.Mcp.csproj`. This repo exists to test the shipped NuGet, not the source.
- Do not hardcode tool names as string literals (`"convert_to_markdown"`). Use `ToolCatalog.ConvertToMarkdown.Name` etc.
- Do not commit real license files. License goes through the `GROUPDOCS_LICENSE` CI secret. Fixtures in `Files/` come from the upstream GroupDocs.Markdown-for-.NET examples repo (see `Files/README.md`).
