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
using static SDG.Provider.UnturnedEconInfo;

namespace SpeedMann.Unturnov.Models
{
    public class GunAttachments
    {
        public List<GunAttachment> attachments;
        public GunAttachment magAttachment;
        public byte ammo;
        public GunAttachments(ushort sightId, byte sightQuality, ushort tacticalId, byte tacticalQuality, ushort gripId, byte gripQuality, ushort barrelId, byte barrelQuality, ushort magId, byte magQuality, byte ammo)
        {
            setAttachments(sightId, sightQuality, tacticalId, tacticalQuality, gripId, gripQuality, barrelId, barrelQuality, magId, magQuality, ammo);
        }
        public GunAttachments(byte [] oldAttachments)
        {
            byte[] sight = new byte[] { oldAttachments[0], oldAttachments[1] };
            byte sightQuality = oldAttachments[13];

            byte[] tactical = new byte[] { oldAttachments[2], oldAttachments[3] };
            byte tacticalQuality = oldAttachments[14];

            byte[] grip = new byte[] { oldAttachments[4], oldAttachments[5] };
            byte gripQuality = oldAttachments[15];

            byte[] barrel = new byte[] { oldAttachments[6], oldAttachments[7] };
            byte barrelQuality = oldAttachments[16];

            byte[] mag = new byte[] { oldAttachments[8], oldAttachments[9] };
            byte magQuality = oldAttachments[17];
            byte ammo = oldAttachments[10];

            setAttachments(
                BitConverter.ToUInt16(sight, 0), sightQuality,
                BitConverter.ToUInt16(tactical, 0), tacticalQuality,
                BitConverter.ToUInt16(grip, 0), gripQuality,
                BitConverter.ToUInt16(barrel, 0), barrelQuality,
                BitConverter.ToUInt16(mag, 0), magQuality, ammo);
        }
        private void setAttachments(ushort sightId, byte sightQuality, ushort tacticalId, byte tacticalQuality, ushort gripId, byte gripQuality, ushort barrelId, byte barrelQuality, ushort magId, byte magQuality, byte ammo)
        {
            attachments = new List<GunAttachment>
            {
                new GunAttachment(sightId, sightQuality, setSight),
                new GunAttachment(tacticalId, tacticalQuality, setTactical),
                new GunAttachment(gripId, gripQuality, setGrip),
                new GunAttachment(barrelId, barrelQuality, setBarrel),
            };
            magAttachment = new GunAttachment(magId, magQuality, setMag);
            this.ammo = ammo;
        }
        public static void setSight(ref byte[] state, ushort id, byte quality)
        {
            byte[] array = BitConverter.GetBytes(id);
            state[0] = array[0];
            state[1] = array[1];
        }
        public static void setTactical(ref byte[] state, ushort id, byte quality)
        {
            byte[] array = BitConverter.GetBytes(id);
            state[2] = array[0];
            state[3] = array[1];
        }
        public static void setGrip(ref byte[] state, ushort id, byte quality)
        {
            byte[] array = BitConverter.GetBytes(id);
            state[4] = array[0];
            state[5] = array[1];
        }
        public static void setBarrel(ref byte[] state, ushort id, byte quality)
        {
            byte[] array = BitConverter.GetBytes(id);
            state[6] = array[0];
            state[7] = array[1];
        }
        public static void setMag(ref byte[] state, ushort id, byte quality, byte ammo)
        {
            byte[] array = BitConverter.GetBytes(id);
            state[8] = array[0];
            state[9] = array[1];
            state[10] = ammo;
            state[17] = quality;
        }
        private void setMag(ref byte[] state, ushort id, byte quality)
        {
            setMag(ref state, id, quality, ammo);
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
            public byte quality;
            public List<ushort> calibers;
            public bool wasSet = false;
            internal delegate void SetAttachmentInner(ref byte[] state, ushort id, byte quality);
            private event SetAttachmentInner setAttachmentCaller;
            public void SetAttachment(ref byte[] state)
            {
                if(setAttachmentCaller != null && calibers != null)
                {
                    setAttachmentCaller.Invoke(ref state, id, quality);
                    wasSet = true;
                }
            }

            internal GunAttachment(ushort id, byte quality, SetAttachmentInner setFunction)
            {
                this.id = id;
                this.quality = quality;
                calibers = findCalibers(id);
                setAttachmentCaller += setFunction;
            }
        }
    }
}
