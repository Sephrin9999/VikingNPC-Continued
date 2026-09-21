using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using ServerSync;
using UnityEngine;

namespace Norsemen;

public static class CustomizationManager
{
	public static readonly List<string> beards;

	public static readonly List<string> hairs;

	public static readonly List<Color> hairColors;

	public static readonly List<Color> skinColors;

	public static readonly string FileName;

	public static readonly string FilePath;

	public static Dictionary<Heightmap.Biome, Equipment> equipment;

    internal static readonly CustomSyncedValue<string> sync;

	public static Color GetRandomSkinColor()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		int index = UnityEngine.Random.Range(0, skinColors.Count);
		return skinColors[index];
	}

	public static void GetHairAndBeards(ObjectDB db)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Invalid comparison between Unknown and I4
		foreach (GameObject item in db.m_items)
		{
            
            ItemDrop component = item.GetComponent<ItemDrop>();

            if (component == null)
            {
                continue;
            }

            if ((int)component.m_itemData.m_shared.m_itemType == 10)
            {
                if (Utils.CustomStartsWith(item.name, "Beard"))
                {
                    beards.Add(item.name);
                }
                else if (Utils.CustomStartsWith(item.name, "Hair"))
                {
                    hairs.Add(item.name);
                }
            }
		}
		beards.RemoveAll((string x) => x.Contains("_"));
		hairs.RemoveAll((string x) => x.Contains("_"));
	}

	public static void Setup()
	{
		GetOrSerialize();
		SetupFileWatcher();
		sync.ValueChanged += OnConfigChange;
	}

	public static void OnConfigChange()
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
			Dictionary<Heightmap.Biome, Equipment> other = ConfigManager.deserializer.Deserialize<Dictionary<Heightmap.Biome, Equipment>>(value);
			equipment.Clear();
			equipment.AddRange(other);
		}
		catch
		{
			NorsemenPlugin.LogError("Failed to deserialize server's equipments");
		}
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

	public static void ReadConfigValues(object sender, FileSystemEventArgs e)
	{
		if (ZNet.instance==null || !ZNet.instance.IsServer())
		{
			return;
		}
		try
		{
			string input = File.ReadAllText(FilePath);
			Dictionary<Heightmap.Biome, Equipment> dictionary = ConfigManager.deserializer.Deserialize<Dictionary<Heightmap.Biome, Equipment>>(input);
			equipment = dictionary;
			UpdateSync(ZNet.instance);
			NorsemenPlugin.LogDebug(FileName + " changed");
		}
		catch (Exception ex)
		{
			NorsemenPlugin.LogError("Failed to deserialize " + FileName);
			Debug.LogError((object)ex.Message);
		}
	}

	public static void UpdateSync(ZNet net)
	{
		if (net.IsServer())
		{
			string value = ConfigManager.serializer.Serialize(equipment);
			sync.Value = value;
		}
	}

	public static void GetOrSerialize()
	{
		if (!File.Exists(FilePath))
		{
			string file = EmbeddedResourceManager.GetFile(FileName);
			equipment = ConfigManager.deserializer.Deserialize<Dictionary<Heightmap.Biome, Equipment>>(file);
			File.WriteAllText(FilePath, file);
		}
		else
		{
			Read();
		}
	}

	public static void Read()
	{
		try
		{
			string input = File.ReadAllText(FilePath);
			Dictionary<Heightmap.Biome, Equipment> dictionary = ConfigManager.deserializer.Deserialize<Dictionary<Heightmap.Biome, Equipment>>(input);
			equipment = dictionary;
		}
		catch (Exception ex)
		{
			NorsemenPlugin.LogError("Failed to deserialize: " + FileName);
			Debug.LogError((object)ex.Message);
		}
	}

	public static void Add(Heightmap.Biome biome, params ConditionalWeightedSet[] set)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!equipment.ContainsKey(biome))
		{
			equipment[biome] = new Equipment();
		}
		equipment[biome].RandomSets.Add(set);
	}

	public static void Add(Heightmap.Biome biome, params ConditionalChanceItem[] item)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!equipment.ContainsKey(biome))
		{
			equipment[biome] = new Equipment();
		}
		equipment[biome].RandomItems.Add(item);
	}

	public static void Add(Heightmap.Biome biome, params ConditioanlWeightedItem[] weapon)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!equipment.ContainsKey(biome))
		{
			equipment[biome] = new Equipment();
		}
		equipment[biome].RandomWeapons.Add(weapon);
	}

	public static List<ConditionalWeightedSet> GetSets(Heightmap.Biome biome)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (!equipment.TryGetValue(biome, out var value))
		{
			return new List<ConditionalWeightedSet>();
		}
		return value.RandomSets;
	}

	public static List<ConditionalChanceItem> GetItems(Heightmap.Biome biome)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (!equipment.TryGetValue(biome, out var value))
		{
			return new List<ConditionalChanceItem>();
		}
		return value.RandomItems;
	}

	public static List<ConditioanlWeightedItem> GetWeapons(Heightmap.Biome biome)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (!equipment.TryGetValue(biome, out var value))
		{
			return new List<ConditioanlWeightedItem>();
		}
		return value.RandomWeapons;
	}

	static CustomizationManager()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		beards = new List<string>();
		hairs = new List<string>();
		hairColors = new List<Color>
		{
			Color.black,
			new Color(0.98f, 0.94f, 0.75f, 1f),
			new Color(0.63f, 0.36f, 0f, 1f),
			new Color(0.15f, 0.08f, 0.05f, 1f),
			new Color(0.35f, 0.25f, 0.15f, 1f),
			new Color(0.55f, 0.45f, 0.35f, 1f),
			new Color(0.95f, 0.87f, 0.51f, 1f),
			new Color(0.85f, 0.75f, 0.45f, 1f),
			new Color(0.72f, 0.65f, 0.52f, 1f),
			new Color(0.45f, 0.18f, 0.08f, 1f),
			new Color(0.55f, 0.12f, 0.05f, 1f),
			new Color(0.72f, 0.25f, 0.12f, 1f),
			new Color(0.4f, 0.4f, 0.4f, 1f),
			new Color(0.65f, 0.65f, 0.65f, 1f),
			new Color(0.88f, 0.88f, 0.9f, 1f),
			new Color(0.25f, 0.25f, 0.28f, 1f)
		};
		skinColors = new List<Color>
		{
			new Color(1f, 1f, 1f, 1f),
			new Color(1f, 0.95f, 0.92f, 1f),
			new Color(1f, 0.92f, 0.86f, 1f),
			new Color(1f, 0.94f, 0.88f, 1f),
			new Color(1f, 0.9f, 0.82f, 1f),
			new Color(1f, 0.88f, 0.78f, 1f),
			new Color(0.98f, 0.85f, 0.72f, 1f),
			new Color(0.95f, 0.82f, 0.68f, 1f),
			new Color(0.92f, 0.78f, 0.62f, 1f),
			new Color(0.88f, 0.72f, 0.56f, 1f),
			new Color(0.85f, 0.68f, 0.52f, 1f),
			new Color(0.78f, 0.62f, 0.48f, 1f),
			new Color(1f, 0.88f, 0.75f, 1f),
			new Color(0.98f, 0.9f, 0.8f, 1f),
			new Color(0.95f, 0.86f, 0.76f, 1f)
		};
		FileName = "Equipment.yml";
		FilePath = Path.Combine(ConfigManager.DirectoryPath, FileName);
		equipment = new Dictionary<Heightmap.Biome, Equipment>();
		sync = new CustomSyncedValue<string>(ConfigManager.ConfigSync, "RustyMods.Norsemen.Equipment.Sync", "");
	}
}
