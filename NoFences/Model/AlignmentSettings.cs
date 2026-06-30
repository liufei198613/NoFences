using System;
using System.Xml.Serialization;

namespace NoFences.Model
{
    [Serializable]
    public class AlignmentSettings
    {
        [XmlElement]
        public bool EnableSnapping { get; set; } = true;

        [XmlElement]
        public bool EnableEdgeAlignment { get; set; } = true;

        [XmlElement]
        public bool EnableGridAlignment { get; set; } = false;

        [XmlElement]
        public int GridSize { get; set; } = 50;

        [XmlElement]
        public int SnapThreshold { get; set; } = 10;
    }
}
