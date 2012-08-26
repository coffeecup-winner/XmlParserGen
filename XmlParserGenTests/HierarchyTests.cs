using Microsoft.CSharp;
using NUnit.Framework;
using System;

namespace XmlParserGen.Tests {
    [TestFixture]
    public class HierarchyTests : TestsBase {
        [Test]
        public void EmptyHierarchyTest() {
            string config = "<root />";
            string xml = "<root />";
            Parse(config, xml);
        }
        [Test]
        public void ListWithListNodesTest() {
            string config = @"
<root>
  <books list=""book"">
    <name type=""string"" /> 
    <author>
      <name type=""string"" />
    </author>
  </books>
</root>";
            string xml = @"
<root>
  <books>
    <book>
      <name>Watership down</name>
      <author>
        <name>Richard Adams</name>
      </author>
    </book>
    <book>
      <name>The curious incident of the dog in the night-time</name>
      <author>
        <name>Mark Haddon</name>
      </author>
    </book>
  </books>
</root>";
            dynamic root = Parse(config, xml);
            Assert.That(root.Books.Count, Is.EqualTo(2));
            Assert.That(root.Books[0].Name, Is.EqualTo("Watership down"));
            Assert.That(root.Books[1].Author.Name, Is.EqualTo("Mark Haddon"));
        }
        [Test]
        public void ListWithoutListNodesTest() {
            string config = @"
<root>
  <books list=""book"" no_list_node=""true"">
    <name type=""string"" /> 
    <author>
      <name type=""string"" />
    </author>
  </books>
</root>";
            string xml = @"
<root>
  <book>
    <name>Watership down</name>
    <author>
      <name>Richard Adams</name>
    </author>
  </book>
  <book>
    <name>The curious incident of the dog in the night-time</name>
    <author>
      <name>Mark Haddon</name>
    </author>
  </book>
</root>";
            dynamic root = Parse(config, xml);
            Assert.That(root.Books.Count, Is.EqualTo(2));
            Assert.That(root.Books[0].Name, Is.EqualTo("Watership down"));
            Assert.That(root.Books[1].Author.Name, Is.EqualTo("Mark Haddon"));
        }
    }
}
