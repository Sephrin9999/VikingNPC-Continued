using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Norsemen;

[PublicAPI]
public static class PrefabManager
{
	internal static List<GameObject> PrefabsToRegister;

	internal static List<Clone> Clones;

	internal static List<Norseman> Norsemen;

	static PrefabManager()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		PrefabsToRegister = new List<GameObject>();
		Clones = new List<Clone>();
		Norsemen = new List<Norseman>();
		Harmony harmony = NorsemenPlugin.instance._harmony;
		harmony.Patch((MethodBase)AccessTools.DeclaredMethod(typeof(FejdStartup), "Awake", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(AccessTools.DeclaredMethod(typeof(PrefabManager), "Patch_FejdStartup", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		harmony.Patch((MethodBase)AccessTools.DeclaredMethod(typeof(ZNetScene), "Awake", (Type[])null, (Type[])null), new HarmonyMethod(AccessTools.DeclaredMethod(typeof(PrefabManager), "Patch_ZNetScene_Awake", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
	}

	public static void RegisterPrefab(GameObject prefab)
	{
        if (prefab != null)
        {
			PrefabsToRegister.Add(prefab);
		}
	}

	public static void RegisterPrefab(string assetBundleName, string prefabName)
	{
		RegisterPrefab(AssetBundleManager.LoadAsset<GameObject>(assetBundleName, prefabName));
	}

	public static void RegisterPrefab(AssetBundle assetBundle, string prefabName)
	{
		RegisterPrefab(assetBundle.LoadAsset<GameObject>(prefabName));
	}

	[HarmonyPriority(700)]
	internal static void Patch_ZNetScene_Awake(ZNetScene __instance)
	{
		foreach (GameObject item in PrefabsToRegister)
		{
            if (item.GetComponent<ZNetView>() != null)
            {
				__instance.m_prefabs.Add(item);
			}
		}
	}

	[HarmonyPriority(700)]
	internal static void Patch_FejdStartup(FejdStartup __instance)
	{
		Helpers._ZNetScene = __instance.m_objectDBPrefab.GetComponent<ZNetScene>();
		Helpers._ObjectDB = __instance.m_objectDBPrefab.GetComponent<ObjectDB>();
		CustomizationManager.GetHairAndBeards(Helpers._ObjectDB);
		foreach (Clone clone in Clones)
		{
			clone.Create();
		}
		foreach (Norseman norseman in Norsemen)
		{
			norseman.Create();
		}
		ConfigManager.SetupWatcher();
	}
}
