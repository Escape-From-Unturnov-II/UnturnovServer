using Rocket.API;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using SpeedMann.Unturnov.Models.Config;
using SpeedMann.Unturnov.Models.Config.ItemExtensions;
using SpeedMann.Unturnov.Models.Config.QuestExtensions;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace SpeedMann.Unturnov
{
    public class UnturnovConfiguration : IRocketPluginConfiguration
    {
        public bool Debug;
        public string DatabaseConnectionString;
        public ushort PluginCraftingFlag = 40404;

        public uint BedTimer;

        public TeleportConfig TeleportConfig = new TeleportConfig();
        public ScavConfig ScavConfig = new ScavConfig();
        public List<EmptyMagazineExtension> UnloadMagBlueprints;
        public List<AirdropSignal> AirdropSignals;
        
        public List<CombineDescription> AutoCombine;
        [XmlArrayItem(ElementName = "Item")]
        public List<ItemExtension> GunModdingResults;
        

        public PlayerKitConfig NewPlayerKitConfig;
        public PlacementRestrictionConfig PlacementRestrictionConfig;
        public HideoutConfig HideoutConfig;
        public DeathAdditionConfig DeathDropConfig;
        public SecureCaseConfig SecureCaseConfig;
        public OpenableItemsConfig OpenableItemsConfig;

        public void LoadDefaults()
        {
            Debug = true;
            DatabaseConnectionString = "SERVER=127.0.0.1;DATABASE=unturnov;UID=root;PASSWORD=;PORT=3306;charset=utf8";
            
            

            BedTimer = 1;
            DeathDropConfig = new DeathAdditionConfig()
            {
                DeathDropFlag = 0,

                DeathDrops = new List<DeathDrop>()
                {
                    new DeathDrop()
                    {
                      Id = 37125,
                      RequiredFalgValue = 0,
                    },
                },
            };
            NewPlayerKitConfig = new PlayerKitConfig(new List<ItemExtensionAmount> 
            { 
                new ItemExtensionAmount(37500),
                new ItemExtensionAmount(37501),
            });

            UnloadMagBlueprints = new List<EmptyMagazineExtension>
            {
                #region 12ga
                new EmptyMagazineExtension
                {
                    Name = "12ga_20_round",
                    Id = 37775,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "buck_mag",
                            Id = 37994,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "slug_mag",
                            Id = 37995,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "12ga_5_round",
                    Id = 37776,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "buck_mag",
                            Id = 38000,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "slug_mag",
                            Id = 38009,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                #endregion
                #region 5.45x39
                new EmptyMagazineExtension
                {
                    Name = "30_ak12",
                    Id = 38178,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "bt_mag",
                            Id = 38182,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "ps_mag",
                            Id = 37641,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "30_6L23",
                    Id = 38177,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "bt_mag",
                            Id = 38181,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "ps_mag",
                            Id = 37620,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "45_6L23",
                    Id = 38179,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "bt_mag",
                            Id = 38183,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "ps_mag",
                            Id = 37642,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "60_6L23",
                    Id = 38180,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "bt_mag",
                            Id = 38184,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "ps_mag",
                            Id = 37643,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                #endregion
                #region 5.56x45
                new EmptyMagazineExtension
                {
                    Name = "30_6L29",
                    Id = 38163,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M855_mag",
                            Id = 37650,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M856A1_mag",
                            Id = 38170,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "30_Circle",
                    Id = 38164,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M855_mag",
                            Id = 37659,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M856A1_mag",
                            Id = 38164,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "30_HK",
                    Id = 38165,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M855_mag",
                            Id = 37613,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M856A1_mag",
                            Id = 38172,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "30_PMAG_Black",
                    Id = 38166,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M855_mag",
                            Id = 37614,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M856A1_mag",
                            Id = 38173,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "30_PMAG_Tan",
                    Id = 38167,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M855_mag",
                            Id = 37615,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M856A1_mag",
                            Id = 38174,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "30_STANAG",
                    Id = 38168,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M855_mag",
                            Id = 37600,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M856A1_mag",
                            Id = 38175,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "60_PMAG",
                    Id = 38169,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M855_mag",
                            Id = 37616,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M856A1_mag",
                            Id = 38176,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                #endregion
                #region 7.62x39
                new EmptyMagazineExtension
                {
                    Name = "10_Ribbed",
                    Id = 38185,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "BP_mag",
                            Id = 38193,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "PS_mag",
                            Id = 37679,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "10_sks_clip",
                    Id = 38186,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "BP_mag",
                            Id = 38194,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "PS_mag",
                            Id = 38050,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "20_sks_clip",
                    Id = 38187,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "BP_mag",
                            Id = 38195,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "PS_mag",
                            Id = 38055,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "30_ak55",
                    Id = 38188,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "BP_mag",
                            Id = 38196,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "PS_mag",
                            Id = 37660,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "30_banana",
                    Id = 38189,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "BP_mag",
                            Id = 38197,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "PS_mag",
                            Id = 37671,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "30_PMAG",
                    Id = 38190,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "BP_mag",
                            Id = 38198,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "PS_mag",
                            Id = 37640,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "35_SKS_Mag",
                    Id = 38191,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "BP_mag",
                            Id = 38199,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "PS_mag",
                            Id = 38056,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "75_drum",
                    Id = 38192,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "BP_mag",
                            Id = 38200,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "PS_mag",
                            Id = 37672,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                #endregion
                #region 7.62x51
                new EmptyMagazineExtension
                {
                    Name = "10_AICS",
                    Id = 38203,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M62_mag",
                            Id = 38210,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M80_mag",
                            Id = 38042,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M993_mag",
                            Id = 38216,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "10_VPO",
                    Id = 38204,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M62_mag",
                            Id = 38211,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M80_mag",
                            Id = 38049,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "20_KAC",
                    Id = 38305,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M62_mag",
                            Id = 38306,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M80_mag",
                            Id = 38307,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "20_MMW",
                    Id = 38205,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M62_mag",
                            Id = 38212,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M80_mag",
                            Id = 38092,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "20_PMAG",
                    Id = 38297,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M62_mag",
                            Id = 38298,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M80_mag",
                            Id = 38299,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "20_SA-58",
                    Id = 38206,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M62_mag",
                            Id = 38213,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M80_mag",
                            Id = 38030,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "30_SA-58",
                    Id = 38207,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M62_mag",
                            Id = 38214,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M80_mag",
                            Id = 38039,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "5_AICS",
                    Id = 38201,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M62_mag",
                            Id = 38208,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M80_mag",
                            Id = 38041,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M993_mag",
                            Id = 38215,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "5_VPO",
                    Id = 38202,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M62_mag",
                            Id = 38209,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "M80_mag",
                            Id = 38045,
                            RefillAmmoBlueprintIndex = 0,
                        }
                    }
                },
                #endregion
                #region 7.62x54
                new EmptyMagazineExtension
                {
                    Name = "10_SVDS",
                    Id = 38218,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "BT_mag",
                            Id = 38220,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "PS_mag",
                            Id = 38060,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "5_Mosin_Clip",
                    Id = 38217,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "BT_mag",
                            Id = 38219,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "PS_mag",
                            Id = 38100,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                #endregion
                #region 9.18
                new EmptyMagazineExtension
                {
                    Name = "20_PP-91",
                    Id = 38222,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "BZT_mag",
                            Id = 37990,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "PBM_mag",
                            Id = 38225,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "30_PP-91",
                    Id = 38223,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "BZT_mag",
                            Id = 37993,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "PBM_mag",
                            Id = 38226,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "8_PM",
                    Id = 38221,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "BZT_mag",
                            Id = 38078,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "PBM_mag",
                            Id = 38224,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                #endregion
                #region 9.19
                new EmptyMagazineExtension
                {
                    Name = "15_P226R",
                    Id = 38227,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "AP_mag",
                            Id = 38236,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "Pst_mag",
                            Id = 38082,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "17_Glock",
                    Id = 38228,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "AP_mag",
                            Id = 38237,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "Pst_mag",
                            Id = 38120,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "18_MP-443",
                    Id = 38229,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "AP_mag",
                            Id = 38238,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "Pst_mag",
                            Id = 38080,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "20_MP5",
                    Id = 38230,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "AP_mag",
                            Id = 38239,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "Pst_mag",
                            Id = 37682,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "30_MP5",
                    Id = 38231,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "AP_mag",
                            Id = 38240,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "Pst_mag",
                            Id = 37689,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "30_MP9",
                    Id = 38232,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "AP_mag",
                            Id = 38241,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "Pst_mag",
                            Id = 37692,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "30_PP-19-01",
                    Id = 38233,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "AP_mag",
                            Id = 38242,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "Pst_mag",
                            Id = 37980,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "33_Glock",
                    Id = 38234,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "AP_mag",
                            Id = 38243,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "Pst_mag",
                            Id = 38128,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "50_MP5",
                    Id = 38235,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "AP_mag",
                            Id = 38244,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "Pst_mag",
                            Id = 37690,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                #endregion
                #region 9.39
                new EmptyMagazineExtension
                {
                    Name = "10_6L24",
                    Id = 38245,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "SP-5_mag",
                            Id = 38066,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "SPP_mag",
                            Id = 38247,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                new EmptyMagazineExtension
                {
                    Name = "20_6L25",
                    Id = 38246,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "SP-5_mag",
                            Id = 38069,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "SPP_mag",
                            Id = 38248,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                #endregion
                #region 366
                new EmptyMagazineExtension
                {
                    Name = "VPO-209",
                    Id = 38251,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "AP-M_mag",
                            Id = 38252,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "FMJ_mag",
                            Id = 38130,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                #endregion
                #region 12.7x55
                new EmptyMagazineExtension
                {
                    Name = "20_ASh-12",
                    Id = 38249,
                    LoadedMagazines = new List<EmptyMagazineExtension.LoadedMagazineVariant>
                    {
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "PS12B_mag",
                            Id = 38250,
                            RefillAmmoBlueprintIndex = 0,
                        },
                        new EmptyMagazineExtension.LoadedMagazineVariant
                        {
                            Name = "PS12_mag",
                            Id = 37675,
                            RefillAmmoBlueprintIndex = 0,
                        },
                    }
                },
                #endregion
            };
            AirdropSignals = new List<AirdropSignal>
            {
                new AirdropSignal(38107, 0, 10),
            };
            AutoCombine = new List<CombineDescription>
            {
                #region RUB
                // 5/10
                new CombineDescription(37001, 2, 37002),
                // 10/50
                new CombineDescription(37002, 5, 37003),
                // 50/100
                new CombineDescription(37003, 2, 37004),
                // 100/500
                new CombineDescription(37004, 5, 37005),
                // 500/1000
                new CombineDescription(37005, 2, 37006),
                // 1000/5000
                new CombineDescription(37006, 5, 37007),
                // 5000/10000
                new CombineDescription(37007, 2, 37008),
                // 10000/50000
                new CombineDescription(37008, 5, 37009),
                // 50000/100000
                new CombineDescription(37009, 2, 37010),
                #endregion
                #region USD
                // 1/5
                new CombineDescription(37011, 5, 37013),
                // 2/10
                new CombineDescription(37012, 5, 37014),
                // 5/10
                new CombineDescription(37013, 2, 37014),
                // 10/50
                new CombineDescription(37014, 5, 37016),
                // 20/100
                new CombineDescription(37015, 5, 37017),
                // 50/100
                new CombineDescription(37016, 2, 37017),
                // 100/500
                new CombineDescription(37017, 5, 37018),
                // 500/1000
                new CombineDescription(37018, 2, 37019),
                // 1000/5000
                new CombineDescription(37019, 5, 37020),
                #endregion
                #region EUR
                // 5/10
                new CombineDescription(37021, 2, 37022),
                // 10/50
                new CombineDescription(37022, 5, 37024),
                // 20/100
                new CombineDescription(37023, 5, 37025),
                // 50/100
                new CombineDescription(37024, 2, 37025),
                // 100/500
                new CombineDescription(37025, 5, 37027),
                // 200/1000
                new CombineDescription(37026, 5, 37028),
                // 500/1000
                new CombineDescription(37027, 2, 37028),
                // 1000/5000
                new CombineDescription(37028, 5, 37029),
                // 5000/10000
                new CombineDescription(37029, 2, 37030),
                #endregion
            };
            GunModdingResults = new List<ItemExtension>
            {
                #region AK-74N
                new ItemExtension
                {
                    Id = 37621
                },
                new ItemExtension
                {
                    Id = 37622
                },
                new ItemExtension
                {
                    Id = 37623
                },
                new ItemExtension
                {
                    Id = 37624
                },
                new ItemExtension
                {
                    Id = 37625
                },
                new ItemExtension
                {
                    Id = 37626
                },
                new ItemExtension
                {
                    Id = 37627
                },
                new ItemExtension
                {
                    Id = 37628
                },
                #endregion
                #region AK-101
                new ItemExtension
                {
                    Id = 37651
                },
                new ItemExtension
                {
                    Id = 37652
                },
                new ItemExtension
                {
                    Id = 37653
                },
                new ItemExtension
                {
                    Id = 37654
                },
                new ItemExtension
                {
                    Id = 37655
                },
                new ItemExtension
                {
                    Id = 37656
                },
                new ItemExtension
                {
                    Id = 37657
                },
                new ItemExtension
                {
                    Id = 37658
                },
                #endregion
                #region AKM
                new ItemExtension
                {
                    Id = 37661
                },
                new ItemExtension
                {
                    Id = 37662
                },
                new ItemExtension
                {
                    Id = 37663
                },
                new ItemExtension
                {
                    Id = 37664
                },
                new ItemExtension
                {
                    Id = 37665
                },
                new ItemExtension
                {
                    Id = 37666
                },
                new ItemExtension
                {
                    Id = 37667
                },
                new ItemExtension
                {
                    Id = 37668
                },
                #endregion
                #region AKS-74U
                new ItemExtension
                {
                    Id = 37631
                },
                new ItemExtension
                {
                    Id = 37632
                },
                new ItemExtension
                {
                    Id = 37633
                },
                new ItemExtension
                {
                    Id = 37634
                },
                new ItemExtension
                {
                    Id = 37635
                },
                new ItemExtension
                {
                    Id = 37636
                },
                new ItemExtension
                {
                    Id = 37637
                },
                new ItemExtension
                {
                    Id = 37638
                },
                #endregion
                #region M4A1
                new ItemExtension
                {
                    Id = 37601
                },
                new ItemExtension
                {
                    Id = 37602
                },
                new ItemExtension
                {
                    Id = 37603
                },
                new ItemExtension
                {
                    Id = 37604
                },
                new ItemExtension
                {
                    Id = 37605
                },
                new ItemExtension
                {
                    Id = 37606
                },
                new ItemExtension
                {
                    Id = 37607
                },
                new ItemExtension
                {
                    Id = 37608
                },
                #endregion
                #region MK47
                new ItemExtension
                {
                    Id = 37646
                },
                new ItemExtension
                {
                    Id = 37647
                },
                new ItemExtension
                {
                    Id = 37648
                },
                new ItemExtension
                {
                    Id = 37649
                },
                #endregion
                #region SA-58
                new ItemExtension
                {
                    Id =  38031
                },
                new ItemExtension
                {
                    Id =  38032
                },
                new ItemExtension
                {
                    Id =  38033
                },
                new ItemExtension
                {
                    Id =  38034
                },
                new ItemExtension
                {
                    Id =  38035
                },
                new ItemExtension
                {
                    Id =  38036
                },
                new ItemExtension
                {
                    Id =  38037
                },
                new ItemExtension
                {
                    Id =  38038
                },
                #endregion
                #region AS-Val
                new ItemExtension
                {
                    Id =  38070
                },
                new ItemExtension
                {
                    Id =  38071
                },
                new ItemExtension
                {
                    Id =  38072
                },
                new ItemExtension
                {
                    Id =  38073
                },
                #endregion
                #region VPO-209 
                new ItemExtension
                {
                    Id = 38131
                },
                new ItemExtension
                {
                    Id = 38132
                },
                new ItemExtension
                {
                    Id = 38133
                },
                new ItemExtension
                {
                    Id = 38134
                },
                new ItemExtension
                {
                    Id = 38135
                },
                new ItemExtension
                {
                    Id = 38136
                },
                new ItemExtension
                {
                    Id = 38137
                },
                new ItemExtension
                {
                    Id = 38138
                },
                #endregion
                #region SKS
                new ItemExtension
                {
                    Id = 38051
                },
                new ItemExtension
                {
                    Id = 38052
                },
                new ItemExtension
                {
                    Id = 38053
                },
                #endregion
                #region VSS
                new ItemExtension
                {
                    Id = 38067
                },
                new ItemExtension
                {
                    Id = 38068
                },
                #endregion
                #region M700
                new ItemExtension
                {
                    Id = 38043
                },
                new ItemExtension
                {
                    Id = 38044
                },
                #endregion
                #region Mosin Infantry
                new ItemExtension
                {
                    Id = 38101
                },
                new ItemExtension
                {
                    Id = 38102
                },
                new ItemExtension
                {
                    Id = 38103
                },
                new ItemExtension
                {
                    Id = 38104
                },
                #endregion
                #region M870
                new ItemExtension
                {
                    Id = 38021
                },
                new ItemExtension
                {
                    Id = 38022
                },
                new ItemExtension
                {
                    Id = 38023
                },
                new ItemExtension
                {
                    Id = 38024
                },
                new ItemExtension
                {
                    Id = 38025
                },
                new ItemExtension
                {
                    Id = 38026
                },
                new ItemExtension
                {
                    Id = 38027
                },
                new ItemExtension
                {
                    Id = 38028
                },
                #endregion
                #region MP-153 
                new ItemExtension
                {
                    Id = 38011
                },
                new ItemExtension
                {
                    Id = 38012
                },
                new ItemExtension
                {
                    Id = 38013
                },
                new ItemExtension
                {
                    Id = 38014
                },
                new ItemExtension
                {
                    Id = 38015
                },
                new ItemExtension
                {
                    Id = 38016
                },
                new ItemExtension
                {
                    Id = 38017
                },
                new ItemExtension
                {
                    Id = 38018
                },
                #endregion
                #region Saiga
                new ItemExtension
                {
                    Id = 38001
                },
                new ItemExtension
                {
                    Id = 38002
                },
                new ItemExtension
                {
                    Id = 38003
                },
                new ItemExtension
                {
                    Id = 38004
                },
                new ItemExtension
                {
                    Id = 38005
                },
                new ItemExtension
                {
                    Id = 38006
                },
                new ItemExtension
                {
                    Id = 38007
                },
                new ItemExtension
                {
                    Id = 38008
                },
                #endregion
                #region MP5
                new ItemExtension
                {
                    Id =  37683
                },
                new ItemExtension
                {
                    Id =  37685
                },
                new ItemExtension
                {
                    Id =  37686
                },
                new ItemExtension
                {
                    Id =  37687
                },
                #endregion
                #region PP-19-01
                new ItemExtension
                {
                    Id = 37981
                },
                new ItemExtension
                {
                    Id = 37983
                },
                new ItemExtension
                {
                    Id = 37984
                },
                new ItemExtension
                {
                    Id = 37987
                },
                #endregion
                #region PP-91
                new ItemExtension
                {
                    Id = 37991
                },
                new ItemExtension
                {
                    Id = 37992
                },
                #endregion
                #region Glock 17
                new ItemExtension
                {
                    Id = 38121
                },
                new ItemExtension
                {
                    Id = 38122
                },
                new ItemExtension
                {
                    Id = 38123
                },
                #endregion
                #region Glock 18C
                new ItemExtension
                {
                    Id = 38124
                },
                new ItemExtension
                {
                    Id = 38125
                },
                new ItemExtension
                {
                    Id = 38126
                },
                new ItemExtension
                {
                    Id = 38127
                },
                #endregion
                #region P226R
                new ItemExtension
                {
                    Id = 38083
                },
                new ItemExtension
                {
                    Id = 38084
                },
                #endregion
            };
            TeleportConfig = new TeleportConfig(false, 5, 50304,
                new List<RaidTeleport>{
                    new RaidTeleport( "DefaultMap",
                        50305, 
                        new QuestCooldown(50005, 50306, 50307),
                        1,
                        new TeleportDestination("FA1"),
                        new List<TeleportDestination> {
                            new TeleportDestination("FA1"),
                            new TeleportDestination("FA2"),
                            new TeleportDestination("FA3"),
                            new TeleportDestination("FA4"),
                            new TeleportDestination("FA5"),
                            new TeleportDestination("FA6"),
                            new TeleportDestination("FA7"),
                        })
                    });
            ScavConfig = new ScavConfig(50303, 0, 
                new QuestCooldown(50008, 50308, 50309),
                new List<ScavKitTier>
            {
                new ScavKitTier
                {
                    CooldownInMin = 1,

                    GlassesConfig = new KitTierEntry
                    {
                        CountMin = 0,
                        CountMax = 1,
                        WeightMin = 0,
                        WeightMax = 100,
                        NoItemChance = 0.25f,
                    },
                    MaskConfig = new KitTierEntry
                    {
                        CountMin = 0,
                        CountMax = 1,
                        WeightMin = 0,
                        WeightMax = 100,
                        NoItemChance = 0.25f,
                    },
                    HatConfig = new KitTierEntry
                    {
                        CountMin = 0,
                        CountMax = 1,
                        WeightMin = 0,
                        WeightMax = 100,
                        NoItemChance = 0.3f,
                    },
                    VestConfig = new KitTierEntry
                    {
                        CountMin = 1,
                        CountMax = 1,
                        WeightMin = 0,
                        WeightMax = 100,
                        NoItemChance = 0f,
                    },
                    BackpackConfig = new KitTierEntry
                    {
                        CountMin = 0,
                        CountMax = 1,
                        WeightMin = 0,
                        WeightMax = 100,
                        NoItemChance = 0.5f,
                    },
                    ShirtConfig = new KitTierEntry
                    {
                        CountMin = 1,
                        CountMax = 1,
                        WeightMin = 0,
                        WeightMax = 100,
                        NoItemChance = 0,
                    },
                    PantsConfig = new KitTierEntry
                    {
                        CountMin = 1,
                        CountMax = 1,
                        WeightMin = 0,
                        WeightMax = 100,
                        NoItemChance = 0,
                    },


                    GunConfig = new KitTierEntry
                    {
                        CountMin = 1,
                        CountMax = 1,
                        WeightMin = 0,
                        WeightMax = 100,
                        NoItemChance = 0,
                    },
                    MedConfig = new KitTierEntry
                    {
                        CountMin = 0,
                        CountMax = 3,
                        WeightMin = 0,
                        WeightMax = 100,
                        NoItemChance = 0.25f,
                    },
                    SupplyConfig = new KitTierEntry
                    {
                        CountMin = 0,
                        CountMax = 0,
                        WeightMin = 0,
                        WeightMax = 100,
                        NoItemChance = 0,
                    },
                }
            },
                new ScavSpawnTableSet
            {
                GlassesTable = new SpawnTableExtension(),
                MaskTable = new SpawnTableExtension(),
                HatTable = new SpawnTableExtension
                {
                    Items = new List<SpawnTableEntry>
                    {
                        // ushanka
                        new SpawnTableEntry(37438, 10),
                        // Kolpak
                        new SpawnTableEntry(37430, 20),
                        // Fast MT
                        new SpawnTableEntry(37415, 15),
                        // SSh-68
                        new SpawnTableEntry(37436, 30),
                        // Ronin 
                        new SpawnTableEntry(37434, 80),
                    }
                },
                VestTable = new SpawnTableExtension
                {
                    Items = new List<SpawnTableEntry>
                    {
                        // scav vest
                        new SpawnTableEntry(37333, 10),
                        new SpawnTableEntry(37314, 15),
                        // paca
                        new SpawnTableEntry(37328, 20),
                        new SpawnTableEntry(37329, 30),
                        // module 3m
                        new SpawnTableEntry(37323, 20),
                        new SpawnTableEntry(37324, 30),
                        // 6B5-15
                        new SpawnTableEntry(37302, 50),
                        // AVS
                        new SpawnTableEntry(37308, 90),
                        // 6B43
                        new SpawnTableEntry(37303, 200),
                    }
                },
                BackpackTable = new SpawnTableExtension
                {
                    Items = new List<SpawnTableEntry>
                    {
                        // army bag
                        new SpawnTableEntry(37473, 10),
                        // MBSS
                        new SpawnTableEntry(37473, 15),
                        // scav bp
                        new SpawnTableEntry(37476, 20),
                        // Day Pack
                        new SpawnTableEntry(37476, 30),
                        // Pilgrim
                        new SpawnTableEntry(37471, 80),
                    }
                },
                ShirtTable = new SpawnTableExtension
                {
                    Items = new List<SpawnTableEntry>
                    {
                        new SpawnTableEntry(37512, 10),
                    }
                },
                PantsTable = new SpawnTableExtension
                {
                    Items = new List<SpawnTableEntry>
                    {
                        new SpawnTableEntry(37513, 10),
                    }
                },

                GunTable = new SpawnTableExtension
                {
                    Items = new List<SpawnTableEntry>
                    {
                        // mac
                        new SpawnTableEntry(38079, 10),
                    }
                },
                MedTable = new SpawnTableExtension
                {
                    Items = new List<SpawnTableEntry>
                    {
                        // Bandage
                        new SpawnTableEntry(37072, 10),
                        // Army bandage
                        new SpawnTableEntry(37187, 15),
                        // painkillers 
                        new SpawnTableEntry(37191, 15),
                        // chees med
                        new SpawnTableEntry(37186, 20),
                        // splint
                        new SpawnTableEntry(37071, 20),
                        
                        // car med 
                        new SpawnTableEntry(37188, 40),
                        // alu splint
                        new SpawnTableEntry(37193, 40),
                        // salewa
                        new SpawnTableEntry(37194, 80),
                    }
                },
                SupplyTable = new SpawnTableExtension(),
            });
            

            HideoutConfig = new HideoutConfig
            {
                Notification_UI = new Notification_UI(52310, 5230),
                HideoutSpawnRotation = 0,
                SpawnedBarricadesPerFrame = 5,
                HideoutDimensions = new Vector3Wrapper(new Vector3(11, 5, 8)),
                HideoutPositions = new List<Position> 
                { 
                    new Position(868, 8.5f, -350, 0),
                    new Position(879, 8.5f, -350, 0),
                    new Position(879, 8.5f, -350, 180),
                }
            };
            PlacementRestrictionConfig = new PlacementRestrictionConfig
            {
                Debug = true,
                Offset = 0.1f,
                Notification_UI = new Notification_UI(52310, 5230),
                Restrictions = new List<PlacementRestriction>
                {
                    new PlacementRestriction
                    {
                        RestrictedItems = new List<ItemExtension>
                        {
                           new ItemExtension(330, "carrot seed"),
                        },
                        ValidFoundationSetNames = new List<string>
                        {
                            "planter"
                        }
                    },
                },
                FoundationSets = new List<FoundationSet>
                {
                    new FoundationSet
                    {
                        Name = "planter",
                        Foundations = new List<PlacementFoundation>
                        {
                            new PlacementFoundation(331, EAssetType.ITEM ,"planter"),
                            new PlacementFoundation(1345, EAssetType.ITEM, "plot"),
                        }
                    },
                    new FoundationSet
                    {
                        Name = "craftingtest",
                        Foundations = new List<PlacementFoundation>
                        {
                            new PlacementFoundation(331, EAssetType.OBJECT ,"planter"),
                        }
                    },
                }
            };

            SecureCaseConfig = new SecureCaseConfig
            {
                Debug = true,
                Notification_UI = new Notification_UI(52170, 5230),
                CaseUpgradeFlagId = 50302,
                CaseSizes = new List<CaseSize>()
                {
                    new CaseSize(2, 2),
                    new CaseSize(3, 2),
                    new CaseSize(4, 2),
                    new CaseSize(3, 3),
                    new CaseSize(3, 4),
                },
                BlacklistedItems = new List<ItemExtension>()
                {
                    new ItemExtension(10),
                },
            };
            OpenableItemsConfig = new OpenableItemsConfig
            {
                Debug = true,
                Notification_UI = new Notification_UI
                {
                    Enabled = true,
                    UI_Id = 52172,
                    UI_Key = 5230,
                },
                OpenableItems = new List<OpenableItem>
                {
                    new OpenableItem
                    {
                        Name = "KeyTool",
                        TableName = "KeyTool",
                        Id = 52100,
                        Height = 3,
                        Width = 3,
                        UsedWhitelists = new List<string>
                        {
                            "Keys"
                        }
                    },
                    new OpenableItem
                    {
                        Name = "Wallet",
                        TableName = "Wallet",
                        Id = 52101,
                        Height = 2,
                        Width = 2,
                        UsedWhitelists = new List<string>
                        {
                            "Money"
                        }
                    },
                },
                ItemWhitelists = new List<ItemWhitelist>
                {
                new ItemWhitelist
                {
                    Name = "Keys",
                    WhitelistedItems = new List<ItemExtension>
                    {
                        new ItemExtension
                        {
                            Name = "Factory_Key",
                            Id = 55206,
                        },
                        new ItemExtension
                        {
                            Name = "Factory_Pumping_Key",
                            Id = 55225,
                        },
                    }
                },
                new ItemWhitelist
                {
                    Name = "Money",
                    WhitelistedItems = new List<ItemExtension>
                    {
                        new ItemExtension
                        {
                            Name = "5 RUB",
                            Id = 37001,
                        },
                        new ItemExtension
                        {
                            Name = "10 RUB",
                            Id = 37002,
                        },
                        new ItemExtension
                        {
                            Name = "50 RUB",
                            Id = 37003,
                        },
                        new ItemExtension
                        {
                            Name = "100 RUB",
                            Id = 37004,
                        },
                        new ItemExtension
                        {
                            Name = "500 RUB",
                            Id = 37005,
                        },
                        new ItemExtension
                        {
                            Name = "1.000 RUB",
                            Id = 37006,
                        },
                        new ItemExtension
                        {
                            Name = "5.000 RUB",
                            Id = 37007,
                        },
                        new ItemExtension
                        {
                            Name = "10.000 RUB",
                            Id = 37008,
                        },
                        new ItemExtension
                        {
                            Name = "50.000 RUB",
                            Id = 37009,
                        },
                        new ItemExtension
                        {
                            Name = "100.000 RUB",
                            Id = 37010,
                        },
                    }
                },
            },
            };
        }

        public void addNames()
        {
            addNames(DeathDropConfig.DeathDrops);
            addNames(GunModdingResults);

            foreach (PlacementRestriction restriction in PlacementRestrictionConfig.Restrictions)
            {
                addNames(restriction.RestrictedItems);
            }
            foreach (FoundationSet foundationSet in PlacementRestrictionConfig.FoundationSets)
            {
                addNames(foundationSet.Foundations);
            }
            foreach (CombineDescription combDesc in AutoCombine)
            {
                addName(combDesc);
                addName(combDesc.Result);
            }

            foreach(EmptyMagazineExtension magazineExtension in UnloadMagBlueprints)
            {
                addName(magazineExtension);
                addNames(magazineExtension.LoadedMagazines);
            }
            Unturnov.Inst.Configuration.Save();
        }

        private void addNames<T>(List<T> itemExtensions) where T : ItemExtension
        {
            if (itemExtensions == null) return;

            foreach (T itemExtension in itemExtensions)
            {
                addName(itemExtension);
            }
        }
        private void addName<T>(T itemExtension) where T : ItemExtension
        {
            ItemAsset itemAsset = (ItemAsset)Assets.find(EAssetType.ITEM, itemExtension.Id);
            if (itemAsset != null)
            {
                itemExtension.Name = itemAsset.name;
            }
        }
        public void updateConfig()
        {

        }
    }
}
