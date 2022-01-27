using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Helper
{
    public class UnturnedPrivateFields
    {
        private static FieldInfo SendRefreshCrafting;
        public static bool getRefreshCraftingSender(PlayerCrafting crafting, out ClientInstanceMethod result)
        {
            if (SendRefreshCrafting != null)
            {
                result = (ClientInstanceMethod)SendRefreshCrafting.GetValue(crafting);
                return true;
            }
            result = null;
            return false;
        }

        public static void Init()
        {
            Type type;

            type = typeof(PlayerCrafting);
            SendRefreshCrafting = type.GetField("SendRefreshCrafting", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}
