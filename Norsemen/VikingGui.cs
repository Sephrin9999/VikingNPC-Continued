using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Norsemen;

public static class VikingGui
{
    private static readonly MethodInfo SetupDragItemMethod =
       AccessTools.Method(typeof(InventoryGui), "SetupDragItem");

    [HarmonyPatch(typeof(InventoryGui), "Awake")]
	private static class InventoryGui_Awake_Patch
	{
		private static void Prefix(InventoryGui __instance)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("norsemen_buttons");
			RectTransform val2 = val.AddComponent<RectTransform>();
			((Transform)val2).SetParent((Transform)(object)__instance.m_container, false);
			val2.anchorMin = new Vector2(0f, 0f);
			val2.anchorMax = new Vector2(0f, 0f);
			val2.pivot = new Vector2(0f, 1f);
			val2.anchoredPosition = Vector2.zero;
			val.AddComponent<NorseGui>();
		}

		private static void Postfix(InventoryGui __instance)
		{
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			GameObject gameObject = ((Component)((Component)__instance).transform.Find("root/Player/Armor")).gameObject;
			RectTransform container = __instance.m_container;
			GameObject tooltipPrefab = __instance.ContainerGrid.m_elementPrefab.GetComponent<UITooltip>().m_tooltipPrefab;
			armor = new Display(gameObject, container, tooltipPrefab, "NorsemanArmor");
			health = new Display(gameObject, container, tooltipPrefab, "NorsemanHealth");
			RectTransform rect = health.rect;
			Vector3 localPosition = ((Transform)rect).localPosition;
			Rect rect2 = health.rect.rect;
            float height = rect2.height;
            rect2 = ((Graphic)health.icon).rectTransform.rect;
            ((Transform)rect).localPosition = localPosition - new Vector3(0f, height + rect2.height, 0f);
            health.icon.sprite = __instance.m_minStationLevelIcon.sprite;
			armor.Hide();
			health.Hide();
			if (!((Object)(object)NorseGui.instance == (Object)null))
			{
				NorseGui.instance.behaviour.SetupGlow(__instance.m_repairButtonGlow);
				NorseGui.instance.patrol.SetupGlow(__instance.m_repairButtonGlow);
				NorseGui.instance.access.SetupGlow(__instance.m_repairButtonGlow);
			}
		}
	}

	public class Display
	{
		public GameObject obj;

		public RectTransform rect;

		public Image bkg;

		public Image icon;

		public TMP_Text text;

		public UITooltip tooltip;

		public Display(GameObject source, RectTransform parent, GameObject tooltipPrefab, string name)
		{
			obj = Object.Instantiate<GameObject>(source, (Transform)(object)parent);
			((Object)obj).name = name;
			rect = obj.GetComponent<RectTransform>();
			bkg = ((Component)obj.transform.Find("bkg")).GetComponent<Image>();
			icon = ((Component)obj.transform.Find("armor_icon")).GetComponent<Image>();
			text = obj.GetComponentInChildren<TMP_Text>();
			tooltip = obj.AddComponent<UITooltip>();
			tooltip.m_tooltipPrefab = tooltipPrefab;
			obj.transform.SetSiblingIndex(2);
		}

		public void Show(string txt)
		{
			obj.SetActive(true);
			if (!((Object)(object)text == (Object)null))
			{
				text.text = txt;
			}
		}

		public void Hide()
		{
			obj.SetActive(false);
		}
	}

	[HarmonyPatch(typeof(Player), "SetLocalPlayer")]
	private static class Player_SetLocalPlayer
	{
		private static void Postfix()
		{
			m_currentViking = null;
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "OnSelectedItem")]
	private static class InventoryGui_OnSelectedItem
	{
		private static bool Prefix(InventoryGui __instance, InventoryGrid grid, ItemDrop.ItemData item, InventoryGrid.Modifier mod, GameObject ___m_dragGo)
		{
            //IL_001d: Unknown result type (might be due to invalid IL or missing references)
            //IL_001f: Invalid comparison between Unknown and I4
            //IL_00cb: Unknown result type (might be due to invalid IL or missing references)
            //IL_00d0: Unknown result type (might be due to invalid IL or missing references)
            if (Player.m_localPlayer == null
				|| item == null
				|| item.m_shared.m_questItem
				|| mod != InventoryGrid.Modifier.Move
                || ___m_dragGo != null
                || __instance.IsContainerOpen()
				|| m_currentViking == null)
            {
				return true;
			}
			if (((Character)Player.m_localPlayer).IsTeleporting())
			{
				return false;
			}
			((Humanoid)Player.m_localPlayer).RemoveEquipAction(item);
			((Humanoid)Player.m_localPlayer).UnequipItem(item, true);
			Inventory inventory = ((Humanoid)Player.m_localPlayer).GetInventory();
			Inventory inventory2 = ((Humanoid)m_currentViking).GetInventory();
			if (grid.GetInventory() == inventory2)
			{
				inventory.MoveItemToThis(inventory2, item);
			}
			else
			{
				inventory2.MoveItemToThis(inventory, item);
			}
			__instance.m_moveItemEffects.Create(((Component)__instance).transform.position, Quaternion.identity, (Transform)null, 1f, -1);
			return false;
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "UpdateContainer")]
	private static class InventoryGUI_UpdateContainer
	{
		private static bool Prefix(
			InventoryGui __instance, 
			Player player, 
			Animator ___m_animator, 
			ItemDrop.ItemData ___m_dragItem, 
			ref bool ___m_firstContainerUpdate, 
			ref float ___m_containerHoldTime, 
			ref int ___m_containerHoldState)
		{
            //IL_00d8: Unknown result type (might be due to invalid IL or missing references)
            //IL_00e3: Unknown result type (might be due to invalid IL or missing references)
            
            if (!___m_animator.GetBool(visible) || m_currentViking == null)
            {
                NorseGui.instance?.Hide();
                return true;
            }
            m_currentViking.SetInUse(player);
			((Component)__instance.m_container).gameObject.SetActive(true);
            __instance.ContainerGrid.UpdateInventory(((Humanoid)m_currentViking).GetInventory(), (Player)null, ___m_dragItem);
			__instance.m_containerName.text = m_currentViking.GetName();
            if (___m_firstContainerUpdate)
            {
                __instance.ContainerGrid.ResetView();
                ___m_firstContainerUpdate = false;
                ___m_containerHoldTime = 0f;
                ___m_containerHoldState = 0;
            }
            float num = Vector3.Distance(((Component)m_currentViking).transform.position, ((Component)player).transform.position);
			if (num > __instance.m_autoCloseDistance)
			{
				CloseVikingInventory();
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "OnTakeAll")]
	private static class InventoryGui_OnTakeAll
	{
		private static bool Prefix(InventoryGui __instance)
		{
            if (((Character)Player.m_localPlayer).IsTeleporting()
				|| __instance.IsContainerOpen()
				|| m_currentViking == null)
            {
				return true;
			}
            SetupDragItemMethod.Invoke(
				__instance,
				new object[] { null, null, 1 });
            Inventory inventory = ((Humanoid)m_currentViking).GetInventory();
			((Humanoid)Player.m_localPlayer).GetInventory().MoveAll(inventory);
			return false;
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "OnStackAll")]
	private static class InventoryGui_OnStackAll
	{
		private static bool Prefix(InventoryGui __instance)
		{
            if (((Character)Player.m_localPlayer).IsTeleporting()
                || __instance.IsContainerOpen()
                || m_currentViking == null)
            {
				return true;
			}
            SetupDragItemMethod.Invoke(
				__instance,
				new object[] { null, null, 1 });
            ((Humanoid)m_currentViking).GetInventory().StackAll(((Humanoid)Player.m_localPlayer).GetInventory(), false);
			return false;
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "Hide")]
	private static class InventoryGui_Hide
	{
		private static void Postfix()
		{
			CloseVikingInventory();
			armor.Hide();
			health.Hide();
			NorseGui.instance?.Hide();
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "IsContainerOpen")]
	private static class InventoryGui_IsContainerOpen
	{
		private static void Postfix(ref bool __result)
		{
			__result |= (Object)(object)m_currentViking != (Object)null;
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "UpdateContainerWeight")]
	private static class InventoryGui_UpdateContainerWeight
	{
		private static void Postfix(InventoryGui __instance)
		{
            if (!__instance.IsContainerOpen() && m_currentViking != null)
            {
				int num = Mathf.CeilToInt(((Humanoid)m_currentViking).GetInventory().GetTotalWeight());
				__instance.m_containerWeight.text = num.ToString();
			}
		}
	}

	public static Display armor = null;

	public static Display health = null;

	private static readonly int visible = Animator.StringToHash("visible");

    public static Viking m_currentViking;

	public static void Show(this InventoryGui gui, Viking viking)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		m_currentViking = viking;
		gui.Show((Container)null, 1);
		string name = viking.GetName();
		string tooltip = viking.GetTooltip();
		armor.Show(viking.GetArmor().ToString("0"));
		armor.tooltip.Set(name, tooltip, (RectTransform)null, default(Vector2));
		health.Show($"{((Character)viking).GetHealth():0}/{((Character)viking).GetMaxHealth():0}");
		health.tooltip.Set(name, tooltip, (RectTransform)null, default(Vector2));
		NorseGui.instance?.Show();
	}

	public static void CloseVikingInventory()
	{
		m_currentViking?.SetInUse(null);
		m_currentViking = null;
	}
}
