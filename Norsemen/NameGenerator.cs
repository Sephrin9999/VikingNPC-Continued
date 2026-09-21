using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using ServerSync;
using UnityEngine;

namespace Norsemen;

public static class NameGenerator
{
	[Serializable]
	public class Names
	{
		private static readonly System.Random rng = new System.Random();

		public List<string> MaleNames = new List<string>();

		public List<string> FemaleNames = new List<string>();

		public string GenerateMaleName()
		{
			return names.MaleNames[rng.Next(names.MaleNames.Count)];
		}

		public string GenerateFemaleName()
		{
			return names.FemaleNames[rng.Next(names.FemaleNames.Count)];
		}
	}

	public static Names names = new Names();

	internal static CustomSyncedValue<string> sync = new CustomSyncedValue<string>(ConfigManager.ConfigSync, "RustyMods.Norsemen.Names.Sync", "");

	public static string FileName = "Names.yml";

	public static string FilePath = Path.Combine(ConfigManager.DirectoryPath, FileName);

	public static void Setup()
	{
		GetOrSerialize();
		SetupFileWatcher();
		sync.ValueChanged += OnConfigChange;
	}

	public static void SetupFileWatcher()
	{
		FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(ConfigManager.DirectoryPath, FileName);
		fileSystemWatcher.Changed += ReadConfigValues;
		fileSystemWatcher.Created += ReadConfigValues;
		fileSystemWatcher.Renamed += ReadConfigValues;
		fileSystemWatcher.IncludeSubdirectories = true;
		fileSystemWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
		fileSystemWatcher.EnableRaisingEvents = true;
	}

	public static void OnConfigChange()
	{
        if (ZNet.instance == null || ZNet.instance.IsServer())
        {
			return;
		}
		string value = sync.Value;
		if (!string.IsNullOrEmpty(value))
		{
			Names names = ConfigManager.deserializer.Deserialize<Names>(value);
			if (names.MaleNames.Count > 0)
			{
				NameGenerator.names.MaleNames = names.MaleNames;
			}
			if (names.FemaleNames.Count > 0)
			{
				NameGenerator.names.FemaleNames = names.FemaleNames;
			}
		}
	}

	public static void ReadConfigValues(object sender, FileSystemEventArgs e)
	{
        if (ZNet.instance != null && ZNet.instance.IsServer())
        {
			Read();
			UpdateSync(ZNet.instance);
			NorsemenPlugin.LogDebug(FileName + " changed");
		}
	}

	public static void UpdateSync(ZNet net)
	{
		if (net.IsServer())
		{
			string value = ConfigManager.serializer.Serialize(names);
			sync.Value = value;
		}
	}

	public static void GetOrSerialize()
	{
		if (File.Exists(FilePath))
		{
			Read();
			return;
		}
		string file = EmbeddedResourceManager.GetFile("Names.yml");
		names = ConfigManager.deserializer.Deserialize<Names>(file);
		File.WriteAllText(FilePath, file);
	}

	public static void Read()
	{
		try
		{
			string input = File.ReadAllText(FilePath);
			Names names = ConfigManager.deserializer.Deserialize<Names>(input);
			if (names.MaleNames.Count > 0)
			{
				NameGenerator.names.MaleNames = names.MaleNames;
			}
			if (names.FemaleNames.Count > 0)
			{
				NameGenerator.names.FemaleNames = names.FemaleNames;
			}
		}
		catch
		{
			NorsemenPlugin.LogError("Failed to deserialize " + FileName);
		}
	}
}
