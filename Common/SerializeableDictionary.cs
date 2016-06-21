using System.Collections.Generic;
using System;
using System.Xml.Serialization;
// Source: http://huseyint.com/2007/12/xml-serializable-generic-dictionary-tipi/


[XmlRoot("dictionary")]
public class SerializableDictionary<TKey, TValue>
    : Dictionary<TKey, TValue>, IXmlSerializable
{
    /// <summary>Gets or sets the element with the specified key.</summary>
    /// <param name="key">The key of the element to get or set.</param>
    /// <returns>The element with the specified key. See <see cref="List{T}.this[int]"/></returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="key"/> is null</exception>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found</exception>
    public new TValue this[TKey key]
    {
        get {
            try {
                return base[key];
            } catch (KeyNotFoundException knfe) {

                throw new NotSupportedException($"Could not find an entry named '{key}'!", knfe);
            } }
        set { base[key] = value; }
    }

    #region IXmlSerializable Members
    public System.Xml.Schema.XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(System.Xml.XmlReader reader)
    {
        XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
        XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

        bool wasEmpty = reader.IsEmptyElement;
        reader.Read();

        if (wasEmpty)
            return;

        while (reader.NodeType != System.Xml.XmlNodeType.EndElement) {
            reader.ReadStartElement("item");

            reader.ReadStartElement("key");
            TKey key = (TKey)keySerializer.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("value");
            TValue value = (TValue)valueSerializer.Deserialize(reader);
            reader.ReadEndElement();

            this.Add(key, value);

            reader.ReadEndElement();
            reader.MoveToContent();
        }
        reader.ReadEndElement();
    }

    public void WriteXml(System.Xml.XmlWriter writer)
    {
        XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
        XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

        foreach (TKey key in Keys) {
            writer.WriteStartElement("item");

            writer.WriteStartElement("key");
            keySerializer.Serialize(writer, key);
            writer.WriteEndElement();

            writer.WriteStartElement("value");
            TValue value = this[key];
            valueSerializer.Serialize(writer, value);
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
    }
    #endregion
}

/// <summary>
/// A <see cref="SerializableDictionary{TKey, TValue}"/> with <typeparamref name="TKey"/> and <typeparamref name="TValue"/> being <see cref="string"/>.
/// </summary>
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
            
            var value = reader.Value; // Read the Value of the current Node (CDATA-Node)

            this.Add(key, value);
            reader.Read(); // Walk to next Node
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
            writer.WriteString(value);

            writer.WriteEndElement();
        }
    }
    #endregion
}