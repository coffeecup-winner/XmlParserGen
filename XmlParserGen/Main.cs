using System;
using System.CodeDom;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using System.Xml.Linq;

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

            CodeTypeDeclaration authorType = CodeDom.CreateClass("Author");
            authorType.AddProperty<string>("Name");
            ns.Types.Add(authorType);
            rootType.AddProperty(authorType.Name);

            CodeConstructor constr = new CodeConstructor();
            constr.Attributes = MemberAttributes.Final;
            constr.Parameters.Add(new CodeParameterDeclarationExpression("XElement", "element"));
            rootType.Members.Add(constr);

            var readFromFileMethod = NewMethod(rootType);
            rootType.Members.Add(readFromFileMethod);

            ns.Types.Add(rootType);
        }
        static CodeMemberMethod NewMethod(CodeTypeDeclaration rootType) {
            CodeMemberMethod readFromFileMethod = CodeDom.CreateStaticMethod("ReadFromFile", rootType.Name);
            string filenameParameterName = "filename";
            readFromFileMethod.AddParameter<string>(filenameParameterName);
            var fileOpenExpression = new CodeObjectCreateExpression(typeof(FileStream), new CodeVariableReferenceExpression(filenameParameterName), new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(FileMode)), "Open"));
            var newStreamStatement = CodeDom.DeclareVariable<Stream>(fileOpenExpression);
            readFromFileMethod.Statements.Add(newStreamStatement);
            var tryFinally = new CodeTryCatchFinallyStatement();
            var loadXDocumentExpression = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(XDocument)), "Load", new CodeVariableReferenceExpression(newStreamStatement.Name));
            var newXDocument = CodeDom.DeclareVariable<XDocument>(loadXDocumentExpression);
            tryFinally.TryStatements.Add(newXDocument);
            var returnNewRootObject = new CodeMethodReturnStatement(new CodeObjectCreateExpression(rootType.Name, new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(newXDocument.Name), "Root")));
            tryFinally.TryStatements.Add(returnNewRootObject);
            var disposeStreamExpression = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(newStreamStatement.Name), "Dispose", new CodeExpression[0]);
            tryFinally.FinallyStatements.Add(disposeStreamExpression);
            readFromFileMethod.Statements.Add(tryFinally);
            return readFromFileMethod;
        }
    }
}
