
# TXTFilesMerger

[English version](#english) | [Русская версия](#russian)

## English <a name="english"></a>
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

Grab the latest build from the [**Releases**](https://github.com/YOUR_USERNAME/TXTFilesMerger/releases) page:(https://github.com/RijiyInduk/FileMerger/releases/tag/v1.0.0)

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

## Русская версия <a name="russian"></a>

TXTFilesMerger - утилита для Windows, которая объединяет несколько текстовых файлов
(исходный код, конфиги, документацию) в один файл с чёткими разделителями -
идеально для загрузки целого проекта в LLM (ChatGPT, Claude, Gemini) одной вставкой.

<img width="1009" height="752" alt="programscreen2" src="https://github.com/user-attachments/assets/fd7e23d5-f1cb-4965-98f5-472e71ec47cb" />

## Возможности

- **Добавление файлов** через диалог (множественный выбор) или **перетаскиванием** (drag & drop) - как файлов, *так и папок* (рекурсивно).
- **28+ поддерживаемых расширений**: `.txt .cs .md .json .xml .yaml .yml .html .css .js .ts .py .java .cpp .h .xaml .sql .sh .bat .ps1 .go .rs .php .kt .toml .ini .csv`.
- **Относительные пути в заголовках** (например, `src/Player.cs`) — больше контекста для LLM.
- **Обработка дубликатов** - одноимённые файлы получают суффикс `(1)`, `(2)`.
- **Потоковое чтение/запись** - большие файлы обрабатываются без полной загрузки в память.
- **Определение кодировки** - с учётом BOM; итоговый файл всегда в **UTF-8 без BOM** (удобно для LLM).
- **Пропуск файлов > 50 МБ**; нечитаемые файлы помечаются как `[Read error]` вместо аварийного завершения.
- **Двуязычный интерфейс** (английский / русский) с переключением «на лету».
- **Изменяемый, адаптивный интерфейс** с индикатором прогресса и строкой состояния.

## Скачать

- [⬇️ Download (self-contained)](https://github.com/YOUR_USERNAME/TXTFilesMerger/releases/latest/download/TXTFilesMerger-win-x64-self-contained.zip)
- [⬇️ Download (framework-dependent)](https://github.com/YOUR_USERNAME/TXTFilesMerger/releases/latest/download/TXTFilesMerger-win-x64-framework-dependent.zip)

| Файл | Описание | Нужен .NET? |
|------|----------|-------------|
| `TXTFilesMerger-win-x64-self-contained.zip` | Всё включено — распакуйте и запускайте | ❌ Нет |
| `TXTFilesMerger-win-x64-framework-dependent.zip` | Малый размер | ✅ [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |

> **Не знаете, что выбрать?** Берите **self-contained** — работает сразу, без установки.

## Использование

1. Запустите приложение.
2. Добавьте файлы через **«Добавить файлы...»** или перетащите их в зону drag & drop.
3. Удаляйте отдельные файлы красной кнопкой **✕** или очистите весь список.
4. Нажмите **«Объединить!»** и выберите путь сохранения (`.txt` или `.md`).
5. Готово — итоговый файл создан.

## Формат вывода

```
============================================================
File #1. src/Program.cs
============================================================
<содержимое файла>

============================================================
File #2. README.md
============================================================
<содержимое файла>
```

## Сборка из исходников

Требуется [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/YOUR_USERNAME/TXTFilesMerger.git
cd TXTFilesMerger
dotnet run
```

## Стек технологий

- C# / .NET 8
- Windows Forms
- Полностью код без дизайнера (code-behind)

## Лицензия

[MIT](LICENSE)
