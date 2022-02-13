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
        public List<EmptyMagazineExtension> UnloadMagBlueprints;
        public List<DeathDrop> DeathDrops;
        public List<CombineDescription> AutoCombine;
        [XmlArrayItem(ElementName = "Item")]
        public List<ItemExtension> MultiUseItems;
        [XmlArrayItem(ElementName = "Item")]
        public List<ItemExtension> GunModdingResults;
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

            UnloadMagBlueprints = new List<EmptyMagazineExtension>
            {
                new EmptyMagazineExtension
                {
                    ItemId = 37775,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            ItemId = 37994,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            ItemId = 37995,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    ItemId = 37776,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            ItemId = 38000,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            ItemId = 38009,
                            RefillAmmoBlueprintIndex = 0,
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
            MultiUseItems = new List<ItemExtension>
            {
                new ItemExtension
                {
                    ItemId = 37185
                },
                new ItemExtension
                {
                    ItemId = 37186
                },
                new ItemExtension
                {
                    ItemId = 37187
                },
                new ItemExtension
                {
                    ItemId = 37188
                },
                new ItemExtension
                {
                    ItemId = 37189
                },
                new ItemExtension
                {
                    ItemId = 37190
                },
                new ItemExtension
                {
                    ItemId = 37191
                },
                new ItemExtension
                {
                    ItemId = 37192
                },
                new ItemExtension
                {
                    ItemId = 37193
                },
                new ItemExtension
                {
                    ItemId = 37194
                },
            };
            GunModdingResults = new List<ItemExtension>
            {
                new ItemExtension
                {
                    ItemId = 38051
                },
                new ItemExtension
                {
                    ItemId = 38052
                },
                new ItemExtension
                {
                    ItemId = 38053
                },
            };
        }
        public void updateConfig()
        {

        }
    }
}
