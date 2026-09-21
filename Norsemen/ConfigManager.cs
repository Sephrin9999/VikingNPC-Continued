using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Norsemen;

public static class ConfigManager
{
	public static readonly ISerializer serializer;

	public static readonly IDeserializer deserializer;

	private static readonly ConfigFile Config;

    internal static readonly ConfigSync ConfigSync;

    private static readonly string ConfigFileName;

	private static readonly string ConfigFileFullPath;

	public static readonly string DirectoryPath;

	private static readonly ConfigEntry<LogLevel> logLevels;

	private static readonly ConfigEntry<Toggle> canHaveStars;

	private static readonly ConfigEntry<string> reviveRequirements;

	public static bool ShouldLog(LogLevel type)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return ((Enum)logLevels.Value).HasFlag((Enum)(object)type);
	}

	public static bool CanHaveStars()
	{
		return canHaveStars.Value == Toggle.On;
	}

	public static Piece.Requirement[] GetReviveRequirements()
	{
		return new RequirementList(reviveRequirements.Value).ToPieceRequirements();
	}

	static ConfigManager()
	{
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Expected O, but got Unknown
		serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitDefaults | DefaultValuesHandling.OmitEmptyCollections).DisableAliases().WithNamingConvention(CamelCaseNamingConvention.Instance)
			.Build();
		deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
		Config = ((BaseUnityPlugin)NorsemenPlugin.instance).Config;
		ConfigFileName = "RustyMods.Norsemen.cfg";
		ConfigFileFullPath = Path.Combine(Paths.ConfigPath, ConfigFileName);
        ConfigSync = new ConfigSync(NorsemenPlugin.ModGUID)
        {
            DisplayName = NorsemenPlugin.ModName,
            CurrentVersion = NorsemenPlugin.ModVersion,
            MinimumRequiredVersion = NorsemenPlugin.ModVersion
        };
        DirectoryPath = Path.Combine(Paths.ConfigPath, "Norsemen");
		if (!Directory.Exists(DirectoryPath))
		{
			Directory.CreateDirectory(DirectoryPath);
		}
		ConfigEntry<Toggle> lockingConfig = config("1 - General", "Lock Configuration", Toggle.On, "If on, the configuration is locked and can be changed by server admins only.");
		ConfigSync.AddLockingConfigEntry<Toggle>(lockingConfig);
		logLevels = config<LogLevel>("1 - General", "Log Levels", (LogLevel)2, "Set log levels", synchronizedSetting: false);
		canHaveStars = config("Settings", "Can Have Stars", Toggle.On, "If on, norsemen can have stars");
		reviveRequirements = config("Settings", "Revive Cost", "", new ConfigDescription("Cost to revive viking", (AcceptableValueBase)null, new object[1] { RequirementList.attributes }));
		Harmony harmony = NorsemenPlugin.instance._harmony;
		harmony.Patch((MethodBase)AccessTools.Method(typeof(ZNet), "Awake", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(AccessTools.Method(typeof(ConfigManager), "Patch_ZNet_Awake", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
	}

	public static void Patch_ZNet_Awake(ZNet __instance)
	{
		CustomizationManager.UpdateSync(__instance);
		TalkManager.UpdateSync(__instance);
		NameGenerator.UpdateSync(__instance);
	}

	public static void SetupWatcher()
	{
		FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(Paths.ConfigPath, ConfigFileName);
		fileSystemWatcher.Changed += ReadConfigValues;
		fileSystemWatcher.Created += ReadConfigValues;
		fileSystemWatcher.Renamed += ReadConfigValues;
		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
		fileSystemWatcher.EnableRaisingEvents = true;
	}

	private static void ReadConfigValues(object sender, FileSystemEventArgs e)
	{
		if (!File.Exists(ConfigFileFullPath))
		{
			return;
		}
		try
		{
			NorsemenPlugin.LogDebug("ReadConfigValues called");
			Config.Reload();
		}
		catch
		{
			NorsemenPlugin.LogError("There was an issue loading your " + ConfigFileName);
			NorsemenPlugin.LogError("Please check your config entries for spelling and format!");
		}
	}

	public static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		ConfigDescription val = new ConfigDescription(description.Description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"), description.AcceptableValues, description.Tags);
		ConfigEntry<T> val2 = Config.Bind<T>(group, name, value, val);
		SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry<T>(val2);
		syncedConfigEntry.SynchronizedConfig = synchronizedSetting;
		return val2;
	}

	public static ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		return config(group, name, value, new ConfigDescription(description, (AcceptableValueBase)null, Array.Empty<object>()), synchronizedSetting);
	}
}
