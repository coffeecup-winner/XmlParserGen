using System;
using System.CodeDom;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;

namespace XmlParserGen {
    class MainClass {
        static string GenerateCode(CodeNamespace ns) {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            StringWriter writer = new StringWriter(new StringBuilder());
            provider.GenerateCodeFromNamespace(ns, writer, null);
            return writer.ToString();
        }
        public static void Main(string[] args) {
            CodeNamespace ns = GenerateParser();
            string code = GenerateCode(ns);
            Console.WriteLine(code);
        }
        static CodeNamespace GenerateParser() {
            CodeNamespace ns = new CodeNamespace("XmlParserGen");
            ns.AddImports("System", "System.IO", "System.Xml", "System.Xml.Linq");
            GenerateRoot(ns);
            return ns;
        }
        static void GenerateRoot(CodeNamespace ns) {
            CodeTypeDeclaration rootType = CodeDom.CreateClass("Root");
            rootType.AddProperty("Author");
            CodeConstructor constr = new CodeConstructor();
            constr.Attributes = MemberAttributes.Final;
            constr.Parameters.Add(new CodeParameterDeclarationExpression("XElement", "element"));
            rootType.Members.Add(constr);
            ns.Types.Add(rootType);

            CodeTypeDeclaration authorType = CodeDom.CreateClass("Author");
            authorType.AddProperty<string>("Name");
            ns.Types.Add(authorType);
        }
    }
}
