using Rocket.Core.Logging;
using SDG.Unturned;
using SpeedMann.Unturnov.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Helper
{
    class OpenableItemsHandler
    {
        internal static Dictionary<ushort, OpenableItem> OpenableItems;
        internal static byte[] checkState(ItemAsset asset, byte[] state)
        {
            if(asset == null){
                return new byte[0];
            }
            byte[] defaulState = asset.getState();
            if (!OpenableItems.TryGetValue(asset.id, out OpenableItem oItem))
            {
                return defaulState;
            }
            int oldLength = state.Length;
            int newLength = oItem.Height * oItem.Width * 2 + defaulState.Length;

            if (defaulState.Length == state.Length || state.Length != newLength)
            {
                // increase state array to hold all possible items
                byte[] newState = new byte[newLength];

                int i = 0;
                while (i < state.Length)
                {
                    if (i < newState.Length)
                    {
                        newState[i] = state[i];
                    }
                    else
                    {
                        Logger.Log("TODO: Drop Item");
                    }
                }
                state = newState;
            }

            Logger.Log($"Inspected Openable Item {asset.id} with sate Length default: {defaulState.Length} old: {oldLength} new: {state.Length}");
            return state;
        }

        internal static void Init()
        {
            OpenableItems = Unturnov.createDictionaryFromItemExtensions(Unturnov.Conf.OpenableItems);
        }
    }
}
