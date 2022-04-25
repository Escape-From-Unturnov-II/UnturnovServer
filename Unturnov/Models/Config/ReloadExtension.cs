using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models
{
    /*
        Ammo Stack:

        Type Magazine

        Calibers 1
        Caliber_0 12642

        Delete_Empty 
     */
    public class ReloadExtension
    {
        public ItemExtension AmmoStack;
        public List<ReloadInner> Compatibles;

    }
    public class ReloadInner
    {
        [XmlIgnore]
        public ushort AmmoStackId;
        public byte MagazineSize;
        public List<ItemExtension> Gun;
    }
}
