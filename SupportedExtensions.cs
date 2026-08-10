using System;
using System.Collections.Generic;
using System.Text;

namespace FileMerger
{
    public static class SupportedExtensions
    {        
        public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".cs", ".md", ".json", ".xml", ".yaml", ".yml", ".html",
        ".css", ".js", ".ts", ".py", ".java", ".cpp", ".h", ".xaml",
        ".sql", ".sh", ".bat", ".ps1", ".go", ".rs", ".php", ".kt",
        ".toml", ".ini", ".csv"
    };

        public static bool IsSupported(string path)
            => All.Contains(Path.GetExtension(path));

        /// <summary>Строка фильтра для OpenFileDialog.</summary>
        public static string OpenFilter()
        {
            var mask = string.Join(";", All.Select(e => "*" + e));
            return $"Supported files|{mask}|All files (*.*)|*.*";
        }
    }
}
