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
            return LoadClass(element, element.Name.LocalName);
        }
        Class LoadClass(XElement element, string name) {
            Class @class = Classes.FirstOrDefault(c => c.Name == name);
            if(@class != null)
                return @class;
            @class = new Class(name);
            foreach(XElement elem in element.Elements()) {
                string propertyName = elem.Name.LocalName;
                Class propertyType;
                var typeAttr = elem.Attribute("type");
                var listAttr = elem.Attribute("list");
                bool noListNode = elem.Attribute("no_list_node") != null;
                bool isList = listAttr != null;
                Property property;
                if(isList) {
                    propertyType = LoadClass(elem, listAttr.Value);
                    property = new ListProperty(propertyName, propertyType, listAttr.Value, noListNode);
                } else {
                    propertyType = typeAttr != null ? GetType(typeAttr) : LoadClass(elem);
                    property = new Property(propertyName, propertyType);
                }
                @class.Properties.Add(property);
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

