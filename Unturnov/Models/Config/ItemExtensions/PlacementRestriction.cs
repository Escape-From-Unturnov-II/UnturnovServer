using Org.BouncyCastle.Asn1.IsisMtt.X509;
using Rocket.Core.Logging;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models.Config
{
    public class PlacementRestriction
    {
        [XmlArrayItem(ElementName = "Item")]
        public List<ItemExtension> RestrictedItems = new List<ItemExtension>();
        public List<string> ValidFoundationSetNames = new List<string>();
        public string RequiredInfo = "FoundationName";
        [XmlIgnore]
        public Dictionary<ushort, PlacementFoundation> ValidBarricades = new Dictionary<ushort, PlacementFoundation>();
        [XmlIgnore]
        public Dictionary<ushort, PlacementFoundation> ValidObjects = new Dictionary<ushort, PlacementFoundation>();


        internal bool tryGetFoundationSetByName(string name, List<FoundationSet> foundationSets, out List<PlacementFoundation> set)
        {
            set = new List<PlacementFoundation>();
            foreach (FoundationSet list in foundationSets)
            {
                if (list.Name.ToLower().Equals(name.ToLower()))
                {
                    set = list.Foundations;
                    return true;
                }
            }

            return false;
        }
        internal void createFoundationDictionary(List<FoundationSet> foundationSets)
        {
            foreach (string name in ValidFoundationSetNames)
            {
                if (!tryGetFoundationSetByName(name, foundationSets, out List<PlacementFoundation> foundationSet))
                {
                    Logger.LogWarning("FoundationSet with name:" + name + " was not found!");
                    continue;
                }
                foreach (PlacementFoundation foundation in foundationSet)
                {
                    Dictionary<ushort, PlacementFoundation> selectedDict;
                    switch (foundation.type)
                    {
                        case EAssetType.ITEM:
                            selectedDict = ValidBarricades;
                            break;
                        case EAssetType.OBJECT:
                            selectedDict = ValidObjects;
                            break;
                        default:
                            Logger.LogError($"Foundation with Id: {foundation.Id} has invalid type {foundation.type}! \n" +
                                $"Valid types are ITEM and OBJECT");
                            continue;
                    }

                    if (selectedDict.ContainsKey(foundation.Id))
                    {
                        Logger.LogWarning($"Foundation with Id: {foundation.Id} and type: {foundation.type} is a duplicate!");
                        continue;
                    }

                    selectedDict.Add(foundation.Id, foundation);
                }
            }
        }
    }
}
