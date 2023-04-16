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
        public EBuild barricadeType;
        public ushort id;
        [JsonConverter(typeof(JsonManager.Vector3Converter))]
        public Vector3 position;
        [JsonConverter(typeof(JsonManager.QuaternionConverter))]
        public Quaternion rotation;
        public uint planted;
        public List<ItemJar> items = new List<ItemJar>();
        public BarricadeWrapper()
        {

        }
        public BarricadeWrapper(EBuild barricadeType, ushort id, Vector3 location, Quaternion rotation, List<ItemJar> items, uint planted) 
        {
            this.barricadeType = barricadeType;
            this.id = id;
            this.position = location;
            this.rotation = rotation;
            this.planted = planted;

            if (items != null)
                this.items = items;                
        }

       
    }
}
