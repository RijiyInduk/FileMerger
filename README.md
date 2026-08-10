
# TXTFilesMerger

[English version](#english) | [Русская версия](#russian)

A lightweight Windows desktop tool that merges multiple text-based files
(source code, configs, docs) into a single output file with clear separators -
perfect for feeding an entire codebase into an LLM (ChatGPT, Claude, Gemini) in one paste.

<img width="1007" height="751" alt="programscreen1" src="https://github.com/user-attachments/assets/59e23377-0c01-48c2-94e9-f05194ec926a" />

## Features

- **Add files** via dialog (multi-select) or **drag & drop** (files *and* folders, recursive).
- **28+ supported extensions**: `.txt .cs .md .json .xml .yaml .yml .html .css .js .ts .py .java .cpp .h .xaml .sql .sh .bat .ps1 .go .rs .php .kt .toml .ini .csv`.
- **Relative-path titles** (e.g. `src/Player.cs`) for better LLM context.
- **Duplicate handling** - same-named files get a `(1)`, `(2)` suffix.
- **Streaming read/write** - handles large files without loading them fully into memory.
- **Encoding detection** - BOM-aware; output is always **UTF-8 without BOM** (LLM-friendly).
- **Skips files > 50 MB** and logs unreadable files as `[Read error]` instead of crashing.
- **Bilingual UI** (English / Russian), switchable on the fly.
- **Resizable, responsive layout** with progress bar and status line.

## Download

Grab the latest build from the [**Releases**](https://github.com/YOUR_USERNAME/TXTFilesMerger/releases) page:

| File | Description | Requires .NET? |
|------|-------------|----------------|
| `TXTFilesMerger-win-x64-self-contained.zip` | Everything bundled, just unzip & run | ❌ No |
| `TXTFilesMerger-win-x64-framework-dependent.zip` | Small size | ✅ [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |

> **Not sure which one?** Pick **self-contained** - it works out of the box.

## Usage

1. Launch the app.
2. Add files via **Add files...** or drag & drop them onto the drop zone.
3. Remove individual files with the red **✕**, or clear the whole list.
4. Click **Merge!**, choose an output path (`.txt` or `.md`).
5. Done - the merged file is ready.

## Output format

```
============================================================
File #1. src/Program.cs
============================================================
<file contents>

============================================================
File #2. README.md
============================================================
<file contents>
```

## Build from source

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/YOUR_USERNAME/TXTFilesMerger.git
cd TXTFilesMerger
dotnet run
```

## Tech stack

- C# / .NET 8
- Windows Forms
- Pure code-behind UI (no designer)

## License

[MIT](LICENSE)


