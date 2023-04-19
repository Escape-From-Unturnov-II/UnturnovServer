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
using static UnityEngine.Random;

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
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public byte[] state = null;

        public BarricadeWrapper()
        {

        }
        public BarricadeWrapper(EBuild barricadeType, ushort id, Vector3 location, Quaternion rotation, byte[] state) 
        {
            this.barricadeType = barricadeType;
            this.id = id;
            this.position = location;
            this.rotation = rotation;
            this.state = state;
        }
    }
}
