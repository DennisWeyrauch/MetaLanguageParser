using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;
using XMLData;
using System.Runtime.InteropServices.WindowsRuntime;


namespace Common
{

    class Serializer
    {
#if DEBUG
        StorageFolder localFolder = ApplicationData.Current.LocalFolder;

        // Write data to a file
        async void WriteTimestamp()
        {
            var formatter = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("longtime");

            StorageFile sampleFile = await localFolder.CreateFileAsync("dataFile.txt",
                CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(sampleFile, formatter.Format(DateTime.Now));
        }
        
        // Read data from a file
        async Task ReadTimestamp()
        {
            try
            {
                StorageFile sampleFile = await localFolder.GetFileAsync("dataFile.txt");
                String timestamp = await FileIO.ReadTextAsync(sampleFile);
                // Data is contained in timestamp
            }
            catch (Exception)
            {
                // Timestamp not found
            }
        }
#endif
        private Serializer() { }
        public static async Task<T> DeserializeFile<T>(string filename = "responses.xml")
        {
            T obj = default(T);
            var serializer = new XmlSerializer(typeof(T));
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.GetFileAsync(filename);
            Stream stream = await file.OpenStreamForReadAsync();
            obj = (T)serializer.Deserialize(stream); // Quicker
            //obj = Serializer.DeserializeFromStream<T>(ref stream);
            stream.Dispose();
            return obj;
        }

        public static async Task SerializeFile<T>(T obj, string filename = "responses.xml")
        {
            var serializer = new XmlSerializer(typeof(T));
            try
            {
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                Stream stream = await file.OpenStreamForWriteAsync();
                using (StreamWriter writer = new StreamWriter(stream))
                {
                
                    serializer.Serialize(writer, obj);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
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

    }
}
