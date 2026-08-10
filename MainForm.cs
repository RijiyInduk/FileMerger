namespace FileMerger
{
    public sealed class MainForm : Form
    {
        private readonly List<FileEntry> _files = new();
        private readonly FileMergeService _merge = new();
        private CancellationTokenSource? _cts;

        private Button _btnEn = null!, _btnRu = null!, _btnAdd = null!, _btnMerge = null!, _btnClear = null!;
        private Label _lblTitle = null!, _lblListHeader = null!;
        private Panel _dropPanel = null!;
        private FlowLayoutPanel _listPanel = null!;
        private ProgressBar _progress = null!;
        private Label _status = null!;

        public MainForm()
        {
            BuildUi();
            ApplyLanguage(); //(En)
        }

        // ---------- UI ----------
        private void BuildUi()
        {
            Text = "File Merger";
            try
            {
                using var s = System.Reflection.Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("FileMerger.MergerIcon256.ico");
                if (s != null)
                    Icon = new Icon(s);
            }
            catch { /* иконка не критична */ }

            MinimumSize = new Size(760, 560);
            Size = new Size(1024, 760);
            StartPosition = FormStartPosition.CenterScreen;
            AllowDrop = true;
            BackColor = Color.FromArgb(237, 230, 219);
            Padding = new Padding(15, 12, 15, 0);

            // ===== Статус (Dock.Bottom) =====
            _status = new Label
            {
                Height = 28,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0),
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Bottom
            };
            Controls.Add(_status);

            // Прогресс — над статусом (та же нижняя зона)
            _progress = new ProgressBar
            {
                Height = 22,
                Dock = DockStyle.Bottom,
                Visible = false
            };
            Controls.Add(_progress);

            // ===== Корневая таблица: [0] шапка (auto), [1] тело (fill) =====
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);
            root.BringToFront();

            // ===== Строка 0: шапка (EN | RU | Title) =====
            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10),
                BackColor = Color.Transparent
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _btnEn = new Button
            {
                Text = "EN",
                Width = 50,
                Height = 32,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Margin = new Padding(0, 0, 6, 0)
            };
            _btnEn.Click += (_, _) => SwitchLang(AppLang.En);

            _btnRu = new Button
            {
                Text = "RU",
                Width = 50,
                Height = 32,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Margin = new Padding(0, 0, 20, 0)
            };
            _btnRu.Click += (_, _) => SwitchLang(AppLang.Ru);

            _lblTitle = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 18, FontStyle.Bold), // 22 → 20
                ForeColor = Color.FromArgb(60, 45, 30),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = false,                        // отключаем обрезку
                Margin = new Padding(0, 0, 15, 0),           // отступ справа
                UseCompatibleTextRendering = false
            };

            header.Controls.Add(_btnEn, 0, 0);
            header.Controls.Add(_btnRu, 1, 0);
            header.Controls.Add(_lblTitle, 2, 0);
            root.Controls.Add(header, 0, 0);

            // ===== Строка 1: тело (55% / 45%) =====
            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            root.Controls.Add(body, 0, 1);

            // ---- Левая колонка ----
            var left = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0, 0, 10, 10),
                BackColor = Color.Transparent
            };
            left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _btnAdd = new Button
            {
                Text = "Add files...",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 12)
            };
            _btnAdd.Click += (_, _) => AddViaDialog();

            _dropPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                AllowDrop = true,
                BackColor = Color.White,
                Margin = new Padding(0)
            };
            var dropHint = new Label
            {
                Name = "dropHint",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 13)
            };
            _dropPanel.Controls.Add(dropHint);
            _dropPanel.DragEnter += OnDragEnter;
            _dropPanel.DragDrop += OnDragDrop;

            left.Controls.Add(_btnAdd, 0, 0);
            left.Controls.Add(_dropPanel, 0, 1);
            body.Controls.Add(left, 0, 0);

            // ---- Правая колонка: List(fill) + Header(auto) + Clear + Merge ----
            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Margin = new Padding(10, 0, 0, 10),
                BackColor = Color.Transparent
            };
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // List
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // Header
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 46)); // Clear
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 46)); // Merge

            _listPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Margin = new Padding(0)
            };
            _listPanel.ClientSizeChanged += (_, _) => ResizeRows();

            _lblListHeader = new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 11),
                Margin = new Padding(0, 4, 0, 8)
            };

            _btnClear = new Button
            {
                Text = "Clear all",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                Margin = new Padding(0, 0, 0, 12)
            };
            _btnClear.Click += (_, _) => ClearAll();

            _btnMerge = new Button
            {
                Text = "Merge!",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold),
                BackColor = Color.FromArgb(139, 100, 65),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0)
            };
            _btnMerge.FlatAppearance.BorderSize = 0;
            _btnMerge.Click += async (_, _) => await MergeAsync();

            right.Controls.Add(_listPanel, 0, 0);
            right.Controls.Add(_lblListHeader, 0, 1);
            right.Controls.Add(_btnClear, 0, 2);
            right.Controls.Add(_btnMerge, 0, 3);
            body.Controls.Add(right, 1, 0);

            // Скрывать прогресс при служебных кликах
            foreach (var btn in new[] { _btnEn, _btnRu, _btnAdd, _btnClear })
                btn.Click += (_, _) => HideProgress();
        }

        private void HideProgress()
        {
            _progress.Value = 0;
            _progress.Visible = false;
        }

        private void ResizeRows()
        {
            int w = GetRowWidth();
            foreach (Control c in _listPanel.Controls)
                c.Width = w;
        }

        // ---------- Локализация ----------
        private void SwitchLang(AppLang lang)
        {
            LocalizationManager.Set(lang);
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            Text = LocalizationManager.Get("Title");
            _lblTitle.Text = LocalizationManager.Get("Title");
            _btnAdd.Text = LocalizationManager.Get("AddFiles");
            UpdateListHeader();
            _btnClear.Text = LocalizationManager.Get("ClearAll");
            _btnMerge.Text = LocalizationManager.Get(_cts is null ? "Merge" : "Cancel");
            _dropPanel.Controls["dropHint"]!.Text = LocalizationManager.Get("DropHint");
            SetStatus("StatusReady");
        }

        private void SetStatus(string key) => _status.Text = LocalizationManager.Get(key);
        private void SetStatusRaw(string text) => _status.Text = text;

        // ---------- Добавление файлов ----------
        private void AddViaDialog()
        {
            using var dlg = new OpenFileDialog
            {
                Multiselect = true,
                Filter = SupportedExtensions.OpenFilter()
            };
            if (dlg.ShowDialog(this) == DialogResult.OK)
                AddFiles(dlg.FileNames);
        }

        private void OnDragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                e.Effect = DragDropEffects.Copy;
        }

        private void OnDragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is not string[] paths) return;

            var collected = new List<string>();
            foreach (var p in paths)
            {
                if (Directory.Exists(p))
                    collected.AddRange(Directory.EnumerateFiles(p, "*", SearchOption.AllDirectories));
                else if (File.Exists(p))
                    collected.Add(p);
            }
            AddFiles(collected);
        }

        private void AddFiles(IEnumerable<string> paths)
        {
            int ignored = 0;
            var accepted = new List<string>();

            foreach (var p in paths)
            {
                if (SupportedExtensions.IsSupported(p)) accepted.Add(p);
                else ignored++;
            }

            var baseDir = GetCommonRoot(_files.Select(f => f.FullPath).Concat(accepted));

            foreach (var path in accepted)
            {
                string title = MakeTitle(path, baseDir);
                title = EnsureUniqueTitle(title);
                var entry = new FileEntry(path, title);
                _files.Add(entry);
                AddRow(entry);
            }

            var msg = $"{LocalizationManager.Get("Added")}{accepted.Count}   |   " +
                      $"{LocalizationManager.Get("FilesInList")}{_files.Count}";
            if (ignored > 0)
                msg += $"   |   {LocalizationManager.Get("Ignored")}{ignored}";
            SetStatusRaw(msg);

            UpdateListHeader();
        }

        private void UpdateListHeader()
        {
            _lblListHeader.Text = string.Format(LocalizationManager.Get("FileListCount"), _files.Count);
        }
        private void UpdateStatusCount()
        {
            SetStatusRaw($"{LocalizationManager.Get("FilesInList")}{_files.Count}");
        }

        private void AddRow(FileEntry entry)
        {
            var row = new FileEntryRow(entry);
            row.RemoveRequested += RemoveRow;
            row.Width = GetRowWidth();
            _listPanel.Controls.Add(row);
        }

        private void RemoveRow(FileEntryRow row)
        {
            _files.Remove(row.Entry);
            _listPanel.Controls.Remove(row);
            row.Dispose();
            UpdateListHeader();
            UpdateStatusCount();
        }

        private int GetRowWidth()
        {
            return _listPanel.ClientSize.Width - _listPanel.Padding.Horizontal - 2;
        }

        private void ClearAll()
        {
            _files.Clear();
            foreach (Control c in _listPanel.Controls.OfType<Control>().ToList())c.Dispose();
            _listPanel.Controls.Clear();
            SetStatus("StatusReady");   
            UpdateListHeader();
        }

        // ---------- Заголовки / относительные пути ----------
        private static string MakeTitle(string fullPath, string? baseDir)
        {
            if (string.IsNullOrEmpty(baseDir)) return Path.GetFileName(fullPath);
            try
            {
                var rel = Path.GetRelativePath(baseDir, fullPath);
                return rel.Replace('\\', '/');
            }
            catch { return Path.GetFileName(fullPath); }
        }

        private string EnsureUniqueTitle(string title)
        {
            if (_files.All(f => f.DisplayTitle != title)) return title;
            int i = 1;
            var ext = Path.GetExtension(title);
            var noExt = title[..^ext.Length];
            string candidate;
            do { candidate = $"{noExt}({i++}){ext}"; }
            while (_files.Any(f => f.DisplayTitle == candidate));
            return candidate;
        }

        private static string? GetCommonRoot(IEnumerable<string> paths)
        {
            var dirs = paths.Select(Path.GetDirectoryName)
                            .Where(d => !string.IsNullOrEmpty(d))
                            .Distinct().ToList();
            if (dirs.Count == 0) return null;
            if (dirs.Count == 1) return dirs[0];

            var split = dirs.Select(d => d!.Split(Path.DirectorySeparatorChar)).ToList();
            var min = split.Min(s => s.Length);
            var common = new List<string>();
            for (int i = 0; i < min; i++)
            {
                var part = split[0][i];
                if (split.All(s => string.Equals(s[i], part, StringComparison.OrdinalIgnoreCase)))
                    common.Add(part);
                else break;
            }
            return common.Count == 0 ? null : string.Join(Path.DirectorySeparatorChar, common);
        }

        // ---------- Объединение ----------
        private async Task MergeAsync()
        {
            if (_cts is not null)
            {
                _cts.Cancel();
                return;
            }

            if (_files.Count == 0)
            {
                MessageBox.Show(LocalizationManager.Get("EmptyList"), Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|Markdown files (*.md)|*.md",
                FilterIndex = 1,
                FileName = "merged_output.txt"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            _cts = new CancellationTokenSource();

            SetStatus("StatusMerging");
            SetControlsEnabled(false);
            _btnMerge.Enabled = true; // остаётся активной как «Отмена»
            _btnMerge.Text = LocalizationManager.Get("Cancel");
            _progress.Visible = true;
            _progress.Value = 0;

            var progress = new Progress<int>(v => _progress.Value = Math.Min(v, 100));

            try
            {
                var result = await _merge.MergeAsync(_files, dlg.FileName, progress, _cts.Token);

                SetStatusRaw(result.Skipped > 0
                    ? LocalizationManager.Get("SkippedCount") + result.Skipped
                    : LocalizationManager.Get("StatusDone"));

                MessageBox.Show(LocalizationManager.Get("Success") + dlg.FileName, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                SetStatus("StatusCancelled");
                try { File.Delete(dlg.FileName); } catch { /* best-effort */ }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
                _btnMerge.Text = LocalizationManager.Get("Merge");
                SetControlsEnabled(true);
                HideProgress();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_cts is not null)
            {
                _cts.Cancel();
                e.Cancel = true;
                return;
            }
            base.OnFormClosing(e);
        }

        private void SetControlsEnabled(bool enabled)
        {
            _btnAdd.Enabled = _btnMerge.Enabled = _btnClear.Enabled =
            _btnEn.Enabled = _btnRu.Enabled = _dropPanel.Enabled = enabled;
        }
    }
}