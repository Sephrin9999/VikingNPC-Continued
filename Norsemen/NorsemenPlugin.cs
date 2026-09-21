using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LocalizationManager;
using UnityEngine;

namespace Norsemen;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class NorsemenPlugin : BaseUnityPlugin
{
    internal const string ModName = "Norsemen Continued";
    internal const string ModVersion = "0.4.0";
    internal const string Author = "SephrinMods";
    public const string ModGUID = "RustyMods.Norsemen";

    internal static string ConnectionError = "";

	public readonly Harmony _harmony;

	public static NorsemenPlugin instance = null;

	private static ConfigEntry<Toggle> canSteal = null;

	private static ConfigEntry<Toggle> removeEquipment = null;

	private static ConfigEntry<Toggle> canBeEncumbered = null;

	private static ConfigEntry<int> baseCarryWeight = null;

	private static ConfigEntry<KeyCode> altKey = null;

	public static bool CanSteal => canSteal.Value == Toggle.On;

	public static bool RemoveEquipment => removeEquipment.Value == Toggle.On;

	public static bool CanBecomeEncumbered => canBeEncumbered.Value == Toggle.On;

	public static int BaseCarryWeight => baseCarryWeight.Value;

	public static KeyCode AltKey
	{
		get
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return altKey.Value;
		}
	}

	public void Awake()
	{
		instance = this;
		SetupTombstone();
		SetupNorsemen();
		SetupCommands();
		NameGenerator.Setup();
		TalkManager.Setup();
		CustomizationManager.Setup();
		canSteal = ConfigManager.config("Settings", "Steal", Toggle.On, "If on, players can steal from norsemen");
		removeEquipment = ConfigManager.config("Settings", "Naked on Tamed", Toggle.Off, "If on, tamed norsemen will lose equipment on tamed");
		canBeEncumbered = ConfigManager.config("Settings", "Encumbers", Toggle.On, "If on, tamed norsemen can become encumbered");
		baseCarryWeight = ConfigManager.config("Settings", "Base Carry Weight", 300, "Set base carry weight");
		altKey = ConfigManager.config<KeyCode>("Settings", "Alt Interaction Key", (KeyCode)308, "Set alt key to interact with norsemen");
		Localizer.Load();
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		_harmony.PatchAll(executingAssembly);
	}

	private static void SetupNorsemen()
	{
		Faction faction = new Faction("Norsemen", friendly: true);
        Norseman norseman = new Norseman((Heightmap.Biome)1, "Meadows_Norseman_RS", faction);
        norseman.baseHealth = 50f;
		norseman.baseArmor = 0f;
		Norseman norseman2 = new Norseman((Heightmap.Biome)8, "BlackForest_Norseman_RS", faction);
		norseman2.baseHealth = 100f;
		norseman2.baseArmor = 5f;
		Norseman norseman3 = new Norseman((Heightmap.Biome)2, "Swamp_Norseman_RS", faction);
		norseman3.baseHealth = 150f;
		norseman3.baseArmor = 10f;
		Norseman norseman4 = new Norseman((Heightmap.Biome)4, "Mountains_Norseman_RS", faction);
		norseman4.baseHealth = 200f;
		norseman4.baseArmor = 15f;
		Norseman norseman5 = new Norseman((Heightmap.Biome)16, "Plains_Norseman_RS", faction);
		norseman5.baseHealth = 250f;
		norseman5.baseArmor = 20f;
		Norseman norseman6 = new Norseman((Heightmap.Biome)512, "Mistlands_Norseman_RS", faction);
		norseman6.baseHealth = 300f;
		norseman6.baseArmor = 25f;
		Norseman norseman7 = new Norseman((Heightmap.Biome)32, "AshLands_Norseman_RS", faction);
		norseman7.baseHealth = 350f;
		norseman7.baseArmor = 30f;
	}

	private static void SetupTombstone()
	{
		Clone clone = new Clone("Player_tombstone", "Norseman_tombstone_RS");
		clone.OnCreated += delegate(GameObject prefab)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Expected O, but got Unknown
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			MeshRenderer componentInChildren = prefab.GetComponentInChildren<MeshRenderer>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				List<Material> list = new List<Material>();
				Material[] sharedMaterials = ((Renderer)componentInChildren).sharedMaterials;
				foreach (Material val in sharedMaterials)
				{
					Material val2 = new Material(val);
					list.Add(val2);
					val2.SetColor("_EmissionColor", new Color(0f, 0.8f, 1f) * 4f);
				}
				Material[] materials = (((Renderer)componentInChildren).sharedMaterials = list.ToArray());
				((Renderer)componentInChildren).materials = materials;
				prefab.AddComponent<VikingTomb>();
			}
			Transform val3 = prefab.transform.Find("Particle System");
			ParticleSystem val4 = default(ParticleSystem);
			if ((Object)(object)val3 != (Object)null && ((Component)val3).TryGetComponent<ParticleSystem>(out val4))
			{
                ParticleSystem.MainModule main = val4.main;
                main.startColor = new Color(0f, 0.8f, 1f, 0.54f);
            }
			Norseman.tombstone = prefab;
		};
		Clone clone2 = new Clone("fx_summon_twitcher_spawn", "fx_revive_norseman");
		clone2.OnCreated += delegate(GameObject prefab)
		{
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Expected O, but got Unknown
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0230: Unknown result type (might be due to invalid IL or missing references)
			//IL_0235: Unknown result type (might be due to invalid IL or missing references)
			//IL_023d: Expected O, but got Unknown
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			Transform val = prefab.transform.Find("Swirls");
			Transform val2 = prefab.transform.Find("Swirls/Dust");
			Transform val3 = prefab.transform.Find("Swirls/Swirly Spores");
			Transform val4 = prefab.transform.Find("Point light");
			Transform val5 = prefab.transform.Find("Point light (1)");
			if ((Object)(object)val != (Object)null)
			{
				ParticleSystem component = ((Component)val).GetComponent<ParticleSystem>();
                ParticleSystem.MainModule main = component.main;
                main.startColor = new Color(0f, 0.8f, 1f, 1f);
            }
			if ((Object)(object)val2 != (Object)null)
			{
				ParticleSystem component2 = ((Component)val2).GetComponent<ParticleSystem>();
                ParticleSystem.MainModule main2 = component2.main;
                main2.startColor = new Color(0.4f, 0.8f, 1f, 1f);
            }
			if ((Object)(object)val3 != (Object)null)
			{
				ParticleSystem component3 = ((Component)val3).GetComponent<ParticleSystem>();
				Gradient val6 = new Gradient();
				val6.SetKeys((GradientColorKey[])(object)new GradientColorKey[2]
				{
					new GradientColorKey(new Color(0.4f, 0.8f, 0.8f, 1f), 0f),
					new GradientColorKey(new Color(0f, 0.8f, 1f, 1f), 1f)
				}, (GradientAlphaKey[])(object)new GradientAlphaKey[2]
				{
					new GradientAlphaKey(1f, 0f),
					new GradientAlphaKey(0f, 1f)
				});
                ParticleSystem.CustomDataModule customData = component3.customData;
                customData.SetColor((ParticleSystemCustomData)0, new ParticleSystem.MinMaxGradient(val6));
            }
			if ((Object)(object)val4 != (Object)null)
			{
				Light component4 = ((Component)val4).GetComponent<Light>();
				component4.color = new Color(0.1f, 0.8f, 0.8f, 1f);
			}
			if ((Object)(object)val5 != (Object)null)
			{
				Light component5 = ((Component)val5).GetComponent<Light>();
				component5.color = new Color(0.1f, 0.8f, 0.8f, 1f);
			}
            VikingTomb.reviveEffects.m_effectPrefabs = new EffectList.EffectData[1]
			{
				new EffectList.EffectData
				{
					m_prefab = prefab
				}
			};
        };
	}

	private static void SetupCommands()
	{
		NorseCommand norseCommand = new NorseCommand("tame", "tames all nearby norsemen", delegate
		{
			List<Viking> allVikings = Viking.GetAllVikings();
			int num = 0;
			foreach (Viking item in allVikings)
			{
				if (!((Character)item).IsTamed() && NorsemanConfigs.IsTameable(item.prefabName))
				{
					num++;
					((Character)item).SetTamed(true);
                    ZDOMan.instance.GetZDO(((Character)item).GetZDOID()).Set(VikingVars.lastLevelUpTime, ZNet.instance.GetTime().Ticks);
                }
			}
            if (Player.m_localPlayer)
            {
                ((Character)Player.m_localPlayer).Message((MessageHud.MessageType)2, $"Tamed {num} norsemen", 0, (Sprite)null);
            }
			return true;
		}, null, isSecret: false, adminOnly: true);
		NorseCommand norseCommand2 = new NorseCommand("clear_tombs", "removes all nearby norsemen tombstones", delegate
		{
			VikingTomb.RemoveAll();
			return true;
		}, null, isSecret: false, adminOnly: true);
	}

	private void OnDestroy()
	{
		((BaseUnityPlugin)this).Config.Save();
	}

	public static void LogDebug(string msg)
	{
		if (ConfigManager.ShouldLog((LogLevel)32))
		{
            instance.Logger.LogDebug(msg);
        }
	}

	public static void LogError(string msg)
	{
		if (ConfigManager.ShouldLog((LogLevel)2))
		{
            instance.Logger.LogError(msg);
        }
	}

	public static void LogWarning(string msg)
	{
		if (ConfigManager.ShouldLog((LogLevel)4))
		{
            instance.Logger.LogWarning(msg);
        }
	}

	public static void LogInfo(string msg)
	{
		if (ConfigManager.ShouldLog((LogLevel)16))
		{
            instance.Logger.LogInfo(msg);
        }
	}

	public static void LogFatal(string msg)
	{
		if (ConfigManager.ShouldLog((LogLevel)1))
		{
            instance.Logger.LogFatal(msg);
        }
	}

	public NorsemenPlugin()
	{
        //IL_0006: Unknown result type (might be due to invalid IL or missing references)
        //IL_0010: Expected O, but got Unknown
        _harmony = new Harmony(ModGUID);
    }
}
