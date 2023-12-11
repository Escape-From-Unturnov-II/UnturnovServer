using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;

namespace SpeedMann.Unturnov.Models.Config
{
    public class Vector3Wrapper
    {
        [XmlIgnore]
        public Vector3 vector;
        public Vector3Wrapper()
        {
            vector = Vector3.zero;
        }
        public Vector3Wrapper(Vector3 vector)
        {
            this.vector = vector;
        }

        [XmlAttribute("X")]
        public float VectorX
        {
            get { return vector.x; }
            set { vector.x = value; }
        }

        [XmlAttribute("Y")]
        public float VectorY
        {
            get { return vector.y; }
            set { vector.y = value; }
        }

        [XmlAttribute("Z")]
        public float VectorZ
        {
            get { return vector.z; }
            set { vector.z = value; }
        }

    }
}
