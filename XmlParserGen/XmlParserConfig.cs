using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace XmlParserGen {
    public class XmlParserConfig {
        List<Class> classes = new List<Class>();

        public XmlParserConfig(Stream configStream) {
            LoadConfig(configStream);
        }
        public List<Class> Classes { get { return classes; } }
        void LoadConfig(Stream configStream) {
            XDocument document = XDocument.Load(configStream);
            Class root = LoadClass(document.Root);
            root.IsRootType = true;
        }
        Class LoadClass(XElement element) {
            string name = element.Name.LocalName;
            Class @class = Classes.FirstOrDefault(c => c.Name == name);
            if(@class != null)
                return @class;
            @class = new Class(name);
            foreach(XElement elem in element.Elements()) {
                string propertyName = elem.Name.LocalName;
                Class propertyType;
                var type = elem.Attribute("type");
                propertyType = type != null ? GetType(type) : LoadClass(elem);
                @class.Properties.Add(new Property(propertyName, propertyType));
            }
            Classes.Add(@class);
            return @class;
        }
        static Class GetType(XAttribute type) {
            switch(type.Value) {
                case "string": return Class.String;
                case "int": return Class.Int;
                case "double": return Class.Double;
                case "bool": return Class.Bool;
                default: throw new XmlParserConfigIllegalAttributeException(type.Name.LocalName, type.Value);
            }
        }
    }
}

