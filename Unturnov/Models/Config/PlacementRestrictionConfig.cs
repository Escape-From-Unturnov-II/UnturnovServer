using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models.Config
{
    public class PlacementRestrictionConfig
    {
        public bool Debug = true;
        public int Offset = 1;
        public Notification_UI Notification_UI = new Notification_UI();
        public List<PlacementRestriction> Restrictions;
        public List<FoundationSet> FoundationSets;
    }
}
