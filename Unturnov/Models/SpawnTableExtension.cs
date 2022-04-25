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
				if(item.weight >= WeightMin && item.weight  <= WeightMax)
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
	}
}
