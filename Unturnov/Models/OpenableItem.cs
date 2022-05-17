using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    public class OpenableItem : ItemExtension
    {
        public string TableName = "none";
        public byte Width = 3;
        public byte Height = 3;
        public List<string> UsedWhitelists;
    }
}
