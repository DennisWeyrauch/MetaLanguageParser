using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MetaLanguageParser.Resources
{

    /// <summary>
    /// A <see cref="SerializableDictionary{TKey, TValue}"/> with <typeparamref name="TKey"/> and <typeparamref name="TValue"/> being <see cref="string"/>.
    /// </summary>
    /// <code>
    /// <dictionary>
    ///     <entry name="__KEY">__VALUE</entry>
    ///     <entry name="__KEY" xml:space="preserve">__VALUE_withWhitespace</entry>
    /// </dictionary>
    /// </code>
    [XmlRoot("dictionary")]
    public class ConfigurationDictionary : SerializableDictionary<string, string>, IXmlSerializable
    {
        #region IXmlSerializable Members

        public new void ReadXml(System.Xml.XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty) return;
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement) {
                var key = reader.GetAttribute("name"); // Read the attribute "name" of the current node
                    reader.ReadStartElement("entry"); // Assert a startElement called "entry"
                string value = "<INVALID>";
                value = reader.Value; // Read the Value of the current Node (CDATA-Node)
                reader.Read(); // Walk to next Node
                this.Add(key, value);
                reader.ReadEndElement(); // Assert an EndElement called "entry"
                reader.MoveToContent(); // Skip any intermediate non-data nodes
                }
            reader.ReadEndElement();
        }

        public new void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (var key in Keys) {
                writer.WriteStartElement("entry");

                writer.WriteAttributeString("name", key);

                var value = this[key];
                if (value.StartsWith(" ")) {
                    writer.WriteAttributeString("xml:space", "preserve");
                }
                writer.WriteString(value);

                writer.WriteEndElement();
            }
        }
        #endregion
    }
}
