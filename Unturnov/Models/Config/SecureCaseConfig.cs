using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models.Config
{
    public class SecureCaseConfig
    {
        public bool Debug;
        public Notification_UI Notification_UI;
        public ushort CaseUpgradeFlagId;
        public List<CaseSize> CaseSizes;

        [XmlArrayItem(ElementName = "Item")]
        public List<ItemExtension> BlacklistedItems;
    }
}
