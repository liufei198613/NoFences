using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NoFences.Model;
using NoFences.UI;

namespace NoFences.Util
{
    public class TrayIconManager : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ContextMenuStrip _contextMenu;
        private readonly Form _menuOwner;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public TrayIconManager(string iconPath)
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add(new ToolStripMenuItem("\u65b0\u5efa\u5206\u533a", null, CreatePartition_Click));
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add(new ToolStripMenuItem("\u8bbe\u7f6e\u4e2d\u5fc3", null, Settings_Click));
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add(new ToolStripMenuItem("\u9000\u51fa", null, Exit_Click));

            _menuOwner = new Form
            {
                ShowInTaskbar = false,
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                Size = new Size(1, 1),
                Location = new Point(-32000, -32000),
                Opacity = 0
            };
            _menuOwner.CreateControl();

            _notifyIcon = new NotifyIcon
            {
                Text = "NoFences",
                Visible = true,
                Icon = LoadTrayIcon(iconPath) ?? SystemIcons.Application
            };
            _notifyIcon.MouseClick += NotifyIcon_MouseClick;
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ShowContextMenuAtCursor();
            }
        }

        private void ShowContextMenuAtCursor()
        {
            _contextMenu.Close();
            SetForegroundWindow(_menuOwner.Handle);

            var anchor = Cursor.Position;
            var size = _contextMenu.GetPreferredSize(Size.Empty);
            var location = new Point(anchor.X - size.Width, anchor.Y - size.Height);
            var screen = Screen.FromPoint(anchor).WorkingArea;

            if (location.X < screen.Left)
            {
                location.X = screen.Left;
            }

            if (location.Y < screen.Top)
            {
                location.Y = screen.Top;
            }

            _contextMenu.Show(location);
        }

        private void CreatePartition_Click(object sender, EventArgs e)
        {
            FenceManager.Instance.CreateFence("\u65b0\u5206\u533a");
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            using (var settingsDialog = new SettingsDialog())
            {
                settingsDialog.ShowDialog();
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            Application.Exit();
        }

        public void Dispose()
        {
            _notifyIcon.MouseClick -= NotifyIcon_MouseClick;
            _notifyIcon.Dispose();
            _contextMenu.Dispose();
            _menuOwner.Dispose();
        }

        private static Icon LoadTrayIcon(string iconPath)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("NoFences.resources.image.png"))
                {
                    if (stream != null)
                    {
                        using (var image = Image.FromStream(stream))
                        {
                            return CreateIconFromImage(image);
                        }
                    }
                }
            }
            catch
            {
            }

            try
            {
                if (File.Exists(iconPath))
                {
                    using (var stream = new FileStream(iconPath, FileMode.Open, FileAccess.Read))
                    using (var image = Image.FromStream(stream))
                    {
                        return CreateIconFromImage(image);
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private static Icon CreateIconFromImage(Image image)
        {
            using (var bitmap = new Bitmap(image))
            {
                var handle = bitmap.GetHicon();
                try
                {
                    return (Icon)Icon.FromHandle(handle).Clone();
                }
                finally
                {
                    DestroyIcon(handle);
                }
            }
        }
    }
}
