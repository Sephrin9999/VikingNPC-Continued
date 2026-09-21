using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace Norsemen;

public class EnvList
{
	public static readonly ConfigurationManagerAttributes attributes = new ConfigurationManagerAttributes
	{
		CustomDrawer = DrawTable
	};

	public readonly List<string> Envs;

	public EnvList(List<string> envs)
	{
		Envs = envs;
		if (Envs.Count == 0)
		{
			Envs.Add("");
		}
	}

	public EnvList(string envs)
	{
		Envs = envs.Split(',').ToList();
	}

	public EnvList()
	{
		Envs = new List<string> { "" };
	}

	public void Add(string env)
	{
		Envs.Add(env);
	}

	public List<string> GetValidatedList()
	{
        return EnvMan.instance != null
			? Envs.Where(env => EnvMan.instance.m_environments.Any(e => e.m_name == env)).ToList()
			: Envs.Where(env => !Utility.IsNullOrWhiteSpace(env)).ToList();
    }

	public override string ToString()
	{
		return string.Join(",", Envs);
	}

	public static void DrawTable(ConfigEntryBase cfg)
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected O, but got Unknown
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		bool valueOrDefault = cfg.Description.Tags.Select((object a) => (a.GetType().Name == "ConfigurationManagerAttributes") ? ((bool?)a.GetType().GetField("ReadOnly")?.GetValue(a)) : ((bool?)null)).FirstOrDefault((bool? v) => v.HasValue) == true;
		List<string> list = new List<string>();
		bool flag = false;
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		foreach (string env in new EnvList((string)cfg.BoxedValue).Envs)
		{
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			string text = GUILayout.TextField(env, new GUIStyle(GUI.skin.textField), Array.Empty<GUILayoutOption>());
			string item = (valueOrDefault ? env : text);
			bool flag2 = GUILayout.Button("x", new GUIStyle(GUI.skin.button)
			{
				fixedWidth = 21f
			}, Array.Empty<GUILayoutOption>());
			bool flag3 = GUILayout.Button("+", new GUIStyle(GUI.skin.button)
			{
				fixedWidth = 21f
			}, Array.Empty<GUILayoutOption>());
			GUILayout.EndHorizontal();
			if (flag2 && !valueOrDefault)
			{
				flag = true;
			}
			else
			{
				list.Add(item);
			}
			if (flag3 && !valueOrDefault)
			{
				flag = true;
				list.Add("");
			}
		}
		GUILayout.EndVertical();
		if (flag)
		{
			cfg.BoxedValue = new EnvList(list).ToString();
		}
	}
}
