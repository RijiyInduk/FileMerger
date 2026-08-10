using System;
using System.Collections.Generic;
using System.Text;

namespace FileMerger
{
    /// <summary>Одна строка: обрезанное имя + красная кнопка удаления.</summary>
    public sealed class FileEntryRow : Panel
    {
        public FileEntry Entry { get; }
        public event Action<FileEntryRow>? RemoveRequested;

        private readonly Label _label;

        public FileEntryRow(FileEntry entry)
        {
            Entry = entry;
            Height = 32;
            Margin = new Padding(0, 0, 0, 2);
            BorderStyle = BorderStyle.None;

            var removeBtn = new Button
            {
                Text = "✕",
                ForeColor = Color.White,
                BackColor = Color.Firebrick,
                FlatStyle = FlatStyle.Flat,
                Width = 28,
                Height = 28,
                Dock = DockStyle.Right,   
                Cursor = Cursors.Hand
            };
            removeBtn.FlatAppearance.BorderSize = 0;
            removeBtn.Click += (_, _) => RemoveRequested?.Invoke(this);

            _label = new Label
            {
                Text = entry.DisplayTitle,
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };
            
            Controls.Add(_label);
            Controls.Add(removeBtn);
        }
    }
}
