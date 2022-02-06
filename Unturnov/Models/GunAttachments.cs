using Rocket.Core.Logging;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    public class GunAttachments
    {
        public List<GunAttachment> attachments;
        public GunAttachment magAttachment;
        public byte ammo;
        public GunAttachments(byte [] oldAttachments)
        {
            byte[] sight = new byte[] { oldAttachments[0], oldAttachments[1] };
            byte[] tactical = new byte[] { oldAttachments[2], oldAttachments[3] };
            byte[] grip = new byte[] { oldAttachments[4], oldAttachments[5] };
            byte[] barrel = new byte[] { oldAttachments[6], oldAttachments[7] };
            byte[] mag = new byte[] { oldAttachments[8], oldAttachments[9] };

            ammo = oldAttachments[10];

            

            attachments = new List<GunAttachment>
            {
                new GunAttachment(BitConverter.ToUInt16(sight, 0), setSight),
                new GunAttachment(BitConverter.ToUInt16(tactical, 0), setTactical),
                new GunAttachment(BitConverter.ToUInt16(grip, 0), setGrip),
                new GunAttachment(BitConverter.ToUInt16(barrel, 0), setBarrel),
            };
            magAttachment = new GunAttachment(BitConverter.ToUInt16(mag, 0), setMag);
        }
        public void setSight(ref byte[] state, ushort id)
        {
            byte[] array = BitConverter.GetBytes(id);
            state[0] = array[0];
            state[1] = array[1];
        }
        public void setTactical(ref byte[] state, ushort id)
        {
            byte[] array = BitConverter.GetBytes(id);
            state[2] = array[0];
            state[3] = array[1];
        }
        public void setGrip(ref byte[] state, ushort id)
        {
            byte[] array = BitConverter.GetBytes(id);
            state[4] = array[0];
            state[5] = array[1];
        }
        public void setBarrel(ref byte[] state, ushort id)
        {
            byte[] array = BitConverter.GetBytes(id);
            state[6] = array[0];
            state[7] = array[1];
        }
        public void setMag(ref byte[] state, ushort id)
        {
            byte[] array = BitConverter.GetBytes(id);
            state[8] = array[0];
            state[9] = array[1];
            state[10] = ammo;
        }
        internal static List<ushort> findCalibers(ushort id)
        {
            List<ushort> calibers = null;
            Asset asset = Assets.find(EAssetType.ITEM, id);

            if(asset != null)
            {
                switch (asset)
                {
                    case ItemSightAsset sight:
                        calibers = new List<ushort>(sight.calibers);
                        break;
                    case ItemTacticalAsset tactical:
                        calibers = new List<ushort>(tactical.calibers);
                        break;
                    case ItemGripAsset grip:
                        calibers = new List<ushort>(grip.calibers);
                        break;
                    case ItemBarrelAsset barrel:
                        calibers = new List<ushort>(barrel.calibers);
                        break;
                    case ItemMagazineAsset mag:
                        calibers = new List<ushort>(mag.calibers);
                        break;
                }
            }
            return calibers;
        }

        public class GunAttachment 
        {
            public ushort id;
            public List<ushort> calibers;
            public bool set = false;
            internal delegate void SetAttachmentInner(ref byte[] state, ushort id);
            private event SetAttachmentInner setAttachmentCaller;
            public void SetAttachment(ref byte[] state)
            {
                if(setAttachmentCaller != null && calibers != null)
                {
                    setAttachmentCaller.Invoke(ref state, id);
                    set = true;
                }
            }

            internal GunAttachment(ushort id, SetAttachmentInner setFunction)
            {
                this.id = id;
                calibers = findCalibers(id);
                setAttachmentCaller += setFunction;
            }
        }
    }
}
