using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Norsemen;

public static class FactionManager
{
    public static readonly Dictionary<Character.Faction, Norsemen.Faction> customFactions;

    public static readonly Dictionary<string, Character.Faction> factions;

    static FactionManager()
	{
        //IL_004e: Unknown result type (might be due to invalid IL or missing references)
        //IL_005b: Expected O, but got Unknown
        //IL_008a: Unknown result type (might be due to invalid IL or missing references)
        //IL_0097: Expected O, but got Unknown
        //IL_00e4: Unknown result type (might be due to invalid IL or missing references)
        //IL_00f2: Expected O, but got Unknown
        //IL_0121: Unknown result type (might be due to invalid IL or missing references)
        //IL_012e: Expected O, but got Unknown
        customFactions = new Dictionary<Character.Faction, Norsemen.Faction>();
        factions = new Dictionary<string, Character.Faction>();
        Harmony harmony = NorsemenPlugin.instance._harmony;
		harmony.Patch((MethodBase)AccessTools.Method(typeof(Enum), "GetValues", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(AccessTools.Method(typeof(FactionManager), "Patch_Enum_GetValues", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		harmony.Patch((MethodBase)AccessTools.Method(typeof(Enum), "GetNames", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(AccessTools.Method(typeof(FactionManager), "Patch_Enum_GetNames", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		harmony.Patch((MethodBase)AccessTools.Method(typeof(BaseAI), "IsEnemy", new Type[2]
		{
			typeof(Character),
			typeof(Character)
		}, (Type[])null), new HarmonyMethod(AccessTools.Method(typeof(FactionManager), "Patch_BaseAI_IsEnemy", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		harmony.Patch((MethodBase)AccessTools.Method(typeof(Enum), "ToString", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(AccessTools.Method(typeof(FactionManager), "Patch_Enum_ToString", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
	}

	private static bool Patch_BaseAI_IsEnemy(Character a, Character b, ref bool __result)
	{
		if (!(a is Viking) && !(b is Viking))
		{
			return true;
		}
		__result = IsEnemy(a, b);
		return false;
	}

	public static bool IsEnemy(Character a, Character b)
	{
        //IL_0047: Unknown result type (might be due to invalid IL or missing references)
        //IL_004d: Unknown result type (might be due to invalid IL or missing references)
        if (a == b)
        {
			return false;
		}
		if (a is Viking viking)
		{
			return IsEnemy(viking, b);
		}
		if (b is Viking viking2)
		{
			return IsEnemy(viking2, a);
		}
		return a.GetFaction() != b.GetFaction();
	}

	public static bool IsEnemy(Viking viking, Character character)
	{
		if (character is Viking b)
		{
			return IsEnemyToOtherViking(viking, b);
		}
		if (character.IsPlayer())
		{
			return IsEnemyToPlayers(viking);
		}
		return IsEnemyToCreatures(viking, character);
	}

	public static bool IsEnemyToCreatures(Viking viking, Character character)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Invalid comparison between Unknown and I4
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected I4, but got Unknown
		if (!customFactions.TryGetValue(((Character)viking).GetFaction(), out var value))
		{
			return true;
		}
		bool flag = ((Character)viking).IsTamed();
		bool flag2 = character.IsTamed();
		if (flag & flag2)
		{
			return false;
		}
        Character.Faction faction = character.GetFaction();
        Character.Faction val = faction;
        if ((int)val != 1)
		{
            switch ((int)val - 8)
            {
			case 0:
				return flag;
			case 2:
                    return character.GetBaseAI().IsAggravated();
                case 3:
				return false;
			default:
                    if (character.GetComponent<Tameable>() != null)
                    {
                        return value.targetTames || character.GetBaseAI().IsAlerted();
                    }
				return true;
			}
		}
		return !flag;
	}

	public static bool IsEnemyToPlayers(Viking viking)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Invalid comparison between Unknown and I4
		if (!customFactions.TryGetValue(((Character)viking).GetFaction(), out var value))
		{
			return true;
		}
		if (((Character)viking).IsTamed())
		{
			return false;
		}
		if (!value.IsFriendly())
		{
			return true;
		}
        return ((BaseAI)viking.m_vikingAI).IsAggravated() && (int)viking.m_aggravatedReason != 1;
    }

	public static bool IsEnemyToOtherViking(Viking a, Viking b)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!customFactions.TryGetValue(((Character)a).GetFaction(), out var value))
		{
			return true;
		}
		if (!customFactions.TryGetValue(((Character)b).GetFaction(), out var value2))
		{
			return true;
		}
		if (((Character)a).IsTamed() && ((Character)b).IsTamed())
		{
			return false;
		}
		return value != value2;
	}

	private static void Patch_Enum_GetValues(Type enumType, ref Array __result)
	{
        if (enumType == typeof(Character.Faction) && factions.Count != 0)
        {
            Character.Faction[] array = new Character.Faction[__result.Length + factions.Count];
            __result.CopyTo(array, 0);
			factions.Values.CopyTo(array, __result.Length);
			__result = array;
		}
	}

	public static void Patch_Enum_ToString(Enum __instance, ref string __result)
	{
        //IL_000a: Unknown result type (might be due to invalid IL or missing references)
        //IL_000f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0035: Unknown result type (might be due to invalid IL or missing references)
        if (__instance is Character.Faction key &&
			factions.Count != 0 &&
			customFactions.TryGetValue(key, out var value))
        {
			__result = value.name;
		}
	}

	private static void Patch_Enum_GetNames(Type enumType, ref string[] __result)
	{
		if (!(enumType != typeof(Character.Faction)) && factions.Count != 0)
		{
			__result = CollectionExtensions.AddRangeToArray<string>(__result, factions.Keys.ToArray());
		}
	}

    public static Character.Faction GetFaction(string name)
    {
        //IL_000f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0010: Unknown result type (might be due to invalid IL or missing references)
        //IL_002a: Unknown result type (might be due to invalid IL or missing references)
        //IL_002b: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a9: Unknown result type (might be due to invalid IL or missing references)
        //IL_005f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0064: Unknown result type (might be due to invalid IL or missing references)
        //IL_006b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0072: Unknown result type (might be due to invalid IL or missing references)
        //IL_0073: Unknown result type (might be due to invalid IL or missing references)
        //IL_0097: Unknown result type (might be due to invalid IL or missing references)
        //IL_009e: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a5: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a6: Unknown result type (might be due to invalid IL or missing references)
        if (Enum.TryParse<Character.Faction>(name, true, out Character.Faction result))
        {
			return result;
		}
		if (factions.TryGetValue(name, out result))
		{
			return result;
		}
        Dictionary<Character.Faction, string> factionMap = GetFactionMap();
        foreach (KeyValuePair<Character.Faction, string> item in factionMap)
        {
			if (item.Value == name)
			{
				result = item.Key;
				factions[name] = result;
				return result;
			}
		}
        result = (Character.Faction)StringExtensionMethods.GetStableHashCode(name);
        factions[name] = result;
		return result;
	}

    private static Dictionary<Character.Faction, string> GetFactionMap()
    {
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Array values = Enum.GetValues(typeof(Character.Faction));
		string[] names = Enum.GetNames(typeof(Character.Faction));
        Dictionary<Character.Faction, string> dictionary = new Dictionary<Character.Faction, string>();
        for (int i = 0; i < values.Length; i++)
		{
            dictionary[(Character.Faction)values.GetValue(i)] = names[i];
        }
		return dictionary;
	}
}
