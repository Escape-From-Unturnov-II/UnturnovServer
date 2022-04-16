using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    public class ItemExtension
    {
        public string Name;
        public ushort Id;

        public ItemExtension()
        {

        }

        public ItemExtension( ushort itemId)
        {
            Id = itemId;
            Name = "";
        }
    }
}
