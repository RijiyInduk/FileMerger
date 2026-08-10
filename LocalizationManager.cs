using System;
using System.Collections.Generic;
using System.Text;

namespace FileMerger
{
    public enum AppLang { En, Ru }

    /// <summary>Простой словарь строк без .resx (проще для одного файла).</summary>
    public static class LocalizationManager
    {
        public static AppLang Current { get; private set; } = AppLang.En;

        private static readonly Dictionary<string, (string en, string ru)> Map = new()
        {
            ["Title"] = ("TXTFileMerger", "TXTFileMerger"),
            ["AddFiles"] = ("Add files...", "Добавить файлы..."),
            ["DropHint"] = ("Drag & drop files or folders here",
                                "Перетащите файлы или папки сюда"),
            ["FileList"] = ("File list", "Список файлов"),
            ["ClearAll"] = ("Clear all", "Очистить всё"),
            ["Merge"] = ("Merge!", "Объединить!"),
            ["StatusReady"] = ("Add files.", "Добавьте файлы."),
            ["StatusMerging"] = ("Merging...", "Объединение..."),
            ["StatusDone"] = ("File created successfully.", "Файл успешно создан."),
            ["EmptyList"] = ("The list is empty. Add files first.",
                                "Список пуст. Сначала добавьте файлы."),
            ["Ignored"] = ("Ignored (unsupported): ", "Пропущено (не поддерживается): "),
            ["SkippedCount"] = ("Skipped files: ", "Пропущено файлов: "),
            ["Success"] = ("Done! File saved to:\n", "Готово! Файл сохранён:\n"),
            ["Added"] = ("Added: ", "Добавлено: "),
            ["FilesInList"] = ("Files in list: ", "Файлов в списке: "),
            ["FileListCount"] = ("File list ({0})", "Список файлов ({0})"),
            ["Cancel"] = ("Cancel", "Отмена"),
            ["StatusCancelled"] = ("Operation cancelled.", "Операция отменена."),
        };

        public static string Get(string key)
        {
            if (!Map.TryGetValue(key, out var v)) return key;
            return Current == AppLang.En ? v.en : v.ru;
        }

        public static void Set(AppLang lang) => Current = lang;
    }
}
