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

            for (int i = 0; i < files.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var entry = files[i];

                await writer.WriteLineAsync(Separator);
                await writer.WriteLineAsync($"File №{i + 1}. {entry.DisplayTitle}");
                await writer.WriteLineAsync(Separator);

                try
                {
                    var encoding = DetectEncoding(entry.FullPath);
                    using var reader = new StreamReader(entry.FullPath, encoding, true);
                    string? line;
                    while ((line = await reader.ReadLineAsync()) is not null)
                        await writer.WriteLineAsync(line);
                }
                catch (Exception ex)
                {                    
                    await writer.WriteLineAsync($"[Read error] {ex.Message}");
                    result.Skipped++;
                }

                await writer.WriteLineAsync();

                progress.Report((int)((i + 1) / (double)files.Count * 100));
            }

            return result;
        }

        /// <summary>Определение кодировки по BOM, иначе UTF-8</summary>
        private static Encoding DetectEncoding(string path)
        {
            var bom = new byte[4];
            using (var fs = File.OpenRead(path))
                _ = fs.Read(bom, 0, 4);

            if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF) return Encoding.UTF8;
            if (bom[0] == 0xFF && bom[1] == 0xFE) return Encoding.Unicode;      // UTF-16 LE
            if (bom[0] == 0xFE && bom[1] == 0xFF) return Encoding.BigEndianUnicode; // UTF-16 BE
            return new UTF8Encoding(false);
        }
    }
}
