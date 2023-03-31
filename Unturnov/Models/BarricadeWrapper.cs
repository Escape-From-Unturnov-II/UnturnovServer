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
        public ushort id;
        public Vector3 location;
        public Quaternion rotation;
        public List<ItemJar> items = new List<ItemJar>();
        public BarricadeWrapper()
        {

        }
        public BarricadeWrapper(ushort id, Vector3 location, Quaternion rotation, List<ItemJar> items) 
        {
            this.id = id;
            this.location = location;
            this.rotation = rotation;

            if(items != null)
                this.items = items;                
        }

       
    }
}
