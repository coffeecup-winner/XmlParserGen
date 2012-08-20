using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
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
            ns.AddImports("System", "System.Collections.Generic", "System.IO", "System.Xml", "System.Xml.Linq");
            foreach(Class @class in config.Classes)
                GenerateClass(ns, @class);
            return ns;
        }
        static void GenerateClass(CodeNamespace ns, Class @class) {
            CodeTypeDeclaration type = CodeDom.CreateClass(@class.Name);
            CodeConstructor constructor = CodeDom.CreateConstructor(@public: !@class.IsRootType);
            constructor.AddParameter<XElement>("element");
            foreach(Property property in @class.Properties) {
                type.AddProperty(property.TypeName, property.Name);
                AddPropertyInitializer(property, constructor);
            }
            type.Members.Add(constructor);
            if(@class.IsRootType)
                type.Members.Add(CreateReadFromFileMethod(type));
            ns.Types.Add(type);
        }
        static void AddPropertyInitializer(Property property, CodeConstructor constructor) {
            ListProperty listProperty = property as ListProperty;
            if(listProperty != null) {
                AddListInitializer(listProperty, constructor);
                return;
            }
            var newElement = CodeDom.VarRef("element").Invoke("Element", CodeDom.Primitive(property.ElementName));
            var assignment = CodeDom.AssignField(property.ElementName,
                property.Type.IsSystemType ? (CodeExpression)newElement.Get("Value") : CodeDom.New(property.TypeName, newElement));
            constructor.Statements.Add(assignment);
        }
        static void AddListInitializer(ListProperty property, CodeConstructor constructor) {
            var createList = CodeDom.AssignField(property.ElementName, CodeDom.New(property.TypeName));
            constructor.Statements.Add(createList);
			CodeExpression element = CodeDom.VarRef("element");
            if(!property.NoListNode)
                element = element.Invoke("Element", CodeDom.Primitive(property.ElementName));
            var enumerableElements = element.Invoke("Elements", CodeDom.Primitive(property.ItemElementName));
            var foreachStatements = CodeDom.ForEach<XElement>(enumerableElements, current => new CodeStatement[] {
                new CodeExpressionStatement(CodeDom.FieldInvoke(property.ElementName, "Add", CodeDom.New(property.Type.Name, current)))
            });
            constructor.Statements.AddRange(foreachStatements);
        }
        static CodeMemberMethod CreateReadFromFileMethod(CodeTypeDeclaration rootType) {
            CodeMemberMethod readFromFileMethod = CodeDom.CreateStaticMethod("ReadFromFile", rootType.Name);
            string filenameParameterName = "filename";
            readFromFileMethod.AddParameter<string>(filenameParameterName);

            var fileOpenExpression = CodeDom.New<FileStream>(CodeDom.VarRef(filenameParameterName), CodeDom.TypeRef<FileMode>().Get("Open"));
            var stream = CodeDom.DeclareVariable<Stream>(fileOpenExpression);

            var xDocument = CodeDom.DeclareVariable<XDocument>(CodeDom.TypeRef<XDocument>().Invoke("Load", CodeDom.VarRef(stream)));
            var returnNewRootObject = CodeDom.Return(CodeDom.New(rootType.Name, CodeDom.VarRef(xDocument).Get("Root")));

            var usingStatements = CodeDom.Using(stream, xDocument, returnNewRootObject);

            readFromFileMethod.Statements.AddRange(usingStatements);
            return readFromFileMethod;
        }
    }
}
