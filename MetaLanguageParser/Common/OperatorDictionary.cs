using MetaLanguageParser.Operands;
using static MetaLanguageParser.Operands.Operation;
using System;
using System.Xml;
using System.Linq;
using System.Xml.Serialization;
using static Common.Extensions;

namespace MetaLanguageParser.Resources
{

    public enum eOpDictType { Boolean, Arithmetic, Destination }

    /// <summary>
    /// A <see cref="SerializableDictionary{TKey, TValue}"/> used for OperatorMapping <para/>
    /// </summary>
    /// <code>
    /// <dictionary>
    ///     <unary>
    ///         <entry name="!">Negate</entry>
    ///     </unary>
    ///     <binary>
    ///         <entry name="&amp;">And</entry>
    ///     </binary>
    /// </dictionary>
    /// </code>
    [XmlRoot("dictionary")]
    public class OperatorDictionary : SerializableDictionary<string, Operation>, IXmlSerializable
    {
        private eOpDictType opType;

        public OperatorDictionary() { }
        public OperatorDictionary(eOpDictType e)
        {
            this.opType = e;
        }
        public void setType(eOpDictType t) { opType = t; }
        #region IXmlSerializable Members

        public new void ReadXml(XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;
            opType = reader.GetAttribute("type").AsEnum<eOpDictType>();
            reader.Read();

            if (wasEmpty) return;
            Type type = null;
            Console.WriteLine(reader.BaseURI);

            while (reader.NodeType != XmlNodeType.EndElement) {
                type = Operation.getOpType(reader.Name, opType);
                reader.ReadStartElement();
                while (reader.NodeType != XmlNodeType.EndElement) {
                    var key = reader.GetAttribute("name"); // Read the attribute "name" of the current node
                    reader.ReadStartElement("entry"); // Assert a startElement called "entry"

                    var value = reader.Value; // Read the Value of the current Node (CDATA-Node)

                    this.Add(key, createFrom(type, value));
                    reader.Read(); // Walk to next Node
                    reader.ReadEndElement(); // Assert an EndElement called "entry"
                    reader.MoveToContent(); // Skip any intermediate non-data nodes
                }
                reader.ReadEndElement();
            }
            reader.ReadEndElement();
        }

        public new void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("type", opType.ToString());
            var groups = Keys.GroupBy(k => this[k].getXmlBase());
            foreach (var group in groups) {
                writer.WriteStartElement(group.Key);
                foreach (var key in group) {
                    writer.WriteStartElement("entry");

                    writer.WriteAttributeString("name", key);

                    var value = this[key];
                    writer.WriteString(value._name);

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }
        #endregion
    }
}