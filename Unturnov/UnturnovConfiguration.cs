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
        public List<CombineDescription> AutoCombine;
        [XmlArrayItem(ElementName = "ItemId")]
        public List<ushort> MultiUseItems;

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
            AutoCombine = new List<CombineDescription>
            {
                new CombineDescription()
                {
                    ItemId = 37001,
                    RequiredAmount = 2,
                    ResultId = 37002,
                },
                new CombineDescription()
                {
                    ItemId = 37002,
                    RequiredAmount = 5,
                    ResultId = 37003,
                },
                new CombineDescription()
                {
                    ItemId = 37003,
                    RequiredAmount = 2,
                    ResultId = 37004,
                },
                new CombineDescription()
                {
                    ItemId = 37004,
                    RequiredAmount = 5,
                    ResultId = 37005,
                },
                new CombineDescription()
                {
                    ItemId = 37005,
                    RequiredAmount = 2,
                    ResultId = 37006,
                },
                new CombineDescription()
                {
                    ItemId = 37006,
                    RequiredAmount = 5,
                    ResultId = 37007,
                },
                new CombineDescription()
                {
                    ItemId = 37007,
                    RequiredAmount = 2,
                    ResultId = 37008,
                },
                new CombineDescription()
                {
                    ItemId = 37008,
                    RequiredAmount = 5,
                    ResultId = 37009,
                },
                new CombineDescription()
                {
                    ItemId = 37009,
                    RequiredAmount = 2,
                    ResultId = 37010,
                },
            };
            MultiUseItems = new List<ushort>
            {
                37185,
                37186,
                37187,
                37188,
                37189,
                37190,
                37191,
                37192,
                37193,
                37194,
            };
        }
        public void updateConfig()
        {

        }
    }
}
