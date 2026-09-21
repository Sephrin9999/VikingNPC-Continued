using System;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace Norsemen;

public class Level
{
	public static readonly ConfigurationManagerAttributes attributes = new ConfigurationManagerAttributes
	{
		CustomDrawer = DrawTable
	};

	public readonly int Min;

	public readonly int Max;

	public readonly float Chance;

	public Level(int min, int max, float chance)
	{
		Min = min;
		Max = max;
		Chance = chance;
	}

	public Level(string level)
	{
		string[] array = level.Split(':');
		Min = ((!int.TryParse(array[0], out var result)) ? 1 : result);
		Max = ((!int.TryParse(array[1], out var result2)) ? 1 : result2);
		Chance = (float.TryParse(array[2], out var result3) ? result3 : 0.5f);
	}

	public static void DrawTable(ConfigEntryBase cfg)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Expected O, but got Unknown
		bool valueOrDefault = cfg.Description.Tags.Select((object a) => (a.GetType().Name == "ConfigurationManagerAttributes") ? ((bool?)a.GetType().GetField("ReadOnly")?.GetValue(a)) : ((bool?)null)).FirstOrDefault((bool? v) => v.HasValue) == true;
		Level level = new Level((string)cfg.BoxedValue);
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label("Min: ", Array.Empty<GUILayoutOption>());
		int num = (valueOrDefault ? level.Min : (int.TryParse(GUILayout.TextField(level.Min.ToString(), new GUIStyle(GUI.skin.textField), Array.Empty<GUILayoutOption>()), out var result) ? result : level.Min));
		GUILayout.Label("Max: ", Array.Empty<GUILayoutOption>());
		int num2 = (valueOrDefault ? level.Max : (int.TryParse(GUILayout.TextField(level.Max.ToString(), new GUIStyle(GUI.skin.textField), Array.Empty<GUILayoutOption>()), out var result2) ? result2 : level.Max));
		GUILayout.Label("LevelUp Chance: ", Array.Empty<GUILayoutOption>());
		float num3 = (valueOrDefault ? level.Chance : (float.TryParse(GUILayout.TextField(level.Chance.ToString(CultureInfo.InvariantCulture), Array.Empty<GUILayoutOption>()), out var result3) ? result3 : level.Chance));
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		if (num != level.Min || num2 != level.Max || Math.Abs(num3 - level.Chance) > 0.01f)
		{
			cfg.BoxedValue = new Level(num, num2, num3).ToString();
		}
	}

	public override string ToString()
	{
		return $"{Min}:{Max}:{Chance}";
	}
}
