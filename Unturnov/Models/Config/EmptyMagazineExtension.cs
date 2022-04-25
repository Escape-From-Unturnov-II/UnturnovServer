using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    /*
     * Unloading mags
     * All magazines defined in LoadedMagazines get replaced with the empty version when empty
     * When getting an empty mag the value will be changed to 0
     * 
        // mag unload blueprint
        Blueprint_5_State_Transfer
        Blueprint_5_Type Gear 
        Blueprint_5_Supplies 1
        // id of this mag
        Blueprint_5_Supply_0_ID 37994 
        Blueprint_5_Outputs 2
        // id of the empty variant of the mag
        Blueprint_5_Output_0_ID 37775
        // id of the loaded ammo
        Blueprint_5_Output_1_ID 37998
        Blueprint_5_Build 30
     * 
     * Loadning empty mags
     * Redirects the Blueprints to the equivalent blueprint of the loaded alternative (matching is done by supply[0] id)
     * 
        // loading empty mag
        Blueprint_0_Type Ammo
        Blueprint_0_Supplies 1
        // id of the ammo to load
        Blueprint_0_Supply_0_ID 37998 
        Blueprint_0_Build 30
     */
    public class EmptyMagazineExtension : ItemExtension
    {
        public List<LoadedMagazineVariant> LoadedMagazines;

        public class LoadedMagazineVariant : ItemExtension
        {
            public byte RefillAmmoBlueprintIndex;
        }
    }

}
