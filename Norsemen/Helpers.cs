using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Norsemen;

public static class Helpers
{
	internal static ZNetScene _ZNetScene;

	internal static ObjectDB _ObjectDB;

	internal static GameObject GetPrefab(string prefabName)
	{
		if ((Object)(object)ZNetScene.instance != (Object)null)
		{
			return ZNetScene.instance.GetPrefab(prefabName);
		}
		if ((Object)(object)_ZNetScene == (Object)null)
		{
			return null;
		}
		GameObject val = _ZNetScene.m_prefabs.Find((GameObject prefab) => ((Object)prefab).name == prefabName);
		if ((Object)(object)val != (Object)null)
		{
			return val;
		}
		GameObject value;
		return CloneManager.clones.TryGetValue(prefabName, out value) ? value : val;
	}

	public static void Add<T>(this List<T> list, params T[] values)
	{
		list.AddRange(values);
	}

	public static void Remove<T>(this GameObject prefab) where T : Component
	{
		T val = default(T);
		if (prefab.TryGetComponent<T>(out val))
		{
			Object.Destroy((Object)(object)val);
		}
	}

	public static void AddRange<T, V>(this Dictionary<T, V> dict, Dictionary<T, V> other)
	{
		foreach (KeyValuePair<T, V> item in other)
		{
			dict[item.Key] = item.Value;
		}
	}

	public static string ToCamelCase(this string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return str;
		}
		return char.ToLowerInvariant(str[0]) + str.Substring(1);
	}

    public static void ClearEquipment(this VisEquipment visEq)
    {
        visEq.SetLeftItem(0, 0, 0);
        visEq.SetRightItem(0, 0);
        visEq.SetChestItem(0);
        visEq.SetLegItem(0);
        visEq.SetHelmetItem(0);
        visEq.SetShoulderItem(0, 0, 0);
        visEq.SetUtilityItem(0);
        visEq.SetTrinketItem("");
    }

    public static void CopyFieldsFrom<T, V>(this T target, V source) where T : Humanoid where V : Humanoid
	{
		Dictionary<string, FieldInfo> dictionary = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToDictionary((FieldInfo f) => f.Name);
		FieldInfo[] fields = typeof(V).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (dictionary.TryGetValue(fieldInfo.Name, out var value) && value.FieldType.IsAssignableFrom(fieldInfo.FieldType))
			{
				value.SetValue(target, fieldInfo.GetValue(source));
			}
		}
	}

	public static void Copy<T>(this T target, T source)
	{
		FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			object value = fieldInfo.GetValue(source);
			if (value != null)
			{
				fieldInfo.SetValue(target, value);
			}
		}
	}

    public static bool IsItemBetter(this ItemDrop.ItemData item, ItemDrop.ItemData other)
    {
        //IL_0007: Unknown result type (might be due to invalid IL or missing references)
        //IL_000c: Unknown result type (might be due to invalid IL or missing references)
        //IL_000d: Unknown result type (might be due to invalid IL or missing references)
        //IL_000e: Unknown result type (might be due to invalid IL or missing references)
        //IL_000f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0011: Invalid comparison between Unknown and I4
        //IL_0033: Unknown result type (might be due to invalid IL or missing references)
        //IL_0039: Invalid comparison between Unknown and I4
        //IL_0015: Unknown result type (might be due to invalid IL or missing references)
        //IL_0018: Invalid comparison between Unknown and I4
        //IL_0061: Unknown result type (might be due to invalid IL or missing references)
        //IL_0066: Unknown result type (might be due to invalid IL or missing references)
        //IL_0068: Unknown result type (might be due to invalid IL or missing references)
        //IL_006a: Unknown result type (might be due to invalid IL or missing references)
        //IL_006b: Unknown result type (might be due to invalid IL or missing references)
        //IL_006d: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a7: Expected I4, but got Unknown
        //IL_0022: Unknown result type (might be due to invalid IL or missing references)
        //IL_0029: Invalid comparison between Unknown and I4
        //IL_00bc: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c1: Unknown result type (might be due to invalid IL or missing references)
        //IL_00cb: Unknown result type (might be due to invalid IL or missing references)
        //IL_00d0: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a9: Unknown result type (might be due to invalid IL or missing references)
        //IL_00ac: Invalid comparison between Unknown and I4
        //IL_00b0: Unknown result type (might be due to invalid IL or missing references)
        //IL_00b3: Invalid comparison between Unknown and I4
        Skills.SkillType skillType = item.m_shared.m_skillType;
        Skills.SkillType val = skillType;
        if ((int)val != 7)
		{
			if ((int)val == 12 && (int)other.m_shared.m_skillType == 12)
			{
				goto IL_003d;
			}
		}
		else if ((int)other.m_shared.m_skillType == 7)
		{
			goto IL_003d;
		}
        ItemDrop.ItemData.ItemType itemType = item.m_shared.m_itemType;
        ItemDrop.ItemData.ItemType val2 = itemType;
        switch ((int)val2 - 3)
        {
		default:
			if ((int)val2 != 22)
			{
				if ((int)val2 != 24)
				{
					break;
				}
				return item.m_shared.m_maxAdrenaline < other.m_shared.m_maxAdrenaline;
			}
			goto case 0;
		case 12:
			return false;
		case 0:
		case 1:
		case 6:
		case 9:
		case 11:
		{
                    HitData.DamageTypes damage = item.GetDamage();
                    float totalDamage = damage.GetTotalDamage();

                    damage = other.GetDamage();
                    return totalDamage < damage.GetTotalDamage();
                }
		case 2:
			return item.m_shared.m_blockPower < other.m_shared.m_blockPower;
		case 3:
		case 4:
		case 5:
		case 7:
		case 8:
		case 10:
			break;
		}
		return item.GetArmor() < other.GetArmor();
		IL_003d:
		return item.m_shared.m_toolTier < other.m_shared.m_toolTier;
	}

    public static bool IsOreVein(this List<DropTable.DropData> drops)
    {
		return drops.Exists(delegate (DropTable.DropData x)
        {
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return !x.m_item.GetComponent<ItemDrop>().m_itemData.m_shared.m_teleportable;
		});
	}
}
