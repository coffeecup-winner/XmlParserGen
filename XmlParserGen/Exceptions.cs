using System;
namespace XmlParserGen {
    public abstract class XmlParserGenException : Exception {
        public XmlParserGenException() { }
        public XmlParserGenException(string message) : base(message) { }
    }

    public abstract class XmlParserConfigException : XmlParserGenException {
        public XmlParserConfigException() { }
        public XmlParserConfigException(string message) : base(message) { }
    }

    public class XmlParserConfigIllegalAttributeException : XmlParserConfigException {
        public XmlParserConfigIllegalAttributeException(string attribute, string value)
            : base(string.Format("Illegal value ({0}) for attribute {1}.", value, attribute)) {
        }
    }
}

