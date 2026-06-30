using NoFences.Model;
using NoFences.Util;
using NoFences.Win32;
using NoFences.Components;
using Peter;
using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using static NoFences.Win32.WindowUtil;

namespace NoFences
{
    public partial class FenceWindow : Form
    {
        private int logicalTitleHeight;
        private int titleHeight;
        private const int titleOffset = 3;
        private const int itemWidth = 75;
        private const int itemHeight = 32 + itemPadding + textHeight;
        private const int textHeight = 35;
        private const int itemPadding = 15;
        private const float shadowDist = 1.5f;

        private readonly FenceInfo fenceInfo;

        private Font titleFont;
        private Font iconFont;

        private string selectedItem;
        private string hoveringItem;
        private bool shouldUpdateSelection;
        private bool shouldRunDoubleClick;
        private bool hasSelectionUpdated;
        private bool hasHoverUpdated;
        private bool isMinified;
        private int prevHeight;

        private int scrollHeight;
        private int scrollOffset;

        private readonly ThrottledExecution throttledMove = new ThrottledExecution(TimeSpan.FromSeconds(4));
        private readonly ThrottledExecution throttledResize = new ThrottledExecution(TimeSpan.FromSeconds(4));

        private readonly ShellContextMenu shellContextMenu = new ShellContextMenu();

        private readonly ThumbnailProvider thumbnailProvider = new ThumbnailProvider();

        // Component system
        private readonly ComponentRenderer componentRenderer = new ComponentRenderer();
        private IComponent activeComponent;

        // Drag and drop for icon reordering
        private string draggedItem = null;
        private Point dragStartPos = Point.Empty;

        // Top-right corner buttons
        private const int ButtonSize = 24;
        private Rectangle _lockButtonRect;
        private Rectangle _menuButtonRect;
        private TextBox titleEditor;

        private void ReloadFonts()
        {
            var family = new FontFamily("Segoe UI");
            titleFont = new Font(family, (int)Math.Floor(logicalTitleHeight / 2.0));
            iconFont = new Font(family, 9);
        }

        public FenceWindow(FenceInfo fenceInfo)
        {
            InitializeComponent();
            DropShadow.ApplyShadows(this);
            BlurUtil.EnableBlur(Handle);
            WindowUtil.HideFromAltTab(Handle);
            DesktopUtil.GlueToDesktop(Handle);
            DesktopUtil.PreventMinimize(Handle);
            logicalTitleHeight = (fenceInfo.TitleHeight < 16 || fenceInfo.TitleHeight > 100) ? 35 : fenceInfo.TitleHeight;
            titleHeight = LogicalToDeviceUnits(logicalTitleHeight);
            
            this.MouseWheel += FenceWindow_MouseWheel;
            thumbnailProvider.IconThumbnailLoaded += ThumbnailProvider_IconThumbnailLoaded;
            DragOver += FenceWindow_DragOver;

            ReloadFonts();

            AllowDrop = true;


            this.fenceInfo = fenceInfo;
            Text = fenceInfo.Name;
            Location = new Point(fenceInfo.PosX, fenceInfo.PosY);

            Width = fenceInfo.Width;
            Height = fenceInfo.Height;

            prevHeight = Height;
            lockedToolStripMenuItem.Checked = fenceInfo.Locked;
            lockedLayoutToolStripMenuItem.Checked = fenceInfo.LockedLayout;
            minifyToolStripMenuItem.Checked = fenceInfo.CanMinify;

            // Initialize alignment settings
            if (fenceInfo.Alignment == null)
            {
                fenceInfo.Alignment = new AlignmentSettings();
            }
            enableSnappingToolStripMenuItem.Checked = fenceInfo.Alignment.EnableSnapping;
            enableEdgeAlignmentToolStripMenuItem.Checked = fenceInfo.Alignment.EnableEdgeAlignment;
            enableGridAlignmentToolStripMenuItem.Checked = fenceInfo.Alignment.EnableGridAlignment;

            // Initialize components
            InitializeComponents();

            // Initialize top-right buttons
            UpdateButtonPositions();

            Minify();
        }

        private void UpdateButtonPositions()
        {
            // Update button positions when window size changes
            _lockButtonRect = new Rectangle(Width - ButtonSize * 2 - 5, 5, ButtonSize, ButtonSize);
            _menuButtonRect = new Rectangle(Width - ButtonSize - 5, 5, ButtonSize, ButtonSize);
        }

        protected override void WndProc(ref Message m)
        {
            //Console.WriteLine(m.Msg.ToString("X4"));

            // Remove border
            if (m.Msg == 0x0083)
            {
                m.Result = IntPtr.Zero;
                return;
            }

            // Mouse leave
            var myrect = new Rectangle(Location, Size);
            if (m.Msg == 0x02a2 && !myrect.IntersectsWith(new Rectangle(MousePosition, new Size(1, 1))))
            {
                Minify();
            }

            // Prevent maximize
            if ((m.Msg == WM_SYSCOMMAND) && m.WParam.ToInt32() == 0xF032)
            {
                m.Result = IntPtr.Zero;
                return;
            }

            // Double-clicking the draggable title area should rename the fence.
            if (m.Msg == 0x00A3) // WM_NCLBUTTONDBLCLK
            {
                if (m.WParam.ToInt32() == HTCAPTION)
                {
                    BeginInvoke(new Action(BeginTitleEdit));
                    m.Result = IntPtr.Zero;
                    return;
                }
            }

            // Handle WM_NCHITTEST for proper click-through with buttons
            if (m.Msg == WM_NCHITTEST)
            {
                var pt = PointToClient(GetPointFromLParam(m.LParam));
                
                // Check if mouse is over the top-right buttons - we need to capture this click
                if (titleEditor != null && titleEditor.Visible && titleEditor.Bounds.Contains(pt))
                {
                    m.Result = (IntPtr)HTCLIENT;
                    return;
                }

                if (_lockButtonRect.Contains(pt) || _menuButtonRect.Contains(pt))
                {
                    // Let the window process this click, don't let it pass through
                    // For buttons, always return HTCLIENT so clicks get handled by MouseDown event
                    m.Result = (IntPtr)HTCLIENT;
                    return;
                }

                var isLocked = lockedToolStripMenuItem.Checked;

                if (pt.Y < titleHeight)
                {
                    if (!isLocked)
                    {
                        FenceWindow_MouseEnter(null, null);
                    }
                    m.Result = isLocked ? (IntPtr)HTCLIENT : (IntPtr)HTCAPTION;
                    return;
                }
                
                // Check if mouse is over a component - let it handle the click
                var component = componentRenderer.GetComponentAt(pt);
                if (component != null)
                {
                    m.Result = (IntPtr)HTCLIENT;
                    return;
                }
                
                // Check if clicking on an icon - let it handle the click
                bool overItem = false;
                var x = itemPadding;
                var y = itemPadding;
                foreach (var file in fenceInfo.Files)
                {
                    var itemRect = new Rectangle(x, y + titleHeight - scrollOffset, itemWidth, itemHeight);
                    if (itemRect.Contains(pt))
                    {
                        overItem = true;
                        break;
                    }
                    x += itemWidth + itemPadding;
                    if (x + itemWidth > Width)
                    {
                        x = itemPadding;
                        y += itemHeight + itemPadding;
                    }
                }
                
                if (overItem || !isLocked)
                {
                    m.Result = (IntPtr)HTCLIENT;
                }
                else
                {
                    // For empty areas, allow click-through
                    m.Result = (IntPtr)HTTRANSPARENT;
                }
                    
                // If locked (position and resize disabled), skip drag/resize handling
                if (!isLocked)
                {
                    // Edge resizing (edges)
                    if (pt.X < 10 && pt.Y < 10)
                        m.Result = new IntPtr(HTTOPLEFT);
                    else if (pt.X > (Width - 10) && pt.Y < 10)
                        m.Result = new IntPtr(HTTOPRIGHT);
                    else if (pt.X < 10 && pt.Y > (Height - 10))
                        m.Result = new IntPtr(HTBOTTOMLEFT);
                    else if (pt.X > (Width - 10) && pt.Y > (Height - 10))
                        m.Result = new IntPtr(HTBOTTOMRIGHT);
                    else if (pt.Y > (Height - 10))
                        m.Result = new IntPtr(HTBOTTOM);
                    else if (pt.X < 10)
                        m.Result = new IntPtr(HTLEFT);
                    else if (pt.X > (Width - 10))
                        m.Result = new IntPtr(HTRIGHT);
                }
                
                return;
            }

            // Other messages
            base.WndProc(ref m);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "确定要移除这个分区吗？", "移除分区", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                FenceManager.Instance.RemoveFence(fenceInfo);
                Close();
            }
        }

        private void deleteItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteFenceItem(hoveringItem);
            hoveringItem = null;
            Save();
            Refresh();
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            deleteItemToolStripMenuItem.Visible = hoveringItem != null;
        }

        private void FenceWindow_DragEnter(object sender, DragEventArgs e)
        {
            SetFileDropEffect(e);
        }

        private void FenceWindow_DragOver(object sender, DragEventArgs e)
        {
            SetFileDropEffect(e);
        }

        private void FenceWindow_DragDrop(object sender, DragEventArgs e)
        {
            SetFileDropEffect(e);

            var dropped = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in dropped)
            {
                if (!ItemExists(file))
                    continue;

                try
                {
                    var movedPath = MoveItemIntoFence(file);
                    if (!fenceInfo.Files.Contains(movedPath))
                    {
                        fenceInfo.Files.Add(movedPath);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"无法移动项目：{Path.GetFileName(file)}\n{ex.Message}",
                        "移动失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            Save();
            Refresh();
        }

        private void SetFileDropEffect(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop) || lockedToolStripMenuItem.Checked)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = e.AllowedEffect.HasFlag(DragDropEffects.Move)
                ? DragDropEffects.Move
                : DragDropEffects.None;
        }

        private void FenceWindow_Resize(object sender, EventArgs e)
        {
            throttledResize.Run(() =>
            {
                fenceInfo.Width = Width;
                fenceInfo.Height = isMinified ? prevHeight : Height;
                Save();
            });

            UpdateButtonPositions();
            Refresh();
        }

        private void FenceWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left &&
                draggedItem != null &&
                ItemExists(draggedItem) &&
                HasMovedBeyondDragThreshold(e.Location, dragStartPos))
            {
                DragFenceItemOut(draggedItem);
                return;
            }

            Refresh();
        }

        private void FenceWindow_MouseEnter(object sender, EventArgs e)
        {
            if (minifyToolStripMenuItem.Checked && isMinified)
            {
                isMinified = false;
                Height = prevHeight;
            }
        }

        private void FenceWindow_MouseLeave(object sender, EventArgs e)
        {
            Minify();
            selectedItem = null;
            Refresh();
        }

        private void Minify()
        {
            if (minifyToolStripMenuItem.Checked && !isMinified)
            {
                isMinified = true;
                prevHeight = Height;
                Height = titleHeight;
                Refresh();
            }
        }

        private void minifyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isMinified)
            {
                Height = prevHeight;
                isMinified = false;
            }
            fenceInfo.CanMinify = minifyToolStripMenuItem.Checked;
            Save();

        }

        private void FenceWindow_Click(object sender, EventArgs e)
        {
            shouldUpdateSelection = true;
            Refresh();
        }

        private void FenceWindow_DoubleClick(object sender, EventArgs e)
        {
            var pt = PointToClient(MousePosition);
            if (pt.Y < titleHeight && !_lockButtonRect.Contains(pt) && !_menuButtonRect.Contains(pt))
            {
                BeginTitleEdit();
                return;
            }

            shouldRunDoubleClick = true;
            Refresh();
        }

        private void FenceWindow_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clip = new Region(ClientRectangle);
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var appSettings = AppSettings.Current;

            // Background
            var themeColor = Color.FromArgb(appSettings.ThemeColorArgb);
            var backgroundOpacity = Math.Max(0, Math.Min(230, appSettings.PartitionOpacity * 230 / 100));
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(backgroundOpacity, themeColor)), ClientRectangle);
            if (appSettings.ShowPartitionBorder)
            {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(170, themeColor)), new Rectangle(0, 0, Width - 1, Height - 1));
            }

            // Title
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(70, Color.Black)), new RectangleF(0, 0, Width, titleHeight));
            if (appSettings.ShowPartitionTitle && (titleEditor == null || !titleEditor.Visible))
            {
                e.Graphics.DrawString(Text, titleFont, Brushes.White, new PointF(Width / 2, titleOffset), new StringFormat { Alignment = StringAlignment.Center });
            }

            // Draw top-right buttons
            DrawLockButton(e.Graphics);
            DrawMenuButton(e.Graphics);

            // Items
            var x = itemPadding;
            var y = itemPadding;
            scrollHeight = 0;
            e.Graphics.Clip = new Region(new Rectangle(0, titleHeight, Width, Height - titleHeight));
            foreach (var file in fenceInfo.Files)
            {
                var entry = FenceEntry.FromPath(file);
                if (entry == null)
                    continue;

                RenderEntry(e.Graphics, entry, x, y + titleHeight - scrollOffset);

                var itemBottom = y + itemHeight;
                if (itemBottom > scrollHeight)
                    scrollHeight = itemBottom;

                x += itemWidth + itemPadding;
                if (x + itemWidth > Width)
                {
                    x = itemPadding;
                    y += itemHeight + itemPadding;
                }
            }

            scrollHeight -= (ClientRectangle.Height - titleHeight);

            // Scroll bars
            if (scrollHeight > 0)
            {
                var contentHeight = Height - titleHeight;
                var scrollbarHeight = contentHeight - scrollHeight;
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(150, Color.Black)), new Rectangle(Width - 5, titleHeight + scrollOffset, 5, scrollbarHeight));

                scrollOffset = Math.Min(scrollOffset, scrollHeight);
            }

            // Render components (calendar, todo list, etc.)
            e.Graphics.Clip = new Region(ClientRectangle);
            componentRenderer.RenderAll(e.Graphics);


            // Click handlers
            if (shouldUpdateSelection && !hasSelectionUpdated)
                selectedItem = null;

            if (!hasHoverUpdated)
                hoveringItem = null;

            shouldRunDoubleClick = false;
            shouldUpdateSelection = false;
            hasSelectionUpdated = false;
            hasHoverUpdated = false;
        }

        private void RenderEntry(Graphics g, FenceEntry entry, int x, int y)
        {
            var icon = entry.ExtractIcon(thumbnailProvider);
            var name = entry.Name;

            var textPosition = new PointF(x, y + icon.Height + 5);
            var textMaxSize = new SizeF(itemWidth, textHeight);

            var stringFormat = new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };

            var textSize = g.MeasureString(name, iconFont, textMaxSize, stringFormat);
            var outlineRect = new Rectangle(x - 2, y - 2, itemWidth + 2, icon.Height + (int)textSize.Height + 5 + 2);
            var outlineRectInner = outlineRect.Shrink(1);

            var mousePos = PointToClient(MousePosition);
            var mouseOver = mousePos.X >= x && mousePos.Y >= y && mousePos.X < x + outlineRect.Width && mousePos.Y < y + outlineRect.Height;

            if (mouseOver)
            {
                hoveringItem = entry.Path;
                hasHoverUpdated = true;
            }

            if (mouseOver && shouldUpdateSelection)
            {
                selectedItem = entry.Path;
                shouldUpdateSelection = false;
                hasSelectionUpdated = true;
            }

            if (mouseOver && shouldRunDoubleClick)
            {
                shouldRunDoubleClick = false;
                entry.Open();
            }

            if (selectedItem == entry.Path)
            {
                if (mouseOver)
                {
                    g.DrawRectangle(new Pen(Color.FromArgb(120, SystemColors.ActiveBorder)), outlineRectInner);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(100, SystemColors.GradientActiveCaption)), outlineRect);
                }
                else
                {
                    g.DrawRectangle(new Pen(Color.FromArgb(120, SystemColors.ActiveBorder)), outlineRectInner);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(80, SystemColors.GradientInactiveCaption)), outlineRect);
                }
            }
            else
            {
                if (mouseOver)
                {
                    g.DrawRectangle(new Pen(Color.FromArgb(120, SystemColors.ActiveBorder)), outlineRectInner);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(80, SystemColors.ActiveCaption)), outlineRect);
                }
            }

            g.DrawIcon(icon, x + itemWidth / 2 - icon.Width / 2, y);
            g.DrawString(name, iconFont, new SolidBrush(Color.FromArgb(180, 15, 15, 15)), new RectangleF(textPosition.Move(shadowDist, shadowDist), textMaxSize), stringFormat);
            g.DrawString(name, iconFont, Brushes.White, new RectangleF(textPosition, textMaxSize), stringFormat);
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BeginTitleEdit();
        }

        private void BeginTitleEdit()
        {
            if (isMinified)
            {
                isMinified = false;
                Height = prevHeight;
            }

            if (titleEditor == null)
            {
                titleEditor = new TextBox
                {
                    BorderStyle = BorderStyle.None,
                    Multiline = true,
                    BackColor = Color.Black,
                    ForeColor = Color.White,
                    Font = titleFont,
                    TextAlign = HorizontalAlignment.Center
                };
                titleEditor.KeyDown += TitleEditor_KeyDown;
                titleEditor.Leave += TitleEditor_Leave;
                Controls.Add(titleEditor);
            }

            var titleEditBounds = GetTitleEditorBounds();
            titleEditor.Bounds = titleEditBounds;
            titleEditor.Text = Text;
            titleEditor.Visible = true;
            titleEditor.BringToFront();
            titleEditor.SelectAll();
            titleEditor.Focus();
        }

        private Rectangle GetTitleEditorBounds()
        {
            var titleRight = Math.Max(0, _lockButtonRect.Left - 6);
            var editorWidth = Math.Max(80, titleRight / 3);
            editorWidth = Math.Min(editorWidth, Math.Max(80, titleRight - 12));
            var editorHeight = Math.Min(titleHeight, titleEditor.PreferredHeight);
            var editorX = Math.Max(0, (titleRight - editorWidth) / 2);
            var editorY = Math.Max(0, (titleHeight - editorHeight) / 2);

            return new Rectangle(editorX, editorY, editorWidth, editorHeight);
        }

        private void CommitTitleEdit()
        {
            if (titleEditor == null || !titleEditor.Visible)
                return;

            var newTitle = titleEditor.Text.Trim();
            titleEditor.Visible = false;
            if (newTitle.Length == 0 || newTitle == Text)
                return;

            Text = newTitle;
            fenceInfo.Name = Text;
            Refresh();
            Save();
        }

        private void CancelTitleEdit()
        {
            if (titleEditor != null)
            {
                titleEditor.Visible = false;
            }
        }

        private void TitleEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                CommitTitleEdit();
                Focus();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                CancelTitleEdit();
                Focus();
            }
        }

        private void TitleEditor_Leave(object sender, EventArgs e)
        {
            CommitTitleEdit();
        }

        private void newFenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FenceManager.Instance.CreateFence("新分区");
        }

        private void FenceWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private readonly object saveLock = new object();
        private void Save()
        {
            lock (saveLock)
            {
                FenceManager.Instance.UpdateFence(fenceInfo);
            }
        }

        private void FenceWindow_LocationChanged(object sender, EventArgs e)
        {
            throttledMove.Run(() =>
            {
                fenceInfo.PosX = Location.X;
                fenceInfo.PosY = Location.Y;
                Save();
            });
        }

        private void lockedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fenceInfo.Locked = lockedToolStripMenuItem.Checked;
            Save();
        }

        private void FenceWindow_Load(object sender, EventArgs e)
        {

        }

        private void titleSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new HeightDialog(fenceInfo.TitleHeight);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                fenceInfo.TitleHeight = dialog.TitleHeight;
                logicalTitleHeight = dialog.TitleHeight;
                titleHeight = LogicalToDeviceUnits(logicalTitleHeight);
                ReloadFonts();
                Minify();
                if (isMinified)
                {
                    Height = titleHeight;
                }
                Refresh();
                Save();
            }
        }

        private void FenceWindow_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            if (hoveringItem != null && !ModifierKeys.HasFlag(Keys.Shift))
            {
                shellContextMenu.ShowContextMenu(new[] { new FileInfo(hoveringItem) }, MousePosition);
            }
            else
            {
                appContextMenu.Show(this, e.Location);
            }
        }

        private void FenceWindow_MouseWheel(object sender, MouseEventArgs e)
        {
            if (scrollHeight < 1)
                return;

            scrollOffset -= Math.Sign(e.Delta) * 10;
            if (scrollOffset < 0)
                scrollOffset = 0;
            if (scrollOffset > scrollHeight)
                scrollOffset = scrollHeight;

            Invalidate();
        }

        private void ThumbnailProvider_IconThumbnailLoaded(object sender, EventArgs e)
        {
            Invalidate();
        }

        private bool ItemExists(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        private bool HasMovedBeyondDragThreshold(Point current, Point start)
        {
            return Math.Abs(current.X - start.X) >= SystemInformation.DragSize.Width ||
                   Math.Abs(current.Y - start.Y) >= SystemInformation.DragSize.Height;
        }

        private void DragFenceItemOut(string itemPath)
        {
            var data = new DataObject();
            data.SetData(DataFormats.FileDrop, new[] { itemPath });

            var result = DoDragDrop(data, DragDropEffects.Move);
            if (result == DragDropEffects.Move && !ItemExists(itemPath))
            {
                fenceInfo.Files.Remove(itemPath);
                hoveringItem = null;
                selectedItem = null;
                draggedItem = null;
                dragStartPos = Point.Empty;
                Save();
                Refresh();
            }
        }

        private void DeleteFenceItem(string itemPath)
        {
            if (string.IsNullOrWhiteSpace(itemPath))
                return;

            fenceInfo.Files.Remove(itemPath);

            if (IsPathInsideFenceItems(itemPath))
            {
                try
                {
                    if (File.Exists(itemPath))
                    {
                        File.Delete(itemPath);
                    }
                    else if (Directory.Exists(itemPath))
                    {
                        Directory.Delete(itemPath, true);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"无法删除项目：{Path.GetFileName(itemPath)}\n{ex.Message}",
                        "删除失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private bool IsPathInsideFenceItems(string path)
        {
            var itemStore = Path.GetFullPath(Path.Combine(FenceManager.Instance.GetFenceDataPath(fenceInfo), "Items"));
            var fullPath = Path.GetFullPath(path);
            return fullPath.StartsWith(itemStore + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }

        private string MoveItemIntoFence(string sourcePath)
        {
            var itemStore = Path.Combine(FenceManager.Instance.GetFenceDataPath(fenceInfo), "Items");
            Directory.CreateDirectory(itemStore);

            var fullSourcePath = Path.GetFullPath(sourcePath);
            var fullStorePath = Path.GetFullPath(itemStore);
            if (fullSourcePath.StartsWith(fullStorePath, StringComparison.OrdinalIgnoreCase))
            {
                return fullSourcePath;
            }

            var destinationPath = GetUniqueDestinationPath(itemStore, Path.GetFileName(sourcePath));
            if (File.Exists(sourcePath))
            {
                File.Move(sourcePath, destinationPath);
                ShellNotifyUtil.NotifyFileMoved(sourcePath, destinationPath);
            }
            else if (Directory.Exists(sourcePath))
            {
                Directory.Move(sourcePath, destinationPath);
                ShellNotifyUtil.NotifyFolderMoved(sourcePath, destinationPath);
            }
            else
            {
                throw new FileNotFoundException("项目不存在。", sourcePath);
            }

            return destinationPath;
        }

        private static string GetUniqueDestinationPath(string directory, string fileName)
        {
            var destinationPath = Path.Combine(directory, fileName);
            if (!File.Exists(destinationPath) && !Directory.Exists(destinationPath))
            {
                return destinationPath;
            }

            var name = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            for (var i = 2; ; i++)
            {
                var candidate = Path.Combine(directory, $"{name} ({i}){extension}");
                if (!File.Exists(candidate) && !Directory.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        // ===================
        // Component Management
        // ===================

        private void InitializeComponents()
        {
            if (fenceInfo.Components == null)
            {
                fenceInfo.Components = new List<ComponentInfo>();
            }

            var dataPath = FenceManager.Instance.GetFenceDataPath(fenceInfo);

            foreach (var componentInfo in fenceInfo.Components)
            {
                IComponent component = null;

                switch (componentInfo.Type)
                {
                    case ComponentType.Calendar:
                        component = new CalendarComponent(componentInfo.Id, dataPath);
                        break;
                    case ComponentType.TodoList:
                        component = new TodoComponent(componentInfo.Id, dataPath);
                        break;
                }

                if (component != null)
                {
                    component.Bounds = new Rectangle(
                        componentInfo.X, componentInfo.Y,
                        componentInfo.Width, componentInfo.Height);
                    componentRenderer.AddComponent(component);
                }
            }
        }

        private void AddComponent(IComponent component)
        {
            var info = new ComponentInfo
            {
                Id = component.Id,
                Type = component.Type,
                X = component.Bounds.X,
                Y = component.Bounds.Y,
                Width = component.Bounds.Width,
                Height = component.Bounds.Height
            };

            fenceInfo.Components.Add(info);
            componentRenderer.AddComponent(component);
            Save();
        }

        // ===================
        // Lock Layout
        // ===================

        private void lockedLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fenceInfo.LockedLayout = lockedLayoutToolStripMenuItem.Checked;
            Save();
        }

        // ===================
        // Sorting Functions
        // ===================

        private void SortFiles(int sortMode)
        {
            if (fenceInfo.LockedLayout)
            {
                MessageBox.Show("Cannot sort while layout is locked.", "Layout Locked",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            fenceInfo.SortMode = sortMode;

            switch (sortMode)
            {
                case 1: // Sort by Name
                    fenceInfo.Files.Sort((a, b) =>
                        string.Compare(Path.GetFileName(a), Path.GetFileName(b),
                            StringComparison.OrdinalIgnoreCase));
                    break;
                case 2: // Sort by Type
                    fenceInfo.Files.Sort((a, b) =>
                    {
                        var extA = Path.GetExtension(a);
                        var extB = Path.GetExtension(b);
                        return string.Compare(extA, extB, StringComparison.OrdinalIgnoreCase);
                    });
                    break;
                case 3: // Sort by Date
                    fenceInfo.Files.Sort((a, b) =>
                    {
                        var dateA = File.GetLastWriteTime(a);
                        var dateB = File.GetLastWriteTime(b);
                        return dateA.CompareTo(dateB);
                    });
                    break;
                case 0: // No sort (original order)
                    break;
            }

            if (!fenceInfo.SortAscending)
            {
                fenceInfo.Files.Reverse();
            }

            Save();
            Refresh();
        }

        private void sortByNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SortFiles(1);
        }

        private void sortByTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SortFiles(2);
        }

        private void sortByDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SortFiles(3);
        }

        private void sortClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SortFiles(0);
        }

        // ===================
        // Alignment Functions
        // ===================

        private void enableSnappingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fenceInfo.Alignment.EnableSnapping = enableSnappingToolStripMenuItem.Checked;
            Save();
        }

        private void enableEdgeAlignmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fenceInfo.Alignment.EnableEdgeAlignment = enableEdgeAlignmentToolStripMenuItem.Checked;
            Save();
        }

        private void enableGridAlignmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fenceInfo.Alignment.EnableGridAlignment = enableGridAlignmentToolStripMenuItem.Checked;
            Save();
        }

        private void alignmentSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open alignment settings dialog
            MessageBox.Show("Alignment settings dialog would open here.", "Alignment Settings",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ApplyAlignment()
        {
            if (!fenceInfo.Alignment.EnableSnapping)
                return;

            var currentRect = new Rectangle(Location, Size);
            var screenBounds = Screen.PrimaryScreen.Bounds;

            if (fenceInfo.Alignment.EnableEdgeAlignment)
            {
                var otherFences = AlignmentUtil.GetAllFenceWindows(Handle);
                var snappedPos = AlignmentUtil.CalculateSnappedPosition(
                    currentRect, otherFences, screenBounds, fenceInfo.Alignment.SnapThreshold);

                if (snappedPos != currentRect.Location)
                {
                    Location = snappedPos;
                }
            }

            if (fenceInfo.Alignment.EnableGridAlignment)
            {
                var gridPos = AlignmentUtil.AlignToGrid(Location, fenceInfo.Alignment.GridSize);
                if (gridPos != Location)
                {
                    Location = gridPos;
                }
            }
        }

        // ===================
        // Add Components
        // ===================

        private void addCalendarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dataPath = FenceManager.Instance.GetFenceDataPath(fenceInfo);
            IComponent component = new CalendarComponent(Guid.NewGuid().ToString(), dataPath);

            // Default position
            var bounds = new Rectangle(20, titleHeight + 20, 300, 280);
            component.Bounds = bounds;

            AddComponent(component);

            // Adjust window size if needed
            if (bounds.Bottom > Height)
            {
                Height = bounds.Bottom + 20;
            }
            if (bounds.Right > Width)
            {
                Width = bounds.Right + 20;
            }

            Refresh();
        }

        private void addTodoListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dataPath = FenceManager.Instance.GetFenceDataPath(fenceInfo);
            IComponent component = new TodoComponent(Guid.NewGuid().ToString(), dataPath);

            // Default position
            var bounds = new Rectangle(20, titleHeight + 20, 280, 350);
            component.Bounds = bounds;

            AddComponent(component);

            // Adjust window size if needed
            if (bounds.Bottom > Height)
            {
                Height = bounds.Bottom + 20;
            }
            if (bounds.Right > Width)
            {
                Width = bounds.Right + 20;
            }

            Refresh();
        }

        // ===================
        // Mouse Event Extensions
        // ===================

        private void FenceWindow_MouseUp(object sender, MouseEventArgs e)
        {
            if (activeComponent != null)
            {
                var relativePos = new Point(
                    e.X - activeComponent.Bounds.X,
                    e.Y - activeComponent.Bounds.Y);
                activeComponent.HandleMouseUp(relativePos);
                activeComponent = null;
                Refresh();
            }

            draggedItem = null;
            dragStartPos = Point.Empty;
        }

        // ===================
        // Top-right Buttons
        // ===================

        private void DrawLockButton(Graphics g)
        {
            // Draw button background
            var mousePos = PointToClient(MousePosition);
            var isHover = _lockButtonRect.Contains(mousePos);
            
            var bgColor = isHover ? Color.FromArgb(80, 80, 80) : Color.FromArgb(50, 50, 50);
            g.FillRectangle(new SolidBrush(bgColor), _lockButtonRect);
            g.DrawRectangle(Pens.Gray, _lockButtonRect);

            // Draw lock icon (simple text-based representation)
            var lockIcon = fenceInfo.Locked ? "🔒" : "🔓";
            var font = new Font("Segoe UI Symbol", 12);
            var textSize = g.MeasureString(lockIcon, font);
            var x = _lockButtonRect.X + (_lockButtonRect.Width - textSize.Width) / 2;
            var y = _lockButtonRect.Y + (_lockButtonRect.Height - textSize.Height) / 2;
            g.DrawString(lockIcon, font, Brushes.White, x, y);
        }

        private void DrawMenuButton(Graphics g)
        {
            // Draw button background
            var mousePos = PointToClient(MousePosition);
            var isHover = _menuButtonRect.Contains(mousePos);
            
            var bgColor = isHover ? Color.FromArgb(80, 80, 80) : Color.FromArgb(50, 50, 50);
            g.FillRectangle(new SolidBrush(bgColor), _menuButtonRect);
            g.DrawRectangle(Pens.Gray, _menuButtonRect);

            // Draw hamburger menu (three horizontal lines)
            var lineHeight = 2;
            var spacing = 4;
            var totalHeight = 3 * lineHeight + 2 * spacing;
            var startY = _menuButtonRect.Y + (_menuButtonRect.Height - totalHeight) / 2;
            
            var brush = Brushes.White;
            for (int i = 0; i < 3; i++)
            {
                var y = startY + i * (lineHeight + spacing);
                g.FillRectangle(brush, _menuButtonRect.X + 4, y, _menuButtonRect.Width - 8, lineHeight);
            }
        }

        private void FenceWindow_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Check if clicking on top-right buttons - check first to ensure they get priority
                if (_lockButtonRect.Contains(e.Location))
                {
                    ToggleLock();
                    Refresh();
                    return;
                }

                if (_menuButtonRect.Contains(e.Location))
                {
                    ShowFenceMenu();
                    Refresh();
                    return;
                }

                if (!lockedToolStripMenuItem.Checked)
                {
                    // Check if clicking on a component
                    var component = componentRenderer.GetComponentAt(e.Location);
                    if (component != null)
                    {
                        activeComponent = component;
                        var relativePos = new Point(
                            e.X - component.Bounds.X,
                            e.Y - component.Bounds.Y);
                        component.HandleMouseDown(relativePos);
                        Refresh();
                        return;
                    }

                    // Start dragging icon for reordering
                    if (!fenceInfo.LockedLayout && hoveringItem != null)
                    {
                        draggedItem = hoveringItem;
                        dragStartPos = e.Location;
                    }
                }
            }
        }

        private void ToggleLock()
        {
            fenceInfo.Locked = !fenceInfo.Locked;
            lockedToolStripMenuItem.Checked = fenceInfo.Locked;
            if (!fenceInfo.Locked && isMinified)
            {
                isMinified = false;
                Height = prevHeight;
            }
            Save();
        }

        private void ShowFenceMenu()
        {
            var menu = new ContextMenuStrip();
            var deleteItem = new ToolStripMenuItem("删除分区", null, DeleteFence_Click);
            menu.Items.Add(deleteItem);
            menu.Show(this, _menuButtonRect.Left, _menuButtonRect.Bottom);
        }

        private void DeleteFence_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "确定要删除这个分区吗？", "删除分区", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                FenceManager.Instance.RemoveFence(fenceInfo);
                Close();
            }
        }

        private static Point GetPointFromLParam(IntPtr lParam)
        {
            var value = lParam.ToInt32();
            return new Point((short)(value & 0xffff), (short)((value >> 16) & 0xffff));
        }
    }

}
