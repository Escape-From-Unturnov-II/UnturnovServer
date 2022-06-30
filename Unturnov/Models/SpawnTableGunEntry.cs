using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    public class SpawnTableGunEntry : SpawnTableEntry
    {
        public List<SpawnTableEntry> Sights = new List<SpawnTableEntry>();
        public List<SpawnTableEntry> Tacticals = new List<SpawnTableEntry>();
        public List<SpawnTableEntry> Grips = new List<SpawnTableEntry>();
        public List<SpawnTableEntry> Muzzle = new List<SpawnTableEntry>();

        public int MinMags = 0;
        public int MaxMags = 0;
        public List<SpawnTableEntry> Mags = new List<SpawnTableEntry>();
        public int MinSpareAmmo = 0;
        public int MaxSpareAmmo = 0;
        public List<SpawnTableEntry> SpareAmmo = new List<SpawnTableEntry>();
    }
}
