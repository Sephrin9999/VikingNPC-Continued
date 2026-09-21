using System;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace Norsemen;

public static class SpawnManager
{
	public class SpawnInfo : SpawnSystem.SpawnData
    {
		public class Configs
		{
			public ConfigEntry<Toggle> Enabled = null;

			public ConfigEntry<Heightmap.Biome> Biome = null;

			public ConfigEntry<Heightmap.BiomeArea> Area = null;

			public ConfigEntry<int> MaxSpawned = null;

			public ConfigEntry<float> Interval = null;

			public ConfigEntry<float> Chance = null;

			public ConfigEntry<float> Distance = null;

			public ConfigEntry<string> RequiredKey = null;

			public ConfigEntry<string> RequiredEnvs = null;

			public ConfigEntry<TimeOfDay> TOD = null;

			public ConfigEntry<string> Altitude;

			public ConfigEntry<Region> Forest;

			public ConfigEntry<string> Level;
		}

		public enum TimeOfDay
		{
			Both,
			Night,
			Day
		}

		public enum Region
		{
			Both,
			InForest,
			OutForest
		}

		private readonly string PrefabName;

		public readonly Heightmap.Biome Biome;

		public readonly Configs configs;

		private void ConfigChanged(object o, EventArgs e)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			base.m_enabled = configs.Enabled.Value == Toggle.On;
			base.m_biome = configs.Biome.Value;
			base.m_biomeArea = configs.Area.Value;
			base.m_maxSpawned = configs.MaxSpawned.Value;
			base.m_spawnInterval = configs.Interval.Value;
			base.m_spawnChance = configs.Chance.Value;
			base.m_spawnDistance = configs.Distance.Value;
			base.m_requiredGlobalKey = configs.RequiredKey.Value;
			base.m_requiredEnvironments = new EnvList(configs.RequiredEnvs.Value).GetValidatedList();
			TimeOfDay value = configs.TOD.Value;
			bool spawnAtDay = ((value == TimeOfDay.Both || value == TimeOfDay.Day) ? true : false);
			base.m_spawnAtDay = spawnAtDay;
			value = configs.TOD.Value;
			spawnAtDay = (uint)value <= 1u;
			base.m_spawnAtNight = spawnAtDay;
			base.m_minAltitude = ((configs.Altitude != null) ? new MinMax(configs.Altitude.Value).Min : (-1000f));
			base.m_maxAltitude = ((configs.Altitude != null) ? new MinMax(configs.Altitude.Value).Max : 1000f);
			bool flag = configs.Forest == null;
			bool flag2 = flag;
			if (!flag2)
			{
				Region value2 = configs.Forest.Value;
				spawnAtDay = (uint)value2 <= 1u;
				flag2 = spawnAtDay;
			}
			base.m_inForest = flag2;
			bool flag3 = configs.Forest == null;
			bool flag4 = flag3;
			if (!flag4)
			{
				Region value2 = configs.Forest.Value;
				spawnAtDay = ((value2 == Region.Both || value2 == Region.OutForest) ? true : false);
				flag4 = spawnAtDay;
			}
			base.m_outsideForest = flag4;
			base.m_minLevel = ((configs.Level == null) ? 1 : new Level(configs.Level.Value).Min);
			base.m_maxLevel = ((configs.Level == null) ? 1 : new Level(configs.Level.Value).Max);
			base.m_overrideLevelupChance = ((configs.Level != null) ? new Level(configs.Level.Value).Chance : 0f);
		}

		public void SetupConfigs()
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Expected O, but got Unknown
			//IL_025e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0269: Expected O, but got Unknown
			//IL_0305: Unknown result type (might be due to invalid IL or missing references)
			//IL_0310: Expected O, but got Unknown
			//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03bd: Expected O, but got Unknown
			configs.Enabled = ConfigManager.config(PrefabName, "Enabled", Toggle.On, "If on, viking can spawn");
			configs.Enabled.SettingChanged += ConfigChanged;
			configs.Biome = ConfigManager.config<Heightmap.Biome>(PrefabName, "Biome", Biome, "Set biomes viking can spawn in");
			configs.Biome.SettingChanged += ConfigChanged;
			configs.Area = ConfigManager.config<Heightmap.BiomeArea>(PrefabName, "Biome Area", (Heightmap.BiomeArea)3, "Set particular part of biome viking can spawn in");
			configs.Area.SettingChanged += ConfigChanged;
			configs.MaxSpawned = ConfigManager.config(PrefabName, "Max Spawned", base.m_maxSpawned, "Set maximum amount allowed spawned in a zone");
			configs.MaxSpawned.SettingChanged += ConfigChanged;
			configs.Interval = ConfigManager.config(PrefabName, "Spawn Interval", base.m_spawnInterval, "Set how often vikings will try to spawn");
			configs.Interval.SettingChanged += ConfigChanged;
			configs.Chance = ConfigManager.config(PrefabName, "Spawn Chance", base.m_spawnChance, new ConfigDescription("Set chance to spawn", (AcceptableValueBase)(object)new AcceptableValueRange<float>(0f, 100f), Array.Empty<object>()));
			configs.Chance.SettingChanged += ConfigChanged;
			configs.Distance = ConfigManager.config(PrefabName, "Spawn Distance", base.m_spawnDistance, "Spawn range, 0 = use global settings");
			configs.Distance.SettingChanged += ConfigChanged;
			configs.RequiredKey = ConfigManager.config(PrefabName, "Required Key", "", "Only spawn if this key is present");
			configs.RequiredKey.SettingChanged += ConfigChanged;
			configs.RequiredEnvs = ConfigManager.config(PrefabName, "Required Envs", new EnvList().ToString(), new ConfigDescription("List of required environments for viking to spawn", (AcceptableValueBase)null, new object[1] { EnvList.attributes }));
			configs.RequiredEnvs.SettingChanged += ConfigChanged;
			configs.TOD = ConfigManager.config(PrefabName, "Spawn Time Of Day", TimeOfDay.Both, "Set time of day requirement");
			configs.TOD.SettingChanged += ConfigChanged;
			configs.Altitude = ConfigManager.config(PrefabName, "Spawn Altitude", new MinMax(base.m_minAltitude, base.m_maxAltitude).ToString(), new ConfigDescription("Set [min]-[max] altitude", (AcceptableValueBase)null, new object[1] { MinMax.attributes }));
			configs.Altitude.SettingChanged += ConfigChanged;
			configs.Forest = ConfigManager.config(PrefabName, "Spawn Region", Region.Both, "Set which region viking can spawn in");
			configs.Forest.SettingChanged += ConfigChanged;
			configs.Level = ConfigManager.config(PrefabName, "Spawn Level", new Level(base.m_minLevel, base.m_maxLevel, base.m_overrideLevelupChance).ToString(), new ConfigDescription("Set [min]:[max]:[chanceToLevel]", (AcceptableValueBase)null, new object[1] { Level.attributes }));
			configs.Level.SettingChanged += ConfigChanged;
			ConfigChanged(null, null);
		}

		public SpawnInfo(Norseman viking)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			PrefabName = viking.name;
			base.m_name = viking.name;
			Biome = viking.biome;
			configs = new Configs();
			base.m_groupSizeMin = 0;
			base.m_groupSizeMax = 1;
			base.m_levelUpMinCenterDistance = 1f;
			base.m_minTilt = 0f;
			base.m_maxTilt = 50f;
			base.m_groupRadius = 50f;
			((Behaviour)SpawnList).enabled = true;
			SpawnList.m_spawners.Add((SpawnSystem.SpawnData)(object)this);
		}
	}

	public static readonly SpawnSystemList SpawnList;

	static SpawnManager()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		SpawnList = ((Component)NorsemenPlugin.instance).gameObject.AddComponent<SpawnSystemList>();
		Harmony harmony = NorsemenPlugin.instance._harmony;
		harmony.Patch((MethodBase)AccessTools.Method(typeof(SpawnSystem), "Awake", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(AccessTools.Method(typeof(SpawnManager), "Patch_SpawnSystem_Awake", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
	}

	public static void Patch_SpawnSystem_Awake(SpawnSystem __instance)
	{
		__instance.m_spawnLists.Add(SpawnList);
	}
}
