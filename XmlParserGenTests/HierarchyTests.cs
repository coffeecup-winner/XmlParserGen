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
        [Test]
        public void SeveralListsTest() {
            string config = @"
<root>
  <books list=""book"" />
  <authors list=""author"" />
</root>";
            string xml = @"
<root>
  <books />
  <authors />
</root>
";
            dynamic root = Parse(config, xml);
            Assert.That(root.Books.Count, Is.EqualTo(0));
            Assert.That(root.Authors.Count, Is.EqualTo(0));
        }
        [Test]
        public void AttributesTest() {
            string config = @"
<root>
  <books list=""book"">
    <book.attributes>
      <author type=""string"" />
      <name type=""string"" />
    </book.attributes>
  </books>
</root>
";
            string xml = @"
<root>
  <books>
    <book author=""Mark Haddon"" name=""The curious incident of the dog in the night-time"" />
  </books>
</root>
";
            dynamic root = Parse(config, xml);
            Assert.That(root.Books.Count, Is.EqualTo(1));
            Assert.That(root.Books[0].Author, Is.EqualTo("Mark Haddon"));
            Assert.That(root.Books[0].Name, Is.EqualTo("The curious incident of the dog in the night-time"));
        }
        [Test]
        public void GlobalDefinitionsTest() {
            string config = @"
<root>
  <root.definitions>
    <name>
      <str type=""string"" />
    </name>
  </root.definitions>
  <books list=""book"">
    <name defined=""true"" />
    <author>
      <name defined=""true"" />
    </author>
  </books>
</root>
";
            string xml = @"
<root>
  <books>
    <book>
      <name><str>Watership down</str></name>
      <author>
        <name><str>Richard Adams</str></name>
      </author>
    </book>
  </books>
</root>
";
            dynamic root = Parse(config, xml);
            Assert.That(root.Books[0].Name.Str, Is.EqualTo("Watership down"));
            Assert.That(root.Books[0].Author.Name.Str, Is.EqualTo("Richard Adams"));
            Assert.That(root.Books[0].Name.GetType(), Is.EqualTo(root.Books[0].Author.Name.GetType()));
        }
        [Test]
        public void CustomNamesTest() {
            string config = @"<root><book name=""bOOk"" typename=""BooK""></book></root>";
            string xml = @"<root><book /></root>";
            dynamic root = Parse(config, xml);
            Assert.That(root.bOOk, Is.Not.Null);
            Assert.That(root.bOOk.GetType().Name, Is.EqualTo("BooK"));
        }
    }
}
