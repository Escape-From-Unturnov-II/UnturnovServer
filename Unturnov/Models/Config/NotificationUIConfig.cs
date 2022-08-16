using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    public class Notification_UI
    {
        public bool Enabled = true;
        public ushort UI_Id = 52310;
        public short UI_Key = 5230;

        public Notification_UI()
        {

        }
        public Notification_UI(ushort id, short key, bool enabled = true)
        {
            Enabled = enabled;
            UI_Id = id;
            UI_Key = key;
        }
    }
}
