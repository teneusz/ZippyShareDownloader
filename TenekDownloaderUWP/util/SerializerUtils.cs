using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace TenekDownloaderUWP.util
{

       public class SerializerUtils { 

        public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new() { 

//        {
//            TextWriter writer = null;
//            try
//            {
//                var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite);
//                writer = new StreamWriter(filePath, append);
//                writer.Write(contentsToWriteToFile);
//            }
//            finally
//            {
//                writer?.Close();
//            }
        }

        /// <summary>
        /// Reads an object instance from an Json file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to read from the file.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the Json file.</returns>
        public static T ReadFromJsonFile<T>(string filePath) where T : new()
        {
//            TextReader reader = null;
//            T result;
//            try
//            {
//                reader = new StreamReader(filePath);
//                var fileContents = reader.ReadToEnd();
//                result = JsonConvert.DeserializeObject<T>(fileContents);
//            }
//            catch (Exception ex)
//            {
//                result = new T();
//            }
//            finally
//            {
//                reader?.Close();
//            }
//
//            return result;
        return new T();
        }

    }
}