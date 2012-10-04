using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace XmlParserGen {
    public class Class {
        public static readonly Class String = new Class(typeof(string).Name, typeof(string).Namespace, true);
        public static readonly Class Int = new Class(typeof(int).Name, typeof(int).Namespace, true);
        public static readonly Class Double = new Class(typeof(double).Name, typeof(double).Namespace, true);
        public static readonly Class Bool = new Class(typeof(bool).Name, typeof(bool).Namespace, true);

        readonly bool isSystemType;
        readonly string name;
        readonly string @namespace;
        readonly List<Property> properties = new List<Property>();

        public Class(string name, string @namespace, bool isSystemType = false) {
            this.name = name;
            this.@namespace = @namespace;
            this.isSystemType = isSystemType;
        }
        public bool IsSystemType { get { return isSystemType; } }
        public bool IsRootType { get; set; }
        public string Name { get { return name; } }
        public string Namespace { get { return @namespace; } }
        public List<Property> Properties { get { return properties; } }
    }
    
    public class Property {
        readonly string name;
        readonly Class type;
        readonly string elementName;

        public Property(string name, string elementName, Class type) {
            this.name = name ?? elementName.Capitalize();
            this.type = type;
            this.elementName = elementName;
        }
        public string Name { get { return name; } }
        public Class Type { get { return type; } }
        public virtual string TypeName { get { return type.Name; } }
        public string ElementName { get { return elementName; } }
        public virtual void Accept(IPropertyVisitor visitor) {
            visitor.Visit(this);
        }
    }

    public class AttributeProperty : Property {
        public AttributeProperty(string name, string attributeName, Class type)
            : base(name, attributeName, type) {
        }
        public string AttributeName { get { return ElementName; } }
        public override void Accept(IPropertyVisitor visitor) {
            visitor.Visit(this);
        }
    }

    public class ListProperty : Property {
        readonly string itemElementName;
        readonly bool noListNode;

        public ListProperty(string name, string elementName, Class type, string itemElementName, bool noListNode)
            : base(name, elementName, type) {
            this.itemElementName = itemElementName;
            this.noListNode = noListNode;
        }
        public override string TypeName { get { return "List<" + base.TypeName + ">"; } }
        public bool NoListNode { get { return noListNode; } }
        public string ItemElementName { get { return itemElementName; } }
        public override void Accept(IPropertyVisitor visitor) {
            visitor.Visit(this);
        }
    }

    static class StringExtensions {
        public static string Capitalize(this string text) {
            return char.ToUpper(text[0]) + text.Substring(1);
        }
    }

    public interface IPropertyVisitor {
        void Visit(Property property);
        void Visit(AttributeProperty property);
        void Visit(ListProperty property);
    }
}
