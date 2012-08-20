namespace XmlParserGen {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    
    
    public class Author {
        
        private string name;
        
        public Author(System.Xml.Linq.XElement element) {
            this.name = element.Element("name").Value;
        }
        
        public string Name {
            get {
                return this.name;
            }
        }
    }
    
    public class Book {
        
        private string name;
        
        private Author author;
        
        public Book(System.Xml.Linq.XElement element) {
            this.name = element.Element("name").Value;
            this.author = new Author(element.Element("author"));
        }
        
        public string Name {
            get {
                return this.name;
            }
        }
        
        public Author Author {
            get {
                return this.author;
            }
        }
    }
    
    public class Root {
        
        private List<Book> books;
        
        Root(System.Xml.Linq.XElement element) {
            this.books = new List<Book>();
            System.Collections.Generic.IEnumerator<System.Xml.Linq.XElement> iterator = element.Elements("book").GetEnumerator();
            try {
                for (
                ; iterator.MoveNext(); 
                ) {
                    this.books.Add(new Book(iterator.Current));
                }
            }
            finally {
                iterator.Dispose();
            }
        }
        
        public List<Book> Books {
            get {
                return this.books;
            }
        }
        
        public static Root ReadFromFile(string filename) {
            System.IO.Stream stream = new System.IO.FileStream(filename, System.IO.FileMode.Open);
            try {
                System.Xml.Linq.XDocument xDocument = System.Xml.Linq.XDocument.Load(stream);
                return new Root(xDocument.Root);
            }
            finally {
                stream.Dispose();
            }
        }
    }
}

