using Rocket.API;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov
{
    public class UnturnovConfiguration : IRocketPluginConfiguration
    {
        public bool Debug;
        public ushort DeathDropFlag;
        public List<MagazineExtension> UnloadMagBlueprints;
        public List<DeathDrop> DeathDrops;
        [XmlIgnore]
        public List<CraftDescription> AutoCraft = new List<CraftDescription>();
       
        public void LoadDefaults()
        {
            Debug = true;
            DeathDropFlag = 0;
            DeathDrops = new List<DeathDrop>()
            {
                new DeathDrop()
                {
                  ItemId = 37125,
                  RequiredFalgValue = 0,
                },
            };
            UnloadMagBlueprints = new List<MagazineExtension>
            {
                new MagazineExtension
                {
                    EmptyMagazineId = 37775,
                    MagazineTypeIds = new List<MagazineExtension.MagazineType>
                    {
                        new MagazineExtension.MagazineType
                        {
                            MagazineId = 37994,
                            refillAmmoIndex = 0,
                        },
                        new MagazineExtension.MagazineType
                        {
                            MagazineId = 37995,
                            refillAmmoIndex = 0,
                        }
                    }
                },
                new MagazineExtension
                {
                    EmptyMagazineId = 37776,
                    MagazineTypeIds = new List<MagazineExtension.MagazineType>
                    {
                        new MagazineExtension.MagazineType
                        {
                            MagazineId = 38000,
                            refillAmmoIndex = 0,
                        },
                        new MagazineExtension.MagazineType
                        {
                            MagazineId = 38009,
                            refillAmmoIndex = 0,
                        }
                    }
                },
            };
            /*
            AutoCraft = new List<CraftDescription>
            {
                new CraftDescription()
                {
                    ResourceItemId = 66,
                    BlueprintItemId = 393,
                    BlueprintIndex = 0,
                },
                new CraftDescription()
                {
                    ResourceItemId = 393,
                    BlueprintItemId = 95,
                    BlueprintIndex = 0,
                }
            };
            */
        }
        public void updateConfig()
        {

        }
    }
}
