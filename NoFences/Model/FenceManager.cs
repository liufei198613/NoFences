using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace NoFences.Model
{
    public class FenceManager
    {
        public static FenceManager Instance { get; } = new FenceManager();

        private const string MetaFileName = "__fence_metadata.xml";
        private const string ComponentDataFileName = "__component_data.xml";

        private readonly string basePath;

        public FenceManager()
        {
            basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NoFences");
            EnsureDirectoryExists(basePath);
        }

        public int LoadFences()
        {
            RemoveDuplicateEmptyDefaultFences();

            var loadedCount = 0;
            foreach (var dir in Directory.EnumerateDirectories(basePath))
            {
                var metaFile = Path.Combine(dir, MetaFileName);
                if (!File.Exists(metaFile))
                    continue;

                FenceInfo fenceInfo;
                try
                {
                    var serializer = new XmlSerializer(typeof(FenceInfo));
                    using (var reader = new StreamReader(metaFile))
                    {
                        fenceInfo = serializer.Deserialize(reader) as FenceInfo;
                    }
                }
                catch
                {
                    continue;
                }

                if (fenceInfo == null)
                    continue;

                // Initialize alignment settings if null
                if (fenceInfo.Alignment == null)
                {
                    fenceInfo.Alignment = new AlignmentSettings();
                }

                // Initialize components list if null
                if (fenceInfo.Components == null)
                {
                    fenceInfo.Components = new System.Collections.Generic.List<ComponentInfo>();
                }

                new FenceWindow(fenceInfo).Show();
                loadedCount++;
            }

            return loadedCount;
        }

        public void CreateFence(string name)
        {
            var fenceInfo = new FenceInfo(Guid.NewGuid())
            {
                Name = name,
                PosX = 100,
                PosY = 250,
                Height = 300,
                Width = 300,
                Locked = false,
                CanMinify = false,
                LockedLayout = false,
                Alignment = new AlignmentSettings(),
                Components = new System.Collections.Generic.List<ComponentInfo>()
            };

            UpdateFence(fenceInfo);
            new FenceWindow(fenceInfo).Show();
        }

        public void RemoveFence(FenceInfo info)
        {
            var path = GetFolderPath(info);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public void UpdateFence(FenceInfo fenceInfo)
        {
            var path = GetFolderPath(fenceInfo);
            EnsureDirectoryExists(path);

            var metaFile = Path.Combine(path, MetaFileName);
            var serializer = new XmlSerializer(typeof(FenceInfo));
            using (var writer = new StreamWriter(metaFile))
            {
                serializer.Serialize(writer, fenceInfo);
            }
        }

        public void SaveComponentData(FenceInfo fenceInfo, object componentData)
        {
            var path = GetFolderPath(fenceInfo);
            EnsureDirectoryExists(path);

            var dataFile = Path.Combine(path, ComponentDataFileName);
            var serializer = new XmlSerializer(componentData.GetType());
            using (var writer = new StreamWriter(dataFile))
            {
                serializer.Serialize(writer, componentData);
            }
        }

        public T LoadComponentData<T>(FenceInfo fenceInfo) where T : class, new()
        {
            var path = GetFolderPath(fenceInfo);
            var dataFile = Path.Combine(path, ComponentDataFileName);

            if (!File.Exists(dataFile))
                return null;

            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using (var reader = new StreamReader(dataFile))
                {
                    return serializer.Deserialize(reader) as T;
                }
            }
            catch
            {
                return null;
            }
        }

        public string GetFenceDataPath(FenceInfo fenceInfo)
        {
            return GetFolderPath(fenceInfo);
        }

        private void EnsureDirectoryExists(string dir)
        {
            var di = new DirectoryInfo(dir);
            if (!di.Exists)
                di.Create();
        }

        private string GetFolderPath(FenceInfo fenceInfo)
        {
            return Path.Combine(basePath, fenceInfo.Id.ToString());
        }

        private void RemoveDuplicateEmptyDefaultFences()
        {
            var defaults = new List<(string Directory, string MetaFile, DateTime LastWriteTime)>();

            foreach (var dir in Directory.EnumerateDirectories(basePath))
            {
                var metaFile = Path.Combine(dir, MetaFileName);
                if (!File.Exists(metaFile))
                    continue;

                try
                {
                    var serializer = new XmlSerializer(typeof(FenceInfo));
                    using (var reader = new StreamReader(metaFile))
                    {
                        if (serializer.Deserialize(reader) is FenceInfo info &&
                            IsEmptyDefaultFence(info))
                        {
                            defaults.Add((dir, metaFile, File.GetLastWriteTime(metaFile)));
                        }
                    }
                }
                catch
                {
                    // Ignore unreadable metadata here; LoadFences will skip it later.
                }
            }

            foreach (var duplicate in defaults.OrderByDescending(item => item.LastWriteTime).Skip(1))
            {
                try
                {
                    Directory.Delete(duplicate.Directory, true);
                }
                catch
                {
                    // A failed cleanup should not block app startup.
                }
            }
        }

        private static bool IsEmptyDefaultFence(FenceInfo info)
        {
            var hasDefaultName = info.Name == "First fence" ||
                                 info.Name == "New fence" ||
                                 info.Name == "默认分区" ||
                                 info.Name == "新分区";
            var hasNoFiles = info.Files == null || info.Files.Count == 0;
            var hasNoComponents = info.Components == null || info.Components.Count == 0;
            return hasDefaultName && hasNoFiles && hasNoComponents;
        }
    }
}
