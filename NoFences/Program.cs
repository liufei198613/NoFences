using NoFences.Model;
using NoFences.Util;
using NoFences.Win32;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace NoFences
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //allows the context menu to be in dark mode
            //inherits from the system settings
            WindowUtil.SetPreferredAppMode(1);

            using (var mutex = new Mutex(true, "No_fences", out var createdNew))
            {
                if (createdNew)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    // Initialize system tray icon
                    var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    var exeDir = Path.GetDirectoryName(exePath);
                    var iconPath = Path.Combine(exeDir, "resources", "image.png");
                    
                    // Check if resources exists in project structure (development)
                    if (!File.Exists(iconPath))
                    {
                        var projectRoot = Directory.GetParent(exeDir).Parent.Parent.FullName;
                        iconPath = Path.Combine(projectRoot, "resources", "image.png");
                    }

                    using var trayIcon = new TrayIconManager(iconPath);
                    AppSettings.Current.ApplyRuntimeSettings();

                    var loadedFenceCount = FenceManager.Instance.LoadFences();
                    if (loadedFenceCount == 0)
                        FenceManager.Instance.CreateFence("默认分区");

                    Application.Run(new ApplicationContext());
                }
            }
        }

    }
}
