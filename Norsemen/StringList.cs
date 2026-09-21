using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace Norsemen;

public class StringList
{
	public readonly List<string> list;

	public static readonly ConfigurationManagerAttributes attributes = new ConfigurationManagerAttributes
	{
		CustomDrawer = Draw
	};

	public StringList(List<string> prefabs)
	{
		list = prefabs;
		if (list.Count == 0)
		{
			list.Add("");
		}
	}

	public StringList(params string[] prefabs)
	{
		list = prefabs.ToList();
		if (list.Count == 0)
		{
			list.Add("");
		}
	}

	public StringList(string config)
	{
		list = config.Split(',').ToList();
		if (list.Count == 0)
		{
			list.Add("");
		}
	}

	public override string ToString()
	{
		return string.Join(",", list);
	}

	public static void Draw(ConfigEntryBase cfg)
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Expected O, but got Unknown
		bool valueOrDefault = cfg.Description.Tags.Select((object a) => (a.GetType().Name == "ConfigurationManagerAttributes") ? ((bool?)a.GetType().GetField("ReadOnly")?.GetValue(a)) : ((bool?)null)).FirstOrDefault((bool? v) => v.HasValue) == true;
		bool flag = false;
		List<string> list = new List<string>();
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		foreach (string item2 in new StringList((string)cfg.BoxedValue).list)
		{
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			string item = item2;
			string text = GUILayout.TextField(item2, Array.Empty<GUILayoutOption>());
			if (text != item2 && !valueOrDefault)
			{
				flag = true;
				item = text;
			}
			if (GUILayout.Button("x", new GUIStyle(GUI.skin.button)
			{
				fixedWidth = 21f
			}, Array.Empty<GUILayoutOption>()) && !valueOrDefault)
			{
				flag = true;
			}
			else
			{
				list.Add(item);
			}
			if (GUILayout.Button("+", new GUIStyle(GUI.skin.button)
			{
				fixedWidth = 21f
			}, Array.Empty<GUILayoutOption>()) && !valueOrDefault)
			{
				list.Add("");
				flag = true;
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		if (flag)
		{
			cfg.BoxedValue = new StringList(list).ToString();
		}
	}
}
