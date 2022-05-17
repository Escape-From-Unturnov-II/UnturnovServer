using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models.Config
{
    public class OpenableItemsConfig
    {
        public bool Debug;
        public Notification_UI Notification_UI;
        [XmlArrayItem(ElementName = "Item")]
        public List<OpenableItem> OpenableItems;
        public List<ItemWhitelist> ItemWhitelists;
    }
}
