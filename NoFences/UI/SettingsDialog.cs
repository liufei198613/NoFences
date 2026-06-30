using NoFences.Model;
using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace NoFences.UI
{
    public partial class SettingsDialog : Form
    {
        private readonly AppSettings settings;
        private readonly Panel contentPanel = new Panel();
        private readonly FlowLayoutPanel sidebar = new FlowLayoutPanel();
        private readonly DataGridView rulesGrid = new DataGridView();
        private readonly FlowLayoutPanel backupsPanel = new FlowLayoutPanel();

        private ComboBox languageCombo;
        private CheckBox startWithWindowsCheck;
        private CheckBox highPriorityCheck;
        private CheckBox autoExpandCheck;
        private CheckBox autoSwitchTabCheck;
        private CheckBox doubleClickFolderCheck;
        private CheckBox hideDesktopIconsCheck;
        private CheckBox animationCheck;
        private CheckBox hardwareCheck;
        private CheckBox autoArrangeCheck;
        private CheckBox organizeHiddenCheck;
        private CheckBox organizeOnStartupCheck;
        private CheckBox organizeRealtimeCheck;
        private CheckBox organizeMappedCheck;
        private ComboBox themeTargetCombo;
        private TrackBar opacityTrack;
        private TextBox backgroundImageText;
        private ComboBox titlePositionCombo;
        private ComboBox tabPositionCombo;
        private CheckBox showBorderCheck;
        private CheckBox roundedCheck;
        private CheckBox showTitleCheck;
        private CheckBox largeFontCheck;
        private CheckBox shortcutArrowCheck;
        private CheckBox iconOnlyCheck;
        private TextBox backupPathText;
        private CheckBox dailyBackupCheck;
        private TextBox showDesktopHotkeyText;
        private TextBox organizeHotkeyText;
        private CheckBox glassCheck;
        private TrackBar blurTrack;
        private CheckBox partitionAnimationCheck;
        private int selectedThemeColor;

        public SettingsDialog()
        {
            InitializeComponent();
            Controls.Clear();

            settings = AppSettings.Current;
            selectedThemeColor = settings.ThemeColorArgb;

            BuildShell();
            ShowGeneralPage();
        }

        private void BuildShell()
        {
            Text = "设置中心";
            Size = new Size(790, 535);
            MinimumSize = Size;
            MaximumSize = Size;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(31, 31, 39);
            ForeColor = Color.White;

            sidebar.Dock = DockStyle.Left;
            sidebar.Width = 152;
            sidebar.FlowDirection = FlowDirection.TopDown;
            sidebar.WrapContents = false;
            sidebar.Padding = new Padding(14, 14, 12, 0);
            sidebar.BackColor = Color.FromArgb(27, 28, 36);
            Controls.Add(sidebar);

            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Padding = new Padding(36, 22, 36, 54);
            contentPanel.BackColor = Color.FromArgb(31, 31, 39);
            Controls.Add(contentPanel);

            AddNavButton("常规设置", ShowGeneralPage);
            AddNavButton("整理规则", ShowOrganizePage);
            AddNavButton("主题设置", ShowThemePage);
            AddNavButton("备份桌面", ShowBackupPage);
            AddNavButton("精灵盒子", ShowWizardPage);
            AddNavButton("高级设置", ShowAdvancedPage);
            AddNavButton("关于我们", ShowAboutPage);

            var bottom = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 46,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 6, 24, 8),
                BackColor = Color.FromArgb(31, 31, 39)
            };
            Controls.Add(bottom);
            bottom.BringToFront();

            bottom.Controls.Add(MakeButton("确定", () => { SaveFromControls(); DialogResult = DialogResult.OK; Close(); }));
            bottom.Controls.Add(MakeButton("取消", () => { DialogResult = DialogResult.Cancel; Close(); }));
            bottom.Controls.Add(MakeButton("应用", () => SaveFromControls()));
        }

        private void AddNavButton(string text, Action action)
        {
            var button = new Button
            {
                Text = text,
                Width = 125,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(31, 31, 39),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Margin = new Padding(0, 0, 0, 7)
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += (s, e) =>
            {
                foreach (Button nav in sidebar.Controls.OfType<Button>())
                {
                    nav.BackColor = Color.FromArgb(31, 31, 39);
                }

                button.BackColor = Color.FromArgb(76, 76, 86);
                action();
            };
            sidebar.Controls.Add(button);
        }

        private void SelectNav(string text)
        {
            foreach (Button nav in sidebar.Controls.OfType<Button>())
            {
                nav.BackColor = nav.Text == text ? Color.FromArgb(76, 76, 86) : Color.FromArgb(31, 31, 39);
            }
        }

        private void ShowGeneralPage()
        {
            SelectNav("常规设置");
            var page = ResetPage();
            languageCombo = AddComboRow(page, "语言", new[] { "Chinese (Simplified) / 简体中文", "English" }, settings.Language);
            startWithWindowsCheck = AddCheck(page, "开机启动 NoFences", settings.StartWithWindows);
            highPriorityCheck = AddCheck(page, "高优先级加速启动 NoFences", settings.HighPriorityStartup);
            autoExpandCheck = AddCheck(page, "鼠标移动到标题栏自动展开分区", settings.AutoExpandOnTitleHover);
            autoSwitchTabCheck = AddCheck(page, "鼠标移动到标签时自动切换标签", settings.AutoSwitchTabOnHover);
            doubleClickFolderCheck = AddCheck(page, "在映射分区中双击文件夹查看内部文件", settings.DoubleClickFolderInPartition);
            hideDesktopIconsCheck = AddCheck(page, "双击隐藏桌面图标", settings.DoubleClickHideDesktopIcons);
            animationCheck = AddCheck(page, "自动开启系统窗口内的动画控件和元素", settings.EnableSystemAnimation);
            hardwareCheck = AddCheck(page, "开启硬件加速", settings.EnableHardwareAcceleration);
        }

        private void ShowOrganizePage()
        {
            SelectNav("整理规则");
            var page = ResetPage();
            autoArrangeCheck = AddCheck(page, "整理完成后自动重排图标", settings.AutoArrangeAfterOrganize);
            organizeHiddenCheck = AddCheck(page, "整理隐藏文件", settings.OrganizeHiddenFiles);
            AddSeparator(page, "新增文件自动整理");
            var row = AddRow(page);
            organizeOnStartupCheck = MakeCheck("程序启动时", settings.OrganizeOnStartup);
            organizeRealtimeCheck = MakeCheck("实时整理", settings.OrganizeRealtime);
            organizeMappedCheck = MakeCheck("整理映射分区", settings.OrganizeMappedPartitions);
            row.Controls.Add(organizeOnStartupCheck);
            row.Controls.Add(organizeRealtimeCheck);
            row.Controls.Add(organizeMappedCheck);
            AddSeparator(page, "自定义规则");
            BuildRulesGrid();
            page.Controls.Add(rulesGrid);
        }

        private void ShowThemePage()
        {
            SelectNav("主题设置");
            var page = ResetPage();
            themeTargetCombo = AddComboRow(page, "选择分区", new[] { "所有分区" }, settings.ThemeTarget);
            AddSeparator(page, "颜色");
            AddPalette(page);
            opacityTrack = AddTrack(page, "透明度", settings.PartitionOpacity, 20, 100);
            backgroundImageText = AddTextButtonRow(page, "背景图片", settings.BackgroundImagePath, "选择图片", PickBackgroundImage);
            titlePositionCombo = AddComboRow(page, "标题位置", new[] { "上边", "下边", "左边", "右边" }, settings.TitlePosition);
            tabPositionCombo = AddComboRow(page, "标签位置", new[] { "上边", "下边", "左边", "右边" }, settings.TabPosition);
            AddSeparator(page, "其它");
            var row1 = AddRow(page);
            showBorderCheck = MakeCheck("显示分区边框", settings.ShowPartitionBorder);
            roundedCheck = MakeCheck("分区圆角显示", settings.RoundedPartitionCorners);
            row1.Controls.Add(showBorderCheck);
            row1.Controls.Add(roundedCheck);
            showTitleCheck = AddCheck(page, "显示分区标题", settings.ShowPartitionTitle);
            var row2 = AddRow(page);
            largeFontCheck = MakeCheck("大字体显示", settings.LargeFont);
            shortcutArrowCheck = MakeCheck("快捷方式小箭头", settings.CompactShortcutArrow);
            iconOnlyCheck = MakeCheck("仅显示图标", settings.IconOnly);
            row2.Controls.Add(largeFontCheck);
            row2.Controls.Add(shortcutArrowCheck);
            row2.Controls.Add(iconOnlyCheck);
        }

        private void ShowBackupPage()
        {
            SelectNav("备份桌面");
            var page = ResetPage();
            var top = AddRow(page);
            top.Controls.Add(MakeButton("导出", ExportBackup));
            top.Controls.Add(MakeButton("导入", ImportBackup));
            AddSeparator(page, "备份布局");
            backupPathText = AddTextButtonRow(page, "备份路径", settings.BackupPath, "更改", PickBackupPath);
            dailyBackupCheck = AddCheck(page, "每天自动备份布局（最多保留7天）", settings.DailyBackupLayout);
            backupsPanel.Width = 560;
            backupsPanel.Height = 160;
            backupsPanel.FlowDirection = FlowDirection.LeftToRight;
            backupsPanel.WrapContents = true;
            backupsPanel.BackColor = Color.Transparent;
            page.Controls.Add(backupsPanel);
            RefreshBackupList();
            var buttons = AddRow(page);
            buttons.Margin = new Padding(14, 18, 0, 0);
            buttons.Controls.Add(MakeButton("备份", CreateBackup));
            buttons.Controls.Add(MakeButton("应用", RefreshBackupList));
            buttons.Controls.Add(MakeButton("删除", DeleteSelectedBackups));
        }

        private void ShowWizardPage()
        {
            SelectNav("精灵盒子");
            var page = ResetPage();
            page.Controls.Add(MakeLabel("精灵盒子功能预留中。后续可在这里放置快捷工具、小组件和自动化动作。", 520));
        }

        private void ShowAdvancedPage()
        {
            SelectNav("高级设置");
            var page = ResetPage();
            showDesktopHotkeyText = AddTextRow(page, "置顶桌面快捷键", settings.ShowDesktopHotkey);
            organizeHotkeyText = AddTextRow(page, "整理桌面快捷键", settings.OrganizeDesktopHotkey);
            glassCheck = AddCheck(page, "分区毛玻璃效果（不支持动态壁纸）", settings.GlassEffect);
            blurTrack = AddTrack(page, "模糊度", settings.BlurAmount, 0, 100);
            partitionAnimationCheck = AddCheck(page, "开启分区动画效果", settings.PartitionAnimation);
        }

        private void ShowAboutPage()
        {
            SelectNav("关于我们");
            var page = ResetPage();
            page.Controls.Add(MakeLabel("NoFences", 520, 16, FontStyle.Bold));
            page.Controls.Add(MakeLabel("一个轻量的桌面分区工具。", 520));
        }

        private FlowLayoutPanel ResetPage()
        {
            contentPanel.Controls.Clear();
            var page = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.FromArgb(31, 31, 39)
            };
            contentPanel.Controls.Add(page);
            return page;
        }

        private FlowLayoutPanel AddRow(Control parent)
        {
            var row = new FlowLayoutPanel
            {
                Width = 570,
                Height = 34,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            parent.Controls.Add(row);
            return row;
        }

        private ComboBox AddComboRow(Control parent, string label, string[] values, string selected)
        {
            var row = AddRow(parent);
            row.Controls.Add(MakeLabel(label, 76));
            var combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 275,
                BackColor = Color.FromArgb(27, 28, 36),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            combo.Items.AddRange(values);
            combo.SelectedItem = values.Contains(selected) ? selected : values[0];
            row.Controls.Add(combo);
            return combo;
        }

        private TextBox AddTextRow(Control parent, string label, string text)
        {
            var row = AddRow(parent);
            row.Controls.Add(MakeLabel(label, 118));
            var box = MakeTextBox(text, 250);
            row.Controls.Add(box);
            return box;
        }

        private TextBox AddTextButtonRow(Control parent, string label, string text, string buttonText, Action action)
        {
            var row = AddRow(parent);
            row.Controls.Add(MakeLabel(label, 76));
            var box = MakeTextBox(text, 380);
            row.Controls.Add(box);
            row.Controls.Add(MakeButton(buttonText, action));
            return box;
        }

        private TrackBar AddTrack(Control parent, string label, int value, int min, int max)
        {
            var row = AddRow(parent);
            row.Controls.Add(MakeLabel(label, 76));
            var track = new TrackBar
            {
                Width = 470,
                Minimum = min,
                Maximum = max,
                Value = Math.Max(min, Math.Min(max, value)),
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(31, 31, 39)
            };
            row.Controls.Add(track);
            return track;
        }

        private CheckBox AddCheck(Control parent, string text, bool isChecked)
        {
            var check = MakeCheck(text, isChecked);
            check.Width = 560;
            parent.Controls.Add(check);
            return check;
        }

        private CheckBox MakeCheck(string text, bool isChecked)
        {
            return new CheckBox
            {
                Text = text,
                Checked = isChecked,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Width = 150,
                Height = 34,
                FlatStyle = FlatStyle.Flat
            };
        }

        private Label MakeLabel(string text, int width, int fontSize = 10, FontStyle style = FontStyle.Regular)
        {
            return new Label
            {
                Text = text,
                ForeColor = Color.White,
                Width = width,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Microsoft YaHei UI", fontSize, style)
            };
        }

        private TextBox MakeTextBox(string text, int width)
        {
            return new TextBox
            {
                Text = text ?? "",
                Width = width,
                BackColor = Color.FromArgb(27, 28, 36),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private Button MakeButton(string text, Action action)
        {
            var button = new Button
            {
                Text = text,
                Width = 84,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(54, 54, 64),
                ForeColor = Color.White,
                Margin = new Padding(6, 0, 0, 0)
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 90);
            button.Click += (s, e) => action();
            return button;
        }

        private void AddSeparator(Control parent, string text)
        {
            var row = AddRow(parent);
            row.Controls.Add(MakeLabel(text, 130));
            var line = new Panel
            {
                Width = 420,
                Height = 1,
                BackColor = Color.FromArgb(70, 70, 80),
                Margin = new Padding(6, 14, 0, 0)
            };
            row.Controls.Add(line);
        }

        private void AddPalette(Control parent)
        {
            var row = AddRow(parent);
            row.Padding = new Padding(14, 0, 0, 0);
            var colors = new[]
            {
                0xFF55E6C1, 0xFF81ECEC, 0xFF74B9FF, 0xFFA29BFE, 0xFFDDE2E8,
                0xFF00B894, 0xFF00CEC9, 0xFF0984E3, 0xFF6C5CE7, 0xFFF1C40F,
                0xFFFAB1A0, 0xFFFF7675, 0xFF636E72, 0xFFFDCB6E, 0xFFE17055,
                0xFFD63031, 0xFFE84393, 0xFF00C8FF
            };

            foreach (var colorValue in colors)
            {
                var swatch = new Panel
                {
                    Width = 26,
                    Height = 24,
                    BackColor = Color.FromArgb(unchecked((int)colorValue)),
                    Margin = new Padding(0, 3, 6, 0),
                    BorderStyle = unchecked((int)colorValue) == selectedThemeColor ? BorderStyle.Fixed3D : BorderStyle.None
                };
                swatch.Click += (s, e) =>
                {
                    selectedThemeColor = unchecked((int)colorValue);
                    foreach (Panel panel in row.Controls.OfType<Panel>())
                    {
                        panel.BorderStyle = BorderStyle.None;
                    }
                    swatch.BorderStyle = BorderStyle.Fixed3D;
                };
                row.Controls.Add(swatch);
            }
        }

        private void BuildRulesGrid()
        {
            rulesGrid.Width = 555;
            rulesGrid.Height = 200;
            rulesGrid.BackgroundColor = Color.FromArgb(31, 31, 39);
            rulesGrid.BorderStyle = BorderStyle.None;
            rulesGrid.RowHeadersVisible = false;
            rulesGrid.AllowUserToAddRows = false;
            rulesGrid.AllowUserToDeleteRows = true;
            rulesGrid.AutoGenerateColumns = false;
            rulesGrid.ForeColor = Color.White;
            rulesGrid.GridColor = Color.FromArgb(70, 70, 80);
            rulesGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(54, 54, 64);
            rulesGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            rulesGrid.EnableHeadersVisualStyles = false;
            rulesGrid.DefaultCellStyle.BackColor = Color.FromArgb(31, 31, 39);
            rulesGrid.DefaultCellStyle.ForeColor = Color.White;
            rulesGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(76, 76, 86);
            rulesGrid.Columns.Clear();
            rulesGrid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "", DataPropertyName = "Enabled", Width = 28 });
            rulesGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "标题", DataPropertyName = "Title", Width = 96 });
            rulesGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "文件名", DataPropertyName = "FileName", Width = 85 });
            rulesGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "后缀", DataPropertyName = "Extensions", Width = 245 });
            rulesGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "分区", DataPropertyName = "TargetPartition", Width = 95 });
            rulesGrid.DataSource = settings.OrganizeRules.Select(r => new OrganizeRule
            {
                Enabled = r.Enabled,
                Title = r.Title,
                FileName = r.FileName,
                Extensions = r.Extensions,
                TargetPartition = r.TargetPartition
            }).ToList();
        }

        private void SaveFromControls()
        {
            if (languageCombo != null) settings.Language = languageCombo.Text;
            if (startWithWindowsCheck != null) settings.StartWithWindows = startWithWindowsCheck.Checked;
            if (highPriorityCheck != null) settings.HighPriorityStartup = highPriorityCheck.Checked;
            if (autoExpandCheck != null) settings.AutoExpandOnTitleHover = autoExpandCheck.Checked;
            if (autoSwitchTabCheck != null) settings.AutoSwitchTabOnHover = autoSwitchTabCheck.Checked;
            if (doubleClickFolderCheck != null) settings.DoubleClickFolderInPartition = doubleClickFolderCheck.Checked;
            if (hideDesktopIconsCheck != null) settings.DoubleClickHideDesktopIcons = hideDesktopIconsCheck.Checked;
            if (animationCheck != null) settings.EnableSystemAnimation = animationCheck.Checked;
            if (hardwareCheck != null) settings.EnableHardwareAcceleration = hardwareCheck.Checked;
            if (autoArrangeCheck != null) settings.AutoArrangeAfterOrganize = autoArrangeCheck.Checked;
            if (organizeHiddenCheck != null) settings.OrganizeHiddenFiles = organizeHiddenCheck.Checked;
            if (organizeOnStartupCheck != null) settings.OrganizeOnStartup = organizeOnStartupCheck.Checked;
            if (organizeRealtimeCheck != null) settings.OrganizeRealtime = organizeRealtimeCheck.Checked;
            if (organizeMappedCheck != null) settings.OrganizeMappedPartitions = organizeMappedCheck.Checked;
            if (themeTargetCombo != null) settings.ThemeTarget = themeTargetCombo.Text;
            settings.ThemeColorArgb = selectedThemeColor;
            if (opacityTrack != null) settings.PartitionOpacity = opacityTrack.Value;
            if (backgroundImageText != null) settings.BackgroundImagePath = backgroundImageText.Text;
            if (titlePositionCombo != null) settings.TitlePosition = titlePositionCombo.Text;
            if (tabPositionCombo != null) settings.TabPosition = tabPositionCombo.Text;
            if (showBorderCheck != null) settings.ShowPartitionBorder = showBorderCheck.Checked;
            if (roundedCheck != null) settings.RoundedPartitionCorners = roundedCheck.Checked;
            if (showTitleCheck != null) settings.ShowPartitionTitle = showTitleCheck.Checked;
            if (largeFontCheck != null) settings.LargeFont = largeFontCheck.Checked;
            if (shortcutArrowCheck != null) settings.CompactShortcutArrow = shortcutArrowCheck.Checked;
            if (iconOnlyCheck != null) settings.IconOnly = iconOnlyCheck.Checked;
            if (backupPathText != null) settings.BackupPath = backupPathText.Text;
            if (dailyBackupCheck != null) settings.DailyBackupLayout = dailyBackupCheck.Checked;
            if (showDesktopHotkeyText != null) settings.ShowDesktopHotkey = showDesktopHotkeyText.Text;
            if (organizeHotkeyText != null) settings.OrganizeDesktopHotkey = organizeHotkeyText.Text;
            if (glassCheck != null) settings.GlassEffect = glassCheck.Checked;
            if (blurTrack != null) settings.BlurAmount = blurTrack.Value;
            if (partitionAnimationCheck != null) settings.PartitionAnimation = partitionAnimationCheck.Checked;

            if (rulesGrid.DataSource is System.Collections.Generic.List<OrganizeRule> rules)
            {
                settings.OrganizeRules = rules;
            }

            settings.Save();
            settings.ApplyRuntimeSettings();
            foreach (var form in Application.OpenForms.OfType<FenceWindow>())
            {
                form.Refresh();
            }
        }

        private void PickBackgroundImage()
        {
            using (var dialog = new OpenFileDialog { Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp|所有文件|*.*" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    backgroundImageText.Text = dialog.FileName;
                }
            }
        }

        private void PickBackupPath()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    backupPathText.Text = dialog.SelectedPath;
                    settings.BackupPath = dialog.SelectedPath;
                    RefreshBackupList();
                }
            }
        }

        private void ExportBackup()
        {
            using (var dialog = new SaveFileDialog { Filter = "NoFences 备份|*.zip", FileName = $"NoFences-{DateTime.Now:yyyyMMddHHmmss}.zip" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    CreateBackup(dialog.FileName);
                }
            }
        }

        private void ImportBackup()
        {
            using (var dialog = new OpenFileDialog { Filter = "NoFences 备份|*.zip|所有文件|*.*" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                ZipFile.ExtractToDirectory(dialog.FileName, AppSettings.GetDataPath(), true);
                MessageBox.Show(this, "导入完成，重启 NoFences 后会加载新的布局。", "导入备份", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CreateBackup()
        {
            SaveFromControls();
            Directory.CreateDirectory(settings.BackupPath);
            CreateBackup(Path.Combine(settings.BackupPath, $"NoFences-{DateTime.Now:yyyyMMddHHmmss}.zip"));
            RefreshBackupList();
        }

        private void CreateBackup(string zipPath)
        {
            var source = AppSettings.GetDataPath();
            var backupRoot = Path.GetFullPath(settings.BackupPath);
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
                {
                    var full = Path.GetFullPath(file);
                    if (full.StartsWith(backupRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    archive.CreateEntryFromFile(full, Path.GetRelativePath(source, full));
                }
            }
        }

        private void RefreshBackupList()
        {
            backupsPanel.Controls.Clear();
            var path = backupPathText?.Text ?? settings.BackupPath;
            if (!Directory.Exists(path))
            {
                return;
            }

            foreach (var file in Directory.EnumerateFiles(path, "*.zip").OrderByDescending(File.GetCreationTime).Take(12))
            {
                var check = new CheckBox
                {
                    Text = Path.GetFileNameWithoutExtension(file).Replace("NoFences-", ""),
                    Tag = file,
                    Width = 165,
                    Height = 42,
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(42, 42, 50),
                    Appearance = Appearance.Button,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                backupsPanel.Controls.Add(check);
            }
        }

        private void DeleteSelectedBackups()
        {
            foreach (var check in backupsPanel.Controls.OfType<CheckBox>().Where(c => c.Checked).ToList())
            {
                try
                {
                    File.Delete((string)check.Tag);
                }
                catch
                {
                }
            }

            RefreshBackupList();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            SaveFromControls();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
