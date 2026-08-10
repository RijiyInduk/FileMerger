using System;
using System.Collections.Generic;
using System.Text;

namespace FileMerger
{
    public sealed class MergeResult
    {
        public int Total { get; set; }
        public int Skipped { get; set; }
    }

    public sealed class FileMergeService
    {
        private const string Separator = "============================================================"; // 60x '='
        private const long MaxBytes = 50L * 1024 * 1024; // 50 МБ

        /// <summary>Асинхронно объединяет файлы построчно. Прогресс 0..100.</summary>
        public async Task<MergeResult> MergeAsync(
            IReadOnlyList<FileEntry> files,
            string outputPath,
            IProgress<int> progress,
            CancellationToken ct = default)
        {
            var result = new MergeResult { Total = files.Count };
            var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

            await using var writer = new StreamWriter(outputPath, append: false, utf8NoBom);

            int lastPct = -1;

            for (int i = 0; i < files.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var entry = files[i];

                await writer.WriteLineAsync(Separator);
                await writer.WriteLineAsync($"File №{i + 1}. {entry.DisplayTitle}");
                await writer.WriteLineAsync(Separator);

                try
                {
                    var info = new FileInfo(entry.FullPath);
                    if (info.Length > MaxBytes)
                    {
                        await writer.WriteLineAsync(LocalizationManager.Format("SkippedTooLarge", info.Length / 1024 / 1024));
                        result.Skipped++;
                    }
                    else
                    {
                        var encoding = DetectEncoding(entry.FullPath);
                        using var reader = new StreamReader(entry.FullPath, encoding, true);
                        string? line;
                        while ((line = await reader.ReadLineAsync()) is not null)
                        {
                            ct.ThrowIfCancellationRequested();
                            await writer.WriteLineAsync(line);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    throw; // отмену пробрасываем наверх, не превращая в [Read error]
                }
                catch (Exception ex)
                {
                    await writer.WriteLineAsync(LocalizationManager.Format("ReadError", ex.Message));
                    result.Skipped++;
                }

                await writer.WriteLineAsync();

                // Троттлинг: репортим только при изменении процента
                int pct = (int)((i + 1) / (double)files.Count * 100);
                if (pct != lastPct)
                {
                    progress.Report(pct);
                    lastPct = pct;
                }
            }

            return result;
        }

        /// <summary>Определение кодировки по BOM, иначе UTF-8</summary>
        private static Encoding DetectEncoding(string path)
        {
            var bom = new byte[4];
            int read;
            using (var fs = File.OpenRead(path))
                read = fs.Read(bom, 0, 4);

            if (read >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF) return Encoding.UTF8;
            if (read >= 2 && bom[0] == 0xFF && bom[1] == 0xFE) return Encoding.Unicode;      // UTF-16 LE
            if (read >= 2 && bom[0] == 0xFE && bom[1] == 0xFF) return Encoding.BigEndianUnicode; // UTF-16 BE
            return new UTF8Encoding(false);
        }
    }
}