# Files — real document fixtures for the integration suite

Files are sourced from the upstream
[GroupDocs.Markdown-for-.NET](https://github.com/groupdocs-markdown/GroupDocs.Markdown-for-.NET)
examples repo. Each fixture maps to one or more MCP tool methods via the
upstream example that uses it. The integration suite stages these into the
MCP server's runtime storage folder (`./Files`) and the test fixture's temp
storage directory.

| File | MCP method(s) | Upstream example | Notes |
|---|---|---|---|
| `business-plan.docx` | `ConvertToMarkdown`, `GetDocumentInfo` | `Examples/.../BasicUsage/Convert/ExportWordprocessing` | Primary Word fixture (~56 KB) |
| `business-plan.pdf` | `ConvertToMarkdown`, `GetDocumentInfo` | `Examples/.../BasicUsage/Convert/ExportPdf` | Primary PDF fixture (~191 KB) |
| `business-plan.epub` | `ConvertToMarkdown`, `GetDocumentInfo` | `Examples/.../BasicUsage/Convert/ExportEbook` | EPUB e-book fixture (~66 KB) |
| `cost-analysis.xlsx` | `ConvertToMarkdown`, `GetDocumentInfo` | `Examples/.../BasicUsage/Convert/ExportSpreadsheet` | Spreadsheet — `pageCount` reports worksheet count (~17 KB) |
| `protected.docx` | (`password` parameter on all tools) | `Examples/.../AdvancedUsage/Loading/LoadAPasswordProtectedDocument` | Password-protected DOCX. Password: `secret` |

The server intentionally exposes only `convert_to_markdown` and
`get_document_info` — reverse Markdown→document composition
(`MarkdownConverter.FromMarkdownString`) throws `NotImplementedException` in
GroupDocs.Markdown 26.3.0, so no `compose_from_markdown` tool ships and no
synthetic Markdown fixture is needed.

## Refresh command

```bash
EX="<path-to>/GroupDocs.Markdown-for-.NET/Examples/GroupDocs.Markdown.Examples.CSharp"
cp "$EX/DeveloperGuide/BasicUsage/Convert/ExportWordprocessing/business-plan.docx" ./Files/
cp "$EX/DeveloperGuide/BasicUsage/Convert/ExportPdf/business-plan.pdf" ./Files/
cp "$EX/DeveloperGuide/BasicUsage/Convert/ExportEbook/business-plan.epub" ./Files/
cp "$EX/DeveloperGuide/BasicUsage/Convert/ExportSpreadsheet/cost-analysis.xlsx" ./Files/
cp "$EX/DeveloperGuide/AdvancedUsage/Loading/LoadAPasswordProtectedDocument/protected.docx" ./Files/
```
