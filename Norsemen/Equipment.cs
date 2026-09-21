using System;
using System.Collections.Generic;

namespace Norsemen;

[Serializable]
public class Equipment
{
	public List<ConditionalChanceItem> RandomItems = new List<ConditionalChanceItem>();

	public List<ConditionalWeightedSet> RandomSets = new List<ConditionalWeightedSet>();

	public List<ConditioanlWeightedItem> RandomWeapons = new List<ConditioanlWeightedItem>();
}
