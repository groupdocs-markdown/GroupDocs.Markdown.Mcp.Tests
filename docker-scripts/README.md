# Docker Scripts — Integration Test Automation

Complete bash script suite for running GroupDocs.Markdown.Mcp integration tests in both local and Docker environments. **Works on Windows (Git Bash/WSL), Linux, and macOS.**

## 📋 Overview

This folder contains production-ready bash scripts for testing the GroupDocs.Markdown.Mcp NuGet package across all scenarios:

- **ToolDiscovery** — MCP server handshake and tool listing (exactly 2 tools)
- **ConvertToMarkdown** — document → Markdown conversion (DOCX / PDF / EPUB / XLSX), image modes, password-protected documents
- **GetDocumentInfo** — JSON document info (format, page count, encryption flag)
- **ErrorHandling** — Unknown files, corrupted bytes, password parameters

## 📁 Files

Numeric prefixes indicate the recommended running sequence.

| File | Purpose |
|------|---------|
| `00_quick-start.sh` | **Cheatsheet** — Copy-paste reference. Not meant to be executed directly. |
| `01_verify-setup.sh` | **Preflight** — Check Docker, .NET SDK, project structure, connectivity. |
| `02_test-all-scenarios.sh` | **Main runner** — Runs the integration suite locally against the published NuGet (via `dnx`). |
| `03_test-docker-compose.sh` | **SDK-container runner** — Runs the suite inside a `dotnet/sdk` container; still uses `dnx` to spawn the MCP server. |
| `04_run-server-with-samples.sh` | **Interactive Docker MCP server** — Launches the published `markdown-net-mcp` container with `Files/` mounted, for ad-hoc smoke testing. |
| `helpers.sh` | Library — shared logging, Docker, .NET utilities (sourced, not run). |
| `README.md` | This file. |

## 🚀 Quick Start

### Prerequisites

**Windows:**
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (includes Docker CLI)
- [Git Bash](https://git-scm.com/download/win) or [WSL2](https://learn.microsoft.com/en-us/windows/wsl/install)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (for local testing)

**Linux:**
```bash
sudo apt-get install docker.io dotnet-sdk-10.0 git
```

**macOS:**
```bash
brew install docker dotnet git
# Also install Docker Desktop or Docker for Mac
```

### Basic Usage

```bash
# Run all tests against the LATEST nuget.org release (fastest — local execution, default)
./02_test-all-scenarios.sh

# Run tests in Docker containers (also defaults to latest)
./03_test-docker-compose.sh

# Try the published Docker MCP image with the repo's Files/ fixtures mounted
./04_run-server-with-samples.sh

# Run a specific test scenario
./02_test-all-scenarios.sh --filter ConvertToMarkdown

# Pin to a specific package version (reproducible / CI runs)
./02_test-all-scenarios.sh --version 26.5.0

# Use a license to remove evaluation limits
./02_test-all-scenarios.sh --license /path/to/GroupDocs.Total.lic
```

## 📖 Detailed Usage

### `02_test-all-scenarios.sh` — Local Test Runner

**Best for:** Local development, quick validation, CI/CD pipelines

```bash
Usage:
  ./02_test-all-scenarios.sh [OPTIONS]

Options:
  --version VERSION       Test specific package version (default: latest)
                          Use "latest" or omit to track nuget.org's most recent
                          stable release. Pin (e.g. "26.5.0") for reproducible
                          / shared / CI runs.
  --filter PATTERN        Run only tests matching pattern
  --no-build              Skip local .NET build, use pre-built
  --license PATH          Path to GroupDocs license file
  --help                  Show help message

Examples:
  # All tests against latest stable (default)
  ./02_test-all-scenarios.sh

  # Only ConvertToMarkdown tests
  ./02_test-all-scenarios.sh --filter ConvertToMarkdown

  # Pin to 26.5.0 with a custom license
  ./02_test-all-scenarios.sh --version 26.5.0 --license /path/to/lic

  # Skip rebuild (use cached binaries)
  ./02_test-all-scenarios.sh --no-build --filter ToolDiscovery
```

**What it does:**
1. ✅ Validates Docker, .NET SDK, and project structure
2. ✅ Builds the test project in Release mode
3. ✅ Runs all integration tests with specified filters
4. ✅ Reports results in console + JSON format

**Output:**
```
Passed  ToolDiscoveryTests.ServerInfo_AdvertisesGroupDocsMarkdownMcp
Passed  ConvertToMarkdownTests.ConvertToMarkdown_BusinessPlanDocx_ProducesMarkdownFile
Passed  ErrorHandlingTests.ConvertToMarkdown_UnknownFile_ReturnsErrorListingAvailableFiles
...
✓ All integration test scenarios completed!
```

---

### `03_test-docker-compose.sh` — Containerized Test Runner

**Best for:** CI/CD, isolated environments, cross-platform reproducibility

```bash
Usage:
  ./03_test-docker-compose.sh [OPTIONS]

Options:
  --version VERSION       Test specific package version (default: latest)
                          Use "latest" or omit to track nuget.org's most recent
                          stable release. Pin (e.g. "26.5.0") for reproducible
                          / shared / CI runs.
  --filter PATTERN        Run only tests matching pattern
  --license PATH          Path to GroupDocs license file
  --keep                  Keep containers running (for debugging)
  --help                  Show help message

Examples:
  # Run all tests in Docker against latest stable (default)
  ./03_test-docker-compose.sh

  # Pin to a specific version
  ./03_test-docker-compose.sh --version 26.5.0

  # Keep containers for inspection
  ./03_test-docker-compose.sh --keep
```

**What it does:**
1. ✅ Generates an isolated docker-compose.yml
2. ✅ Mounts the `Files/` fixtures volume (read-only)
3. ✅ Runs tests in a .NET SDK 10.0 container
4. ✅ Auto-cleans resources (unless `--keep` is set)

**Generated docker-compose file structure:**
```yaml
services:
  markdown-server:
    image: ghcr.io/groupdocs-markdown/markdown-net-mcp:latest
    volumes:
      - files-volume:/data:ro

  test-runner:
    image: mcr.microsoft.com/dotnet/sdk:10.0-alpine
    volumes:
      - workspace-volume:/workspace:ro
      - files-volume:/data:ro
    depends_on:
      - markdown-server
```

---

## 📂 Fixture Documents

The `Files/` folder (at the repo root) holds real document fixtures sourced
from the upstream GroupDocs.Markdown-for-.NET examples repo:

```
Files/
  ├── business-plan.docx      (Word → Markdown)
  ├── business-plan.pdf       (PDF → Markdown)
  ├── business-plan.epub      (EPUB → Markdown)
  ├── cost-analysis.xlsx      (spreadsheet → Markdown)
  └── protected.docx          (password-protected; password: secret)
```

See `Files/README.md` for fixture provenance. The server intentionally does
not expose a `compose_from_markdown` tool (reverse Markdown → document
composition isn't implemented in GroupDocs.Markdown 26.3.0), so no synthetic
Markdown source fixture is needed.

**To add your own:** drop the file into `Files/`, add its name to
`SampleDocuments.RealSamples`, and reference it from a test.

---

## 🔐 License Files

### Using a License

```bash
# Remove evaluation limits / notice from converted Markdown
export GROUPDOCS_LICENSE_PATH=/path/to/GroupDocs.Total.lic
./02_test-all-scenarios.sh

# Or inline
./02_test-all-scenarios.sh --license /path/to/GroupDocs.Total.lic
```

### Evaluation Mode (Default)

The whole suite passes **unlicensed** — GroupDocs.Markdown's `Convert()`
succeeds in evaluation mode (output may be limited and carries an evaluation
notice, but the call does not throw). No test is gated on a license; a license
only removes those limits.

License file must be readable by the process running the tests.

---

## 🎯 Test Scenarios & Filters

Run individual test suites with `--filter`:

| Filter | Tests | Command |
|--------|-------|---------|
| `ToolDiscovery` | 3 tests | `./02_test-all-scenarios.sh --filter ToolDiscovery` |
| `ConvertToMarkdown` | 8 tests | `./02_test-all-scenarios.sh --filter ConvertToMarkdown` |
| `GetDocumentInfo` | 6 tests | `./02_test-all-scenarios.sh --filter GetDocumentInfo` |
| `ErrorHandling` | 3 tests | `./02_test-all-scenarios.sh --filter ErrorHandling` |
| (default) | 20 tests | `./02_test-all-scenarios.sh` |

**Example: Quick validation (only discovery)**
```bash
./02_test-all-scenarios.sh --filter ToolDiscovery  # ~5 seconds
```

---

## 🐳 Docker Configuration

### Using Latest Package

```bash
./03_test-docker-compose.sh --version latest
```

### Building Docker Images Locally

```bash
# Build from server source (requires main repo)
docker build -t markdown-net-mcp:local ../../../GroupDocs.Markdown.Mcp/

# Run tests against the local image
./03_test-docker-compose.sh
```

### Debugging Docker Tests

```bash
# Keep containers running
./03_test-docker-compose.sh --keep

# View logs
docker logs <container-id>

# Inspect running containers
docker ps

# Clean up manually
docker compose -f docker-compose.test.yml down -v
```

---

## 🔧 Advanced Usage

### Test Multiple Versions

```bash
for version in 26.5.0 latest; do
  echo "Testing version $version..."
  ./02_test-all-scenarios.sh --version $version || exit 1
done
```

### Parallel Testing (Different Filters)

```bash
# Run in separate terminals
./02_test-all-scenarios.sh --filter ToolDiscovery &
./02_test-all-scenarios.sh --filter ConvertToMarkdown &
./02_test-all-scenarios.sh --filter ErrorHandling &
wait
```

### CI/CD Integration

**GitHub Actions:**
```yaml
- name: Run integration tests
  run: |
    cd docker-scripts
    ./02_test-all-scenarios.sh --version ${{ matrix.version }}
  env:
    GROUPDOCS_LICENSE_PATH: ${{ secrets.GROUPDOCS_LICENSE }}
```

**Azure Pipelines:**
```yaml
- script: |
    cd docker-scripts
    chmod +x 02_test-all-scenarios.sh
    ./02_test-all-scenarios.sh
  displayName: 'Run Integration Tests'
```

---

## 📋 Helper Functions

The `helpers.sh` file provides reusable bash functions for scripting:

```bash
#!/bin/bash
source "$(dirname "$0")/helpers.sh"

# Logging
log_info "Starting..."
log_success "Completed!"
log_error "Failed"
log_warning "Be careful"

# Checks
check_command docker
check_dotnet_sdk 10
check_file_exists /path/to/file

# Docker utilities
docker_pull_image "ghcr.io/groupdocs-markdown/markdown-net-mcp:latest"
check_docker_daemon

# .NET utilities
dotnet_restore "/path/to/project"
dotnet_build "/path/to/project" "Release"
```

---

## 🐛 Troubleshooting

### "Docker daemon is not running"
```bash
# Windows/macOS: Start Docker Desktop
# Linux
sudo systemctl start docker
```

### "Permission denied" on .sh files (Windows Git Bash)
```bash
git update-index --chmod=+x 02_test-all-scenarios.sh
./02_test-all-scenarios.sh
```

### "dotnet command not found"
```bash
# Install .NET 10 SDK
# https://dotnet.microsoft.com/download/dotnet/10.0

# Or skip build
./02_test-all-scenarios.sh --no-build
```

### Tests timeout in Docker
```bash
# Increase Docker Desktop memory: Preferences → Resources → Memory (8GB+)
# Or use local testing instead
./02_test-all-scenarios.sh
```

---

## 📊 Expected Output

### All Tests Pass
```
Passed  ToolDiscoveryTests.ServerInfo_AdvertisesGroupDocsMarkdownMcp
Passed  ToolDiscoveryTests.ListTools_ExposesConvertAndGetDocumentInfo
Passed  ToolDiscoveryTests.AllTools_HaveNonEmptyDescriptionAndInputSchema
Passed  ConvertToMarkdownTests.ConvertToMarkdown_BusinessPlanDocx_ProducesMarkdownFile
Passed  ConvertToMarkdownTests.ConvertToMarkdown_RealSample_DoesNotReportFailure (×4)
Passed  ConvertToMarkdownTests.ConvertToMarkdown_SkipImages_OmitsBase64DataUris
Passed  ConvertToMarkdownTests.ConvertToMarkdown_ProtectedDocx_WithPassword_Succeeds
Passed  ConvertToMarkdownTests.ConvertToMarkdown_ProtectedDocx_WithoutPassword_ReturnsGracefulError
Passed  GetDocumentInfoTests.GetDocumentInfo_BusinessPlanPdf_ReturnsJsonWithFormatAndPageCount
Passed  GetDocumentInfoTests.GetDocumentInfo_RealSample_ReportsExpectedFormat (×4)
Passed  GetDocumentInfoTests.GetDocumentInfo_ProtectedDocx_WithPassword_Succeeds
Passed  ErrorHandlingTests.ConvertToMarkdown_UnknownFile_ReturnsErrorListingAvailableFiles
Passed  ErrorHandlingTests.ConvertToMarkdown_CorruptedFile_DoesNotCrashServer
Passed  ErrorHandlingTests.PasswordParameter_IsAcceptedByTool

Total: 20, Passed: 20, Time: ~20s

✓ All integration test scenarios completed!
```

### Fixtures Detected
```
✓ Found 5 fixture documents in Files/
  Files/business-plan.pdf (191 KB)
  Files/cost-analysis.xlsx (17 KB)
  ...
```

---

## 📝 Notes

- Scripts are **cross-platform** (Windows/Linux/macOS)
- All paths are **automatically normalized** for your OS
- Docker volumes are **read-only** for safety
- Tests can run **locally or containerized**
- License path is **optional** (the suite passes unlicensed)
- Results are logged in **JSON format** for CI integration

---

## 📚 Related Files

- [GroupDocs.Markdown.Mcp NuGet](https://www.nuget.org/packages/GroupDocs.Markdown.Mcp)
- [Integration Tests Guide](../how-to/06-run-integration-tests.md)
- [Docker Setup Guide](../how-to/02-run-via-docker.md)
- [Test Project](../src/GroupDocs.Markdown.Mcp.Tests/GroupDocs.Markdown.Mcp.Tests.csproj)

---

## 💡 Tips & Best Practices

1. **For quick validation:** Use `--filter ToolDiscovery` (~5 seconds)
2. **For CI/CD:** Use `02_test-all-scenarios.sh` (local is faster than Docker)
3. **For debugging:** Use `03_test-docker-compose.sh --keep` to inspect containers
4. **For multiple versions:** Use a loop with the `--version` flag
5. **For fixtures:** Drop files in `Files/`, they're auto-staged
6. **For licenses:** Store in a secure location, pass via env var in CI

---

**Last Updated:** 2026-05-19
**Minimum Requirements:** Bash 4.0+, Docker 20.10+, .NET 10 SDK
