using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NoFences.Model
{
    [Serializable]
    public class ComponentInfo
    {
        [XmlElement]
        public string Id { get; set; }

        [XmlElement]
        public ComponentType Type { get; set; }

        [XmlElement]
        public int X { get; set; }

        [XmlElement]
        public int Y { get; set; }

        [XmlElement]
        public int Width { get; set; }

        [XmlElement]
        public int Height { get; set; }

        [XmlIgnore]
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}
