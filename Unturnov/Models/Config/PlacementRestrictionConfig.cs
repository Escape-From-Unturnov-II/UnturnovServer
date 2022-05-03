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
        public float SearchCenterHeightChange = -0.5f;
        public float SearchRadius = 2f;
        public List<PlacementRestriction> Restrictions;
        public List<FoundationSet> FoundationSets;
    }
}
