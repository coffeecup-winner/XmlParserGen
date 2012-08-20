using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace XmlParserGen {
    public class Class {
        public static readonly Class String = new Class(typeof(string).FullName, true);
        public static readonly Class Int = new Class(typeof(int).FullName, true);
        public static readonly Class Double = new Class(typeof(double).FullName, true);
        public static readonly Class Bool = new Class(typeof(bool).FullName, true);

        readonly bool isSystemType;
        readonly string name;
        readonly List<Property> properties = new List<Property>();

        public Class(string name, bool isSystemType = false) {
            this.name = name.Capitalize();
            this.isSystemType = isSystemType;
        }
        public bool IsSystemType { get { return isSystemType; } }
        public bool IsRootType { get; set; }
        public string Name { get { return name; } }
        public List<Property> Properties { get { return properties; } }
    }
    
    public class Property {
        readonly string name;
        readonly Class type;
        readonly string elementName;

        public Property(string elementName, Class type) {
            this.name = elementName.Capitalize();
            this.type = type;
            this.elementName = elementName;
        }
        public string Name { get { return name; } }
        public Class Type { get { return type; } }
        public virtual string TypeName { get { return type.Name; } }
        public string ElementName { get { return elementName; } }
    }

    public class ListProperty : Property {
        readonly string itemElementName;

        public ListProperty(string elementName, Class type, string itemElementName)
            : base(elementName, type) {
            this.itemElementName = itemElementName;
        }
        public override string TypeName { get { return "List<" + base.TypeName + ">"; } }
        public string ItemElementName { get { return itemElementName; } }
    }

    static class StringExtensions {
        public static string Capitalize(this string text) {
            return char.ToUpper(text[0]) + text.Substring(1);
        }
    }
}
