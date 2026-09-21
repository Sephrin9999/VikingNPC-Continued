using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace Norsemen;

public class VikingTomb : MonoBehaviour
{
	[HarmonyPatch(typeof(TombStone), "GetHoverText")]
	private static class TombStone_GetHoverText
	{
		private static bool Prefix(TombStone __instance, ref string __result)
		{
            VikingTomb vikingTomb;
            if (!((Component)__instance).TryGetComponent<VikingTomb>(out vikingTomb))
            {
				return true;
			}
			__result = vikingTomb.GetHoverText();
			return false;
		}
	}

	[HarmonyPatch(typeof(TombStone), "Interact")]
	private static class TombStone_Interact
	{
		private static bool Prefix(TombStone __instance, Humanoid character, bool hold, bool alt, ref bool __result)
		{
            VikingTomb vikingTomb;
            if (!((Component)__instance).TryGetComponent<VikingTomb>(out vikingTomb))
            {
				return true;
			}
			__result = vikingTomb.Interact(character, hold, alt);
			return false;
		}
	}

	[HarmonyPatch(typeof(Player))]
	private static class Player_GetActionProgress
	{
        [HarmonyPatch("GetActionProgress")]
        [HarmonyPatch(new[]
			{
				typeof(string),
				typeof(float),
				typeof(Player.MinorActionData)
			}, new[]
			{
				ArgumentType.Out,
				ArgumentType.Out,
				ArgumentType.Out
			})]
        [HarmonyPatch(/*Could not decode attribute arguments.*/)]
		private static void Postfix(Player __instance, ref string name, ref float progress, ref Player.MinorActionData data)
		{
			if (!((Object)(object)__instance != (Object)(object)Player.m_localPlayer) && data == null && !((Object)(object)m_currentTomb == (Object)null) && m_currentTomb.IsReviving() && m_currentTomb.m_action != null)
			{
				data = m_currentTomb.m_action;
				name = m_currentTomb.m_action.m_progressText;
				progress = Mathf.Clamp01(m_currentTomb.m_action.m_time / m_currentTomb.m_action.m_duration);
			}
		}
	}

	public static readonly List<VikingTomb> instances;

	public ZNetView m_nview = null;

	public TombStone m_tombstone = null;

    public Container m_container = null;

    public bool m_revived;

	public float m_reviveDuration = 10f;

	public float m_cancelDistance = 10f;

	public float m_revertDistance = 5f;

	public float m_checkInterval = 0.5f;

	public static readonly EffectList reviveEffects;

	public Player m_reviver;

	public Player.MinorActionData m_action;

	public static VikingTomb m_currentTomb;

	public string m_vikingPrefab = "";

	public void Awake()
	{
		instances.Add(this);
		m_nview = ((Component)this).GetComponent<ZNetView>();
		m_tombstone = ((Component)this).GetComponent<TombStone>();
        m_container = ((Component)this).GetComponent<Container>();
    }

	public void Start()
	{
		if (m_nview.IsValid())
		{
			m_vikingPrefab = m_nview.GetZDO().GetString(VikingVars.vikingPrefab, "");
		}
	}

	public void OnDestroy()
	{
		instances.Remove(this);
	}

	public void FixedUpdate()
	{
		UpdateRevive();
	}

	public void UpdateRevive()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (m_action != null && !((Object)(object)m_reviver == (Object)null))
		{
			float num = Vector3.Distance(((Component)this).transform.position, ((Component)m_reviver).transform.position);
			if (num > m_cancelDistance)
			{
				m_action = null;
				m_currentTomb = null;
				m_reviver = null;
			}
			else if (num > m_revertDistance)
			{
                Player.MinorActionData action = m_action;
				action.m_time -= Time.fixedDeltaTime;
			}
			else
			{
				Player.MinorActionData action2 = m_action;
				action2.m_time += Time.fixedDeltaTime;
			}
		}
	}

	public string GetHoverText()
	{
		if (!m_nview.IsValid())
		{
			return string.Empty;
		}
		if (m_container.GetInventory().NrOfItems() == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(m_tombstone.m_text + " " + m_tombstone.GetOwnerName());
		bool flag = m_nview.GetZDO().GetBool(VikingVars.reviving, false);
		if (!flag)
		{
			stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_container_open");
            Piece.Requirement[] reviveRequirements = ConfigManager.GetReviveRequirements();
			if (reviveRequirements.Length != 0)
			{
				stringBuilder.Append("\n<color=yellow>Cost</color>: ");
				foreach (Piece.Requirement val in reviveRequirements)
				{
					stringBuilder.Append($" {val.m_resItem.m_itemData.m_shared.m_name} x{val.m_amount}");
				}
			}
		}
		string text = (flag ? "$norseman_cancel" : "$norseman_revive");
		stringBuilder.Append("\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] " + text);
		return Localization.instance.Localize(stringBuilder.ToString());
	}

	public void Setup(Viking viking)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		ZDO zDO = m_nview.GetZDO();
		if (zDO != null)
		{
			m_vikingPrefab = ((Object)viking).name.Replace("(Clone)", "");
			zDO.Set(VikingVars.vikingPrefab, m_vikingPrefab);
			zDO.Set(ZDOVars.s_tamedName, viking.GetText());
			zDO.Set(ZDOVars.s_level, ((Character)viking).GetLevel(), false);
			zDO.Set(ZDOVars.s_hairColor, viking.m_hairColor);
            zDO.Set(ZDOVars.s_modelIndex, ((Humanoid)viking).GetVisEquipment().GetModelIndex(), false);
            zDO.Set(ZDOVars.s_hairItem, ((Humanoid)viking).GetHair());
            zDO.Set(ZDOVars.s_hairColor, viking.m_hairColor);
            zDO.Set(ZDOVars.s_beardItem, ((Humanoid)viking).GetBeard());
            zDO.Set(ZDOVars.s_skinColor, viking.m_skinColor);
			zDO.Set(VikingVars.behaviour, (int)viking.m_vikingAI.m_behaviour, false);
			zDO.Set(VikingVars.patrol, (int)viking.m_vikingAI.m_moveType, false);
			zDO.Set(VikingVars.isElf, viking.m_isElf);
		}
	}

	public bool Revive()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_reviver == (Object)null)
		{
			return false;
		}
		string text = m_nview.GetZDO().GetString(VikingVars.vikingPrefab, "");
		GameObject prefab = ZNetScene.instance.GetPrefab(text);
		if ((Object)(object)prefab == (Object)null)
		{
			return false;
		}
		string text2 = m_nview.GetZDO().GetString(ZDOVars.s_tamedName, "");
		int num = m_nview.GetZDO().GetInt(ZDOVars.s_level, 0);
		Vector3 vec = m_nview.GetZDO().GetVec3(ZDOVars.s_hairColor, Vector3.zero);
		int num2 = m_nview.GetZDO().GetInt(ZDOVars.s_modelIndex, 0);
		string text3 = m_nview.GetZDO().GetString(ZDOVars.s_hairItem, "");
		string text4 = m_nview.GetZDO().GetString(ZDOVars.s_beardItem, "");
        ((Character)m_reviver).Message(
			MessageHud.MessageType.Center,
			$"Tomb hair: '{text3}' | beard: '{text4}'",
			0,
			null,
			false);
        Vector3 vec2 = m_nview.GetZDO().GetVec3(ZDOVars.s_skinColor, Vector3.one);
        ZPackage inventoryPackage = new ZPackage();
        m_container.GetInventory().Save(inventoryPackage);
        string text5 = inventoryPackage.GetBase64();
        int num3 = m_nview.GetZDO().GetInt(VikingVars.behaviour, 0);
		int num4 = m_nview.GetZDO().GetInt(VikingVars.patrol, 0);
		bool flag = m_nview.GetZDO().GetBool(VikingVars.isElf, false);
		int stableHashCode = StringExtensionMethods.GetStableHashCode(text);
		ZDO val = ZDOMan.instance.CreateNewZDO(((Component)this).transform.position, stableHashCode);
		val.SetPrefab(stableHashCode);
		val.Set(VikingVars.isSet, true);
		val.Set(ZDOVars.s_tamedName, text2);
		val.Set(ZDOVars.s_level, num, false);
		val.Set(ZDOVars.s_hairItem, text3);
		val.Set(ZDOVars.s_beardItem, text4);
		val.Set(ZDOVars.s_skinColor, vec2);
		val.Set(ZDOVars.s_hairColor, vec);
		val.Set(VikingVars.behaviour, num3, false);
		val.Set(VikingVars.patrol, num4, false);
		val.Set(ZDOVars.s_tamed, true);
		val.Set(ZDOVars.s_modelIndex, num2, false);
		val.Set(VikingVars.createTombStone, true);
		val.Set(ZDOVars.s_items, text5);
		val.Set(ZDOVars.s_addedDefaultItems, true);
		val.Set(VikingVars.lastLevelUpTime, ZNet.instance.GetTime().Ticks);
		val.Set(VikingVars.createTombStone, true);
		val.Set(VikingVars.isElf, flag);
		val.Set(ZDOVars.s_follow, m_reviver.GetPlayerName());
        AccessTools.Method(typeof(ZNetScene), "CreateObject", new[] { typeof(ZDO) })
			.Invoke(ZNetScene.instance, new object[] { val });
        m_container.GetInventory().RemoveAll();
		ConsumeReviveRequirements(m_reviver);
		m_revived = true;
		m_reviver = null;
		return true;
	}

	public bool IsReviving()
	{
		return ((MonoBehaviour)this).IsInvoking("CheckReviveProgress");
	}

	public void CheckReviveProgress()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_reviver == (Object)null || m_action == null)
		{
			NorsemenPlugin.LogDebug("Reviver is null or progress action is null, canceling");
			m_nview.GetZDO().Set(VikingVars.reviving, false);
			((MonoBehaviour)this).CancelInvoke("CheckReviveProgress");
		}
		else if (m_action.m_time >= m_action.m_duration)
		{
			((MonoBehaviour)this).CancelInvoke("CheckReviveProgress");
			((MonoBehaviour)this).Invoke("Revive", 1f);
			reviveEffects.Create(((Component)this).transform.position, Quaternion.identity, (Transform)null, 1f, -1);
			m_nview.GetZDO().Set(VikingVars.reviving, false);
			m_action = null;
			m_currentTomb = null;
		}
		else if (m_action.m_time <= 0f)
		{
			((MonoBehaviour)this).CancelInvoke("CheckReviveProgress");
			m_nview.GetZDO().Set(VikingVars.reviving, false);
			m_action = null;
			m_reviver = null;
			m_currentTomb = null;
		}
	}

	public bool HasReviveRequirements(Player character)
	{
        Piece.Requirement[] reviveRequirements = ConfigManager.GetReviveRequirements();
		if (reviveRequirements.Length != 0)
		{
			foreach (Piece.Requirement val in reviveRequirements)
			{
				if (!((Humanoid)character).GetInventory().HaveItem(val.m_resItem.m_itemData.m_shared.m_name, true))
				{
					((Character)character).Message(MessageHud.MessageType.Center, "Missing item: " + val.m_resItem.m_itemData.m_shared.m_name, 0, (Sprite)null);
					return false;
				}
				int num = ((Humanoid)character).GetInventory().CountItems(val.m_resItem.m_itemData.m_shared.m_name, -1, true);
				if (val.m_amount > num)
				{
					int num2 = val.m_amount - num;
					((Character)character).Message(MessageHud.MessageType.Center, $"Missing {val.m_resItem.m_itemData.m_shared.m_name} x{num2}", 0, (Sprite)null);
					return false;
				}
			}
		}
		return true;
	}

	public void ConsumeReviveRequirements(Player character)
	{
		Piece.Requirement[] reviveRequirements = ConfigManager.GetReviveRequirements();
		if (reviveRequirements.Length != 0)
		{
			foreach (Piece.Requirement val in reviveRequirements)
			{
				((Humanoid)character).GetInventory().RemoveItem(val.m_resItem.m_itemData.m_shared.m_name, val.m_amount, -1, true);
			}
		}
	}

	public bool Interact(Humanoid character, bool hold, bool alt)
	{
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Expected O, but got Unknown
		if (hold || m_container.GetInventory().NrOfItems() == 0)
		{
			return false;
		}
		Player val = (Player)(object)((character is Player) ? character : null);
		if (val == null)
		{
			return false;
		}
		bool flag = m_nview.GetZDO().GetBool(VikingVars.reviving, false);
		if (!m_nview.IsOwner() & flag)
		{
			((Character)character).Message(MessageHud.MessageType.Center, "$msg_inuse", 0, (Sprite)null);
			return false;
		}
		if (alt)
		{
			if ((Object)(object)m_currentTomb != (Object)null)
			{
				if ((Object)(object)m_reviver != (Object)(object)character)
				{
					((Character)character).Message(MessageHud.MessageType.Center, "$norseman_revive_inuse " + m_currentTomb.m_tombstone.GetOwnerName(), 0, (Sprite)null);
				}
				else
				{
					m_nview.GetZDO().Set(VikingVars.reviving, false);
					((MonoBehaviour)this).CancelInvoke("CheckReviveProgress");
					m_action = null;
					m_reviver = null;
					m_currentTomb = null;
				}
				return false;
			}
			if (m_revived)
			{
				return false;
			}
			if (!((MonoBehaviour)this).IsInvoking("CheckReviveProgress"))
			{
				if (!HasReviveRequirements(val))
				{
					return false;
				}
				m_nview.ClaimOwnership();
				m_reviver = val;
				m_currentTomb = this;
				m_action = new Player.MinorActionData
				{
					m_duration = m_reviveDuration,
					m_progressText = "$norseman_reviving " + m_tombstone.GetOwnerName(),
					m_type = (Player.MinorActionData.ActionType)2
				};
				((MonoBehaviour)this).InvokeRepeating("CheckReviveProgress", 1f, m_checkInterval);
				m_nview.GetZDO().Set(VikingVars.reviving, true);
			}
			else
			{
				m_nview.GetZDO().Set(VikingVars.reviving, false);
				((MonoBehaviour)this).CancelInvoke("CheckReviveProgress");
				m_action = null;
				m_reviver = null;
				m_currentTomb = null;
			}
			return true;
		}
		if (flag)
		{
			return false;
		}
		return m_container.Interact(character, false, false);
	}

	public static void RemoveAll()
	{
		int num = 0;
		foreach (VikingTomb instance in instances)
		{
			ZNetScene.instance.Destroy(((Component)instance).gameObject);
			num++;
		}
		NorsemenPlugin.LogDebug($"Removed {num} norsemen tombstones");
	}

	static VikingTomb()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		instances = new List<VikingTomb>();
		reviveEffects = new EffectList();
	}
}
