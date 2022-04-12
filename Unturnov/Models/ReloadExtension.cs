using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    public class ReloadExtension
    {
        public ItemExtension AmmoStack;
        public List<ReloadInner> Compatible;

    }
    public class ReloadInner
    {
        public ItemExtension Magazine;
        public List<ItemExtension> Gun;
    }
}
