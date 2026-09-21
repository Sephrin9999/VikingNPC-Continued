using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace Norsemen;

public class NorsemanConfigs
{
	private static readonly Dictionary<string, NorsemanConfigs> configs = new Dictionary<string, NorsemanConfigs>();

	public Heightmap.Biome biome;

	private ConfigEntry<float> baseHealth;

	private ConfigEntry<Toggle> canMine;

	private ConfigEntry<Toggle> canLumber;

	private ConfigEntry<Toggle> canFish;

	private ConfigEntry<Toggle> workRequiresFood;

	private ConfigEntry<float> tamingTime;

	private ConfigEntry<float> baseArmor;

	private ConfigEntry<Toggle> tameable;

	private ConfigEntry<float> workResourceInterval;

	public NorsemanConfigs(string vikingName)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		biome = (Heightmap.Biome)0;
		baseHealth = null;
		canMine = null;
		canLumber = null;
		canFish = null;
		workRequiresFood = null;
		tamingTime = null;
		baseArmor = null;
		tameable = null;
		workResourceInterval = null;
		configs[vikingName] = this;
	}

	public static bool TryGetConfig(string prefab, out NorsemanConfigs config)
	{
		return configs.TryGetValue(prefab, out config);
	}

	public void Setup(Norseman norseman)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		baseHealth = ConfigManager.config(norseman.name, "Base Health", norseman.baseHealth, new ConfigDescription("Set base health", (AcceptableValueBase)(object)new AcceptableValueRange<float>(1f, 1000f), Array.Empty<object>()));
		((Character)norseman.m_viking).m_health = baseHealth.Value;
		canMine = ConfigManager.config(norseman.name, "Can Mine", Toggle.On, "If on, will mine ores if has pickaxe");
		canLumber = ConfigManager.config(norseman.name, "Can Lumber", Toggle.On, "If on, will lumber if has axe");
		canFish = ConfigManager.config(norseman.name, "Can Fish", Toggle.On, "If on, will fish if has fishing rod and bait");
		workRequiresFood = ConfigManager.config(norseman.name, "Work Requires Food", Toggle.On, "If on, will only mine and lumber if not hungry");
		norseman.spawnInfo.SetupConfigs();
		tamingTime = ConfigManager.config(norseman.name, "Taming Duration", 1800f, "Set time it take to tame, in seconds");
		baseArmor = ConfigManager.config(norseman.name, "Base Armor", norseman.baseArmor, "Set base armor");
		tameable = ConfigManager.config(norseman.name, "Tameable", Toggle.On, "If on, norseman is tameable");
		workResourceInterval = ConfigManager.config(norseman.name, "Work Resource Interval", 10f, "Set minimum required time to receive resource from working");
		Subscribe();
	}

	private void Subscribe()
	{
		baseHealth.SettingChanged += OnHealthChange;
		canMine.SettingChanged += OnWorkConfigChange;
		canFish.SettingChanged += OnWorkConfigChange;
		canLumber.SettingChanged += OnWorkConfigChange;
		tamingTime.SettingChanged += OnTameTimeConfigChanged;
	}

	private void OnHealthChange(object sender, EventArgs e)
	{
		List<Viking> allVikings = Viking.GetAllVikings();
		for (int i = 0; i < allVikings.Count; i++)
		{
			Viking viking = allVikings[i];
            ZDO zdo = ZDOMan.instance.GetZDO(((Character)viking).GetZDOID());

            if (zdo != null)
            {
				((Character)viking).m_health = baseHealth.Value;
                ((Character)viking).SetMaxHealth(baseHealth.Value);
            }
		}
	}

	private static void OnWorkConfigChange(object sender, EventArgs e)
	{
		List<Viking> allVikings = Viking.GetAllVikings();
		for (int i = 0; i < allVikings.Count; i++)
		{
			Viking viking = allVikings[i];
            ZDO zdo = ZDOMan.instance.GetZDO(((Character)viking).GetZDOID());

            if (zdo != null)
            {
				viking.m_vikingAI.ResetWorkTargets();
			}
		}
	}

	private void OnTameTimeConfigChanged(object sender, EventArgs args)
	{
		List<Viking> allVikings = Viking.GetAllVikings();
		for (int i = 0; i < allVikings.Count; i++)
		{
			Viking viking = allVikings[i];
            ZDO zdo = ZDOMan.instance.GetZDO(((Character)viking).GetZDOID());

            if (zdo != null)
            {
				viking.m_tamingTime = tamingTime.Value;
                zdo.Set(ZDOVars.s_tameTimeLeft, viking.m_tamingTime);
            }
		}
	}

	public static float GetBaseArmor(string prefabName, float defaultValue = 0f)
	{
		NorsemanConfigs config;
		return TryGetConfig(prefabName, out config) ? config.baseArmor.Value : defaultValue;
	}

	public static float GetTameTime(string prefabName, float defaultValue = 1800f)
	{
		NorsemanConfigs config;
		return TryGetConfig(prefabName, out config) ? config.tamingTime.Value : defaultValue;
	}

	public static float GetBaseHealth(string prefabName, float defaultValue = 50f)
	{
		NorsemanConfigs config;
		return TryGetConfig(prefabName, out config) ? config.baseHealth.Value : defaultValue;
	}

	public static bool CanVikingFish(string prefabName)
	{
		NorsemanConfigs config;
		return TryGetConfig(prefabName, out config) && config.canFish.Value == Toggle.On;
	}

	public static bool CanVikingLumber(string prefabName)
	{
		NorsemanConfigs config;
		return TryGetConfig(prefabName, out config) && config.canLumber.Value == Toggle.On;
	}

	public static bool CanVikingMine(string prefabName)
	{
		NorsemanConfigs config;
		return TryGetConfig(prefabName, out config) && config.canMine.Value == Toggle.On;
	}

	public static bool DoesWorkRequireFood(string prefabName)
	{
		NorsemanConfigs config;
		return TryGetConfig(prefabName, out config) && config.workRequiresFood.Value == Toggle.On;
	}

	public static float GetWorkResourceInterval(string prefabName)
	{
		NorsemanConfigs config;
		return TryGetConfig(prefabName, out config) ? config.workResourceInterval.Value : 1f;
	}

	public static bool IsTameable(string prefabName)
	{
		NorsemanConfigs config;
		return TryGetConfig(prefabName, out config) && config.tameable.Value == Toggle.On;
	}
}
