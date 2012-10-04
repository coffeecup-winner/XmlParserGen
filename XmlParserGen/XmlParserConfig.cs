using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace XmlParserGen {
    public class XmlParserConfig {
        Dictionary<string, Class> classes = new Dictionary<string, Class>();

        public XmlParserConfig(Stream configStream) {
            LoadConfig(configStream);
        }
        public XmlParserConfig(string configText) {
            LoadConfig(configText);
        }
        public List<Class> Classes { get { return classes.Values.ToList(); } }
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
            Class @class;
            if(this.classes.TryGetValue(name, out @class))
                return @class;
            @class = new Class(AttributeValueOrDefault(element, "typename") ?? name.Capitalize());
            foreach(XElement elem in element.Elements()) {
                string propertyName = elem.Name.LocalName;
                if(propertyName == name + ".attributes")
                    LoadAttributes(elem, @class);
                else if(propertyName == name + ".definitions")
                    LoadDefinitions(elem);
                else
                    LoadProperty(@class, elem, propertyName);
            }
            this.classes.Add(name, @class);
            return @class;
        }
        void LoadAttributes(XElement element, Class @class) {
            foreach(XElement elem in element.Elements()) {
                var typeAttr = elem.Attribute("type");
                string elemName = elem.Name.LocalName;
                AttributeProperty property = new AttributeProperty(AttributeValueOrDefault(elem, "name"), elemName, GetType(typeAttr));
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
                propertyType = isDefined ? this.classes[listAttr.Value] : LoadClass(elem, listAttr.Value);
                property = new ListProperty(AttributeValueOrDefault(elem, "name"), propertyName, propertyType, listAttr.Value, noListNode);
            } else {
                if(typeAttr != null)
                    propertyType = GetType(typeAttr);
                else
                    propertyType = isDefined ? this.classes[elem.Name.LocalName] : LoadClass(elem);
                property = new Property(AttributeValueOrDefault(elem, "name"), propertyName, propertyType);
            }
            @class.Properties.Add(property);
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

        static string AttributeValueOrDefault(XElement element, string name) {
            var attr = element.Attribute(name);
            return attr != null ? attr.Value : null;
        }
    }
}

