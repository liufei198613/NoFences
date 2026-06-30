using System;
using System.Runtime.InteropServices;

namespace NoFences.Win32
{
    internal static class ShellNotifyUtil
    {
        private const uint SHCNE_RENAMEITEM = 0x00000001;
        private const uint SHCNE_CREATE = 0x00000002;
        private const uint SHCNE_DELETE = 0x00000004;
        private const uint SHCNE_MKDIR = 0x00000008;
        private const uint SHCNE_RMDIR = 0x00000010;
        private const uint SHCNE_UPDATEDIR = 0x00001000;
        private const uint SHCNE_RENAMEFOLDER = 0x00020000;
        private const uint SHCNF_PATHW = 0x0005;
        private const uint SHCNF_FLUSH = 0x1000;

        public static void NotifyFileMoved(string sourcePath, string destinationPath)
        {
            SHChangeNotify(SHCNE_RENAMEITEM, SHCNF_PATHW | SHCNF_FLUSH, sourcePath, destinationPath);
            SHChangeNotify(SHCNE_DELETE, SHCNF_PATHW | SHCNF_FLUSH, sourcePath, null);
            SHChangeNotify(SHCNE_CREATE, SHCNF_PATHW | SHCNF_FLUSH, destinationPath, null);
            NotifyParentDirectories(sourcePath, destinationPath);
        }

        public static void NotifyFolderMoved(string sourcePath, string destinationPath)
        {
            SHChangeNotify(SHCNE_RENAMEFOLDER, SHCNF_PATHW | SHCNF_FLUSH, sourcePath, destinationPath);
            SHChangeNotify(SHCNE_RMDIR, SHCNF_PATHW | SHCNF_FLUSH, sourcePath, null);
            SHChangeNotify(SHCNE_MKDIR, SHCNF_PATHW | SHCNF_FLUSH, destinationPath, null);
            NotifyParentDirectories(sourcePath, destinationPath);
        }

        private static void NotifyParentDirectories(string sourcePath, string destinationPath)
        {
            var sourceParent = System.IO.Path.GetDirectoryName(sourcePath);
            var destinationParent = System.IO.Path.GetDirectoryName(destinationPath);

            if (!string.IsNullOrWhiteSpace(sourceParent))
            {
                SHChangeNotify(SHCNE_UPDATEDIR, SHCNF_PATHW | SHCNF_FLUSH, sourceParent, null);
            }

            if (!string.IsNullOrWhiteSpace(destinationParent) &&
                !string.Equals(sourceParent, destinationParent, StringComparison.OrdinalIgnoreCase))
            {
                SHChangeNotify(SHCNE_UPDATEDIR, SHCNF_PATHW | SHCNF_FLUSH, destinationParent, null);
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, string dwItem1, string dwItem2);
    }
}
