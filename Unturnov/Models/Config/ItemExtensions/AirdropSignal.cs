using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models.Config.ItemExtensions
{
    public class AirdropSignal : ItemExtension
    {
        public ushort SpawnTableId;
        public float DelayInSec;
        //TODO: add support for guns and diff. mags
        public AirdropSignal(ushort itemId, ushort spawnTableId, float callDelayInSec, string name = "") : base (itemId, name)
        {
            SpawnTableId = spawnTableId;
            DelayInSec = callDelayInSec;
        }
    }
}
