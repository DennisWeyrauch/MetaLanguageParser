using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Common
{
    public class Serializer
    {
        private Serializer() { }
        /// <summary>
        /// Deserialize a XML-File into an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The Type to transform into</typeparam>
        /// <param name="file">The filename or path to read from.</param>
        /// <returns>An instance of <typeparamref name="T"/></returns>
        public static T DeserializeFile<T>(string file)
        {
            T obj = default(T);
            var serializer = new XmlSerializer(typeof(T));
            Stream stream = File.OpenRead(file);
            try {
                obj = (T)serializer.Deserialize(stream); // Quicker
            } finally {
                stream.Dispose();
            }
            //obj = Serializer.DeserializeFromStream<T>(ref stream);
            return obj;
        }

        /// <summary>
        /// Serialize an instance of <typeparamref name="T"/> into a XML-File.
        /// </summary>
        /// <typeparam name="T">The Type to transform from.</typeparam>
        /// <param name="file">The filename or path to write to.</param>
        public static void SerializeFile<T>(T obj, string file)
        {
            var serializer = new XmlSerializer(typeof(T));
                Stream stream = File.Create(file);
            try {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    serializer.Serialize(writer, obj);
                }
            } catch (IOException) { Console.Error.WriteLine("Could not acquire WriteLock."); } 
                catch (Exception) { throw; } finally { stream.Dispose(); }

        }

        #region Binary and Stream
        static public byte[] SerializeToBinary<T>(T obj)
        {
            Stream mem = SerializeToStream<T>(obj);
            byte[] raw = new byte[(int)mem.Length];
            mem.Read(raw, 0, (int)mem.Length);
            mem.Dispose();
            return raw;
        }

        /// <summary>
        /// Serializes the given Object to an IO-Stream. The Position will be reset to 0
        /// </summary>
        /// <typeparam id="T"></typeparam>
        /// <param id="obj"></param>
        /// <returns></returns>
        static public MemoryStream SerializeToStream<T>(T obj)
        {
            Stream stream = new MemoryStream();
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(stream, obj);
            stream.Seek(0L, SeekOrigin.Begin);
            return (MemoryStream)stream;
        }

        static public T DeserializeFromBinary<T>(ref byte[] raw)
        {
            Stream mem = new MemoryStream();
            mem.Write(raw, 0, (int)raw.Length);
            T obj = DeserializeFromStream<T>(ref mem);
            mem.Dispose();
            return obj;
        }

        /// <summary>
        /// For Strings use <code>new MemoryStream(Encoding.UTF8.GetBytes(xmlstring));</code>
        /// This Function only calls a lokal XMLSerializer and stream.Seek(0), after which it deserializes the given Stream.
        /// </summary>
        /// <typeparam id="T"></typeparam>
        /// <param id="stream"></param>
        /// <returns></returns>
        static public T DeserializeFromStream<T>(ref Stream stream)
        {
            var xmlser = new XmlSerializer(typeof(T));
            stream.Seek(0L, SeekOrigin.Begin);
            return (T)xmlser.Deserialize(stream);
        }
        #endregion

    }
}
