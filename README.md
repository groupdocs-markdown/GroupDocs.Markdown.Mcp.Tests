# GroupDocs.Markdown.Mcp.Tests

Integration tests for the [`GroupDocs.Markdown.Mcp`](https://www.nuget.org/packages/GroupDocs.Markdown.Mcp)
NuGet package — an MCP server that exposes
[GroupDocs.Markdown](https://products.groupdocs.com/markdown) as AI-callable tools
for converting documents to Markdown.

This repository validates the **published** NuGet artifact end-to-end: it
launches the server via `dnx`, connects as an MCP client, and exercises every
advertised tool. It also doubles as a copy-pasteable set of example configs
and user-facing how-to guides for every deployment channel.

## Documentation

- [how-to/](how-to/) — step-by-step guides for every deployment channel
  ([NuGet](how-to/01-install-from-nuget.md),
  [Docker](how-to/02-run-via-docker.md),
  [MCP registry](how-to/03-verify-mcp-registry.md),
  [Claude Desktop](how-to/04-use-with-claude-desktop.md),
  [VS Code / Copilot](how-to/05-use-with-vscode-copilot.md),
  [running the tests](how-to/06-run-integration-tests.md)).
- [examples/](examples/) — ready-to-paste `claude-desktop.json`,
  `vscode-mcp.json`, and `docker-compose.yml`.
- [AGENTS.md](AGENTS.md) — orientation for AI coding agents working in this repo.
- [llms.txt](llms.txt) — machine-readable summary for LLM tooling.
- [changelog/](changelog/) — one entry per change set (see
  [changelog/README.md](changelog/README.md) for format).

## What gets tested

| Area | Covered by |
|---|---|
| Package installs and starts via `dnx` | [McpServerFixture](src/GroupDocs.Markdown.Mcp.Tests/Fixtures/McpServerFixture.cs) |
| MCP handshake, server info, exactly 2 tools | [ToolDiscoveryTests](src/GroupDocs.Markdown.Mcp.Tests/ToolDiscoveryTests.cs) |
| `ConvertToMarkdown` — DOCX / PDF / EPUB / XLSX, image modes, passwords | [ConvertToMarkdownTests](src/GroupDocs.Markdown.Mcp.Tests/ConvertToMarkdownTests.cs) |
| `GetDocumentInfo` — JSON shape, per-format expectations | [GetDocumentInfoTests](src/GroupDocs.Markdown.Mcp.Tests/GetDocumentInfoTests.cs) |
| Unknown / corrupted files, password parameter | [ErrorHandlingTests](src/GroupDocs.Markdown.Mcp.Tests/ErrorHandlingTests.cs) |

## Running locally

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
dotnet test
```

Test a specific published version:

```bash
dotnet test -p:McpPackageVersion=26.7.0
# or
MCP_PACKAGE_VERSION=26.7.0 dotnet test
```

The first run downloads the NuGet package — subsequent runs are cached.

## CI

[.github/workflows/integration.yml](.github/workflows/integration.yml) runs on:

- Every push / PR.
- Nightly cron — catches regressions in nuget.org, the dnx shim, or the .NET runtime.
- `workflow_dispatch` with a `package_version` input — manual smoke of any version.
- `repository_dispatch` (`nuget-published`) — fires from the main repo's publish pipeline
  so every release is smoke-tested against live nuget.org. See
  [Release smoke hook](#release-smoke-hook).

Matrix: `ubuntu-latest`, `windows-latest`, `macos-latest`. No native-dependency
install step is needed — GroupDocs.Markdown uses a self-contained SkiaSharp
native asset and no System.Drawing.

## Release smoke hook

To auto-verify each release, add this step to the main repo's publish workflow
after the `dotnet nuget push` step:

```yaml
- name: Dispatch smoke tests
  env:
    GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
  run: |
    gh api repos/groupdocs-markdown/GroupDocs.Markdown.Mcp.Tests/dispatches \
      -f event_type=nuget-published \
      -f 'client_payload[package_version]=${{ steps.version.outputs.version }}'
```

## Evaluation vs licensed mode

The whole suite is designed to pass **unlicensed** — GroupDocs.Markdown's
`Convert()` succeeds in evaluation mode (output may be limited and carries an
evaluation notice, but the call does not throw). A license only removes those
limits:

- **Unset (default):** every test runs; conversions succeed with possible
  evaluation limits.
- **Set (`GROUPDOCS_LICENSE_PATH`):** conversions run unrestricted.

For CI, store a base64-encoded `.lic` file as repo secret `GROUPDOCS_LICENSE`
— the workflow decodes it into `$RUNNER_TEMP` and exports
`GROUPDOCS_LICENSE_PATH` automatically. It is optional.

## Fixture documents

Real document fixtures live under [Files/](Files/) — sourced from the upstream
GroupDocs.Markdown-for-.NET examples repo (see [Files/README.md](Files/README.md)
for provenance). The project auto-copies everything in `Files/` to the test
output, and the fixture stages them into the MCP server's storage folder.

## Using this repo as a starter

Copy configs from [examples/](examples/):

- [claude-desktop.json](examples/claude-desktop.json) — Claude Desktop MCP server config.
- [vscode-mcp.json](examples/vscode-mcp.json) — VS Code / GitHub Copilot.
- [docker-compose.yml](examples/docker-compose.yml) — containerized deployment.

## License

MIT — see [LICENSE](LICENSE).
