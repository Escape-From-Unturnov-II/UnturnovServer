using SDG.Framework.Devkit;
using SDG.Unturned;
using System.Linq;
using UnityEngine;

/*
 * Allows teleportint to location, NPC spawnpoints, LocationNodes
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
        public TeleportDestination(float x, float y, float z, float rotation = 0)
        {
            Position = new Vector3((float)x, (float)y, (float)z);
            Rotation = rotation;
        }
        public void SetupTeleportDestination()
        {
            if (Position.x != 0
                || Position.y != 0
                || Position.z != 0)
            {
                return;
            }

            if (NodeName != ""
                && TryFindLocationByName(NodeName, out Vector3 position, out float rotation))
            {
                Position = position;
                Rotation = rotation;
            }
        }

        public static bool TryFindLocationByName(string nodeName, out Vector3 position, out float rotation)
        {
            rotation = 0;
            position = Vector3.zero;

            if (nodeName == "")
            {
                return false;
            }

            Spawnpoint spawnpoint = SpawnpointSystemV2.Get().FindSpawnpoint(nodeName);

            if (spawnpoint != null)
            {
                position = spawnpoint.transform.position;
                rotation = spawnpoint.transform.rotation.eulerAngles.y;
                return true;
            }

            LocationNode node = LevelNodes.nodes.OfType<LocationNode>().Where(n => n.name.ToLower().Contains(nodeName.ToLower())).FirstOrDefault();
            if (node != null)
            {
                position = node.point + new Vector3(0f, 0.5f, 0f);
                return true;
            }
            return false;
        }
    }
}
