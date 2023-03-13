using Org.BouncyCastle.Asn1.BC;
using Rocket.Core.Logging;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SpeedMann.Unturnov.Models
{
    public class GunAttachments
    {
        public List<GunAttachment> attachments;
        public GunAttachment magAttachment;
        public byte ammo;
        public GunAttachments(ushort sightId, ushort tacticalId, ushort gripId, ushort barrelId, ushort magId, byte ammo)
        {
            setAttachments(sightId, tacticalId, gripId, barrelId, magId, ammo);
        }
        public GunAttachments(byte [] oldAttachments)
        {
            byte[] sight = new byte[] { oldAttachments[0], oldAttachments[1] };
            byte[] tactical = new byte[] { oldAttachments[2], oldAttachments[3] };
            byte[] grip = new byte[] { oldAttachments[4], oldAttachments[5] };
            byte[] barrel = new byte[] { oldAttachments[6], oldAttachments[7] };
            byte[] mag = new byte[] { oldAttachments[8], oldAttachments[9] };

            setAttachments(
                BitConverter.ToUInt16(sight, 0), 
                BitConverter.ToUInt16(tactical, 0),
                BitConverter.ToUInt16(grip, 0),
                BitConverter.ToUInt16(barrel, 0),
                BitConverter.ToUInt16(mag, 0),
                oldAttachments[10]);
        }
        private void setAttachments(ushort sightId, ushort tacticalId, ushort gripId, ushort barrelId, ushort magId, byte ammo)
        {
            attachments = new List<GunAttachment>
            {
                new GunAttachment(sightId, setSight),
                new GunAttachment(tacticalId, setTactical),
                new GunAttachment(gripId, setGrip),
                new GunAttachment(barrelId, setBarrel),
            };
            magAttachment = new GunAttachment(magId, setMag);
            this.ammo = ammo;
        }
        public static void setSight(ref byte[] state, ushort id)
        {
            byte[] array = BitConverter.GetBytes(id);
            state[0] = array[0];
            state[1] = array[1];
        }
        public static void setTactical(ref byte[] state, ushort id)
        {
            byte[] array = BitConverter.GetBytes(id);
            state[2] = array[0];
            state[3] = array[1];
        }
        public static void setGrip(ref byte[] state, ushort id)
        {
            byte[] array = BitConverter.GetBytes(id);
            state[4] = array[0];
            state[5] = array[1];
        }
        public static void setBarrel(ref byte[] state, ushort id)
        {
            byte[] array = BitConverter.GetBytes(id);
            state[6] = array[0];
            state[7] = array[1];
        }
        public static void setMag(ref byte[] state, ushort id, byte ammo)
        {
            byte[] array = BitConverter.GetBytes(id);
            state[8] = array[0];
            state[9] = array[1];
            state[10] = ammo;
        }
        private void setMag(ref byte[] state, ushort id)
        {
            setMag(ref state, id, ammo);
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
            public bool wasSet = false;
            internal delegate void SetAttachmentInner(ref byte[] state, ushort id);
            private event SetAttachmentInner setAttachmentCaller;
            public void SetAttachment(ref byte[] state)
            {
                if(setAttachmentCaller != null && calibers != null)
                {
                    setAttachmentCaller.Invoke(ref state, id);
                    wasSet = true;
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
