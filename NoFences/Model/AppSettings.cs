using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace NoFences.Model
{
    public class AppSettings
    {
        private static readonly string DataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NoFences");

        private static readonly string SettingsPath = Path.Combine(DataPath, "settings.xml");

        private static AppSettings current;

        public static AppSettings Current => current ??= Load();

        public string Language { get; set; } = "zh-CN";
        public bool StartWithWindows { get; set; } = true;
        public bool HighPriorityStartup { get; set; }
        public bool AutoExpandOnTitleHover { get; set; }
        public bool AutoSwitchTabOnHover { get; set; }
        public bool DoubleClickFolderInPartition { get; set; } = true;
        public bool DoubleClickHideDesktopIcons { get; set; } = true;
        public bool EnableSystemAnimation { get; set; } = true;
        public bool EnableHardwareAcceleration { get; set; } = true;

        public bool AutoArrangeAfterOrganize { get; set; } = true;
        public bool OrganizeHiddenFiles { get; set; }
        public bool OrganizeOnStartup { get; set; }
        public bool OrganizeRealtime { get; set; }
        public bool OrganizeMappedPartitions { get; set; }
        public List<OrganizeRule> OrganizeRules { get; set; } = OrganizeRule.GetDefaults();

        public string ThemeTarget { get; set; } = "All";
        public int ThemeColorArgb { get; set; } = Color.Black.ToArgb();
        public int PartitionOpacity { get; set; } = 44;
        public string BackgroundImagePath { get; set; } = "";
        public string TitlePosition { get; set; } = "Top";
        public string TabPosition { get; set; } = "Top";
        public bool ShowPartitionBorder { get; set; }
        public bool RoundedPartitionCorners { get; set; }
        public bool ShowPartitionTitle { get; set; } = true;
        public bool LargeFont { get; set; }
        public bool CompactShortcutArrow { get; set; } = true;
        public bool IconOnly { get; set; }

        public string BackupPath { get; set; } = Path.Combine(DataPath, "Backups");
        public bool DailyBackupLayout { get; set; } = true;

        public string ShowDesktopHotkey { get; set; } = "";
        public string OrganizeDesktopHotkey { get; set; } = "";
        public bool GlassEffect { get; set; }
        public int BlurAmount { get; set; } = 35;
        public bool PartitionAnimation { get; set; }

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var serializer = new XmlSerializer(typeof(AppSettings));
                    using (var reader = new StreamReader(SettingsPath))
                    {
                        if (serializer.Deserialize(reader) is AppSettings settings)
                        {
                            settings.EnsureDefaults();
                            settings.Save();
                            return settings;
                        }
                    }
                }
            }
            catch
            {
            }

            var defaultSettings = new AppSettings();
            defaultSettings.Save();
            return defaultSettings;
        }

        public void Save()
        {
            Directory.CreateDirectory(DataPath);
            Directory.CreateDirectory(BackupPath);

            var serializer = new XmlSerializer(typeof(AppSettings));
            using (var writer = new StreamWriter(SettingsPath))
            {
                serializer.Serialize(writer, this);
            }
        }

        public void ApplyRuntimeSettings()
        {
            ApplyStartupSetting();
            try
            {
                Process.GetCurrentProcess().PriorityClass = HighPriorityStartup
                    ? ProcessPriorityClass.High
                    : ProcessPriorityClass.Normal;
            }
            catch
            {
            }
        }

        public static string GetDataPath()
        {
            Directory.CreateDirectory(DataPath);
            return DataPath;
        }

        private void EnsureDefaults()
        {
            if (OrganizeRules == null || OrganizeRules.Count == 0)
            {
                OrganizeRules = OrganizeRule.GetDefaults();
            }

            if (string.IsNullOrWhiteSpace(BackupPath))
            {
                BackupPath = Path.Combine(DataPath, "Backups");
            }

            if (ThemeColorArgb == unchecked((int)0xFF00B894))
            {
                ThemeColorArgb = Color.Black.ToArgb();
                PartitionOpacity = 44;
                ShowPartitionBorder = false;
            }
        }

        private void ApplyStartupSetting()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key == null)
                    {
                        return;
                    }

                    if (StartWithWindows)
                    {
                        key.SetValue("NoFences", $"\"{Application.ExecutablePath}\"");
                    }
                    else
                    {
                        key.DeleteValue("NoFences", false);
                    }
                }
            }
            catch
            {
            }
        }
    }

    public class OrganizeRule
    {
        public bool Enabled { get; set; }
        public string Title { get; set; } = "";
        public string FileName { get; set; } = "*";
        public string Extensions { get; set; } = "";
        public string TargetPartition { get; set; } = "";

        public static List<OrganizeRule> GetDefaults()
        {
            return new List<OrganizeRule>
            {
                new OrganizeRule { Enabled = false, Title = "Shortcut", Extensions = ".lnk;.url", TargetPartition = "Shortcut" },
                new OrganizeRule { Enabled = true, Title = "Folder", Extensions = "\\", TargetPartition = "Folder" },
                new OrganizeRule { Enabled = true, Title = "Document", Extensions = ".doc;.docx;.pdf;.xps;.htm;.html;.txt;.xls;.xlsx;.ppt;.pptx", TargetPartition = "Document" },
                new OrganizeRule { Enabled = true, Title = "Image", Extensions = ".bmp;.jpg;.jpeg;.png;.gif;.tif;.tiff;.webp", TargetPartition = "Image" },
                new OrganizeRule { Enabled = true, Title = "Archive", Extensions = ".7z;.bz2;.bzip2;.gz;.gzip;.lzh;.lzma;.rar;.tar;.zip", TargetPartition = "Archive" },
                new OrganizeRule { Enabled = true, Title = "Other", Extensions = "*", TargetPartition = "Other" }
            };
        }
    }
}
