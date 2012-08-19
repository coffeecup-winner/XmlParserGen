using System;

namespace XmlParserGen {
    class MainClass {
        public static void Main(string[] args) {
            string filename = "test.xml";
            string code = XmlParserGenerator.Generate(filename);
            Console.WriteLine(code);
        }
    }
}
