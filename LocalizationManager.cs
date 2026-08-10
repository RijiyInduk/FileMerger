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
            ["DropHint"] = ("Drag & drop files or folders here", "Перетащите файлы или папки сюда"),
            ["FileList"] = ("File list", "Список файлов"),
            ["ClearAll"] = ("Clear all", "Очистить всё"),
            ["Merge"] = ("Merge!", "Объединить!"),
            ["StatusReady"] = ("Add files.", "Добавьте файлы."),
            ["StatusMerging"] = ("Merging...", "Объединение..."),
            ["StatusDone"] = ("File created successfully.", "Файл успешно создан."),
            ["EmptyList"] = ("The list is empty. Add files first.", "Список пуст. Сначала добавьте файлы."),
            ["Ignored"] = ("Ignored (unsupported): ", "Пропущено (не поддерживается): "),
            ["SkippedCount"] = ("Skipped files: ", "Пропущено файлов: "),
            ["Success"] = ("Done! File saved to:\n", "Готово! Файл сохранён:\n"),
            ["Added"] = ("Added: ", "Добавлено: "),
            ["FilesInList"] = ("Files in list: ", "Файлов в списке: "),
            ["FileListCount"] = ("File list ({0})", "Список файлов ({0})"),
            ["Cancel"] = ("Cancel", "Отмена"),
            ["StatusCancelled"] = ("Operation cancelled.", "Операция отменена."),
            ["ReadError"] = ("[Read error] {0}", "[Ошибка чтения] {0}"),
            ["SkippedTooLarge"] = ("[Skipped: file too large ({0} MB)]", "[Пропущено: файл слишком большой ({0} МБ)]"),
        };

        public static string Get(string key)
        {
            if (!Map.TryGetValue(key, out var v)) return key;
            return Current == AppLang.En ? v.en : v.ru;
        }

        /// <summary>Форматированная строка с подстановкой аргументов ({0}, {1}...).</summary>
        public static string Format(string key, params object[] args)
            => string.Format(Get(key), args);

        public static void Set(AppLang lang) => Current = lang;
    }
}