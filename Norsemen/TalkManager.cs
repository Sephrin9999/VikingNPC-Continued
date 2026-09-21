using System.Collections.Generic;
using System.IO;
using BepInEx;
using ServerSync;
using UnityEngine;

namespace Norsemen;

public static class TalkManager
{
	public enum TalkType
	{
		Generic,
		PlayerBase,
		Greets,
		Farewells,
		Damaged,
		Thieved,
		Puke,
		Eat
	}

	public static string FileName;

	public static string FilePath;

	public static Dictionary<TalkType, List<string>> talks;

	internal static CustomSyncedValue<string> sync;

	static TalkManager()
	{
		talks = new Dictionary<TalkType, List<string>>();
		FileName = "RandomTalks.yml";
		FilePath = Path.Combine(ConfigManager.DirectoryPath, FileName);
		sync = new CustomSyncedValue<string>(ConfigManager.ConfigSync, "RustyMods.Norseman.RandomTalk.Sync");
		sync.ValueChanged += OnConfigChanged;
	}

	public static List<string> GetTalk(TalkType type)
	{
		List<string> value;
		return talks.TryGetValue(type, out value) ? value : new List<string>();
	}

	public static void OnConfigChanged()
	{
        if (ZNet.instance == null || ZNet.instance.IsServer())
        {
			return;
		}
		string value = sync.Value;
		if (string.IsNullOrEmpty(value))
		{
			return;
		}
		try
		{
			Dictionary<TalkType, List<string>> dictionary = ConfigManager.deserializer.Deserialize<Dictionary<TalkType, List<string>>>(value);
			talks = dictionary;
		}
		catch
		{
			NorsemenPlugin.LogError("Failed to deserialize server's random talks");
		}
	}

	public static void UpdateSync(ZNet net)
	{
		if (net.IsServer())
		{
			string value = ConfigManager.serializer.Serialize(talks);
			sync.Value = value;
		}
	}

	public static void Setup()
	{
		GetOrSerialize();
	}

	public static void GetOrSerialize()
	{
		if (File.Exists(FilePath))
		{
			Read(FilePath);
			return;
		}
		string file = EmbeddedResourceManager.GetFile(FileName);
		talks = ConfigManager.deserializer.Deserialize<Dictionary<TalkType, List<string>>>(file);
		File.WriteAllText(FilePath, file);
	}

	public static void Read(string filePath)
	{
		try
		{
			string input = File.ReadAllText(filePath);
			Dictionary<TalkType, List<string>> dictionary = ConfigManager.deserializer.Deserialize<Dictionary<TalkType, List<string>>>(input);
			talks = dictionary;
		}
		catch
		{
			NorsemenPlugin.LogError("Failed to deserialize random talks");
		}
	}

	public static void SetupWatcher()
	{
		FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(ConfigManager.DirectoryPath, FileName);
		fileSystemWatcher.Changed += ReadConfigValues;
		fileSystemWatcher.Created += ReadConfigValues;
		fileSystemWatcher.Renamed += ReadConfigValues;
		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
		fileSystemWatcher.EnableRaisingEvents = true;
	}

	public static void ReadConfigValues(object sender, FileSystemEventArgs e)
	{
        if (ZNet.instance != null && ZNet.instance.IsServer())
        {
			Read(FilePath);
			UpdateSync(ZNet.instance);
			NorsemenPlugin.LogInfo(FileName + " file changed");
		}
	}
}
