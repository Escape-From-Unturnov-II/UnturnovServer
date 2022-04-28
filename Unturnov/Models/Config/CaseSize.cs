using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models.Config
{
    public class CaseSize
    {
        public byte Width = 3;
        public byte Height = 3;

        public CaseSize(byte width, byte height)
        {
            Width = width;
            Height = height;
        }
        public CaseSize()
        {

        }
    }
}
