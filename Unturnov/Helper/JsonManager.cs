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
using SpeedMann.Unturnov.Models.Hideout;
using Org.BouncyCastle.Crypto;

namespace SpeedMann.Unturnov.Helper
{
    internal class JsonManager
    {
        private static bool Debug = true;
        static string PluginSavesPath;
        static string PluginDirectoryPath;
        internal static void Init(string PluginDirectory, bool debug = true)
        {
            Debug = debug;
            PluginSavesPath = $"{PluginDirectory}\\Saves";
            // creates dicts if needed
            Directory.CreateDirectory(PluginSavesPath);
            PluginDirectoryPath = PluginDirectory;

        }
        internal static bool tryWriteToSaves(Player player, string fileName, object data)
        {
            if (!checkAndReturnSaveFilePath(player, fileName, out string filePath))
            {
                return false;
            }
            return tryWriteToDisc(filePath, data);
        }
        internal static bool tryReadFromSaves<T>(Player player, string fileName, out T readData)
        {
            readData = default;
            if (!checkAndReturnSaveFilePath(player, fileName, out string filePath))
            {
                return false;
            }
            if (!tryReadFromDisc(filePath, out var jsonData))
            {
                return false;
            }

            try
            {
                readData = jsonData.ToObject<T>();
            }
            catch (Exception)
            {
                Logger.LogError($"Could not parse json data from file {filePath} to {typeof(T)}");
                return false;
            }
            
            return true;
        }
        internal static bool tryReadFromDisc(string outputPath, out JToken readData)
        {
            readData = null;
            if (!File.Exists(outputPath))
            {
                return false;
            }
            try
            {
                // using handles closing streams automatically
                using (StreamReader file = File.OpenText(outputPath))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    readData = JToken.ReadFrom(reader);
                }
                if(Debug)
                    Logger.Log($"Loaded json data from {outputPath}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Could not load json data from file {outputPath}\n {ex}");
                return false;
            }
            return true;
        }
        internal static bool tryWriteToDisc(string outputPath, object data)
        {
            try
            {
                using (FileStream stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, data);
                    if (Debug)
                        Logger.Log($"Saved json data to {outputPath}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Could not save json data to file {outputPath}\n {ex}");
                return false;
            }
            return true;
        }
        internal static Task<bool> tryReadFromDiscAsync(string outputPath, out JToken readData)
        {
            return Task.FromResult(tryReadFromDisc(outputPath, out readData));
        }
        internal static Task<bool> tryWriteToDiscAsync(string outputPath, object data)
        {
            return Task.FromResult(tryWriteToDisc(outputPath, data));
        }
        #region Converters
        public class EBuildConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(EBuild);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                string value = (string)reader.Value;
                return Enum.Parse(typeof(EBuild), value);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value is EBuild)
                {
                    writer.WriteValue(value.ToString());
                }
            }
        }
        public class Vector3Converter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Vector3);
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
                return objectType == typeof(Quaternion);
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
        private static bool checkAndReturnSaveFilePath(Player player, string fileName, out string filePath)
        {
            filePath = "";
            var playerId = player.channel.owner.playerID;
            // TODO: handle +_{playerId.characterID}
            string playerSavesPath = $"{PluginSavesPath}\\{playerId.steamID}";
            try
            {
                Directory.CreateDirectory(playerSavesPath);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error creating directories of path {playerSavesPath}:\n {ex}");
                return false;
            }
            filePath = $"{playerSavesPath}\\{fileName}.json";
            return true;
        }
    }
}
