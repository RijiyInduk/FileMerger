using System;
using System.Collections.Generic;
using System.Text;

namespace FileMerger
{
    /// <summary>A single row: truncated name + red remove button.</summary>
    public sealed class FileEntryRow : Panel
    {
        public FileEntry Entry { get; }
        public event Action<FileEntryRow>? RemoveRequested;

        private readonly Label _label;
        private readonly Button _removeBtn;

        public FileEntryRow(FileEntry entry)
        {
            Entry = entry;
            Height = 32;
            Margin = new Padding(0, 0, 0, 2);
            BorderStyle = BorderStyle.None;

            _removeBtn = new Button
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
            _removeBtn.FlatAppearance.BorderSize = 0;
            _removeBtn.Click += OnRemoveClick;

            _label = new Label
            {
                Text = entry.DisplayTitle,
                AutoEllipsis = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };

            Controls.Add(_label);
            Controls.Add(_removeBtn);
        }

        private void OnRemoveClick(object? sender, EventArgs e)
            => RemoveRequested?.Invoke(this);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _removeBtn.Click -= OnRemoveClick; 
                RemoveRequested = null;            
            }
            base.Dispose(disposing);
        }
    }
}