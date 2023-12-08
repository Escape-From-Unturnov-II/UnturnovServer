using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpeedMann.Unturnov.Models.Config
{
    public class HideoutConfig
    {
        public bool Debug;
        public uint SpawnedBarricadesPerFrame;
        public Vector3 HideoutDimensions;
        public Vector3 HideoutOriginOffset;
        public Notification_UI Notification_UI;
        public List<Position> HideoutPositions = new List<Position>();
    }
}
