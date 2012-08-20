using System;
using System.IO;
using XmlParserGen;

namespace ParseXml {
    class MainClass {
        public static void Main(string[] args) {
            Root root = Root.ReadFromFile("test.xml");
            foreach(Book book in root.Books)
                Console.WriteLine("{0}. {1}", book.Author.Name, book.Name);
        }
    }
}