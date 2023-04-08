using SDG.Unturned;
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
        public Vector3 location;
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
            this.location = location;
            this.rotation = rotation;
            this.planted = planted;

            if (items != null)
                this.items = items;                
        }

       
    }
}
