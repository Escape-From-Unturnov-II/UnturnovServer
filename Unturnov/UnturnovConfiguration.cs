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
        public List<ReloadExtension> ReloadExtensions;
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
                #region AK-74N
                new ItemExtension
                {
                    ItemId = 37621
                },
                new ItemExtension
                {
                    ItemId = 37622
                },
                new ItemExtension
                {
                    ItemId = 37623
                },
                new ItemExtension
                {
                    ItemId = 37624
                },
                new ItemExtension
                {
                    ItemId = 37625
                },
                new ItemExtension
                {
                    ItemId = 37626
                },
                new ItemExtension
                {
                    ItemId = 37627
                },
                new ItemExtension
                {
                    ItemId = 37628
                },
                #endregion
                #region AK-101
                new ItemExtension
                {
                    ItemId = 37651
                },
                new ItemExtension
                {
                    ItemId = 37652
                },
                new ItemExtension
                {
                    ItemId = 37653
                },
                new ItemExtension
                {
                    ItemId = 37654
                },
                new ItemExtension
                {
                    ItemId = 37655
                },
                new ItemExtension
                {
                    ItemId = 37656
                },
                new ItemExtension
                {
                    ItemId = 37657
                },
                new ItemExtension
                {
                    ItemId = 37658
                },
                #endregion
                #region AKM
                new ItemExtension
                {
                    ItemId = 37661
                },
                new ItemExtension
                {
                    ItemId = 37662
                },
                new ItemExtension
                {
                    ItemId = 37663
                },
                new ItemExtension
                {
                    ItemId = 37664
                },
                new ItemExtension
                {
                    ItemId = 37665
                },
                new ItemExtension
                {
                    ItemId = 37666
                },
                new ItemExtension
                {
                    ItemId = 37667
                },
                new ItemExtension
                {
                    ItemId = 37668
                },
                #endregion
                #region AKS-74U
                new ItemExtension
                {
                    ItemId = 37631
                },
                new ItemExtension
                {
                    ItemId = 37632
                },
                new ItemExtension
                {
                    ItemId = 37633
                },
                new ItemExtension
                {
                    ItemId = 37634
                },
                new ItemExtension
                {
                    ItemId = 37635
                },
                new ItemExtension
                {
                    ItemId = 37636
                },
                new ItemExtension
                {
                    ItemId = 37637
                },
                new ItemExtension
                {
                    ItemId = 37638
                },
                #endregion
                #region M4A1
                new ItemExtension
                {
                    ItemId = 37601
                },
                new ItemExtension
                {
                    ItemId = 37602
                },
                new ItemExtension
                {
                    ItemId = 37603
                },
                new ItemExtension
                {
                    ItemId = 37604
                },
                new ItemExtension
                {
                    ItemId = 37605
                },
                new ItemExtension
                {
                    ItemId = 37606
                },
                new ItemExtension
                {
                    ItemId = 37607
                },
                new ItemExtension
                {
                    ItemId = 37608
                },
                #endregion
                #region MK47
                new ItemExtension
                {
                    ItemId = 37646
                },
                new ItemExtension
                {
                    ItemId = 37647
                },
                new ItemExtension
                {
                    ItemId = 37648
                },
                new ItemExtension
                {
                    ItemId = 37649
                },
                #endregion
                #region SA-58
                new ItemExtension
                {
                    ItemId =  38031
                },
                new ItemExtension
                {
                    ItemId =  38032
                },
                new ItemExtension
                {
                    ItemId =  38033
                },
                new ItemExtension
                {
                    ItemId =  38034
                },
                new ItemExtension
                {
                    ItemId =  38035
                },
                new ItemExtension
                {
                    ItemId =  38036
                },
                new ItemExtension
                {
                    ItemId =  38037
                },
                new ItemExtension
                {
                    ItemId =  38038
                },
                #endregion
                #region AS-Val
                new ItemExtension
                {
                    ItemId =  38070
                },
                new ItemExtension
                {
                    ItemId =  38071
                },
                new ItemExtension
                {
                    ItemId =  38072
                },
                new ItemExtension
                {
                    ItemId =  38073
                },
                #endregion
                #region VPO-209 
                new ItemExtension
                {
                    ItemId = 38131
                },
                new ItemExtension
                {
                    ItemId = 38132
                },
                new ItemExtension
                {
                    ItemId = 38133
                },
                new ItemExtension
                {
                    ItemId = 38134
                },
                new ItemExtension
                {
                    ItemId = 38135
                },
                new ItemExtension
                {
                    ItemId = 38136
                },
                new ItemExtension
                {
                    ItemId = 38137
                },
                new ItemExtension
                {
                    ItemId = 38138
                },
                #endregion
                #region SKS
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
                #endregion
                #region VSS
                new ItemExtension
                {
                    ItemId = 38067
                },
                new ItemExtension
                {
                    ItemId = 38068
                },
                #endregion
                #region M700
                new ItemExtension
                {
                    ItemId = 38043
                },
                new ItemExtension
                {
                    ItemId = 38044
                },
                #endregion
                #region Mosin Infantry
                new ItemExtension
                {
                    ItemId = 38101
                },
                new ItemExtension
                {
                    ItemId = 38102
                },
                new ItemExtension
                {
                    ItemId = 38103
                },
                new ItemExtension
                {
                    ItemId = 38104
                },
                #endregion
                #region M870
                new ItemExtension
                {
                    ItemId = 38021
                },
                new ItemExtension
                {
                    ItemId = 38022
                },
                new ItemExtension
                {
                    ItemId = 38023
                },
                new ItemExtension
                {
                    ItemId = 38024
                },
                new ItemExtension
                {
                    ItemId = 38025
                },
                new ItemExtension
                {
                    ItemId = 38026
                },
                new ItemExtension
                {
                    ItemId = 38027
                },
                new ItemExtension
                {
                    ItemId = 38028
                },
                #endregion
                #region MP-153 
                new ItemExtension
                {
                    ItemId = 38011
                },
                new ItemExtension
                {
                    ItemId = 38012
                },
                new ItemExtension
                {
                    ItemId = 38013
                },
                new ItemExtension
                {
                    ItemId = 38014
                },
                new ItemExtension
                {
                    ItemId = 38015
                },
                new ItemExtension
                {
                    ItemId = 38016
                },
                new ItemExtension
                {
                    ItemId = 38017
                },
                new ItemExtension
                {
                    ItemId = 38018
                },
                #endregion
                #region Saiga
                new ItemExtension
                {
                    ItemId = 38001
                },
                new ItemExtension
                {
                    ItemId = 38002
                },
                new ItemExtension
                {
                    ItemId = 38003
                },
                new ItemExtension
                {
                    ItemId = 38004
                },
                new ItemExtension
                {
                    ItemId = 38005
                },
                new ItemExtension
                {
                    ItemId = 38006
                },
                new ItemExtension
                {
                    ItemId = 38007
                },
                new ItemExtension
                {
                    ItemId = 38008
                },
                #endregion
                #region MP5
                new ItemExtension
                {
                    ItemId =  37683
                },
                new ItemExtension
                {
                    ItemId =  37685
                },
                new ItemExtension
                {
                    ItemId =  37686
                },
                new ItemExtension
                {
                    ItemId =  37687
                },
                #endregion
                #region PP-19-01
                new ItemExtension
                {
                    ItemId = 37981
                },
                new ItemExtension
                {
                    ItemId = 37983
                },
                new ItemExtension
                {
                    ItemId = 37984
                },
                new ItemExtension
                {
                    ItemId = 37987
                },
                #endregion
                #region PP-91
                new ItemExtension
                {
                    ItemId = 37991
                },
                new ItemExtension
                {
                    ItemId = 37992
                },
                #endregion
                #region Glock 17
                new ItemExtension
                {
                    ItemId = 38121
                },
                new ItemExtension
                {
                    ItemId = 38122
                },
                new ItemExtension
                {
                    ItemId = 38123
                },
                #endregion
                #region Glock 18C
                new ItemExtension
                {
                    ItemId = 38124
                },
                new ItemExtension
                {
                    ItemId = 38125
                },
                new ItemExtension
                {
                    ItemId = 38126
                },
                new ItemExtension
                {
                    ItemId = 38127
                },
                #endregion
                #region P226R
                new ItemExtension
                {
                    ItemId = 38083
                },
                new ItemExtension
                {
                    ItemId = 38084
                },
                #endregion
            };
            ReloadExtensions = new List<ReloadExtension>
            {
                new ReloadExtension
                {
                    AmmoStack = new ItemExtension(37998),
                    Compatible = new List<ReloadInner>
                    {
                        new ReloadInner
                        {
                            Magazine = new ItemExtension(38010),
                            Gun = new List<ItemExtension>
                            {
                                new ItemExtension(38021),
                            }
                        }
                    }
                }
            };
        }
        public void updateConfig()
        {

        }
    }
}
