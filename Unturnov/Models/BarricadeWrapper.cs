using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SDG.Unturned;
using SpeedMann.Unturnov.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpeedMann.Unturnov.Models
{
    public class BarricadeWrapper
    {
        [JsonConverter(typeof(JsonManager.EBuildConverter))]
        public EBuild barricadeType;
        public ushort id;
        [JsonConverter(typeof(JsonManager.Vector3Converter))]
        public Vector3 position;
        [JsonConverter(typeof(JsonManager.QuaternionConverter))]
        public Quaternion rotation;

        //optional fields, only for specific barricade types
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<ItemJarWrapper> items = null;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public uint planted = 0;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ushort storedLiquid = 0;

        public BarricadeWrapper()
        {

        }
        public BarricadeWrapper(EBuild barricadeType, ushort id, Vector3 location, Quaternion rotation) 
        {
            this.barricadeType = barricadeType;
            this.id = id;
            this.position = location;
            this.rotation = rotation;
        }
        public BarricadeWrapper(EBuild barricadeType, ushort id, Vector3 location, Quaternion rotation, List<ItemJarWrapper> items) 
            : this(barricadeType, id, location, rotation)
        {
            this.items = items;
        }
        public BarricadeWrapper(EBuild barricadeType, ushort id, Vector3 location, Quaternion rotation, uint planted)
            : this(barricadeType, id, location, rotation)
        {
            this.planted = planted;
        }
        public BarricadeWrapper(EBuild barricadeType, ushort id, Vector3 location, Quaternion rotation, ushort storedLiquid)
            : this(barricadeType, id, location, rotation)
        {
            this.storedLiquid = storedLiquid;
        }
    }
}
