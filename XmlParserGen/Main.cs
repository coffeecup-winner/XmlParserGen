using System;

namespace XmlParserGen {
    class MainClass {
        public static void Main(string[] args) {
            string code = XmlParserGenerator.Generate(args[0]);
            Console.WriteLine(code);
        }
    }
}
