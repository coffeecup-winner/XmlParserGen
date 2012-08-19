using System;
using System.CodeDom;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
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
            GenerateConstructor(authorType);
            ns.Types.Add(authorType);
            rootType.AddProperty(authorType.Name);

            GenerateConstructor(rootType, @public: false);

            var readFromFileMethod = CreateReadFromFileMethod(rootType);
            rootType.Members.Add(readFromFileMethod);

            ns.Types.Add(rootType);
        }
        static void GenerateConstructor(CodeTypeDeclaration authorType, bool @public = true) {
            CodeConstructor authorCtor = CodeDom.CreateConstructor(@public);
            authorCtor.AddParameter<XElement>("element");
            foreach(var field in authorType.Members.OfType<CodeMemberField>()) {
                CodeExpression fieldInitializeExression;
                if(field.Type.BaseType.StartsWith("System.")) //TODO: rewrite to get rid of this check
                    fieldInitializeExression = CodeDom.VarRef("element").Invoke("Element",
                        new CodePrimitiveExpression(field.Name)).Get("Value");
                else
                    fieldInitializeExression = CodeDom.New(field.Type.BaseType,
                        CodeDom.VarRef("element").Invoke("Element", new CodePrimitiveExpression(field.Name)));
                var assignment = new CodeAssignStatement(CodeDom.FieldRef(field.Name),
                    fieldInitializeExression);
                authorCtor.Statements.Add(assignment);
            }
            authorType.Members.Add(authorCtor);
        }
        static CodeMemberMethod CreateReadFromFileMethod(CodeTypeDeclaration rootType) {
            CodeMemberMethod readFromFileMethod = CodeDom.CreateStaticMethod("ReadFromFile", rootType.Name);
            string filenameParameterName = "filename";
            readFromFileMethod.AddParameter<string>(filenameParameterName);
            var fileOpenExpression = CodeDom.New<FileStream>(CodeDom.VarRef(filenameParameterName), CodeDom.TypeRef<FileMode>().Get("Open"));
            var stream = CodeDom.DeclareVariable<Stream>(fileOpenExpression);
            readFromFileMethod.Statements.Add(stream);
            var tryFinally = new CodeTryCatchFinallyStatement();
            var loadXDocumentExpression = CodeDom.TypeRef<XDocument>().Invoke("Load", CodeDom.VarRef(stream));
            var xDocument = CodeDom.DeclareVariable<XDocument>(loadXDocumentExpression);
            tryFinally.TryStatements.Add(xDocument);
            var returnNewRootObject = CodeDom.Return(CodeDom.New(rootType.Name, CodeDom.VarRef(xDocument).Get("Root")));
            tryFinally.TryStatements.Add(returnNewRootObject);
            var disposeStreamExpression = CodeDom.VarRef(stream).Invoke("Dispose");
            tryFinally.FinallyStatements.Add(disposeStreamExpression);
            readFromFileMethod.Statements.Add(tryFinally);
            return readFromFileMethod;
        }
    }
}
