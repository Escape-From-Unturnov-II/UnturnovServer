using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    internal class SavedPlayerKit
    {
        internal byte health = 0;
        internal byte food = 0;
        internal byte water = 0;
        internal byte virus = 0;

        internal List<Item> kitItems = new List<Item>();

        internal SavedPlayerKit()
        {

        }
        internal SavedPlayerKit(List<Item> kitItems)
        {
            this.kitItems = kitItems;
        }
    }
}
