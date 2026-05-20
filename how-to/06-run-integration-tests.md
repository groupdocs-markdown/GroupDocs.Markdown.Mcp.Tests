# Run the integration tests

This repo's test suite validates the **published** `GroupDocs.Markdown.Mcp`
NuGet package end-to-end — it spawns the server via `dnx`, connects as an MCP
client, and exercises every advertised tool. Useful when you want to confirm a
release is healthy before promoting it, or gate CI on live smoke checks.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Network access to nuget.org (the first run downloads the package)
- Optional: a GroupDocs license file (the suite passes unlicensed — a license
  only removes the evaluation notice / limits from converted Markdown)

## Run locally

```bash
# All tests against the default pinned version (26.5.0)
dotnet test -c Release
```

## Run against a different published version

```bash
# Via MSBuild property
dotnet test -c Release -p:McpPackageVersion=26.5.0

# Or via env var
MCP_PACKAGE_VERSION=26.5.0 dotnet test -c Release
```

Version resolution order (highest wins):

1. `MCP_PACKAGE_VERSION` environment variable
2. `McpPackageVersion` MSBuild property → baked into assembly metadata
3. Default: `26.5.0`

## Licensed vs unlicensed

The whole suite is designed to pass **unlicensed**. GroupDocs.Markdown's
`Convert()` succeeds in evaluation mode — output may be limited and carries an
evaluation notice, but the call does not throw. A license only removes those
limits; no test is gated on it. To run with a license:

```bash
export GROUPDOCS_LICENSE_PATH=/absolute/path/to/GroupDocs.Total.lic
dotnet test -c Release
```

The license path is forwarded into the server child process by
`McpServerFixture`.

## Run a subset

```bash
# Only discovery (fastest — no tool invocations after handshake)
dotnet test -c Release --filter "FullyQualifiedName~ToolDiscovery"

# Only conversion tests
dotnet test -c Release --filter "FullyQualifiedName~ConvertToMarkdown"

# Only error-handling tests
dotnet test -c Release --filter "FullyQualifiedName~ErrorHandling"
```

## Expected output

```
Passed  ToolDiscoveryTests.ServerInfo_AdvertisesGroupDocsMarkdownMcp
Passed  ToolDiscoveryTests.ListTools_ExposesConvertAndGetDocumentInfo
Passed  ToolDiscoveryTests.AllTools_HaveNonEmptyDescriptionAndInputSchema
Passed  ConvertToMarkdownTests.ConvertToMarkdown_BusinessPlanDocx_ProducesMarkdownFile
Passed  ConvertToMarkdownTests.ConvertToMarkdown_RealSample_DoesNotReportFailure (×4 formats)
Passed  ConvertToMarkdownTests.ConvertToMarkdown_SkipImages_OmitsBase64DataUris
Passed  ConvertToMarkdownTests.ConvertToMarkdown_ProtectedDocx_WithPassword_Succeeds
Passed  ConvertToMarkdownTests.ConvertToMarkdown_ProtectedDocx_WithoutPassword_ReturnsGracefulError
Passed  GetDocumentInfoTests.GetDocumentInfo_BusinessPlanPdf_ReturnsJsonWithFormatAndPageCount
Passed  GetDocumentInfoTests.GetDocumentInfo_RealSample_ReportsExpectedFormat (×4 formats)
Passed  GetDocumentInfoTests.GetDocumentInfo_ProtectedDocx_WithPassword_Succeeds
Passed  ErrorHandlingTests.ConvertToMarkdown_UnknownFile_ReturnsErrorListingAvailableFiles
Passed  ErrorHandlingTests.ConvertToMarkdown_CorruptedFile_DoesNotCrashServer
Passed  ErrorHandlingTests.PasswordParameter_IsAcceptedByTool

Total: 20, Passed: 20, Time: ~20s
```

The first test run is slower (~60s) because `dnx` downloads the package into
the NuGet cache.

> The server intentionally does **not** expose a `compose_from_markdown` tool —
> `MarkdownConverter.FromMarkdownString` throws `NotImplementedException` in
> GroupDocs.Markdown 26.3.0 (the engine version). When the engine ships reverse
> conversion, add `ComposeFromMarkdownTests` and bump `ListTools_*` from 2 → 3.

## Add real-world fixtures

Real document fixtures live under [Files/](../Files/) — sourced from the
upstream GroupDocs.Markdown-for-.NET examples repo (see
[Files/README.md](../Files/README.md)). To test additional format-specific
behaviour:

1. Drop the file into [Files/](../Files/). The csproj's
   `<None Include="..\..\Files\**\*" CopyToOutputDirectory="PreserveNewest" />`
   glob copies it to the test output, which `McpServerFixture` seeds into the
   server's storage path.
2. Add the filename to `SampleDocuments.RealSamples` so the fixture stages it.
3. Add a test referencing it by filename:

```csharp
var response = await _fixture.Client.CallToolAsync(
    catalog.ConvertToMarkdown.Name,
    new Dictionary<string, object?>
    {
        ["file"] = new Dictionary<string, object?> { ["filePath"] = "contract.docx" },
    });
```

## Use in CI

The workflow at [.github/workflows/integration.yml](../.github/workflows/integration.yml)
runs on four triggers:

- **`push` + `pull_request`** — validates repo changes.
- **Nightly cron** (`0 6 * * *` UTC) — catches regressions in nuget.org, `dnx`,
  or the .NET runtime.
- **`workflow_dispatch`** with a `package_version` input — smoke-test any
  published version manually.
- **`repository_dispatch`** (`nuget-published` event) — fires from the main
  repo's publish pipeline after `dotnet nuget push`. Payload:
  `{ "package_version": "x.y.z" }`.

Matrix: `ubuntu-latest`, `windows-latest`, `macos-latest`. No native-dependency
install step is needed — GroupDocs.Markdown uses a self-contained SkiaSharp
native asset and no System.Drawing.

### Wire the release-smoke hook in the server repo

Add this step to the server repo's publish workflow, right after the push step:

```yaml
- name: Dispatch integration tests
  env:
    GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
  run: |
    gh api \
      repos/groupdocs-markdown/GroupDocs.Markdown.Mcp.Tests/dispatches \
      -f event_type=nuget-published \
      -f 'client_payload[package_version]=${{ steps.version.outputs.version }}'
```

The `GITHUB_TOKEN` scope is enough if both repos are in the same org.
Otherwise use a fine-grained PAT with `Contents: write` on the test repo.

### License secret in CI

Store a base64-encoded `.lic` file as the repo secret `GROUPDOCS_LICENSE`.
The workflow decodes it into `$RUNNER_TEMP` and exports `GROUPDOCS_LICENSE_PATH`
— conversions then run unrestricted. It is optional; the suite passes without it.

```bash
# Locally: base64-encode and set the secret
base64 -w0 GroupDocs.Total.lic | gh secret set GROUPDOCS_LICENSE \
  --repo groupdocs-markdown/GroupDocs.Markdown.Mcp.Tests
```

## Debugging failures

### Inspect server stderr

`McpServerFixture` doesn't currently capture the child process's stderr — if a
test fails with a cryptic error, reproduce the call manually:

```bash
mkdir -p /tmp/gd && cp some.pdf /tmp/gd/
(
  echo '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"p","version":"1"}}}'
  echo '{"jsonrpc":"2.0","method":"notifications/initialized"}'
  echo '{"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"convert_to_markdown","arguments":{"file":{"filePath":"some.pdf"}}}}'
  sleep 5
) | GROUPDOCS_MCP_STORAGE_PATH=/tmp/gd dnx GroupDocs.Markdown.Mcp@26.5.0 --yes \
    > stdout.log 2> stderr.log
tail -50 stderr.log
```

The tools also surface the underlying exception in the response text — a real
failure starts with `Conversion to Markdown failed for` /
`Document-info lookup failed for` followed by the exception type and message.

### Verbose test output

```bash
dotnet test -c Release --logger "console;verbosity=detailed"
```

## Troubleshooting

| Symptom | Fix |
|---|---|
| `dnx: command not found` during test | Ensure .NET 10 SDK is installed. On Windows, `CommandResolver` looks for `dnx.cmd`; check it exists at `C:\Program Files\dotnet\dnx.cmd`. |
| First run takes minutes | NuGet download. Subsequent runs hit the cache. |
| Cross-OS flakes in CI | Different line endings in `Files/` fixtures. Commit binary fixtures with binary mode in `.gitattributes`. |

## Next steps

- [03 — MCP registry](03-verify-mcp-registry.md) — cross-check registry state
- [01 — NuGet install](01-install-from-nuget.md) — manual smoke the same way users do
