using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Norsemen;

public static class CommandManager
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

        public static Terminal.ConsoleEventFailable _003C_003E9__3_0;


        internal object _003CPatch_Terminal_Awake_003Eb__3_0(Terminal.ConsoleEventArgs args)
        {
			if (args.Length < 2)
			{
				return false;
			}
			if (!commands.TryGetValue(args[1], out var value))
			{
				return false;
			}
			return value.Run(args);
		}

		internal bool _003CPatch_Terminal_Awake_003Eb__3_1(KeyValuePair<string, NorseCommand> x)
		{
			return !x.Value.IsSecret();
		}

		internal string _003CPatch_Terminal_Awake_003Eb__3_2(KeyValuePair<string, NorseCommand> x)
		{
			return x.Key;
		}
	}

	private static readonly string startCommand;

	public static readonly Dictionary<string, NorseCommand> commands;

    private static readonly List<string> tabOptions = new List<string>();
    private static int tabIndex = -1;
    private static string lastTabWord = "";

    static CommandManager()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		commands = new Dictionary<string, NorseCommand>();
		Harmony harmony = NorsemenPlugin.instance._harmony;
		startCommand = "Norsemen".ToLower();
		harmony.Patch((MethodBase)AccessTools.Method(typeof(Terminal), "Awake", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(AccessTools.Method(typeof(CommandManager), "Patch_Terminal_Awake", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		harmony.Patch((MethodBase)AccessTools.Method(typeof(Terminal), "updateSearch", (Type[])null, (Type[])null), new HarmonyMethod(AccessTools.Method(typeof(CommandManager), "Patch_Terminal_UpdateSearch", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
		harmony.Patch((MethodBase)AccessTools.Method(typeof(Terminal), "tabCycle", (Type[])null, (Type[])null), new HarmonyMethod(AccessTools.Method(typeof(CommandManager), "Patch_Terminal_TabCycle", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
	}

	private static void Patch_Terminal_Awake()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		string text = startCommand;
		object obj = _003C_003Ec._003C_003E9__3_0;
		if (obj == null)
		{
            Terminal.ConsoleEventFailable val = delegate (Terminal.ConsoleEventArgs args)
            {
                if (args.Length < 2)
                {
                    return false;
                }

                string selection = args[1].ToLowerInvariant();

                // Preserve existing Norsemen subcommands.
                if (commands.TryGetValue(selection, out NorseCommand value))
                {
                    return value.Run(args);
                }

                // Recognize biome selections and optional race suffixes.
                string biome = selection;
                string race = "Configured";

                if (selection.EndsWith("-h"))
                {
                    biome = selection.Substring(0, selection.Length - 2);
                    race = "Human";
                }
                else if (selection.EndsWith("-e"))
                {
                    biome = selection.Substring(0, selection.Length - 2);
                    race = "Elf";
                }

                string[] validBiomes =
                {
                    "meadows",
                    "blackforest",
                    "swamp",
                    "mountains",
                    "plains",
                    "mistlands",
                    "ashlands",
                    "deepnorth"
                };

                if (Array.IndexOf(validBiomes, biome) < 0)
                {
                    Debug.LogWarning($"[VikingNPC] Unknown Norsemen command or biome: {selection}");
                    return false;
                }
                string prefabName = biome switch
                {
                    "meadows" => "Meadows_Norseman_RS",
                    "blackforest" => "BlackForest_Norseman_RS",
                    "swamp" => "Swamp_Norseman_RS",
                    "mountains" => "Mountains_Norseman_RS",
                    "plains" => "Plains_Norseman_RS",
                    "mistlands" => "Mistlands_Norseman_RS",
                    "ashlands" => "AshLands_Norseman_RS",
                    "deepnorth" => "DeepNorth_Norseman_RS",
                    _ => null
                };

                Debug.Log($"[VikingNPC] Selected prefab: {prefabName}");
                if (ZNetScene.instance == null)
                {
                    Debug.LogWarning("[VikingNPC] Cannot spawn: ZNetScene is not available.");
                    return false;
                }

                GameObject prefab = ZNetScene.instance.GetPrefab(prefabName);

                if (prefab == null)
                {
                    Debug.LogWarning($"[VikingNPC] Cannot find registered prefab: {prefabName}");
                    return false;
                }

                Debug.Log($"[VikingNPC] Prefab found successfully: {prefab.name}");
                Player player = Player.m_localPlayer;

                if (player == null)
                {
                    Debug.LogWarning("[VikingNPC] Cannot spawn: Local player not found.");
                    return false;
                }

                // Spawn approximately 3 meters in front of the player.
                Vector3 spawnPosition =
                    player.transform.position + player.transform.forward * 3f;

                spawnPosition.y = ZoneSystem.instance.GetSolidHeight(spawnPosition);

                GameObject spawnedNPC = UnityEngine.Object.Instantiate(
                    prefab,
                    spawnPosition,
                    Quaternion.identity
                );

                if (spawnedNPC == null)
                {
                    Debug.LogWarning("[VikingNPC] Failed to instantiate Norseman.");
                    return false;
                }

                //Debug.Log($"[VikingNPC] Successfully spawned {prefabName} at {spawnPosition}");
                Viking spawnedViking = spawnedNPC.GetComponent<Viking>();

                if (spawnedViking != null)
                {
                    switch (race)
                    {
                        case "Human":
                            spawnedViking.ForcedSpawnRace = NorsemanRace.Human;
                            break;

                        case "Elf":
                            spawnedViking.ForcedSpawnRace = NorsemanRace.Elf;
                            break;
                    }

                    //Debug.Log($"[VikingNPC] Applied spawn race: {race}");
                }

                //Debug.Log($"[VikingNPC] Biome: {biome}, Race: {race}");

                return true;
            };
            _003C_003Ec._003C_003E9__3_0 = val;
			obj = (object)val;
		}
        new Terminal.ConsoleCommand(
			 text,
			 "use help to find available commands",
			 (Terminal.ConsoleEventFailable)obj,
			 false,
			 false,
			 false,
			 false,
			 false,
			 false,
			 new Terminal.ConsoleOptionsFetcher(() =>
				 commands
					 .Where(x => !x.Value.IsSecret())
					 .Select(x => x.Key)
					 .ToList()),
			 false,
			 false
		 );
    }

	private static bool Patch_Terminal_UpdateSearch(Terminal __instance, string word)
	{
		if (__instance.m_search == null)
		{
			return true;
		}
		string[] array = ((TMP_InputField)__instance.m_input).text.Split(' ');
		if (array.Length < 3)
		{
			return true;
		}
		if (array[0] != startCommand)
		{
			return true;
		}
		return HandleSearch(__instance, word, array);
	}

	private static bool HandleSearch(Terminal __instance, string word, string[] strArray)
	{
		if (!commands.TryGetValue(strArray[1], out var value))
		{
			return true;
		}
		if (value.HasOptions() && strArray.Length == 3)
		{
			List<string> list = value.FetchOptions();
			string currentSearch = strArray[2];
			List<string> list2;
			if (!Utility.IsNullOrWhiteSpace(currentSearch))
			{
				int num = list.IndexOf(currentSearch);
				list2 = ((num != -1) ? list.GetRange(num, list.Count - num) : list);
				list2 = list2.FindAll((string x) => x.ToLower().Contains(currentSearch.ToLower()));
			}
			else
			{
				list2 = list;
			}
			if (list2.Count <= 0)
			{
				__instance.m_search.text = value.m_description;
			}
			else
			{
                list2.Remove(word);
                __instance.m_search.text = "";

                int num2 = 10;
                int num3 = Math.Min(list2.Count, num2);

                for (int num4 = 0; num4 < num3; num4++)
                {
                    string text = list2[num4];
                    TMP_Text search = __instance.m_search;
                    search.text = search.text + text + " ";
                }

                if (list2.Count <= num2)
                {
                    return false;
                }

                int num5 = list2.Count - num2;
                TMP_Text search2 = __instance.m_search;
                search2.text += $"... {num5} more.";
            }
		}
		else
		{
			__instance.m_search.text = value.m_description;
		}
		return false;
	}

	private static bool Patch_Terminal_TabCycle(Terminal __instance, string word, List<string> options, bool usePrefix)
	{
		if (options == null || options.Count == 0)
		{
			return true;
		}
        if (usePrefix)
        {
            if (word.Length < 1)
            {
                return true;
            }

            word = word.Substring(1);
        }
        return HandleTabCycle(__instance, word, options, usePrefix);
	}

    private static bool HandleTabCycle(
    Terminal __instance,
    string word,
    List<string> options,
    bool usePrefix)
    {
        TMP_InputField input = (TMP_InputField)__instance.m_input;
        string text = input.text;
        string[] array = text.Split(' ');

        if (array.Length < 2 ||
            !string.Equals(array[0], startCommand, StringComparison.CurrentCultureIgnoreCase) ||
            !commands.ContainsKey(array[1].ToLower()))
        {
            return true;
        }

        word = word.ToLower();

        // Build a fresh option list when the search word changes.
        if (lastTabWord != word)
        {
            lastTabWord = word;
            tabOptions.Clear();
            tabIndex = -1;

            if (word.Length == 0)
            {
                tabOptions.AddRange(options);
            }
            else
            {
                foreach (string option in options)
                {
                    if (option != null &&
                        option.Length >= word.Length &&
                        option.Substring(0, word.Length)
                            .Equals(word, StringComparison.CurrentCultureIgnoreCase))
                    {
                        tabOptions.Add(option);
                    }
                }
            }

            tabOptions.Sort();
        }

        if (tabOptions.Count == 0)
        {
            return false;
        }

        tabIndex++;

        if (tabIndex >= tabOptions.Count)
        {
            tabIndex = 0;
        }

        string selected = tabOptions[tabIndex];

        int secondSpace = -1;
        int spaces = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == ' ')
            {
                spaces++;

                if (spaces == 2)
                {
                    secondSpace = i;
                    break;
                }
            }
        }

        if (secondSpace >= 0)
        {
            input.text = text.Substring(0, secondSpace + 1) + selected;
        }
        else if (array.Length == 2)
        {
            input.text = text + " " + selected;
        }

        input.caretPosition = input.text.Length;

        return false;
    }
}
