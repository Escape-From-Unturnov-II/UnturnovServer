using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    public struct LoadedMagazineWrapper
    {
        public EmptyMagazineExtension.LoadedMagazineVariant loadedMagazineVariant;
        public ItemAsset itemAsset;

        public LoadedMagazineWrapper(EmptyMagazineExtension.LoadedMagazineVariant loadedMagazineVariant, ItemAsset itemAsset)
        {
            this.loadedMagazineVariant = loadedMagazineVariant;
            this.itemAsset = itemAsset;
        }
    }
}
