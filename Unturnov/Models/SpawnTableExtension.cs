using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeedMann.Unturnov.Models
{
    public class SpawnTableExtension
    {
        public List<SpawnTableEntry> Items = new List<SpawnTableEntry>();
		[XmlIgnore]
		private bool Sorted = false;

		public SpawnTableExtension()
        {

        }
		public SpawnTableExtension(KitTierEntry entry, SpawnTableExtension gobalTable)
        {
			if (entry.CountMax > 0)
			{
				Items = gobalTable.getSortedAndNormalizeWeights(entry.WeightMin, entry.WeightMax);
			}
		}
		public List<SpawnTableEntry> getSortedAndNormalizeWeights(int WeightMin, int WeightMax)
		{
			List<SpawnTableEntry> spawnList = new List<SpawnTableEntry>();

			if (!Sorted)
			{
				Items.Sort((a, b) => b.weight - a.weight);
			}
			float weightSum = 0f;
			foreach (SpawnTableEntry item in Items)
			{
				if (item.weight >= WeightMin && item.weight <= WeightMax)
				{
					weightSum += item.weight;
				}

			}
			float currentWeightSum = 0f;
			foreach (SpawnTableEntry item in Items)
			{
				if (item.weight >= WeightMin && item.weight <= WeightMax)
				{
					currentWeightSum += item.weight;
					spawnList.Add(new SpawnTableEntry(item.Id, item.weight, currentWeightSum / weightSum));
				}
			}
			return spawnList;
		}
		public ushort getItem()
		{
			float value = UnityEngine.Random.value;
			int i = 0;
			while (i < Items.Count)
			{
				if (value < Items[i].chance)
				{
					int y = i+1;
					while(y < Items.Count)
                    {
						if (Items[i].chance != Items[y].chance)
                        {
							break;
                        }
						y++;
					}
					// selecting an item between index i and y-1 (first - last with equal chance)
					return Items[UnityEngine.Random.Range(i, y)].Id;
				}
				else
				{
					i++;
				}
			}
			return 0;
		}
	}
}
