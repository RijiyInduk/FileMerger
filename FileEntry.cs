using System;
using System.Collections.Generic;
using System.Text;

namespace FileMerger
{
    /// <summary>A single file in the list.</summary>
    public sealed class FileEntry
    {
        public string FullPath { get; }
        /// <summary>Title path (relative path or file name + duplicate suffix).</summary>
        public string DisplayTitle { get; set; }

        public FileEntry(string fullPath, string displayTitle)
        {
            FullPath = fullPath;
            DisplayTitle = displayTitle;
        }
    }
}
