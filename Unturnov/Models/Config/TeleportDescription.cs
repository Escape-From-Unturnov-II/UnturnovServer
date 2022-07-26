using SDG.Framework.Devkit;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

/*
 * Allows teleport location, NPC spawnpoints, LocationNodes
 */
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
        public void findDestination(out Vector3 position, out float rotation)
        {
            position = Position;
            rotation = Rotation;
            if (position.x == 0
                && position.y == 0
                && position.z == 0
                && findLocationByName(out Vector3 newPosition, out float newRotation))
            {
                position = newPosition;
                if (rotation == 0)
                {
                    rotation = newRotation;
                }
            }
        }
        internal bool findLocationByName(out Vector3 position, out float rotation)
        {
            rotation = 0;
            position = Vector3.zero;

            LocationNode node = null;
            Spawnpoint spawnpoint = null;
            if (NodeName != "")
            {
                spawnpoint = SpawnpointSystem.getSpawnpoint(NodeName);
                
                if(spawnpoint != null)
                {
                    position = spawnpoint.transform.position;
                    rotation = spawnpoint.transform.rotation.eulerAngles.y;
                    return true;
                }
                node = LevelNodes.nodes.OfType<LocationNode>().Where(n => n.name.ToLower().Contains(NodeName.ToLower())).FirstOrDefault();
                if (node != null)
                {
                    position = node.point + new Vector3(0f, 0.5f, 0f);
                    return true;
                }
            }
            return false;
        }
    }
}
