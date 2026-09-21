using System;
using BepInEx.Configuration;

namespace Norsemen;

public class Raid : RandomEvent
{
	private ConfigEntry<float> duration;

	private ConfigEntry<Toggle> baseOnly;

	private ConfigEntry<Toggle> pauseNoPlayers;

	private ConfigEntry<float> range;

	private ConfigEntry<Toggle> enabled;

	private ConfigEntry<string> requiredKeys;

	private ConfigEntry<string> notRequiredKeys;

	private readonly Norseman viking;

	public Raid(Norseman viking)
	{
		this.viking = viking;
        SpawnSystem.SpawnData val = ((SpawnSystem.SpawnData)viking.spawnInfo).Clone();
		val.m_enabled = true;
		val.m_canSpawnCloseToPlayer = true;
		val.m_spawnInterval = 10f;
		val.m_huntPlayer = true;
		val.m_spawnAtDay = true;
		val.m_spawnAtNight = true;
		val.m_spawnChance = 100f;
		val.m_spawnDistance = 15f;
		val.m_maxSpawned = 10;
		val.m_groupSizeMax = 3;
		val.m_groupRadius = 3f;
		val.m_requiredEnvironments.Clear();
		base.m_spawn.Add(val);
		RaidManager.raids.Add(this);
	}

	public void SetupConfigs(params string[] keys)
	{
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Expected O, but got Unknown
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Expected O, but got Unknown
		enabled = ConfigManager.config(viking.name, "Raid Enabled", base.m_enabled ? Toggle.On : Toggle.Off, "If on, " + viking.name + " raids are enabled");
		duration = ConfigManager.config(viking.name, "Raid Duration", base.m_duration, "Set raid duration for " + viking.name);
		baseOnly = ConfigManager.config(viking.name, "Raid Base Only", base.m_nearBaseOnly ? Toggle.On : Toggle.Off, "If on, " + viking.name + " raids are near base only");
		pauseNoPlayers = ConfigManager.config(viking.name, "Raid Pause No Players", base.m_pauseIfNoPlayerInArea ? Toggle.On : Toggle.Off, "If on, " + viking.name + " raids pause if no players are in area.");
		range = ConfigManager.config(viking.name, "Raid Range", base.m_eventRange, "Set " + viking.name + " event radius");
		requiredKeys = ConfigManager.config(viking.name, "Raid Required Keys", new StringList(keys).ToString(), new ConfigDescription("List of required global keys for " + viking.name + " raid", (AcceptableValueBase)null, new object[1] { StringList.attributes }));
		notRequiredKeys = ConfigManager.config(viking.name, "Raid Not Required Keys", "", new ConfigDescription("List of required keys to be false for " + viking.name + " raid", (AcceptableValueBase)null, new object[1] { StringList.attributes }));
		enabled.SettingChanged += OnConfigChange;
		duration.SettingChanged += OnConfigChange;
		baseOnly.SettingChanged += OnConfigChange;
		range.SettingChanged += OnConfigChange;
		pauseNoPlayers.SettingChanged += OnConfigChange;
		requiredKeys.SettingChanged += OnConfigChange;
		notRequiredKeys.SettingChanged += OnConfigChange;
		OnConfigChange();
	}

	private void OnConfigChange(object sender, EventArgs e)
	{
		OnConfigChange();
	}

	public void OnConfigChange()
	{
		Toggle? toggle = enabled?.Value;
		base.m_enabled = toggle.HasValue && toggle == Toggle.On;
		base.m_duration = duration?.Value ?? base.m_duration;
		toggle = baseOnly?.Value;
		base.m_nearBaseOnly = toggle.HasValue && toggle == Toggle.On;
		toggle = pauseNoPlayers?.Value;
		base.m_pauseIfNoPlayerInArea = toggle.HasValue && toggle == Toggle.On;
		base.m_eventRange = range?.Value ?? base.m_eventRange;
		base.m_requiredGlobalKeys = new StringList(requiredKeys?.Value ?? "").list;
		base.m_notRequiredGlobalKeys = new StringList(notRequiredKeys?.Value ?? "").list;
	}
}
