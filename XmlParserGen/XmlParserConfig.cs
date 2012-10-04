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
        public XmlParserConfig(string configText) {
            LoadConfig(configText);
        }
        public List<Class> Classes { get { return classes; } }
        void LoadConfig(Stream configStream) {
            XDocument document = XDocument.Load(configStream);
            LoadConfigCore(document);
        }
        void LoadConfig(string configText) {
            using(StringReader reader = new StringReader(configText))
                LoadConfigCore(XDocument.Load(reader));
        }
        void LoadConfigCore(XDocument document) {
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
                if(propertyName == name + ".attributes")
                    LoadAttributes(elem, @class);
                else if(propertyName == name + ".definitions")
                    LoadDefinitions(elem);
                else
                    LoadProperty(@class, elem, propertyName);
            }
            Classes.Add(@class);
            return @class;
        }
        void LoadAttributes(XElement element, Class @class) {
            foreach(XElement elem in element.Elements()) {
                var typeAttr = elem.Attribute("type");
                AttributeProperty property = new AttributeProperty(elem.Name.LocalName, GetType(typeAttr));
                @class.Properties.Add(property);
            }
        }
        void LoadDefinitions(XElement element) {
            foreach(XElement elem in element.Elements())
                LoadClass(elem);
        }
        void LoadProperty(Class @class, XElement elem, string propertyName) {
            Class propertyType;
            var typeAttr = elem.Attribute("type");
            var listAttr = elem.Attribute("list");
            bool noListNode = elem.Attribute("no_list_node") != null;
            bool isDefined = elem.Attribute("defined") != null;
            bool isList = listAttr != null;
            Property property;
            if(isList) {
                propertyType = isDefined ? FindClass(listAttr.Value) : LoadClass(elem, listAttr.Value);
                property = new ListProperty(propertyName, propertyType, listAttr.Value, noListNode);
            } else {
                if(typeAttr != null)
                    propertyType = GetType(typeAttr);
                else
                    propertyType = isDefined ? FindClass(elem.Name.LocalName) : LoadClass(elem);
                property = new Property(propertyName, propertyType);
            }
            @class.Properties.Add(property);
        }
        Class FindClass(string name) {
            string className = name.Capitalize();
            return Classes.First(c => c.Name == className);
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

