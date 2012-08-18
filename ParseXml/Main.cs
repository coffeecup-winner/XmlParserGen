using System;
using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace ParseXml {
    class MainClass {
        public static void Main(string[] args) {
            Root root = Root.ReadFromFile("test.xml");
            Console.WriteLine(root.Author.Name);
        }
    }

    class Root {
        Author author;

        Root(XElement element) {
            this.author = new Author(element.Element("author"));
        }
        public Author Author { get { return author; } }

        public static Root ReadFromFile(string filename) {
            Stream stream = new FileStream(filename,FileMode.Open);
            try {
                XDocument document = XDocument.Load(stream);
                return new Root(document.Root);
            } finally {
                stream.Dispose();
            }
        }
    }

    class Author {
        string name;

        public Author(XElement element) {
            name = element.Element("name").Value;
        }
        public string Name { get { return name; } }
    }
}
