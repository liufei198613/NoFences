using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NoFences.Model
{
    public class FenceInfo
    {
        /*
         * DO NOT RENAME PROPERTIES. Used for XML serialization.
         */

        public Guid Id { get; set; }

        public string Name { get; set; }

        public int PosX { get; set; }

        public int PosY { get; set; }

        /// <summary>
        /// Gets or sets the DPI scaled window width.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the DPI scaled window height.
        /// </summary>
        public int Height { get; set; }

        public bool Locked { get; set; }

        public bool CanMinify { get; set; }

        /// <summary>
        /// Gets or sets the logical window title height.
        /// </summary>
        public int TitleHeight { get; set; } = 35;

        public List<string> Files { get; set; } = new List<string>();

        // New properties for enhanced functionality

        /// <summary>
        /// Gets or sets whether the layout (icon positions) is locked.
        /// </summary>
        [XmlElement]
        public bool LockedLayout { get; set; }

        /// <summary>
        /// Gets or sets the alignment settings for this fence.
        /// </summary>
        [XmlElement]
        public AlignmentSettings Alignment { get; set; } = new AlignmentSettings();

        /// <summary>
        /// Gets or sets the list of components in this fence.
        /// </summary>
        [XmlArrayItem("Component")]
        public List<ComponentInfo> Components { get; set; } = new List<ComponentInfo>();

        // Sorting settings

        /// <summary>
        /// Gets or sets the sort mode for icons.
        /// </summary>
        [XmlElement]
        public int SortMode { get; set; } // 0 = None, 1 = Name, 2 = Type, 3 = Date

        /// <summary>
        /// Gets or sets whether to sort ascending.
        /// </summary>
        [XmlElement]
        public bool SortAscending { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to use grid layout for icons.
        /// </summary>
        [XmlElement]
        public bool UseGridLayout { get; set; }

        public FenceInfo()
        {

        }

        public FenceInfo(Guid id)
        {
            Id = id;
        }
    }
}
