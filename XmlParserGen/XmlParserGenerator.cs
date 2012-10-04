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
                return GenerateCore(config);
            }
        }
        public static string GenerateFromString(string configText) {
            XmlParserConfig config = new XmlParserConfig(configText);
            return GenerateCore(config);
        }
        static string GenerateCore(XmlParserConfig config) {
            CodeNamespace ns = GenerateParser(config);
            return GenerateCode(ns);
        }
        static string GenerateCode(CodeNamespace ns) {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            StringWriter writer = new StringWriter(new StringBuilder());
            provider.GenerateCodeFromNamespace(ns, writer, null);
            return writer.ToString();
        }
        static CodeNamespace GenerateParser(XmlParserConfig config) {
            CodeNamespace ns = new CodeNamespace(config.Namespace);
            ns.AddImports("System", "System.Collections.Generic", "System.IO", "System.Xml", "System.Xml.Linq");
            foreach(Class @class in config.Classes)
                GenerateClass(ns, @class);
            return ns;
        }
        static void GenerateClass(CodeNamespace ns, Class @class) {
            CodeTypeDeclaration type = CodeDom.CreateClass(@class.Name);
            CodeConstructor constructor = CodeDom.CreateConstructor(@public: !@class.IsRootType);
            constructor.AddParameter<XElement>("element");
            PropertyInitializerVisitor visitor = new PropertyInitializerVisitor(constructor);
            foreach(Property property in @class.Properties) {
                type.AddProperty(property.TypeName, property.Name);
                property.Accept(visitor);
            }
            type.Members.Add(constructor);
            if(@class.IsRootType) {
                type.Members.AddRange(new CodeTypeMember[] {
                    CreateReadFromStringMethod(type),
                    CreateReadFromStreamMethod(type),
                    CreateReadFromFileMethod(type)
                });
            }
            ns.Types.Add(type);
        }
        static CodeMemberMethod CreateReadFromStreamMethod(CodeTypeDeclaration rootType) {
            CodeMemberMethod readFromStreamMethod = CodeDom.CreateStaticMethod("ReadFromStream", rootType.Name);
            string streamParameterName = "stream";
            readFromStreamMethod.AddParameter<Stream>(streamParameterName);

            var xDocument = CodeDom.DeclareVariable<XDocument>(CodeDom.TypeRef<XDocument>().Invoke("Load", CodeDom.VarRef(streamParameterName)));
            var returnNewRootObject = CodeDom.Return(CodeDom.New(rootType.Name, CodeDom.VarRef(xDocument).Get("Root")));

            readFromStreamMethod.Statements.AddRange(new CodeStatement[] { xDocument, returnNewRootObject });
            return readFromStreamMethod;
        }
        static CodeMemberMethod CreateReadFromFileMethod(CodeTypeDeclaration rootType) {
            CodeMemberMethod readFromFileMethod = CodeDom.CreateStaticMethod("ReadFromFile", rootType.Name);
            string filenameParameterName = "filename";
            readFromFileMethod.AddParameter<string>(filenameParameterName);

            var fileOpenExpression = CodeDom.New<FileStream>(CodeDom.VarRef(filenameParameterName), CodeDom.TypeRef<FileMode>().Get("Open"));

            var stream = CodeDom.DeclareVariable<Stream>(fileOpenExpression);
            var returnStatement = CodeDom.Return(CodeDom.TypeRef(rootType).Invoke("ReadFromStream", CodeDom.VarRef(stream)));
            var usingStatements = CodeDom.Using(stream, returnStatement);

            readFromFileMethod.Statements.AddRange(usingStatements);
            return readFromFileMethod;
        }
        static CodeMemberMethod CreateReadFromStringMethod(CodeTypeDeclaration rootType) {
            CodeMemberMethod readFromStringMethod = CodeDom.CreateStaticMethod("ReadFromString", rootType.Name);
            string stringParameterName = "xmlString";
            readFromStringMethod.AddParameter<string>(stringParameterName);

            var stringReader = CodeDom.DeclareVariable<StringReader>(CodeDom.New<StringReader>(CodeDom.VarRef(stringParameterName)));

            var xDocument = CodeDom.DeclareVariable<XDocument>(CodeDom.TypeRef<XDocument>().Invoke("Load", CodeDom.VarRef(stringReader)));
            var returnNewRootObject = CodeDom.Return(CodeDom.New(rootType.Name, CodeDom.VarRef(xDocument).Get("Root")));
            var usingStatements = CodeDom.Using(stringReader, xDocument, returnNewRootObject);

            readFromStringMethod.Statements.AddRange(usingStatements);
            return readFromStringMethod;
        }
    }

    public class PropertyInitializerVisitor : IPropertyVisitor {
        readonly CodeConstructor constructor;

        public PropertyInitializerVisitor(CodeConstructor constructor) {
            this.constructor = constructor;
        }
        public void Visit(Property property) {
            AddSimplePropertyInitializer(property, isAttribute: false);
        }
        public void Visit(AttributeProperty property) {
            AddSimplePropertyInitializer(property, isAttribute: true);
        }
        public void Visit(ListProperty property) {
            var createList = CodeDom.AssignField(CodeDom.GetFieldName(property.Name), CodeDom.New(property.TypeName));
            this.constructor.Statements.Add(createList);
            CodeExpression element = CodeDom.VarRef("element");
            if(!property.NoListNode)
                element = element.Invoke("Element", CodeDom.Primitive(property.ElementName));
            var enumerableElements = element.Invoke("Elements", CodeDom.Primitive(property.ItemElementName));
            var foreachStatements = CodeDom.ForEach<XElement>(enumerableElements, current => new CodeStatement[] {
                new CodeExpressionStatement(CodeDom.FieldInvoke(CodeDom.GetFieldName(property.Name), "Add", CodeDom.New(property.Type.Name, current)))
            });
            this.constructor.Statements.AddRange(foreachStatements);
        }
        void AddSimplePropertyInitializer(Property property, bool isAttribute) {
            var newElement = CodeDom.VarRef("element").Invoke(isAttribute ? "Attribute" : "Element", CodeDom.Primitive(property.ElementName));
            var assignment = CodeDom.AssignField(CodeDom.GetFieldName(property.Name),
                property.Type.IsSystemType ? GetSystemTypeInitializer(property.Type, (CodeExpression)newElement.Get("Value")) : CodeDom.New(property.TypeName, newElement));
            this.constructor.Statements.Add(assignment);
        }
        CodeExpression GetSystemTypeInitializer(Class type, CodeExpression value) {
            if(type == Class.String) return value.Invoke("Trim");
            return CodeDom.TypeRef(type.Name).Invoke("Parse", value);
        }
    }
}
