using System.Xml.Serialization;

namespace MetaLanguageParser.Resources
{

    /// 
    [System.CodeDom.Compiler.GeneratedCode("System.Xml", "4.0.30319.34283")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public partial class root
    {
        [XmlElement("assembly", typeof(rootAssembly), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlElement("data", typeof(rootData), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlElement("metadata", typeof(rootMetadata), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlElement("resheader", typeof(rootResheader), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public System.Collections.Generic.List<object> Items { get; set; }
    }
    /// 
    [System.CodeDom.Compiler.GeneratedCode("System.Xml", "4.0.30319.34283")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class rootAssembly
    {
        [XmlAttribute()]
        public string alias { get; set; }
        /// 
        [XmlAttribute()]
        public string name { get; set; }
    }
    /// 
    [System.CodeDom.Compiler.GeneratedCode("System.Xml", "4.0.30319.34283")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class rootData
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string value { get; set; }
        /// 
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string comment { get; set; }
        /// 
        [XmlAttribute()]
        public string name { get; set; }
        /// 
        [XmlAttribute()]
        public string type { get; set; }
        /// 
        [XmlAttribute()]
        public string mimetype { get; set; }
        /// 
        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string space { get; set; }
    }
    /// 
    [System.CodeDom.Compiler.GeneratedCode("System.Xml", "4.0.30319.34283")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class rootMetadata
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string value { get; set; }
        /// 
        [XmlAttribute()]
        public string name { get; set; }
        /// 
        [XmlAttribute()]
        public string type { get; set; }
        /// 
        [XmlAttribute()]
        public string mimetype { get; set; }
        /// 
        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string space { get; set; }
    }
    /// 
    [System.CodeDom.Compiler.GeneratedCode("System.Xml", "4.0.30319.34283")]
    [System.Serializable()]
    [System.Diagnostics.DebuggerStepThrough()]
    [System.ComponentModel.DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class rootResheader
    {
        /// 
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string value { get; set; }
        /// 
        [XmlAttribute()]
        public string name { get; set; }
    }
}