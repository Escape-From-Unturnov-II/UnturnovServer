using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeedMann.Unturnov.Models;
using SDG.Unturned;
using Rocket.Core.Logging;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SpeedMann.Unturnov.Helper
{
    internal class JsonManager
    {
        static string storagePath = "";
        static string savesFolder = "Saves";
        static string DirectoryPath;
        internal static void Init(string PluginDirectory)
        {
            string savesPath = Path.Combine(PluginDirectory, savesFolder);
            if (!Directory.Exists(savesPath))
            {
                Directory.CreateDirectory(savesPath);
            }
            DirectoryPath = PluginDirectory;
        }
        internal static List<BarricadeWrapper> readBarricadeWrappers(Player player)
        {
            var barricadeWrapper = new List<BarricadeWrapper>();
            string filePath = Path.Combine(DirectoryPath, savesFolder, "test.json");
            if (tryReadFromDisc(filePath, out var readData))
            {
                barricadeWrapper = readData.ToObject<List<BarricadeWrapper>>();
                return barricadeWrapper;
            }
            return barricadeWrapper;
        }
        internal static void saveBarricadeWrappers(Player player, List<BarricadeWrapper> wrappers)
        {
            string filePath = Path.Combine(DirectoryPath, savesFolder, "test.json");
            AsyncTrySaveToDisc(filePath, wrappers);
        }
        private static bool tryReadFromDisc(string outputPath, out JObject readData)
        {
            readData = null;
            if (!File.Exists(outputPath))
            {
                File.Create(outputPath);
                Logger.Log($"Created json file {outputPath}");
                return false;
            }
            JsonTextReader reader = null;
            try
            {
                StreamReader file = File.OpenText(outputPath);
                reader = new JsonTextReader(file);
                
                readData = (JObject)JToken.ReadFrom(reader);
                reader.Close();
                Logger.Log($"Loaded json data from {outputPath}");
            }
            catch (Exception)
            {
                reader?.Close();
                Logger.LogError($"Could not load json data from file {outputPath}");
                return false;
            }
            
            return true;
        }

        private static Task<bool> AsyncTrySaveToDisc(string outputPath, object data)
        {
            try
            {
                using (StreamWriter file = File.CreateText(outputPath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, data);
                    Logger.Log($"Saved json data to {outputPath}");
                }
            }
            catch (Exception)
            {
                Logger.LogError($"Could not save json data to file {outputPath}");
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }
        #region Converters
        public class Vector3Converter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                throw new NotImplementedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                float[] values = serializer.Deserialize<float[]>(reader);
                return new Vector3(values[0], values[1], values[2]);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if(value is Vector3)
                {
                    Vector3 vector3 = (Vector3)value;
                    serializer.Serialize(writer, new float[] { vector3.x, vector3.y, vector3.z });
                }
            }
        }
        public class QuaternionConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                throw new NotImplementedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                float[] values = serializer.Deserialize<float[]>(reader);
                return new Quaternion(values[0], values[1], values[2], values[3]);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value is Quaternion)
                {
                    Quaternion quaternion = (Quaternion)value;
                    serializer.Serialize(writer, new float[] { quaternion.x, quaternion.y, quaternion.z, quaternion.w });
                }
            }
        }
        #endregion
    }
}
