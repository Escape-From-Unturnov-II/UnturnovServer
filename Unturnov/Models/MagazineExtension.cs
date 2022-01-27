using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    public class MagazineExtension
    {
        public ushort EmptyMagazineId;
        public List<MagazineType> MagazineTypeIds;

        public class MagazineType
        {
            public ushort MagazineId;
            public byte refillAmmoIndex;
        }
    }

}
