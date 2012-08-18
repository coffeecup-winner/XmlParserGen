using System;
using System.CodeDom;

namespace XmlParserGen {
    static class CodeTypeDeclarationExtensions {
        public static void AddProperty(this CodeTypeDeclaration type, string propertyType) {
            type.AddProperty(propertyType, propertyType);
        }
        public static void AddProperty(this CodeTypeDeclaration type, string propertyType, string propertyName) {
            AddPropertyCore(type, new CodeTypeReference(propertyType), propertyName);
        }
        public static void AddProperty<TPropertyType>(this CodeTypeDeclaration type, string propertyName) {
            AddPropertyCore(type, new CodeTypeReference(typeof(TPropertyType)), propertyName);
        }
        static void AddPropertyCore(CodeTypeDeclaration type, CodeTypeReference propertyType, string propertyName) {
            CodeMemberField field = new CodeMemberField {
                Type = propertyType,
                Name = char.ToLower(propertyName[0]) + propertyName.Substring(1)
            };
            CodeMemberProperty property = new CodeMemberProperty {
                Type = propertyType,
                Name = propertyName,
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                HasGet = true
            };
            property.GetStatements.Add(new CodeMethodReturnStatement(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name)
            ));

            type.Members.Add(field);
            type.Members.Add(property);
        }
    }

    static class CodeNamespaceExtensions {
        public static void AddImports(this CodeNamespace ns, params string[] imports) {
            foreach(string import in imports)
                ns.Imports.Add(new CodeNamespaceImport(import));
        }
    }

    static class CodeDom {
        public static CodeTypeDeclaration CreateClass(string name, bool @public = true) {
			CodeTypeDeclaration newClass = new CodeTypeDeclaration(name) {
				IsClass = true,
			};
            if(@public)
                newClass.Attributes = MemberAttributes.Public;
            return newClass;
        }
    }
}

