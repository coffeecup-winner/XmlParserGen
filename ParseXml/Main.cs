using System;
using System.IO;
using XmlParserGen;

namespace ParseXml {
    class MainClass {
        public static void Main(string[] args) {
            Root root = Root.ReadFromFile("test.xml");
            Console.WriteLine(root.Author.Name);
        }
    }
}