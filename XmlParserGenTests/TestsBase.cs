using Microsoft.CSharp;
using NUnit.Framework;
using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;

namespace XmlParserGen.Tests {
    [TestFixture]
    public abstract class TestsBase {
        protected object Parse(string config, string xml) {
            string code = XmlParserGenerator.GenerateFromString(config);
            Assembly assembly = CompileInMemoryAssemby(code);
            Type rootType = assembly.GetTypes().First(t => t.Name == "Root");
            MethodInfo readFromFileMethod = rootType.GetMethod("ReadFromString", BindingFlags.Public | BindingFlags.Static);
            object root = readFromFileMethod.Invoke(null, new object[] { xml });
            Assert.That(root.GetType().Name, Is.EqualTo("Root"));
            return root;
        }
        protected Assembly CompileInMemoryAssemby(string code) {
            CodeDomProvider provider = new CSharpCodeProvider();
            CompilerParameters compilerParameters = new CompilerParameters {
                GenerateExecutable = false,
                GenerateInMemory = true,
                IncludeDebugInformation = false
            };
            compilerParameters.ReferencedAssemblies.Add("System.Core.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
            CompilerResults result = provider.CompileAssemblyFromSource(compilerParameters, code);
            if(result.Errors.Count > 0) {
                Assert.Fail("Compiler errors: " + Environment.NewLine +
                    string.Join(Environment.NewLine, result.Errors.OfType<CompilerError>().Select(e => e.ErrorText)));
            }
            return result.CompiledAssembly;
        }
    }
}
