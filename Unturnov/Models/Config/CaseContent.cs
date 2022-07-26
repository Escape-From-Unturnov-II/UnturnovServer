using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    public class CaseContent
    {
        public ulong PlayerId;
        public List<StoredItem> Items;
        public CaseContent(ulong playerId, List<StoredItem> items)
        {
            PlayerId = playerId;
            Items = items;
        }
        public CaseContent()
        {

        }
    }
}
