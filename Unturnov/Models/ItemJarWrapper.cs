using Newtonsoft.Json;
using SDG.Unturned;
using SpeedMann.Unturnov.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    public class ItemJarWrapper
    {
        #region privateValues
        public ItemJar _itemJar = null;
        private ushort _id = 0;
        private byte _x = 0;
        private byte _y = 0;
        private byte _rot = 0;
        private byte _amount = 0;
        private byte _quality = 0;
        private byte[] _state = new byte[0];
        #endregion
        #region JsonGetters
        public ushort id
        {
            get
            {
                return itemJar?.item != null ? itemJar.item.id : _id;
            }
            set
            {
                _id = value;
            }
        }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public byte page = 0;
        public byte x
        {
            get
            {
                return itemJar != null ? itemJar.x : _x;
            }
            set
            {
                _x = value;
            }
        }
        public byte y
        {
            get
            {
                return itemJar != null ? itemJar.y : _y;
            }
            set
            {
                _y = value;
            }
        }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public byte rot
        {
            get
            {
                return itemJar != null ? itemJar.rot : _rot;
            }
            set
            {
                _rot = value;
            }
        }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public byte amount
        {
            get
            {
                return itemJar?.item != null ? itemJar.item.amount : _amount;
            }
            set
            {
                _amount = value;
            }
        }
        public byte quality
        {
            get
            {
                return itemJar?.item != null ? itemJar.item.amount : _quality;
            }
            set
            {
                _quality = value;
            }
        }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public byte[] state
        {
            get
            {
                return itemJar?.item != null ? itemJar.item.state : _state;
            }
            set
            {
                _state = value;
            }
        }
        #endregion

        [JsonIgnore]
        public ItemJar itemJar
        {
            get 
            {
                if(_itemJar == null)
                {
                    tryCreateItemJar();
                }
                return _itemJar;
            }
            set
            {
                _itemJar = value;
            }
        }
        [JsonIgnore]
        public byte index;
        

        public ItemJarWrapper()
        {

        }
        public ItemJarWrapper(ItemJar itemJar, byte page = 0, byte index = 0)
        {
            _itemJar = itemJar;
            this.page = page;
            this.index = index;
        }
        private bool tryCreateItemJar()
        {
            if(_id == 0) 
                return false;

            _itemJar = new ItemJar(_x, _y, _rot, new Item(_id, _amount, _quality, _state));
            return true;
        }
    }
}
