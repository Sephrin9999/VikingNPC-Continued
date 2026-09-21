using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Norsemen;

public static class RaidManager
{
	internal static readonly List<Raid> raids;

	static RaidManager()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		raids = new List<Raid>();
		Harmony harmony = NorsemenPlugin.instance._harmony;
		harmony.Patch((MethodBase)AccessTools.Method(typeof(RandEventSystem), "Awake", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(typeof(RaidManager), "Patch_RandEvenSystem", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		harmony.Patch((MethodBase)AccessTools.Method(typeof(MonsterAI), "SetEventCreature", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(typeof(RaidManager), "Patch_MonsterAI_SetEventCreature", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
	}

	private static void Patch_RandEvenSystem(RandEventSystem __instance)
	{
		__instance.m_events.AddRange((IEnumerable<RandomEvent>)raids);
	}

	private static void Patch_MonsterAI_SetEventCreature(MonsterAI __instance, bool despawn)
	{
		if (despawn && __instance is VikingAI vikingAI)
		{
            ((BaseAI)vikingAI).SetAggravated(true, BaseAI.AggravatedReason.Theif);
            ((MonsterAI)vikingAI).m_attackPlayerObjects = true;
		}
	}
}
