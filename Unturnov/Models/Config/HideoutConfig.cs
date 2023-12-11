using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace SpeedMann.Unturnov.Models.Config
{
    public class HideoutConfig
    {
        public bool Debug;
        public uint SpawnedBarricadesPerFrame;
        public Vector3Wrapper HideoutDimensions = new Vector3Wrapper();
        public Vector3Wrapper HideoutOriginOffset = new Vector3Wrapper();
        public Notification_UI Notification_UI = new Notification_UI();
        public List<Position> HideoutPositions = new List<Position>();
    }
}
