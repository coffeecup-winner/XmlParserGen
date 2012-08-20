namespace XmlParserGen {
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    
    
    public class Author {
        
        private string name;
        
        public Author(System.Xml.Linq.XElement element) {
            this.name = element.Element("name").Value;
        }
        
        public string Name {
            get {
                return this.name;
            }
        }
    }
    
    public class Root {
        
        private Author author;
        
        Root(System.Xml.Linq.XElement element) {
            this.author = new Author(element.Element("author"));
        }
        
        public Author Author {
            get {
                return this.author;
            }
        }
        
        public static Root ReadFromFile(string filename) {
            System.IO.Stream stream = new System.IO.FileStream(filename, System.IO.FileMode.Open);
            try {
                System.Xml.Linq.XDocument xDocument = System.Xml.Linq.XDocument.Load(stream);
                return new Root(xDocument.Root);
            }
            finally {
                stream.Dispose();
            }
        }
    }
}

