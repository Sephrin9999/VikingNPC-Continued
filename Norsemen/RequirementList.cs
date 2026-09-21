using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace Norsemen;

public class RequirementList
{
	public struct Requirement
	{
		public string item;

		public int amount;

		public override string ToString()
		{
			return $"{item}:{amount}";
		}
	}

	public readonly List<Requirement> requirements;

	public static readonly ConfigurationManagerAttributes attributes = new ConfigurationManagerAttributes
	{
		CustomDrawer = Draw
	};

	public RequirementList()
	{
		requirements = new List<Requirement>();
	}

	public RequirementList(List<Requirement> reqs)
	{
		requirements = reqs;
	}

	public RequirementList(string cfg)
	{
		requirements = new List<Requirement>();
		if (string.IsNullOrEmpty(cfg))
		{
			return;
		}
		string[] array = cfg.Split(';');
		foreach (string text in array)
		{
			string[] array2 = text.Split(':');
			if (array2.Length >= 2)
			{
				string item = array2[0].Trim();
				int amount = ((!int.TryParse(array2[1].Trim(), out var result)) ? 1 : result);
				Add(item, amount);
			}
		}
	}

	public void Add(string item, int amount)
	{
		requirements.Add(new Requirement
		{
			item = item,
			amount = amount
		});
	}

	public override string ToString()
	{
		return string.Join(";", requirements);
	}

    public Piece.Requirement[] ToPieceRequirements()
    {
        return requirements
            .Select(r =>
            {
                Piece.Requirement val = new Piece.Requirement();
                GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(r.item);

                val.m_resItem = itemPrefab != null
                    ? itemPrefab.GetComponent<ItemDrop>()
                    : null;

                val.m_amount = r.amount;
                return val;
            })
            .Where(r => r.m_resItem != null)
            .ToArray();
    }

    public static void Draw(ConfigEntryBase cfg)
	{
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Expected O, but got Unknown
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Expected O, but got Unknown
		bool valueOrDefault = cfg.Description.Tags.Select((object a) => (a.GetType().Name == "ConfigurationManagerAttributes") ? ((bool?)a.GetType().GetField("ReadOnly")?.GetValue(a)) : ((bool?)null)).FirstOrDefault((bool? v) => v.HasValue) == true;
		bool flag = false;
		List<Requirement> list = new List<Requirement>();
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		List<Requirement> list2 = new RequirementList((string)cfg.BoxedValue).requirements;
		if (list2.Count == 0)
		{
			list2.Add(new Requirement
			{
				item = "",
				amount = 0
			});
		}
		for (int num = 0; num < list2.Count; num++)
		{
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			Requirement requirement = list2[num];
			string text = requirement.item;
			int num2 = requirement.amount;
			string text2 = GUILayout.TextField(text, Array.Empty<GUILayoutOption>());
			if (text2 != requirement.item && !valueOrDefault)
			{
				flag = true;
				text = text2;
			}
			string s = GUILayout.TextField(num2.ToString(), Array.Empty<GUILayoutOption>());
			int num3 = (int.TryParse(s, out var result) ? result : 0);
			if (num3 != num2 && !valueOrDefault)
			{
				flag = true;
				num2 = result;
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
				list.Add(new Requirement
				{
					item = text,
					amount = num2
				});
			}
			if (GUILayout.Button("+", new GUIStyle(GUI.skin.button)
			{
				fixedWidth = 21f
			}, Array.Empty<GUILayoutOption>()) && !valueOrDefault)
			{
				list.Add(new Requirement
				{
					item = "",
					amount = 0
				});
				flag = true;
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		if (flag)
		{
			cfg.BoxedValue = new RequirementList(list).ToString();
		}
	}
}
