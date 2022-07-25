using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpeedMann.Unturnov.Helper
{
    internal class CraftingStationControler
    {
        bool isPowered( Transform transform)
        {
			ushort maxValue = ushort.MaxValue;
			byte b;
			byte b2;
			BarricadeRegion barricadeRegion;
			BarricadeManager.tryGetPlant(transform.parent, out b, out b2, out maxValue, out barricadeRegion);
			List<InteractableGenerator> list = PowerTool.checkGenerators(transform.position, PowerTool.MAX_POWER_RANGE, maxValue);
			for (int i = 0; i < list.Count; i++)
			{
				InteractableGenerator interactableGenerator = list[i];
				if (interactableGenerator.isPowered && interactableGenerator.fuel > 0 && (interactableGenerator.transform.position - transform.position).sqrMagnitude < interactableGenerator.sqrWirerange)
				{
					return true;
				}
			}
			return false;
		}

    }
}
