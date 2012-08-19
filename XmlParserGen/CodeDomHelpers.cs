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
        public static void AddProperty<TProperty>(this CodeTypeDeclaration type, string propertyName) {
            AddPropertyCore(type, new CodeTypeReference(typeof(TProperty)), propertyName);
        }
        static void AddPropertyCore(CodeTypeDeclaration type, CodeTypeReference propertyType, string propertyName) {
            CodeMemberField field = new CodeMemberField {
                Type = propertyType,
                Name = CodeDom.GetVariableName(propertyName)
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

    static class CodeMemberMethodExtensions {
        public static void AddParameter<TParameter>(this CodeMemberMethod method, string name) {
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(TParameter), name));
        }
        public static void Return(this CodeMemberMethod method, string type) {
            method.ReturnType = new CodeTypeReference(type);
        }
    }

    static class CodeTypeReferenceExpressionExtensions {
        public static CodeMethodInvokeExpression Invoke(this CodeTypeReferenceExpression type, string name,
            params CodeExpression[] parameters) {
            return new CodeMethodInvokeExpression(type, name, parameters);
        }
        public static CodePropertyReferenceExpression Get(this CodeTypeReferenceExpression type, string name) {
            return new CodePropertyReferenceExpression(type, name);
        }
    }

    static class CodeVariableReferenceExpressionExtensions {
        public static CodeMethodInvokeExpression Invoke(this CodeVariableReferenceExpression variable, string name) {
            return new CodeMethodInvokeExpression(variable, name, new CodeExpression[0]);
        }
        public static CodeMethodInvokeExpression Invoke(this CodeVariableReferenceExpression variable, string name,
            params CodeExpression[] parameters) {
            return new CodeMethodInvokeExpression(variable, name, parameters);
        }
        public static CodePropertyReferenceExpression Get(this CodeVariableReferenceExpression variable, string name) {
            return new CodePropertyReferenceExpression(variable, name);
        }
    }

    static class CodeMethodInvokeExpressionExtensions {
        public static CodePropertyReferenceExpression Get(this CodeMethodInvokeExpression invocation, string name) {
            return new CodePropertyReferenceExpression(invocation, name);
        }
    }

    static class CodeDom {
        public static string GetVariableName(string name) {
            return char.ToLower(name[0]) + name.Substring(1);
        }
        public static CodeTypeDeclaration CreateClass(string name, bool @public = true) {
			CodeTypeDeclaration newClass = new CodeTypeDeclaration(name) {
				IsClass = true,
			};
            if(@public)
                newClass.Attributes = MemberAttributes.Public;
            return newClass;
        }
        public static CodeMemberMethod CreateStaticMethod(string name, string returnTypeName, bool @public = true) {
            CodeMemberMethod method = new CodeMemberMethod {
                Name = name,
                ReturnType = new CodeTypeReference(returnTypeName),
                Attributes = MemberAttributes.Static
            };
            if(@public)
                method.Attributes |= MemberAttributes.Public;
            return method;
        }
        public static CodeConstructor CreateConstructor(bool @public = true) {
			CodeConstructor constructor = new CodeConstructor {
				Attributes = MemberAttributes.Final
			};
            if(@public)
                constructor.Attributes |= MemberAttributes.Public;
            return constructor;
        }
        public static CodePrimitiveExpression Primitive(object obj) {
            return new CodePrimitiveExpression(obj);
        }
        public static CodeVariableDeclarationStatement DeclareVariable<T>(CodeExpression expr) {
            return new CodeVariableDeclarationStatement(typeof(T), GetVariableName(typeof(T).Name), expr);
        }
        public static CodeAssignStatement AssignField(string name, CodeExpression expression) {
            return new CodeAssignStatement(FieldRef(name), expression);
        }
        public static CodeVariableReferenceExpression VarRef(string name) {
            return new CodeVariableReferenceExpression(name);
        }
        public static CodeVariableReferenceExpression VarRef(CodeVariableDeclarationStatement declaration) {
            return VarRef(declaration.Name);
        }
        public static CodeFieldReferenceExpression FieldRef(string name) {
            return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name);
        }
        public static CodeTypeReferenceExpression TypeRef<T>() {
            return new CodeTypeReferenceExpression(typeof(T));
        }
        public static CodeObjectCreateExpression New<T>(params CodeExpression[] parameters) {
            return new CodeObjectCreateExpression(typeof(T), parameters);
        }
        public static CodeObjectCreateExpression New(string name, params CodeExpression[] parameters) {
            return new CodeObjectCreateExpression(name, parameters);
        }
        public static CodeMethodReturnStatement Return(CodeExpression expression) {
            return new CodeMethodReturnStatement(expression);
        }
    }
}

