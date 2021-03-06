﻿using Microsoft.CSharp;
using NUnit.Framework;
using System;

namespace XmlParserGen.Tests {
    [TestFixture]
    public class DataTypeTests : TestsBase {
        [Test]
        public void StringTest() {
            string config = @"<root><x type=""string"" /></root>";
            string xml = @"<root><x>ABCDE</x></root>";
            dynamic root = Parse(config, xml);
            Assert.That(root.X, Is.EqualTo("ABCDE"));
        }
        [Test]
        public void IntTest() {
            string config = @"<root><x type=""int"" /></root>";
            string xml = @"<root><x>12</x></root>";
            dynamic root = Parse(config, xml);
            Assert.That(root.X, Is.EqualTo(12));
        }
        [Test]
        public void DoubleTest() {
            string config = @"<root><x type=""double"" /></root>";
            string xml = @"<root><x>1.2</x></root>";
            dynamic root = Parse(config, xml);
            Assert.That(root.X, Is.EqualTo(1.2));
        }
        [Test]
        public void BoolTest() {
            string config = @"<root><x type=""bool"" /><y type=""bool"" /></root>";
            string xml = @"<root><x>true</x><y>false</y></root>";
            dynamic root = Parse(config, xml);
            Assert.That(root.X, Is.True);
            Assert.That(root.Y, Is.False);
        }
        [Test]
        public void TrimStringTest() {
            string config = @"<root><x type=""string"" /></root>";
            string xml = @"<root><x>

    ABCDE

</x></root>";
            dynamic root = Parse(config, xml);
            Assert.That(root.X, Is.EqualTo("ABCDE"));
        }
        [Test]
        public void OmittingPropertiesTest() {
            string config = @"
<root>
  <x>
    <x.attributes>
      <attr type=""string"" />
    </x.attributes>
    <y>
      <z type=""string"" />
    </y>
  </x>
</root>";
            string xml = "<root><x /></root>";
            dynamic root = Parse(config, xml);
            Assert.That(root.X, Is.Not.Null);
            Assert.That((object)root.X.Attr, Is.Null);
            Assert.That((object)root.X.Y, Is.Null);
        }
        [Test]
        public void OmittingListPropertyTest() {
            string config = @"<root><xs list=""x"" /></root>";
            string xml = "<root />";
            dynamic root = Parse(config, xml);
            Assert.That(root.Xs.Count, Is.EqualTo(0));
        }
    }
}
