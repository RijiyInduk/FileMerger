
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

            // Кнопки языка
            _btnEn = MakeButton("EN", 15, 12, 50, 32, (_, _) => SwitchLang(AppLang.En));
            _btnRu = MakeButton("RU", 72, 12, 50, 32, (_, _) => SwitchLang(AppLang.Ru));
            _btnEn.Font = _btnRu.Font = new Font("Segoe UI", 10, FontStyle.Regular);

            // Заголовок программы 
            _lblTitle = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 26, FontStyle.Bold), 
                ForeColor = Color.FromArgb(60, 45, 30),                   
                BackColor = Color.Transparent,
                Top = 12
            };
            Controls.Add(_lblTitle);
            _lblTitle.SendToBack();

            // Кнопка добавить
            _btnAdd = MakeButton("Add files...", 0, 0, 0, 0, (_, _) => AddViaDialog());
            _btnAdd.Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold);

            // Поле drag&drop
            _dropPanel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                AllowDrop = true,
                BackColor = Color.White
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
            Controls.Add(_dropPanel);

            // Заголовок списка (выравнивание по правому краю задаётся в RelayoutAll)
            _lblListHeader = new Label
            {
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 11)
            };
            Controls.Add(_lblListHeader);

            // Панель списка (скролл)
            _listPanel = new FlowLayoutPanel
            {
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            Controls.Add(_listPanel);

            // Кнопки очистить и объединить (размеры/позиции — в RelayoutAll)
            _btnClear = MakeButton("Clear all", 0, 0, 0, 0, (_, _) => ClearAll());
            _btnClear.Font = new Font("Segoe UI", 11, FontStyle.Regular);

            _btnMerge = MakeButton("Merge!", 0, 0, 0, 0, async (_, _) => await MergeAsync());
            _btnMerge.Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold); 
            _btnMerge.BackColor = Color.FromArgb(139, 100, 65);
            _btnMerge.ForeColor = Color.White;
            _btnMerge.FlatStyle = FlatStyle.Flat;
            _btnMerge.FlatAppearance.BorderSize = 0;

            // Прогресс
            _progress = new ProgressBar
            {
                Height = 18,
                Visible = false
            };
            Controls.Add(_progress);

            // Статусная строка (Dock.Bottom — единственный, кому Dock оставляем)
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

            _listPanel.ClientSizeChanged += (_, _) => ResizeRows();

            foreach (var btn in new[] { _btnEn, _btnRu, _btnAdd, _btnClear })
                btn.Click += (_, _) => HideProgress();

            // Единый обработчик раскладки
            Resize += (_, _) => RelayoutAll();
            RelayoutAll();
        }

        private void RelayoutAll()
        {
            const int pad = 15;
            const int gap = 12;
            int W = ClientSize.Width;

            // ===== ЗОНА 3 (низ): статус + прогресс =====
            int statusTop = ClientSize.Height - _status.Height;
            _progress.SetBounds(_status.Left, statusTop, _status.Width, _status.Height);
            _progress.BringToFront();

            // ===== ЗОНА 1 (шапка): кнопки языка + название =====
            FitTitle(W);                                 

            int langTop = 12;
            int langH = 32;
            int langBottom = langTop + langH;            

            // Высота шапки = максимум из высоты кнопок и высоты заголовка
            int titleH = _lblTitle.Height;               
            int headerContentBottom = langTop + Math.Max(langH, titleH);
            int headerBottom = headerContentBottom + 6;  

            // Название центрируем по вертикали в пределах шапки
            int titleAreaLeft = _btnRu.Right + 20;
            _lblTitle.Top = langTop + Math.Max(0, (headerContentBottom - langTop - titleH) / 2);
            _lblTitle.Left = titleAreaLeft +
                Math.Max(0, (W - pad - titleAreaLeft - _lblTitle.Width) / 2);
            _lblTitle.SendToBack();

            // ===== ЗОНА 2 (рабочая область) =====
            int workTop = headerBottom + gap;            
            int workBottom = statusTop - gap;
            int H = workBottom;

            // ---- дальше без изменений ----
            int colGap = 20;
            int leftW = (int)((W - pad * 2 - colGap) * 0.55);
            int rightW = W - pad * 2 - colGap - leftW;
            int rightX = pad + leftW + colGap;

            int topBtnY = workTop;
            int topBtnH = 55;

            _btnAdd.SetBounds(pad, topBtnY, leftW, topBtnH);

            int dropTop = topBtnY + topBtnH + gap;
            _dropPanel.SetBounds(pad, dropTop, leftW, H - dropTop);

            int btnH = 46;
            int mergeY = H - btnH;
            _btnMerge.SetBounds(rightX, mergeY, rightW, btnH);

            int clearY = mergeY - gap - btnH;
            _btnClear.SetBounds(rightX, clearY, rightW, btnH);

            int listTop = topBtnY;
            int headerH = _lblListHeader.Height + 8;
            int listBottom = clearY - gap - headerH;
            _listPanel.SetBounds(rightX, listTop, rightW, Math.Max(60, listBottom - listTop));

            _lblListHeader.Top = _listPanel.Bottom + 4;
            _lblListHeader.Left = rightX + rightW - _lblListHeader.Width;

            ResizeRows();
        }

        /// <summary>Подгоняет шрифт названия, чтобы оно не вылезало за правый край.</summary>
        private void FitTitle(int formWidth)
        {
            int available = formWidth - 15 - (_btnRu.Right + 20); 
            if (available < 60) available = 60;

            float size = 26f;
            while (size > 12f)
            {
                using var f = new Font("Segoe UI Semibold", size, FontStyle.Bold);
                var sz = TextRenderer.MeasureText(_lblTitle.Text, f);
                if (sz.Width <= available) break;
                size -= 1f;
            }
            if (Math.Abs(_lblTitle.Font.Size - size) > 0.1f)
                _lblTitle.Font = new Font("Segoe UI Semibold", size, FontStyle.Bold);
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

        private Button MakeButton(string text, int x, int y, int w, int h, EventHandler onClick)
        {
            var b = new Button { Text = text, Left = x, Top = y, Width = w, Height = h };
            b.Click += onClick;
            Controls.Add(b);
            return b;
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
            RelayoutAll();
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

            // Общий корень для относительных путей
            var baseDir = GetCommonRoot(_files.Select(f => f.FullPath).Concat(accepted));

            foreach (var path in accepted)
            {
                string title = MakeTitle(path, baseDir);
                title = EnsureUniqueTitle(title); // суффикс (1) при дубликате
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
            _lblListHeader.Left = _listPanel.Right - _lblListHeader.Width;
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
        }

        private int GetRowWidth()
        {
            // ClientSize уже учитывает вертикальный скроллбар, если он есть
            return _listPanel.ClientSize.Width - _listPanel.Padding.Horizontal - 2;
        }

        private void ClearAll()
        {
            _files.Clear();
            foreach (Control c in _listPanel.Controls.OfType<Control>().ToList())
                c.Dispose();
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
            // Если операция уже идёт — второй клик работает как отмена
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
            _btnMerge.Enabled = true;                       // кнопка Merge остаётся активной как «Отмена»
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
                try { File.Delete(dlg.FileName); } catch { /* частичный файл удаляем best-effort */ }
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
                _cts.Cancel();          // просим операцию остановиться
                e.Cancel = true;        // не закрываем окно, пока идёт merge
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
