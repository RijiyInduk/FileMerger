using System;
using System.Collections.Generic;
using System.Text;

namespace FileMerger
{
    /// <summary>Один файл в списке.</summary>
    public sealed class FileEntry
    {
        public string FullPath { get; }
        /// <summary>Путь для заголовка (относительный или имя + суффикс дубликата).</summary>
        public string DisplayTitle { get; set; }

        public FileEntry(string fullPath, string displayTitle)
        {
            FullPath = fullPath;
            DisplayTitle = displayTitle;
        }
    }
}
