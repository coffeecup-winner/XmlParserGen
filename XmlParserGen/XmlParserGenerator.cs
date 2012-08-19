using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace XmlParserGen {
    public class XmlParserGenerator {
        public static string Generate(string configFilename) {
            using(Stream stream = new FileStream(configFilename, FileMode.Open)) {
                XmlParserConfig config = new XmlParserConfig(stream);
                CodeNamespace ns = GenerateParser(config);
                return GenerateCode(ns);
            }
        }
        static string GenerateCode(CodeNamespace ns) {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            StringWriter writer = new StringWriter(new StringBuilder());
            provider.GenerateCodeFromNamespace(ns, writer, null);
            return writer.ToString();
        }
        static CodeNamespace GenerateParser(XmlParserConfig config) {
            CodeNamespace ns = new CodeNamespace("XmlParserGen");
            ns.AddImports("System", "System.IO", "System.Xml", "System.Xml.Linq");
            foreach(Class @class in config.Classes)
                GenerateClass(ns, @class);
            return ns;
        }
        static void GenerateClass(CodeNamespace ns, Class @class) {
            CodeTypeDeclaration type = CodeDom.CreateClass(@class.Name);
            CodeConstructor constructor = CodeDom.CreateConstructor(@public: !@class.IsRootType);
            constructor.AddParameter<XElement>("element");
            foreach(Property property in @class.Properties) {
                type.AddProperty(property.Type.Name, property.Name);
                var newElement = CodeDom.VarRef("element").Invoke("Element", CodeDom.Primitive(property.ElementName));
                var assignment = CodeDom.AssignField(property.Name,
                    property.Type.IsSystemType ? (CodeExpression)newElement.Get("Value") : CodeDom.New(property.Type.Name, newElement));
                constructor.Statements.Add(assignment);
            }
            type.Members.Add(constructor);
            if(@class.IsRootType)
                type.Members.Add(CreateReadFromFileMethod(type));
            ns.Types.Add(type);
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
