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
        public Vector3 rotation;
        public BarricadeWrapper()
        {

        }
        public BarricadeWrapper(ushort id, Vector3 location, Vector3 rotation) 
        {
            this.id = id;
            this.location = location;
            this.rotation = rotation;
        }

       
    }
}
