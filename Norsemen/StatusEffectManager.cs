using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Norsemen;

public static class StatusEffectManager
{
	private static readonly List<StatusEffect> statusEffects;

	public static void Register(this StatusEffect statusEffect)
	{
		statusEffects.Add(statusEffect);
	}

	static StatusEffectManager()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		statusEffects = new List<StatusEffect>();
		Harmony harmony = NorsemenPlugin.instance._harmony;
		harmony.Patch((MethodBase)AccessTools.Method(typeof(ObjectDB), "Awake", (Type[])null, (Type[])null), new HarmonyMethod(AccessTools.Method(typeof(StatusEffectManager), "Patch_ObjectDB_Awake", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
	}

	public static void Patch_ObjectDB_Awake(ObjectDB __instance)
	{
		__instance.m_StatusEffects.AddRange(statusEffects);
	}
}
