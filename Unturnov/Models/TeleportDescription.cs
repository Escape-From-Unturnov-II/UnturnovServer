using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;

namespace SpeedMann.Unturnov.Models
{
    public class TeleportDestination
    {
        public string NodeName = "";
        public Vector3 Position = Vector3.zero;
        public float Rotation;
       
        public TeleportDestination()
        {

        }
        public TeleportDestination(string name)
        {
            NodeName = name;    
        }
        public TeleportDestination(float x, float y, float z)
        {
            Position = new Vector3((float)x, (float)y, (float)z);
        }
        public Vector3 findDestinationPosition()
        {
            Vector3 position = Position;
            if (position.x == 0
                && position.y == 0
                && position.z == 0
                && findNodeByName(NodeName, out LocationNode node))
            {
                position = node.point + new Vector3(0f, 0.5f, 0f);
            }
            return position;
        }
        private bool findNodeByName(string name, out LocationNode node)
        {
            //TODO: fix allways 0,0,0
            node = null;
            if (name != "")
            {
                node = LevelNodes.nodes.OfType<LocationNode>().Where(n => n.name.ToLower().Contains(name.ToLower())).FirstOrDefault();
            }
            return node != null;
        }
    }
}
