using System;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace Norsemen;

public class MinMax
{
	public static readonly ConfigurationManagerAttributes attributes = new ConfigurationManagerAttributes
	{
		CustomDrawer = DrawTable
	};

	public readonly float Min;

	public readonly float Max;

	public MinMax(float min, float max)
	{
		Min = min;
		Max = max;
	}

	public MinMax(string altitude)
	{
		string[] array = altitude.Split('-');
		Min = (float.TryParse(array[0], out var result) ? result : (-1000f));
		Max = (float.TryParse(array[1], out var result2) ? result2 : 1000f);
	}

	public static void DrawTable(ConfigEntryBase cfg)
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		bool valueOrDefault = cfg.Description.Tags.Select((object a) => (a.GetType().Name == "ConfigurationManagerAttributes") ? ((bool?)a.GetType().GetField("ReadOnly")?.GetValue(a)) : ((bool?)null)).FirstOrDefault((bool? v) => v.HasValue) == true;
		MinMax minMax = new MinMax((string)cfg.BoxedValue);
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label("Min: ", Array.Empty<GUILayoutOption>());
		float num = (valueOrDefault ? minMax.Min : (float.TryParse(GUILayout.TextField(minMax.Min.ToString(CultureInfo.InvariantCulture), new GUIStyle(GUI.skin.textField), Array.Empty<GUILayoutOption>()), out var result) ? result : minMax.Min));
		GUILayout.Label("Max: ", Array.Empty<GUILayoutOption>());
		float num2 = (valueOrDefault ? minMax.Max : (float.TryParse(GUILayout.TextField(minMax.Max.ToString(CultureInfo.InvariantCulture), new GUIStyle(GUI.skin.textField), Array.Empty<GUILayoutOption>()), out var result2) ? result2 : minMax.Max));
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		if (Math.Abs(num - minMax.Min) > 0.1f || Math.Abs(num2 - minMax.Max) > 0.1f)
		{
			cfg.BoxedValue = new MinMax(num, num2).ToString();
		}
	}

	public override string ToString()
	{
		return $"{Min}-{Max}";
	}
}
