using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models
{
    public class ItemStackConfig
    {
        public bool AutoAddMagazines = true;
        //public bool AutoAddAttachments = true;
        //public bool AutoAddAmmo = true;

        [XmlArrayItem(ElementName = "Item")]
        public List<ItemExtensionAmount> StackableItems;
        public List<ReplaceFullDescription> ReplaceFull;

        public ItemStackConfig()
        {
            AutoAddMagazines = false;
            //AutoAddAttachments = false;
            //AutoAddAmmo = false;
            StackableItems = new List<ItemExtensionAmount>();
            ReplaceFull = new List<ReplaceFullDescription>();
        }

    }
}