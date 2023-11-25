using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;

namespace SpeedMann.Unturnov.Models.Config
{
    public class Position
    {
        [XmlAttribute("X")]
        public float x;
        [XmlAttribute("Y")]
        public float y;
        [XmlAttribute("Z")]
        public float z;
        [XmlAttribute("Rotation")]
        public float rot;
        public Position()
        {

        }
        public Vector3 GetVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}