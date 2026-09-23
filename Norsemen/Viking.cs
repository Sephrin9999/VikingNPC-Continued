using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static MessageHud;

namespace Norsemen;

public class Viking : Humanoid, Interactable, TextReceiver

	
{
    [HarmonyPatch(typeof(TreeBase), nameof(TreeBase.Damage))]
    private static class TreeBase_Damage_Patch
    {
        private static void Prefix(HitData hit)
        {
            if (hit.GetAttacker() is Viking viking && viking.IsWorking())
            {
                hit.m_damage.Modify(0f);
            }
        }
    }

    [HarmonyPatch(typeof(MineRock), nameof(MineRock.Damage))]
    private static class MineRock_Damage_Patch
    {
        private static void Prefix(HitData hit)
        {
            if (hit.GetAttacker() is Viking viking && viking.IsWorking())
            {
                hit.m_damage.Modify(0f);
            }
        }
    }

    [HarmonyPatch(typeof(MineRock5), nameof(MineRock5.Damage))]
    private static class MineRock5_Damage_Patch
    {
        private static void Prefix(HitData hit)
        {
            if (hit.GetAttacker() is Viking viking && viking.IsWorking())
            {
                hit.m_damage.Modify(0f);
            }
        }
    }

    [HarmonyPatch(typeof(Destructible), nameof(Destructible.Damage))]
    private static class Destructible_Damage_Patch
    {
        private static void Prefix(HitData hit)
        {
            if (hit.GetAttacker() is Viking viking && viking.IsWorking())
            {
                hit.m_damage.Modify(0f);
            }
        }
    }

    [HarmonyPatch(typeof(Attack), "HaveAmmo")]
	private static class Attack_HaveAmmo
	{
		private static void Postfix(Humanoid character, ref bool __result)
		{
			if (!__result && character is Viking)
			{
				__result = true;
			}
		}
	}

	[HarmonyPatch(typeof(Attack), "FindAmmo")]
	private static class Attack_FindAmmo_Patch
	{
		private static void Postfix(Humanoid character, ItemDrop.ItemData weapon, ref ItemDrop.ItemData __result)
		{
			if (__result != null || !(character is Viking viking) || string.IsNullOrEmpty(weapon.m_shared.m_ammoType))
			{
				return;
			}
			string ammoType = weapon.m_shared.m_ammoType;
			string text = ammoType;
			if (!(text == "$ammo_arrows"))
			{
				if (text == "$ammo_bolts")
				{
                    ItemDrop.ItemData val = ((Humanoid)viking).GetInventory().AddItem("BoltBone", 10, 1, 0, 0L, "", false);
					if (val != null)
					{
						__result = val;
					}
				}
			}
			else
			{
                ItemDrop.ItemData val2 = ((Humanoid)viking).GetInventory().AddItem("ArrowWood", 10, 1, 0, 0L, "", false);
				if (val2 != null)
				{
					__result = val2;
				}
			}
		}
	}

	[Serializable]
	public class ConditionalItemSet
	{
		public GameObject[] m_items = Array.Empty<GameObject>();

		public string m_requiredDefeatKey = "";

		public float m_weight = 1f;

		public bool HasKey()
		{
			return string.IsNullOrEmpty(m_requiredDefeatKey) || ZoneSystem.instance.GetGlobalKey(m_requiredDefeatKey);
		}
	}

	[Serializable]
	public class ConditionalRandomItem
	{
		public GameObject m_prefab;

		public string m_requiredDefeatKey = "";

		public float m_chance = 0.5f;

		public int m_min = 1;

		public int m_max = 1;

		public bool HasKey()
		{
			return string.IsNullOrEmpty(m_requiredDefeatKey) || ZoneSystem.instance.GetGlobalKey(m_requiredDefeatKey);
		}
	}

	[Serializable]
	public class ConditionalRandomWeapon
	{
		public GameObject m_prefab;

		public string m_requiredDefeatKey = "";

		public float m_weight;

		public bool HasKey()
		{
			return string.IsNullOrEmpty(m_requiredDefeatKey) || ZoneSystem.instance.GetGlobalKey(m_requiredDefeatKey);
		}
	}

	[HarmonyPatch(typeof(ObjectDB), "Awake")]
	private static class ObjectDB_Awake
	{
		private static void Postfix(ObjectDB __instance)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Invalid comparison between Unknown and I4
			ItemDrop val = default(ItemDrop);
			foreach (GameObject item in __instance.m_items)
			{
                if (item.TryGetComponent<ItemDrop>(out val)
					&& val.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable
					&& val.m_itemData.m_shared.m_consumeStatusEffect == null)
                {
					consumableItems.Add(val);
				}
			}
		}
	}

	[HarmonyPatch(typeof(SE_Harpooned), "UpdateStatusEffect")]
	private static class SE_Harpooned_UpdateStatusEffect_Patch
	{
        private static bool Prefix(
			SE_Harpooned __instance,
			float dt,
			Character ___m_attacker,
			float ___m_baseDistance,
			LineConnect ___m_line,
			ref float ___m_drainStaminaTimer,
			ref bool ___m_broken, 
			ref float ___m_time,
			ref float ___m_msgTimer)
        {
            if (((StatusEffect)__instance).m_character == null
				|| ___m_attacker == null
				|| !(___m_attacker is Viking))
            {
				return true;
			}
			BaseUpdateSEC(__instance, dt, ref ___m_time, ref ___m_msgTimer);
            UpdateSE_Harpooned(
				__instance,
				dt,
				___m_attacker,
				___m_baseDistance,
				___m_line,
				ref ___m_drainStaminaTimer,
				ref ___m_broken);
            return false;
		}

        private static void UpdateSE_Harpooned(
			SE_Harpooned __instance,
			float dt,
			Character ___m_attacker,
			float ___m_baseDistance,
			LineConnect ___m_line,
			ref float ___m_drainStaminaTimer,
			ref bool ___m_broken)
        {
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			Rigidbody val = default(Rigidbody);
			if (!((Component)((StatusEffect)__instance).m_character).TryGetComponent<Rigidbody>(out val))
			{
				return;
			}
			float num = Vector3.Distance(((Component)___m_attacker).transform.position, ((Component)((StatusEffect)__instance).m_character).transform.position);
			if (!((StatusEffect)__instance).m_character.IsAttached())
			{
				float num2 = Utils.Pull(val, ((Component)___m_attacker).transform.position, m_pullTo, 25f, __instance.m_pullForce, __instance.m_smoothDistance, true, true, __instance.m_forcePower);
				___m_drainStaminaTimer += dt;
				if (___m_drainStaminaTimer > __instance.m_staminaDrainInterval && (double)num2 > 0.0)
				{
                    ___m_drainStaminaTimer = 0f;
                    ___m_attacker.UseStamina(__instance.m_staminaDrain * num2 * ((StatusEffect)__instance).m_character.GetMass());
				}
			}
            if (___m_line != null)
            {
                ___m_line.SetSlack((1f - Utils.LerpStep(___m_baseDistance / 2f, ___m_baseDistance, num)) * __instance.m_maxLineSlack);
			}
			if (num - ___m_baseDistance > __instance.m_breakDistance)
			{
				___m_broken = true;
			}
		}

        private static void BaseUpdateSEC(
			SE_Harpooned __instance,
			float dt,
			ref float ___m_time,
			ref float ___m_msgTimer)
        {
            ___m_time += dt;

            if (__instance.m_repeatInterval > 0f &&
                !string.IsNullOrEmpty(__instance.m_repeatMessage))
            {
                ___m_msgTimer += dt;

                if (___m_msgTimer > __instance.m_repeatInterval)
                {
                    ___m_msgTimer = 0f;
                    __instance.m_character.Message(
                        __instance.m_repeatMessageType,
                        __instance.m_repeatMessage);
                }
            }
        }
    }

	[HarmonyPatch(typeof(SE_Puke), "Setup")]
	private static class SE_Puke_Setup_Patch
	{
		private static void Postfix(SE_Puke __instance)
		{
			if (((StatusEffect)__instance).m_character is Viking viking)
			{
				viking.m_queuedTexts.Clear();
				viking.QueueSay(TalkManager.GetTalk(TalkManager.TalkType.Puke), "", "emote_despair", pukeFX);
			}
		}
	}

	[HarmonyPatch(typeof(SE_Puke), "UpdateStatusEffect")]
	private static class SE_Puke_Update_Patch
	{
		private static bool Prefix(SE_Puke __instance, float dt, ref float ___m_time, ref float ___m_removeTimer, ref float ___m_tickTimer)
		{
			if (!(((StatusEffect)__instance).m_character is Viking))
			{
				return true;
			}
            ___m_time += dt;
            UpdateStatus(__instance, dt, ref ___m_removeTimer);
            UpdateStats(__instance, dt, ref ___m_tickTimer);
            return false;
		}

        private static void UpdateStatus(
			SE_Puke __instance,
			float dt,
			ref float ___m_removeTimer)
		{
			___m_removeTimer += dt;

			if (___m_removeTimer > __instance.m_removeInterval)
			{
				___m_removeTimer = 0f;
			}
		}

        private static void UpdateStats(
			SE_Puke __instance,
			float dt,
			ref float ___m_tickTimer)
        {
            //IL_0034: Unknown result type (might be due to invalid IL or missing references)
            //IL_0039: Unknown result type (might be due to invalid IL or missing references)
            //IL_0049: Unknown result type (might be due to invalid IL or missing references)
            //IL_0050: Unknown result type (might be due to invalid IL or missing references)
            //IL_0055: Unknown result type (might be due to invalid IL or missing references)
            //IL_005a: Unknown result type (might be due to invalid IL or missing references)
            //IL_005d: Unknown result type (might be due to invalid IL or missing references)
            //IL_0067: Expected O, but got Unknown
            ___m_tickTimer = ___m_tickTimer + dt;
			if (___m_tickTimer >= ((SE_Stats)__instance).m_tickInterval)
			{
                ___m_tickTimer = 0f;
				((StatusEffect)__instance).m_character.Damage(new HitData
				{
					m_damage = 
					{
						m_damage = 1f
					},
					m_point = ((StatusEffect)__instance).m_character.GetTopPoint(),
					m_hitType = (HitData.HitType)14
				});
			}
		}
	}

	public class QueuedSay
	{
		public string text = "";

		public string trigger = "";

		public EffectListRef m_effect;
	}

	[HarmonyPatch(typeof(Teleport), "Interact")]
	private static class Teleport_Interact
	{
		private static void Prefix()
		{
			IsDungeonTeleport = true;
		}

		private static void Postfix()
		{
			IsDungeonTeleport = false;
		}
	}

	[HarmonyPatch(typeof(Player), "TeleportTo")]
	private static class Player_Teleport_To
	{
		private static void Postfix(Player __instance, Vector3 pos, Quaternion rot, bool __result)
		{
            //IL_0021: Unknown result type (might be due to invalid IL or missing references)
            //IL_00aa: Unknown result type (might be due to invalid IL or missing references)
            //IL_00ab: Unknown result type (might be due to invalid IL or missing references)
            if (!__result || __instance != Player.m_localPlayer)
            {
				return;
			}
			List<Viking> vikings = GetVikings(((Component)__instance).transform.position, 20f);
			for (int i = 0; i < vikings.Count; i++)
			{
				Viking viking = vikings[i];
				GameObject followTarget = viking.GetFollowTarget();
                if (followTarget != null && followTarget == __instance.gameObject)
                {
                    if (!((Humanoid)viking).IsTeleportable(false))
                    {
						string format = Localization.instance.Localize("$norsemen_cannot_tp");
						string text = string.Format(format, viking.GetText());
                        ((Character)__instance).Message(
							MessageHud.MessageType.Center,
							text,
							0,
							null,
							false);
                    }
					else
					{
						((Character)viking).TeleportTo(pos, rot, true);
					}
				}
			}
		}
	}

	[HarmonyPatch(typeof(Character), "RPC_SetTamed")]
	private static class Character_RPC_SetTamed
	{
        private static void Prefix(
			Character __instance,
			bool tamed,
			bool ___m_tamed,
			ZNetView ___m_nview)
        {
            if (NorsemenPlugin.RemoveEquipment &&
                __instance is Viking viking &&
                ___m_tamed != tamed &&
                ___m_nview.IsOwner() &&
                tamed)
            {
                ((Humanoid)viking).UnequipAllItems();
                Inventory inventory = ((Humanoid)viking).GetInventory();

                foreach (ItemDrop.ItemData item in inventory.GetAllItems().ToList())
                {
                    if (item.IsEquipable())
                    {
                        inventory.RemoveItem(item, item.m_stack);
                    }
                }
            }
		


        }
    }

	public float armor;

	public ItemDrop.ItemData m_weaponLoaded;
    private bool m_isWorking;
    public bool IsWorking()
    {
        return m_isWorking;
    }
    private Attack PreviousAttack
    {
        get => (Attack)AccessTools.Field(typeof(Humanoid), "m_previousAttack").GetValue(this);
        set => AccessTools.Field(typeof(Humanoid), "m_previousAttack").SetValue(this, value);
    }

    private float TimeSinceLastAttack
    {
        get => (float)AccessTools.Field(typeof(Humanoid), "m_timeSinceLastAttack").GetValue(this);
    }
	    private static void SetAttackDrawPercentage(Attack attack, float value)
    {
        AccessTools.Field(typeof(Attack), "m_attackDrawPercentage").SetValue(attack, value);
    }

    private static void SetHairEquipped(VisEquipment visEquipment, int itemHash)
    {
        visEquipment.SetHairItem(itemHash);
    }

    private static void SetBeardEquipped(VisEquipment visEquipment, int itemHash)
    {
        AccessTools.Field(typeof(VisEquipment), "m_beardItem")
            .SetValue(visEquipment, itemHash);
    }
	    private int CharacterLevel
    {
        get => (int)AccessTools.Field(typeof(Character), "m_level").GetValue(this);
    }

    private void GiveVikingDefaultItem(GameObject prefab)
    {
        ItemDrop.ItemData itemData = PickupPrefab(prefab, 0, false);
        if (itemData != null && !itemData.IsWeapon())
        {
            EquipItem(itemData, false);
        }
    }

    private static float m_lastTalkTime;
    public EffectList m_dodgeEffects;

	public Vector3 m_hairColor;

	public Vector3 m_skinColor;

	public bool m_isElf;

	public GameObject m_elfEars;

	public Material[] m_elfEarMats;

	public ItemDrop.ItemData m_pickaxe;

	public ItemDrop.ItemData m_axe;

	public ItemDrop.ItemData m_fishingRod;

	public ConditionalItemSet[] m_conditionalItemSets;

	public ConditionalRandomItem[] m_conditionalRandomItems;

	public ConditionalRandomWeapon[] m_conditionalRandomWeapons;

	public static readonly List<ItemDrop> consumableItems = new List<ItemDrop>();

	public ItemDrop.ItemData m_lastFoodItem;

	private static float m_pullTo = 1f;

	private static readonly EffectListRef pukeFX = new EffectListRef
	{
		dataRefs = new List<EffectListRef.EffectDataRef>
		{
			new EffectListRef.EffectDataRef("fx_Puke")
			{
				attach = true,
				inheritParentRotation = true,
				childTransform = "Jaw"
			},
			new EffectListRef.EffectDataRef("sfx_Puke_male")
			{
				variant = 0,
				attach = true
			},
			new EffectListRef.EffectDataRef("sfx_Puke_female")
			{
				variant = 1,
				attach = true
			}
		}
	};

	public bool m_attached;

	public bool m_attachedToShip;

	public Transform m_attachPoint;

	public Vector3 m_detachOffset;

	public string m_attachAnimation;

	public Collider[] m_attachColliders;

	public bool m_crouchToggled;

	private static readonly int EmoteSit = Animator.StringToHash("emote_sit");

	public float m_lastTargetUpdate;

	public float m_maxRange;

	public float m_greetRange;

	public float m_byeRange;

	public float m_offset;

	public float m_minTalkInterval;

	public float m_hideDialogDelay;

	public float m_randomTalkInterval;

	public float m_randomTalkChance;

	public float m_randomTalkTimer;

	public static readonly EffectListRef talkFX = new EffectListRef("sfx_dverger_vo_idle");

	public static readonly EffectListRef greetFX = new EffectListRef("sfx_haldor_greet");

	public static readonly EffectListRef goodbyeFX = new EffectListRef("sfx_haldor_laugh");

	public static readonly EffectListRef alertedFX = new EffectListRef("sfx_dverger_vo_attack");

	public bool m_didGreet;

	public bool m_didGoodbye;

	public Player m_targetPlayer;

	public bool m_seeTarget;

	public bool m_hearTarget;

	public readonly Queue<QueuedSay> m_queuedTexts;

	private readonly List<string> m_greetEmotes;

	private readonly List<string> m_randomEmote;

	public uint m_lastRevision;

	public string m_lastDataString;

	public bool m_loading;

	public bool m_saving;

	public bool m_inUse;

	public Player m_currentPlayer;

	public GameObject m_previousFollowTarget;

	public int m_lastInventoryCount;

	public static bool IsDungeonTeleport;

	public static readonly List<Viking> instances = new List<Viking>();

	public VikingAI m_vikingAI;

	public GameObject m_tombstone;

    public BaseAI.AggravatedReason m_aggravatedReason;

    public EffectList m_spawnEffects;

	public EffectList m_skillLevelupEffects;

	public EffectList m_equipStartEffects;

	public EffectList m_perfectDodgeEffects;

	public string prefabName;

	public bool isDead;

	public static readonly List<Player> s_nearbyPlayers = new List<Player>();

	public static readonly EffectListRef m_tamedEffect = new EffectListRef("fx_creature_tamed");

	public static readonly EffectListRef m_sootheEffect = new EffectListRef("vfx_creature_soothed");

	public float m_fedDuration;

	public float m_tamingTime;

	public bool m_startsTamed;

	public float m_tamingSpeedMultiplierRange;

	public float m_tamingBoostMultiplier;

    public Skills.SkillType m_levelUpOwnerSkill;

    public float m_levelUpFactor;

	public int m_maxLevel;

	public float m_secondsToLevelUp;

	public float GetArmor()
	{
		armor = NorsemanConfigs.GetBaseArmor(prefabName);
		if (base.m_chestItem != null)
		{
			armor += base.m_chestItem.GetArmor();
		}
		if (base.m_legItem != null)
		{
			armor += base.m_legItem.GetArmor();
		}
		if (base.m_helmetItem != null)
		{
			armor += base.m_helmetItem.GetArmor();
		}
		if (base.m_shoulderItem != null)
		{
			armor += base.m_shoulderItem.GetArmor();
		}
        m_seman.ApplyArmorMods(ref armor);
        return armor;
	}

	public override float GetBodyArmor()
	{
		return armor;
	}

	public override bool StartAttack(Character target, bool secondaryAttack)
	{
        //IL_0098: Unknown result type (might be due to invalid IL or missing references)
        //IL_009e: Invalid comparison between Unknown and I4
        //IL_0199: Unknown result type (might be due to invalid IL or missing references)
        //IL_01a0: Invalid comparison between Unknown and I4
        if (target != null)
        {
            m_isWorking = false;
        }

        if ((((Character)this).InAttack() && !HaveQueuedChain()) || ((Character)this).InDodge() || !((Character)this).CanMove() || ((Character)this).IsKnockedBack() || ((Character)this).IsStaggering() || ((Character)this).InMinorAction())
		{
			return false;
		}
        ItemDrop.ItemData currentWeapon = ((Humanoid)this).GetCurrentWeapon();
		if (currentWeapon == null || (!currentWeapon.HaveSecondaryAttack() && !currentWeapon.HavePrimaryAttack()))
		{
			return false;
		}
		bool flag = currentWeapon.HaveSecondaryAttack() && (double)UnityEngine.Random.value > 0.5;
		if ((int)currentWeapon.m_shared.m_skillType == 5)
		{
			flag = false;
		}
		if (base.m_currentAttack != null)
		{
			base.m_currentAttack.Stop();
            PreviousAttack = base.m_currentAttack;
            base.m_currentAttack = null;
		}
		Attack val = ((!flag) ? currentWeapon.m_shared.m_attack.Clone() : currentWeapon.m_shared.m_secondaryAttack.Clone());
        if (m_isWorking)
        {
            val.m_hitTerrain = false;
        }
        if (!val.Start(
			this,
			m_body,
			m_zanim,
			m_animEvent,
			base.m_visEquipment,
			currentWeapon,
			PreviousAttack,
			TimeSinceLastAttack,
			UnityEngine.Random.Range(0.5f, 1f)))
        {
			return false;
		}
		if (currentWeapon.m_shared.m_attack.m_requiresReload)
		{
			SetWeaponLoaded(null);
		}
		if (currentWeapon.m_shared.m_attack.m_bowDraw)
		{
            SetAttackDrawPercentage(currentWeapon.m_shared.m_attack, 0f);
        }
		if ((int)currentWeapon.m_shared.m_itemType != 15)
		{
			currentWeapon.m_durability -= 1.5f;
		}
        ClearActionQueue();
        AccessTools.Method(typeof(Humanoid), "StartAttackGroundCheck").Invoke(this, null);
        base.m_currentAttack = val;
		base.m_currentAttackIsSecondary = flag;
		base.m_lastCombatTimer = 0f;
		if (currentWeapon.m_shared.m_name == "$item_stafficeshards")
		{
			((MonoBehaviour)this).Invoke("StopCurrentAttack", 5f);
		}
		return true;
	}

	private void StopCurrentAttack()
	{
		if (base.m_currentAttack != null)
		{
			base.m_currentAttack.Stop();
            PreviousAttack = base.m_currentAttack;
            base.m_currentAttack = null;
		}
	}

	private void SetWeaponLoaded(ItemDrop.ItemData weapon)
	{
		if (weapon != m_weaponLoaded)
		{
			m_weaponLoaded = weapon;
            m_nview.GetZDO().Set(ZDOVars.s_weaponLoaded, weapon != null);
        }
	}

	public void SetBlocking(bool block)
	{
        m_blocking = block;
    }

	public void SetupCustomization()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		m_hairColor = m_nview.GetZDO().GetVec3(ZDOVars.s_hairColor, Vector3.zero);
		m_skinColor = m_nview.GetZDO().GetVec3(ZDOVars.s_skinColor, Vector3.one);
		base.m_hairItem = m_nview.GetZDO().GetString(ZDOVars.s_hairItem, "");
		base.m_beardItem = m_nview.GetZDO().GetString(ZDOVars.s_beardItem, "");
        m_isElf = m_nview.GetZDO().GetBool(VikingVars.isElf, false);
        if (!m_nview.GetZDO().GetBool(VikingVars.isSet, false))
        {
			SetRandomModel(out var isFemale);
			SetRandomName();
			SetRandomHair();
			SetRandomBeard(isFemale);
			SetRandomHairColor();
			SetRandomSkinColor();
			m_isElf = UnityEngine.Random.value > 0.5f;
			m_nview.GetZDO().Set(VikingVars.isElf, m_isElf);
			m_nview.GetZDO().Set(VikingVars.isSet, true);
		}
		SetElfEars();
        SetHairEquipped(
			base.m_visEquipment,
			!string.IsNullOrEmpty(base.m_hairItem)
				? StringExtensionMethods.GetStableHashCode(base.m_hairItem)
				: 0);
        SetBeardEquipped(
			base.m_visEquipment,
			!string.IsNullOrEmpty(base.m_beardItem)
				? StringExtensionMethods.GetStableHashCode(base.m_beardItem)
				: 0);
    }

	public void SetRandomName()
	{
		string text = ((m_nview.GetZDO().GetInt(ZDOVars.s_modelIndex, 0) == 0) ? NameGenerator.names.GenerateMaleName() : NameGenerator.names.GenerateFemaleName());
		m_nview.GetZDO().Set(ZDOVars.s_tamedName, text);
	}

	public void SetRandomSkinColor()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Color randomSkinColor = CustomizationManager.GetRandomSkinColor();
		Vector3 skinColor = Utils.ColorToVec3(randomSkinColor);
		SetSkinColor(skinColor);
	}

	public void SetSkinColor(Vector3 color)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!(m_skinColor == color))
		{
			m_skinColor = color;
			base.m_visEquipment.SetSkinColor(color);
		}
	}

	public void SetRandomModel(out bool isFemale)
	{
		int num = UnityEngine.Random.Range(0, 2);
		base.m_visEquipment.SetModel(num);
		isFemale = num != 0;
	}

	public void SetRandomBeard(bool isFemale)
	{
		if (isFemale)
		{
			((Humanoid)this).SetBeard("");
		}
		else if (CustomizationManager.beards.Count > 0)
		{
			string beard = CustomizationManager.beards[UnityEngine.Random.Range(0, CustomizationManager.beards.Count)];
			((Humanoid)this).SetBeard(beard);
		}
	}

	public void SetRandomHair()
	{
		if (CustomizationManager.hairs.Count > 0)
		{
			string hair = CustomizationManager.hairs[UnityEngine.Random.Range(0, CustomizationManager.hairs.Count)];
			((Humanoid)this).SetHair(hair);
            m_nview.GetZDO().Set(ZDOVars.s_hairItem, hair);
        }
	}



	public void SetRandomHairColor()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Color val = CustomizationManager.hairColors[UnityEngine.Random.Range(0, CustomizationManager.hairColors.Count)];
		SetHairColor(Utils.ColorToVec3(val));
	}

	public void SetHairColor(Vector3 color)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (!(m_hairColor == color))
		{
			m_hairColor = color;
			base.m_visEquipment.SetHairColor(m_hairColor);
		}
	}

    protected override void ApplyArmorDamageMods(ref HitData.DamageModifiers mods)
	{
		if (base.m_chestItem != null)
		{
            mods.Apply(base.m_chestItem.m_shared.m_damageModifiers);
        }
		if (base.m_legItem != null)
		{
            mods.Apply(base.m_legItem.m_shared.m_damageModifiers);
        }
		if (base.m_helmetItem != null)
		{
            mods.Apply(base.m_helmetItem.m_shared.m_damageModifiers);
        }
		if (base.m_shoulderItem != null)
		{
            mods.Apply(base.m_shoulderItem.m_shared.m_damageModifiers);
        }
	}

	public void Dodge(Vector3 dir)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.rotation = Quaternion.LookRotation(dir);
		m_body.rotation = ((Component)this).transform.rotation;
		m_zanim.SetTrigger("dodge");
		((Character)this).AddNoise(5f);
		m_dodgeEffects.Create(((Component)this).transform.position, Quaternion.identity, ((Component)this).transform, 1f, -1);
	}

	public bool CanDodge()
	{
		return !((Character)this).IsBlocking() && !((Character)this).IsRunning() && !((Character)this).IsSwimming();
	}

	public override float GetMaxEitr()
	{
		return 9999f;
	}

	public void SetElfEars()
	{
        //IL_0027: Unknown result type (might be due to invalid IL or missing references)
        //IL_002c: Unknown result type (might be due to invalid IL or missing references)
        if (m_elfEars != null)
        {
			m_elfEars.SetActive(m_isElf);
			SetElfEarColor(Utils.Vec3ToColor(m_skinColor));
		}
	}

	public void SetElfEarColor(Color color)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (m_elfEarMats != null)
		{
			Material[] elfEarMats = m_elfEarMats;
			foreach (Material val in elfEarMats)
			{
				val.color = color;
			}
		}
	}

	public void UpdateEncumber()
	{
		if (((Character)this).IsTamed())
		{
			if (!((Character)this).IsEncumbered())
			{
				m_seman.RemoveStatusEffect(SEMan.s_statusEffectEncumbered, false);
			}
			else if (NorsemenPlugin.CanBecomeEncumbered)
			{
				m_seman.AddStatusEffect(SEMan.s_statusEffectEncumbered, false, 0, 0f);
			}
		}
	}

	public override bool IsEncumbered()
	{
		if (!((Character)this).IsTamed())
		{
			return false;
		}
		return base.m_inventory.GetTotalWeight() > GetMaxCarryWeight();
	}

	public float GetMaxCarryWeight()
	{
		float num = NorsemenPlugin.BaseCarryWeight;
        float num2 = Math.Max(CharacterLevel - 1, 0) * 50;
        num += num2;
		m_seman.ModifyMaxCarryWeight(num, ref num);
		return num;
	}

	public void SetupConditionals()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Heightmap.Biome biome = (Heightmap.Biome)(NorsemanConfigs.TryGetConfig(prefabName, out var config) ? ((int)config.biome) : 0);
		List<ConditionalWeightedSet> sets = CustomizationManager.GetSets(biome);
		List<ConditionalChanceItem> items = CustomizationManager.GetItems(biome);
		List<ConditioanlWeightedItem> weapons = CustomizationManager.GetWeapons(biome);
		m_conditionalItemSets = sets.Select((ConditionalWeightedSet x) => x.set).ToArray();
		m_conditionalRandomItems = items.Select((ConditionalChanceItem x) => x.item).ToArray();
		m_conditionalRandomWeapons = weapons.Select((ConditioanlWeightedItem x) => x.weapon).ToArray();
	}

	public void AddRandomShield()
	{
		GameObject[] randomShield = base.m_randomShield;
		if (randomShield != null && randomShield.Length > 0)
		{
			int num = UnityEngine.Random.Range(0, base.m_randomShield.Length);
			GameObject val = base.m_randomShield[num];
            if (val != null)
            {
                GiveVikingDefaultItem(val);
            }
        }
	}

	public void AddRandomWeapon()
	{
		GameObject[] randomWeapon = base.m_randomWeapon;
		if (randomWeapon != null && randomWeapon.Length > 0)
		{
			int num = UnityEngine.Random.Range(0, base.m_randomWeapon.Length);
			GameObject val = base.m_randomWeapon[num];
            if (val != null)
            {
                GiveVikingDefaultItem(val);
            }
        }
	}

	public void AddConditionalWeapon()
	{
		ConditionalRandomWeapon[] conditionalRandomWeapons = m_conditionalRandomWeapons;
		if (conditionalRandomWeapons == null || conditionalRandomWeapons.Length <= 0)
		{
			return;
		}
		List<ConditionalRandomWeapon> list = new List<ConditionalRandomWeapon>();
		ConditionalRandomWeapon[] conditionalRandomWeapons2 = m_conditionalRandomWeapons;
		foreach (ConditionalRandomWeapon conditionalRandomWeapon in conditionalRandomWeapons2)
		{
			if (conditionalRandomWeapon.HasKey())
			{
				list.Add(conditionalRandomWeapon);
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		float num = list.Sum((ConditionalRandomWeapon x) => x.m_weight);
		float num2 = 0f;
		float num3 = UnityEngine.Random.Range(0f, num);
		List<ConditionalRandomWeapon> list2 = list.OrderBy((ConditionalRandomWeapon x) => x.m_weight).ToList();
		ConditionalRandomWeapon conditionalRandomWeapon2 = null;
		for (int num4 = 0; num4 < list2.Count; num4++)
		{
			ConditionalRandomWeapon conditionalRandomWeapon3 = list2[num4];
			num2 += conditionalRandomWeapon3.m_weight;
			if (num3 < num2)
			{
				conditionalRandomWeapon2 = conditionalRandomWeapon3;
				break;
			}
		}
		if (conditionalRandomWeapon2 == null)
		{
			int index = list2.Count - 1;
			conditionalRandomWeapon2 = list2[index];
		}
        GiveVikingDefaultItem(conditionalRandomWeapon2.m_prefab);
    }

	public void AddRandomArmor()
	{
		GameObject[] randomArmor = base.m_randomArmor;
		if (randomArmor != null && randomArmor.Length > 0)
		{
			int num = UnityEngine.Random.Range(0, base.m_randomArmor.Length);
			GameObject val = base.m_randomArmor[num];
            if (val != null)
            {
                GiveVikingDefaultItem(val);
            }
		}
	}

	public bool AddConditionalSet()
	{
		bool result = false;
		ConditionalItemSet[] conditionalItemSets = m_conditionalItemSets;
		if (conditionalItemSets != null && conditionalItemSets.Length > 0)
		{
			List<ConditionalItemSet> list = new List<ConditionalItemSet>();
			ConditionalItemSet[] conditionalItemSets2 = m_conditionalItemSets;
			foreach (ConditionalItemSet conditionalItemSet in conditionalItemSets2)
			{
				if (conditionalItemSet.HasKey())
				{
					list.Add(conditionalItemSet);
				}
			}
			if (list.Count > 0)
			{
				float num = list.Sum((ConditionalItemSet x) => x.m_weight);
				float num2 = 0f;
				float num3 = UnityEngine.Random.Range(0f, num);
				List<ConditionalItemSet> list2 = list.OrderBy((ConditionalItemSet x) => x.m_weight).ToList();
				ConditionalItemSet conditionalItemSet2 = null;
				for (int num4 = 0; num4 < list2.Count; num4++)
				{
					ConditionalItemSet conditionalItemSet3 = list2[num4];
					num2 += conditionalItemSet3.m_weight;
					if (num3 < num2)
					{
						conditionalItemSet2 = conditionalItemSet3;
						break;
					}
				}
				if (conditionalItemSet2 == null)
				{
					int index = list2.Count - 1;
					conditionalItemSet2 = list[index];
				}
				GameObject[] items = conditionalItemSet2.m_items;
				foreach (GameObject val in items)
				{
                    if (val != null)
                    {
                        GiveVikingDefaultItem(val);
                    }
				}
				result = true;
			}
		}
		return result;
	}

	public void AddDefaults()
	{
		GameObject[] defaultItems = base.m_defaultItems;
		if (defaultItems == null || defaultItems.Length <= 0)
		{
			return;
		}
		GameObject[] defaultItems2 = base.m_defaultItems;
		foreach (GameObject val in defaultItems2)
		{
            if (val != null)
            {
                GiveVikingDefaultItem(val);
            }
		}
	}

	public void AddRandomItems()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected I4, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected I4, but got Unknown
		RandomItem[] randomItems = base.m_randomItems;
		if (randomItems == null || randomItems.Length <= 0)
		{
			return;
		}
        int num = (int)Enum.GetValues(typeof(ItemDrop.ItemData.ItemType))
			.Cast<ItemDrop.ItemData.ItemType>()
			.Max();
        bool[] randomItemSlotFilled = new bool[num];
        RandomItem[] randomItems2 = base.m_randomItems;
		foreach (RandomItem val in randomItems2)
		{
            if (val.m_prefab == null)
            {
				continue;
			}
            float value = UnityEngine.Random.value;
            if (!(value <= val.m_chance))
			{
				int num2 = (int)val.m_prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_itemType;
                if (!randomItemSlotFilled[num2])
                {
                    randomItemSlotFilled[num2] = true;
                    GiveVikingDefaultItem(val.m_prefab);
                }
			}
		}
	}

	public void AddDefaultItems()
	{
		SetupConditionals();
        if (!m_nview.GetZDO().GetBool(ZDOVars.s_addedDefaultItems, false))
        {
			Inventory inventory = base.m_inventory;
			inventory.m_onChanged = (Action)Delegate.Remove(inventory.m_onChanged, new Action(OnInventoryChanged));
			AddConditionalWeapon();
			AddConditionalItems();
			if (!AddConditionalSet())
			{
				AddDefaults();
			}
            m_nview.GetZDO().Set(ZDOVars.s_addedDefaultItems, true);
            Save();
			Inventory inventory2 = base.m_inventory;
			inventory2.m_onChanged = (Action)Delegate.Combine(inventory2.m_onChanged, new Action(OnInventoryChanged));
		}
	}

	public void AddConditionalItems()
	{
		ConditionalRandomItem[] conditionalRandomItems = m_conditionalRandomItems;
		if (conditionalRandomItems == null || conditionalRandomItems.Length <= 0)
		{
			return;
		}
		ConditionalRandomItem[] conditionalRandomItems2 = m_conditionalRandomItems;
		foreach (ConditionalRandomItem conditionalRandomItem in conditionalRandomItems2)
		{
            if (conditionalRandomItem.m_prefab != null &&
				conditionalRandomItem.HasKey())
            {
				float value = UnityEngine.Random.value;
				if (!(value < conditionalRandomItem.m_chance))
				{
					int num = UnityEngine.Random.Range(conditionalRandomItem.m_min, conditionalRandomItem.m_max);
                    GetInventory().AddItem(conditionalRandomItem.m_prefab.name, num, 1, 0, 0L, "", false);
                }
			}
		}
	}

	public List<ItemDrop.ItemData> GetEquipment()
	{
		return new List<ItemDrop.ItemData>(9) { base.m_helmetItem, base.m_legItem, base.m_shoulderItem, base.m_chestItem, base.m_utilityItem, base.m_leftItem, base.m_rightItem, base.m_ammoItem, base.m_trinketItem };
	}

	public void EquipItems()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Invalid comparison between Unknown and I4
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Invalid comparison between Unknown and I4
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Invalid comparison between Unknown and I4
		UnequipIfNotInInventory();
		List<ItemDrop.ItemData> allItemsInGridOrder = base.m_inventory.GetAllItemsInGridOrder();
		for (int i = 0; i < allItemsInGridOrder.Count; i++)
		{
            ItemDrop.ItemData val = allItemsInGridOrder[i];
			if (!val.IsEquipable())
			{
				continue;
			}
			EquipIfBetter(val);
            Skills.SkillType skillType = val.m_shared.m_skillType;
            Skills.SkillType val2 = skillType;
			if ((int)val2 != 7)
			{
				if ((int)val2 == 12)
				{
					if (m_pickaxe == null || m_pickaxe.IsItemBetter(val))
					{
						m_pickaxe = val;
					}
				}
				else if (m_fishingRod == null && (int)val.m_shared.m_animationState == 9)
				{
					m_fishingRod = val;
				}
			}
			else if (m_axe == null || m_axe.IsItemBetter(val))
			{
				m_axe = val;
			}
		}
        SetupVisEquipment(base.m_visEquipment, false);
        GetArmor();
		Save();
	}

    protected override void SetupVisEquipment(VisEquipment visEq, bool isRagdoll)
    {
        int leftItemHash =
            base.m_leftItem != null && base.m_leftItem.m_dropPrefab != null
                ? Utils.GetPrefabName(base.m_leftItem.m_dropPrefab).GetStableHashCode()
                : 0;

        int leftItemVariant =
            base.m_leftItem != null
                ? base.m_leftItem.m_variant
                : 0;

        int rightItemHash =
            base.m_rightItem != null && base.m_rightItem.m_dropPrefab != null
                ? Utils.GetPrefabName(base.m_rightItem.m_dropPrefab).GetStableHashCode()
                : 0;

        int chestItemHash =
            base.m_chestItem != null && base.m_chestItem.m_dropPrefab != null
                ? Utils.GetPrefabName(base.m_chestItem.m_dropPrefab).GetStableHashCode()
                : 0;

        int legItemHash =
            base.m_legItem != null && base.m_legItem.m_dropPrefab != null
                ? Utils.GetPrefabName(base.m_legItem.m_dropPrefab).GetStableHashCode()
                : 0;

        int helmetItemHash =
            base.m_helmetItem != null && base.m_helmetItem.m_dropPrefab != null
                ? Utils.GetPrefabName(base.m_helmetItem.m_dropPrefab).GetStableHashCode()
                : 0;

        // Persist the equipped visual state to the Viking's ZDO.
        // This allows the appearance to survive unload/reload.
        if (
            !isRagdoll &&
            m_nview != null &&
            m_nview.IsValid() &&
            m_nview.GetZDO() != null
        )
        {
            ZDO zdo = m_nview.GetZDO();

            zdo.Set(
                ZDOVars.s_chestItem,
                chestItemHash,
                false
            );

            zdo.Set(
                ZDOVars.s_legItem,
                legItemHash,
                false
            );

            zdo.Set(
                ZDOVars.s_helmetItem,
                helmetItemHash,
                false
            );

            zdo.Set(
                ZDOVars.s_leftItem,
                leftItemHash,
                false
            );

            zdo.Set(
                ZDOVars.s_leftItemVariant,
                leftItemVariant,
                false
            );

            zdo.Set(
                ZDOVars.s_rightItem,
                rightItemHash,
                false
            );
        }

        if (!isRagdoll)
        {
            visEq.SetLeftItem(
                leftItemHash,
                leftItemVariant,
                base.m_leftItem != null ? base.m_leftItem.m_quality : 0
            );

            visEq.SetRightItem(
                rightItemHash,
                base.m_rightItem != null ? base.m_rightItem.m_quality : 0
            );
        }

        visEq.SetChestItem(chestItemHash);
        visEq.SetLegItem(legItemHash);
        visEq.SetHelmetItem(helmetItemHash);

        visEq.SetShoulderItem(
            base.m_shoulderItem != null
                ? base.m_shoulderItem.m_dropPrefab.name.GetStableHashCode()
                : 0,
            base.m_shoulderItem != null
                ? base.m_shoulderItem.m_variant
                : 0,
            base.m_shoulderItem != null
                ? base.m_shoulderItem.m_quality
                : 0
        );

        visEq.SetUtilityItem(
            base.m_utilityItem != null
                ? base.m_utilityItem.m_dropPrefab.name.GetStableHashCode()
                : 0
        );

        visEq.SetTrinketItem(
            base.m_trinketItem != null
                ? base.m_trinketItem.m_dropPrefab.name
                : ""
        );

        visEq.SetBeardItem(
            base.m_beardItem.GetStableHashCode()
        );

        visEq.SetHairItem(
            base.m_hairItem.GetStableHashCode()
        );

        visEq.SetHairColor(m_hairColor);
        visEq.SetSkinColor(m_skinColor);
    }

    public void EquipIfBetter(ItemDrop.ItemData item)
	{
        //IL_0007: Unknown result type (might be due to invalid IL or missing references)
        //IL_000c: Unknown result type (might be due to invalid IL or missing references)
        //IL_000d: Unknown result type (might be due to invalid IL or missing references)
        //IL_000e: Unknown result type (might be due to invalid IL or missing references)
        //IL_000f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0011: Unknown result type (might be due to invalid IL or missing references)
        //IL_004f: Expected I4, but got Unknown
        //IL_0051: Unknown result type (might be due to invalid IL or missing references)
        //IL_0054: Invalid comparison between Unknown and I4
        ItemDrop.ItemData.ItemType itemType = item.m_shared.m_itemType;
        int val = (int)itemType;
        switch (val - 5)
		{
		default:
			if ((int)val != 24)
			{
				break;
			}
			if (base.m_trinketItem == null)
			{
				((Humanoid)this).EquipItem(item, true);
			}
			return;
		case 1:
			if (base.m_helmetItem == null || base.m_helmetItem.IsItemBetter(item))
			{
				((Humanoid)this).EquipItem(item, true);
			}
			return;
		case 2:
			if (base.m_chestItem == null || base.m_chestItem.IsItemBetter(item))
			{
				((Humanoid)this).EquipItem(item, true);
			}
			return;
		case 12:
			if (base.m_shoulderItem == null || base.m_shoulderItem.IsItemBetter(item))
			{
				((Humanoid)this).EquipItem(item, true);
			}
			return;
		case 6:
			if (base.m_legItem == null || base.m_legItem.IsItemBetter(item))
			{
				((Humanoid)this).EquipItem(item, true);
			}
			return;
		case 13:
			if (base.m_utilityItem == null)
			{
				((Humanoid)this).EquipItem(item, true);
			}
			return;
		case 0:
		case 10:
			if (base.m_leftItem == null || base.m_leftItem.IsItemBetter(item))
			{
				((Humanoid)this).EquipItem(item, true);
			}
			return;
		case 4:
			if (base.m_ammoItem == null || base.m_ammoItem.IsItemBetter(item))
			{
				((Humanoid)this).EquipItem(item, true);
			}
			return;
		case 3:
		case 5:
		case 7:
		case 8:
		case 9:
		case 11:
			break;
		}
		if (base.m_rightItem == null || base.m_rightItem.IsItemBetter(item))
		{
			((Humanoid)this).EquipItem(item, true);
		}
	}

	public void UnequipIfNotInInventory()
	{
		List<ItemDrop.ItemData> equipment = GetEquipment();
		for (int i = 0; i < equipment.Count; i++)
		{
            ItemDrop.ItemData val = equipment[i];
			if (val != null && !base.m_inventory.ContainsItem(val))
			{
				((Humanoid)this).UnequipItem(val, true);
			}
		}
		if (m_pickaxe != null && !base.m_inventory.ContainsItem(m_pickaxe))
		{
			m_pickaxe = null;
		}
		if (m_axe != null && !base.m_inventory.ContainsItem(m_axe))
		{
			m_axe = null;
		}
		if (m_fishingRod != null && !base.m_inventory.ContainsItem(m_fishingRod))
		{
			m_fishingRod = null;
		}
	}

	public bool IsHoldingTorch()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Invalid comparison between Unknown and I4
		if (base.m_rightItem != null && (int)base.m_rightItem.m_shared.m_itemType == 15)
		{
			return true;
		}
		if (base.m_leftItem != null && (int)base.m_leftItem.m_shared.m_itemType == 15)
		{
			return true;
		}
		return false;
	}

	public void RPC_Command(long sender, ZDOID characterID, bool message)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Player player = GetPlayer(characterID);
        if (player == null)
        {
			return;
		}
		GameObject followTarget = ((MonsterAI)m_vikingAI).GetFollowTarget();
        if (followTarget == null)
        {
			Follow(((Component)player).gameObject, player.GetPlayerName());
			m_vikingAI.m_followPlayer = player;
			if (message)
			{
                player.Message(
					(MessageHud.MessageType)2,
					GetHoverName() + " $hud_tamefollow",
					0,
					null,
					false);
            }
		}
		else
		{
			UnFollow();
			if (message)
			{
                player.Message(
                    (MessageHud.MessageType)2,
                    GetHoverName() + " $hud_tamestay",
                    0,
                    null,
                    false);
            }
		}
	}

	private static Player GetPlayer(ZDOID characterID)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = ZNetScene.instance.FindInstance(characterID);
        return val != null ? val.GetComponent<Player>() : null;
    }

	public void Command(Humanoid user, bool message = true)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		m_nview.InvokeRPC("RPC_Command", new object[2]
		{
			((Character)user).GetZDOID(),
			message
		});
	}

	public bool IsFollowing()
	{
        return GetFollowTarget() != null;
    }

	public GameObject GetFollowTarget()
	{
		return ((MonsterAI)m_vikingAI).GetFollowTarget();
	}

	public void Follow(GameObject target, string playerName)
	{
		((BaseAI)m_vikingAI).ResetPatrolPoint();
		((MonsterAI)m_vikingAI).SetFollowTarget(target);
		if (!string.IsNullOrEmpty(playerName) && m_nview.IsOwner())
		{
			m_nview.GetZDO().Set(ZDOVars.s_follow, playerName);
		}
	}

	public void UnFollow()
	{
		((MonsterAI)m_vikingAI).SetFollowTarget((GameObject)null);
		((BaseAI)m_vikingAI).SetPatrolPoint();
		m_vikingAI.m_followPlayer = null;
		if (m_nview.IsOwner())
		{
			m_nview.GetZDO().Set(ZDOVars.s_follow, "");
		}
	}

	public void UpdateSavedFollowTarget()
	{
		if (!m_nview.IsOwner() || IsFollowing())
		{
			return;
		}
		string text = m_nview.GetZDO().GetString(ZDOVars.s_follow, "");
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		foreach (Player allPlayer in Player.GetAllPlayers())
		{
			if (allPlayer.GetPlayerName() == text)
			{
				Command((Humanoid)(object)allPlayer, message: false);
				break;
			}
		}
	}

	public void OnConsumedItem(ItemDrop item)
	{
		OnConsumedItem(item.m_itemData);
	}

	public void UpdateHealth()
	{
		float baseHealth = NorsemanConfigs.GetBaseHealth(prefabName);
        float num = baseHealth * (float)GetLevel();
        if (m_lastFoodItem != null && !IsHungry())
		{
			num += m_lastFoodItem.m_shared.m_food;
		}
		((Character)this).SetMaxHealth(num);
	}

	public bool OnConsumedItem(ItemDrop.ItemData item)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (IsHungry() && item.m_shared.m_food > 0f)
		{
			m_sootheEffect.Create(((Character)this).GetCenterPoint(), Quaternion.identity);
			ResetFeedingTimer();
			m_queuedTexts.Clear();
			QueueSay(TalkManager.GetTalk(TalkManager.TalkType.Eat), item.m_shared.m_name);
			string trigger = (item.m_shared.m_isDrink ? "drink" : "eat");
			m_zanim.SetTrigger(trigger);
			return true;
		}
        if (item.m_shared.m_consumeStatusEffect != null)
        {
			bool flag = item.m_shared.m_consumeStatusEffect is SE_Puke;
			bool flag2 = ((Character)this).IsTamed();
			if (item.m_shared.m_consumeStatusEffect.CanAdd((Character)(object)this) && (flag | flag2))
			{
				m_seman.AddStatusEffect(item.m_shared.m_consumeStatusEffect.NameHash(), false, 0, 0f);
				if (flag && !flag2)
				{
                    ((BaseAI)m_vikingAI).SetAggravated(true, (BaseAI.AggravatedReason)0);
                }
				return true;
			}
		}
		return false;
	}

	public void SetupFood()
	{
		((MonsterAI)m_vikingAI).m_consumeItems.AddRange(consumableItems);
	}

	public override bool CanConsumeItem(ItemDrop.ItemData item, bool checkWorldLevel = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		if ((int)item.m_shared.m_itemType != 2)
		{
			return false;
		}
        if (item.m_shared.m_consumeStatusEffect != null &&
			item.m_shared.m_consumeStatusEffect.CanAdd(this))
        {
			return true;
		}
		return IsHungry();
	}

	public override void AttachStart(Transform attachPoint, GameObject colliderRoot, bool hideWeapons, bool isBed, bool onShip, string attachAnimation, Vector3 detachOffset, Transform cameraPos = null)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (m_attached)
		{
			return;
		}
		m_attached = true;
		m_attachedToShip = onShip;
		m_attachPoint = attachPoint;
		m_detachOffset = detachOffset;
		m_attachAnimation = attachAnimation;
		m_zanim.SetBool(attachAnimation, true);
		m_nview.GetZDO().Set(ZDOVars.s_inBed, isBed);
        if (colliderRoot != null)
        {
			m_attachColliders = colliderRoot.GetComponentsInChildren<Collider>();
			ZLog.Log((object)("Ignoring " + m_attachColliders.Length + " colliders"));
			Collider[] attachColliders = m_attachColliders;
			foreach (Collider val in attachColliders)
			{
                Physics.IgnoreCollision(m_collider, val, true);
            }
		}
		if (hideWeapons)
		{
			((Humanoid)this).HideHandItems(false, true);
		}
		m_vikingAI.UpdateAttach();
		((Character)this).ResetCloth();
	}

	public override bool IsAttachedToShip()
	{
		return m_attached && m_attachedToShip;
	}

	public override void AttachStop()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!m_attached)
		{
			return;
		}
        if (m_attachPoint != null)
        {
			((Component)this).transform.position = m_attachPoint.TransformPoint(m_detachOffset);
		}
		if (m_attachColliders != null)
		{
			Collider[] attachColliders = m_attachColliders;
			foreach (Collider val in attachColliders)
			{
                if (val != null)
                {
                    Physics.IgnoreCollision(m_collider, val, false);
                }
			}
			m_attachColliders = null;
		}
		m_body.useGravity = true;
		m_attached = false;
		m_attachPoint = null;
		m_zanim.SetBool(m_attachAnimation, false);
		m_nview.GetZDO().Set(ZDOVars.s_inBed, false);
		((Character)this).ResetCloth();
	}

    protected override void SetCrouch(bool crouch)
    {
		m_crouchToggled = crouch;
	}

	public bool CanCrouch()
	{
		if (((Character)this).IsSwimming())
		{
			return false;
		}
		if (((Character)this).IsRunning())
		{
			return false;
		}
		if (((Character)this).IsBlocking())
		{
			return false;
		}
		if (((Character)this).InAttack())
		{
			return false;
		}
		if (((Character)this).IsDrawingBow())
		{
			return false;
		}
		if (((Character)this).IsDead())
		{
			return false;
		}
		if (((Character)this).IsStaggering())
		{
			return false;
		}
		return true;
	}

    protected override void OnRagdollCreated(Ragdoll ragdoll)
    {
        base.OnRagdollCreated(ragdoll);

        if (ragdoll.TryGetComponent<Norseman.ExtraRagdoll>(out var extraRagdoll))
        {
            extraRagdoll.SetElfEars(m_isElf, Utils.Vec3ToColor(m_skinColor));
        }
    }

    public void LookTowardsTarget()
	{
        //IL_0024: Unknown result type (might be due to invalid IL or missing references)
        //IL_0029: Unknown result type (might be due to invalid IL or missing references)
        //IL_0054: Unknown result type (might be due to invalid IL or missing references)
        //IL_005a: Unknown result type (might be due to invalid IL or missing references)
        //IL_005f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0064: Unknown result type (might be due to invalid IL or missing references)
        //IL_0067: Unknown result type (might be due to invalid IL or missing references)
        //IL_006c: Unknown result type (might be due to invalid IL or missing references)
        //IL_006e: Unknown result type (might be due to invalid IL or missing references)
        if (m_nview.IsOwner() && m_targetPlayer != null)
        {
            Vector3 val = GetVelocity();
            if (val.magnitude < 0.5f && !InAttack())
            {
				val = ((Character)m_targetPlayer).GetEyePoint() - ((Character)this).GetEyePoint();
                Vector3 normalized = val.normalized;
                ((Character)this).SetLookDir(normalized, 0f);
			}
		}
	}

	public void UpdateTalk(float dt)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		if (((BaseAI)m_vikingAI).IsAlerted() || IsFollowing())
		{
			return;
		}
		m_randomTalkTimer += dt;
		if (m_randomTalkTimer < m_randomTalkInterval)
		{
			return;
		}
		m_randomTalkTimer = 0f;
		float value = UnityEngine.Random.value;
		if (value > m_randomTalkChance)
		{
			return;
		}
		UpdateTarget();
        if (m_targetPlayer != null)
        {
			if (m_seeTarget)
			{
				float num = Vector3.Distance(((Component)m_targetPlayer).transform.position, ((Component)this).transform.position);
				if (!m_didGreet && num < m_greetRange)
				{
					m_didGreet = true;
					string trigger = m_greetEmotes[UnityEngine.Random.Range(0, m_greetEmotes.Count)];
					QueueSay(TalkManager.GetTalk(TalkManager.TalkType.Greets), "", trigger, greetFX);
				}
				if (m_didGreet && !m_didGoodbye && num > m_byeRange)
				{
					m_didGoodbye = true;
					string trigger2 = m_greetEmotes[UnityEngine.Random.Range(0, m_greetEmotes.Count)];
					QueueSay(TalkManager.GetTalk(TalkManager.TalkType.Farewells), "", trigger2, goodbyeFX);
				}
				if (m_didGreet && m_didGoodbye)
				{
					QueueSay(TalkManager.GetTalk(TalkManager.TalkType.Generic), "", "", talkFX);
				}
			}
			else if (InPlayerBase())
			{
				QueueSay(TalkManager.GetTalk(TalkManager.TalkType.PlayerBase), "", "", talkFX);
			}
		}
		UpdateSayQueue();
	}

	public void UpdateTarget()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if ((double)(Time.time - m_lastTargetUpdate) <= 1.0)
		{
			return;
		}
		m_lastTargetUpdate = Time.time;
		m_targetPlayer = null;
		Player closestPlayer = Player.GetClosestPlayer(((Component)this).transform.position, m_maxRange);
        if (closestPlayer != null &&
			!((BaseAI)m_vikingAI).IsEnemy(closestPlayer))
        {
			m_seeTarget = ((BaseAI)m_vikingAI).CanSeeTarget((Character)(object)closestPlayer);
			m_hearTarget = ((BaseAI)m_vikingAI).CanHearTarget((Character)(object)closestPlayer);
			if (m_seeTarget || m_hearTarget)
			{
				m_targetPlayer = closestPlayer;
			}
		}
	}

	public bool QueueSay(List<string> texts, string context = "", string trigger = "", EffectListRef effect = null)
	{
		if (texts.Count == 0 || m_queuedTexts.Count >= 3)
		{
			return false;
		}
		string text = texts[UnityEngine.Random.Range(0, texts.Count)];
		if (!string.IsNullOrEmpty(context))
		{
			text = string.Format(text, context);
		}
		QueuedSay item = new QueuedSay
		{
			text = text,
			trigger = trigger,
			m_effect = effect
		};
		m_queuedTexts.Enqueue(item);
		return true;
	}

	public bool Say(List<string> texts, string trigger = "", EffectListRef effect = null)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (texts.Count == 0)
		{
			return false;
		}
		Say(texts[UnityEngine.Random.Range(0, texts.Count)], trigger);
		effect?.Create(((Component)this).transform.position, Quaternion.identity, null, 1f, base.m_visEquipment.GetModelIndex());
		return true;
	}

	public void QueueEmote(string trigger)
	{
		QueuedSay item = new QueuedSay
		{
			text = string.Empty,
			trigger = trigger
		};
		m_queuedTexts.Enqueue(item);
	}

	public void UpdateSayQueue()
	{
        //IL_005c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0061: Unknown result type (might be due to invalid IL or missing references)
        if (m_queuedTexts.Count > 0 && !(Time.time - m_lastTalkTime < m_minTalkInterval))
        {
			QueuedSay queuedSay = m_queuedTexts.Dequeue();
			Say(queuedSay.text, queuedSay.trigger);
			queuedSay.m_effect?.Create(((Component)this).transform.position, Quaternion.identity, null, 1f, base.m_visEquipment.GetModelIndex());
		}
	}

	public void Say(string text, string trigger)
	{
        //IL_0024: Unknown result type (might be due to invalid IL or missing references)
        //IL_002f: Unknown result type (might be due to invalid IL or missing references)
        m_lastTalkTime = Time.time;
        if (!string.IsNullOrEmpty(text))
		{
			Chat.instance.SetNpcText(((Component)this).gameObject, Vector3.up * m_offset, 20f, m_hideDialogDelay, "", text, false);
		}
		if (!string.IsNullOrEmpty(trigger))
		{
            m_animator.SetTrigger(trigger);
        }
	}

	public bool InPlayerBase()
	{
        //IL_0006: Unknown result type (might be due to invalid IL or missing references)
        return EffectArea.IsPointInsideArea(
			transform.position,
			(EffectArea.Type)4,
			30f) != null;
			}

	public void OnInventoryChanged()
	{
		if (IsLoading() || !m_nview.IsOwner())
		{
			return;
		}
		Save();
		if (!((Character)this).IsTamed())
		{
			int num = ((Humanoid)this).GetInventory().NrOfItems();
			if (num < m_lastInventoryCount)
			{
				((BaseAI)m_vikingAI).SetAggravated(true, (BaseAI.AggravatedReason)2);
				//((MonsterAI)m_vikingAI).SetTarget((Character)(object)m_currentPlayer);
			}
		}
		UpdateEncumber();
	}

	public bool Save()
	{
        //IL_0047: Unknown result type (might be due to invalid IL or missing references)
        //IL_004d: Expected O, but got Unknown
        if (!m_nview.IsValid() || !m_nview.IsOwner())
        {
			return false;
		}
		m_saving = true;
		NorsemenPlugin.LogDebug(GetName() + " saving inventory");
		ZPackage val = new ZPackage();
		base.m_inventory.Save(val);
		string @base = val.GetBase64();
        m_nview.GetZDO().Set(ZDOVars.s_items, @base);
        m_lastRevision = m_nview.GetZDO().DataRevision;
        m_lastDataString = @base;
        m_nview.GetZDO().Set(VikingVars.inventoryChanged, true);
        m_saving = false;
		return true;
	}

	public void CheckForChanges()
	{
		if (m_nview.IsValid())
		{
			Load();
			Save();
		}
	}

	public bool IsLoading()
	{
		return m_loading;
	}

	public bool IsSaving()
	{
		return m_saving;
	}

	public void ForceLoad()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		string text = m_nview.GetZDO().GetString(ZDOVars.s_items, "");
		if (!string.IsNullOrEmpty(text))
		{
			ZPackage val = new ZPackage(text);
			m_loading = true;
			base.m_inventory.Load(val);
			m_loading = false;
		}
		m_lastDataString = text;
		m_lastRevision = m_nview.GetZDO().DataRevision;
		UpdateEncumber();
	}

	public void Load()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		uint dataRevision = m_nview.GetZDO().DataRevision;
		if (dataRevision == m_lastRevision)
		{
			return;
		}
		m_lastRevision = dataRevision;
		string text = m_nview.GetZDO().GetString(ZDOVars.s_items, "");
		if (!(text == m_lastDataString))
		{
			if (string.IsNullOrEmpty(text))
			{
				m_lastDataString = text;
				return;
			}
			ZPackage val = new ZPackage(text);
			m_loading = true;
			base.m_inventory.Load(val);
			m_loading = false;
			m_lastDataString = text;
			m_lastRevision = dataRevision;
			UpdateEncumber();
		}
	}

	public bool CheckAccess(long playerID)
	{
		if (!IsPrivate())
		{
			return true;
		}
		long num = m_nview.GetZDO().GetLong(ZDOVars.s_owner, 0L);
		if (num == 0)
		{
			return true;
		}
		return num == playerID;
	}

	public void RPC_RequestOpen(long uid, long playerID)
	{
        
        //IL_0059: Unknown result type (might be due to invalid IL or missing references)
        if (m_nview.IsOwner())
		{
			if (IsInUse())
			{
                
                m_nview.InvokeRPC(uid, "RPC_OpenResponse", new object[1] { false });
				return;
			}
			ZDOMan.instance.ForceSendZDO(uid, m_nview.GetZDO().m_uid);
            m_nview.GetZDO().SetOwner(uid);
            bool saveResult = Save();
            
            m_nview.InvokeRPC(uid, "RPC_OpenResponse", new object[1] { true });
        }
	}

	public void RPC_OpenResponse(long uid, bool granted)
	{
        
        if (Player.m_localPlayer != null)
        {
			if (granted)
			{
                Load();
                
                m_lastInventoryCount = ((Humanoid)this).GetInventory().NrOfItems();
                InventoryGui.instance.Show(this);
			}
			else
			{
                Player.m_localPlayer.Message(
					(MessageHud.MessageType)2,
					"$msg_inuse",
					0,
					null,
					false);
            }
		}
	}

	public void StackAll()
	{
		m_nview.InvokeRPC("RPC_RequestStack", new object[1] { Game.instance.GetPlayerProfile().GetPlayerID() });
	}

	public void RPC_RequestStack(long uid, long playerID)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner())
		{
			if (IsInUse() && uid != ZNet.GetUID())
			{
				m_nview.InvokeRPC(uid, "RPC_StackResponse", new object[1] { false });
				return;
			}
			ZDOMan.instance.ForceSendZDO(uid, m_nview.GetZDO().m_uid);
			m_nview.GetZDO().SetOwner(uid);
			m_nview.InvokeRPC(uid, "RPC_StackResponse", new object[1] { true });
		}
	}

    public void RPC_StackResponse(long uid, bool granted)
    {
        if (Player.m_localPlayer == null)
        {
            return;
        }

        if (granted)
        {
            if (base.m_inventory.StackAll(
                Player.m_localPlayer.GetInventory(), true) > 0)
            {
                InventoryGui.instance.m_moveItemEffects.Create(
                    transform.position,
                    Quaternion.identity,
                    null,
                    1f,
                    -1);
            }
        }
        else
        {
            Player.m_localPlayer.Message(
				(MessageHud.MessageType)2,
				"$msg_inuse",
				0,
				null,
				false);
        }
    }

    public bool IsInUse()
	{
		return m_inUse;
	}

    public void SetInUse(Player player)
    {
        if (!m_nview.IsOwner())
        {
            return;
        }

        bool flag = player != null;
        if (m_inUse == flag)
        {
            return;
        }

        m_currentPlayer = player;
        m_inUse = flag;

        if (player == null)
        {
            EquipItems();

            if (m_previousFollowTarget != null)
            {
                Follow(m_previousFollowTarget, null);
            }
            else
            {
                UnFollow();
            }

            m_vikingAI.ResetWorkTargets();
        }
        else
        {
            m_previousFollowTarget = GetFollowTarget();
            Follow(player.gameObject, player.GetPlayerName());
        }
    }

    public bool IsPrivate()
    {
        if (!m_nview.IsValid())
        {
            return true;
        }

        return m_nview.GetZDO().GetBool(VikingVars.isPrivate, false);
    }

    public void SetPrivate(bool isPrivate)
    {
        if (m_nview.IsValid())
        {
            m_nview.GetZDO().Set(VikingVars.isPrivate, isPrivate);
        }
    }

    public override bool TeleportTo(Vector3 pos, Quaternion rot, bool distantTeleport)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		if (!IsDungeonTeleport)
		{
			float solidHeight = ZoneSystem.instance.GetSolidHeight(pos);
			pos.y = solidHeight;
		}
        if (distantTeleport)
        {
            ZDO zDO = m_nview.GetZDO();

            zDO.SetPosition(pos);
            zDO.SetRotation(rot);

            ZDOMan.instance.ForceSendZDO(zDO.m_uid);

            transform.position = pos;
            transform.rotation = rot;
            return true;
        }
        if (!ZoneSystem.instance.IsZoneLoaded(pos))
		{
			NorsemenPlugin.LogDebug("[" + GetName() + "] tried to teleport, but zone is not loaded");
			return false;
		}
		((Component)this).transform.position = pos;
		((Component)this).transform.rotation = rot;
		return true;
	}

	public void CreateTombStone()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (m_nview.IsOwner() && ((Humanoid)this).GetInventory().NrOfItems() > 0)
		{
            GameObject val = UnityEngine.Object.Instantiate(
				m_tombstone,
				transform.position,
				transform.rotation);
            val.GetComponent<Container>().GetInventory().MoveAll(base.m_inventory);
			val.GetComponent<TombStone>().Setup(GetName(), 0L);
			val.GetComponent<VikingTomb>().Setup(this);
		}
	}

	public void DropItems()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		float num = 0.5f;
		foreach (ItemDrop.ItemData allItem in ((Humanoid)this).GetInventory().GetAllItems())
		{
			if (allItem.IsEquipable())
			{
				continue;
			}
			Quaternion val = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
			Vector3 val2 = UnityEngine.Random.insideUnitSphere * num;
			Vector3 val3 = position + Vector3.up * 0.5f + val2;
			ItemDrop val4 = ItemDrop.DropItem(allItem, allItem.m_stack, val3, val);
			Rigidbody component = ((Component)val4).GetComponent<Rigidbody>();
            if (component != null)
            {
				Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
				if ((double)insideUnitSphere.y < 0.0)
				{
					insideUnitSphere.y = 0f - insideUnitSphere.y;
				}
				component.AddForce(insideUnitSphere * 5f, (ForceMode)2);
			}
		}
	}

    public string GetTooltip()
    {
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.AppendFormat(
            "$se_health: <color=#ff8080ff>{0:0}</color> ($item_total: <color=yellow>{1:0}</color>)",
            GetHealth(),
            GetMaxHealth());

        stringBuilder.AppendFormat(
            "\n$item_armor: <color=orange>{0:0}</color>",
            GetArmor());

        stringBuilder.Append(
            $"\n$norseman_level: <color=orange>{GetLevel()}</color>");

        string text = m_nview.GetZDO().GetString(ZDOVars.s_ownerName, "");

        if (!string.IsNullOrEmpty(text))
        {
            stringBuilder.Append(
                "\n$norseman_owner: <color=orange>" + text + "</color>");
        }

        stringBuilder.Append(
            "\n$norseman_state: <color=orange>$norseman_" +
            m_vikingAI.m_behaviour.ToString().ToLower() +
            "</color>");

        stringBuilder.Append(
            "\n$norseman_movement: <color=orange>$norseman_" +
            m_vikingAI.m_moveType.ToString().ToLower() +
            "</color>");

        stringBuilder.AppendFormat(
            "\n$item_weight: <color=orange>{0:0}</color> ($item_total: <color=yellow>{1:0}</color>)",
            base.m_inventory.GetTotalWeight(),
            GetMaxCarryWeight());

        HitData.DamageModifiers damageModifiers =
            GetDamageModifiers((WeakSpot)null);

        foreach (HitData.DamageType value in Enum.GetValues(typeof(HitData.DamageType)))
        {
            bool flag = false;

            if ((int)value <= 16)
            {
                if ((int)value == 8 || (int)value == 16)
                {
                    flag = true;
                }
            }
            else if ((int)value == 31 || (int)value == 224)
            {
                flag = true;
            }

            if (!flag)
            {
                HitData.DamageModifier modifier =
                    damageModifiers.GetModifier(value);

                if ((int)modifier != 0)
                {
                    string text2 =
                        "$inventory_" + value.ToString().ToLower();

                    string text3 =
                        "$inventory_" + modifier.ToString().ToLower();

                    stringBuilder.Append(
                        "\n" + text2 +
                        ": <color=orange>" +
                        text3 +
                        "</color>");
                }
            }
        }

        List<StatusEffect> statusEffects =
            GetSEMan().GetStatusEffects();

        if (statusEffects.Count > 0)
        {
            stringBuilder.Append("\n\n$norseman_status");

            for (int i = 0; i < statusEffects.Count; i++)
            {
                StatusEffect val2 = statusEffects[i];
                stringBuilder.Append("\n- " + val2.m_name);
            }
        }

        return stringBuilder.ToString();
    }

    protected override void Awake()
    {
		instances.Add(this);
        prefabName = Utils.GetPrefabName(gameObject.name);
        //base.m_inventory.m_name = "NorsemanInventory";
		m_vikingAI = ((Component)this).GetComponent<VikingAI>();
		((Character)this).m_health = NorsemanConfigs.GetBaseHealth(prefabName);
		m_tamingTime = NorsemanConfigs.GetTameTime(prefabName);
		SetupFood();
        base.Awake();
        if (m_nview.IsValid())
		{
			m_nview.Register<long>("RPC_RequestOpen", (Action<long, long>)RPC_RequestOpen);
			m_nview.Register<bool>("RPC_OpenResponse", (Action<long, bool>)RPC_OpenResponse);
			m_nview.Register<long>("RPC_RequestStack", (Action<long, long>)RPC_RequestStack);
			m_nview.Register<bool>("RPC_StackResponse", (Action<long, bool>)RPC_StackResponse);
			m_nview.Register<ZDOID, bool>("RPC_Command", (Action<long, ZDOID, bool>)RPC_Command);
			m_nview.Register<string>("RPC_SetText", (Action<long, string>)RPC_SetText);
		}
		if (ConfigManager.CanHaveStars())
		{
			return;
		}
		((Character)this).m_onLevelSet = (Action<int>)Delegate.Combine(((Character)this).m_onLevelSet, (Action<int>)delegate(int lvl)
		{
			if (lvl > 1)
			{
				((Character)this).SetLevel(1);
			}
		});
	}

    public override void OnDeath()
    {
        isDead = true;

        if (((Character)this).IsTamed())
        {
            CreateTombStone();
        }
        else
        {
            DropItems();
        }

        base.OnDeath();
    }

    public override bool IsDead()
	{
		return isDead;
	}

    protected override void OnDestroy()
    {
        if (!IsDead() && Save())
        {
            NorsemenPlugin.LogDebug(gameObject.name + " OnDestroy, saving inventory");
        }

        instances.Remove(this);
        base.OnDestroy();
    }

    protected override void Start()
    {
		Inventory inventory = base.m_inventory;
		inventory.m_onChanged = (Action)Delegate.Combine(inventory.m_onChanged, new Action(OnInventoryChanged));
		VikingAI vikingAI = m_vikingAI;
		((MonsterAI)vikingAI).m_onConsumedItem = (Action<ItemDrop>)Delegate.Combine(((MonsterAI)vikingAI).m_onConsumedItem, new Action<ItemDrop>(OnConsumedItem));
		VikingAI vikingAI2 = m_vikingAI;
        ((BaseAI)vikingAI2).m_onBecameAggravated = (Action<BaseAI.AggravatedReason>)Delegate.Combine(((BaseAI)vikingAI2).m_onBecameAggravated, new Action<BaseAI.AggravatedReason>(OnAggravated));
        if ((m_startsTamed || m_tamingTime <= 0f) && NorsemanConfigs.IsTameable(prefabName))
		{
			((Character)this).SetTamed(true);
		}
		base.m_visEquipment.m_isPlayer = true;
		
		ForceLoad();
		AddDefaultItems();
		base.m_visEquipment.ClearEquipment();
		EquipItems();
        SetupCustomization();
        CheckLastWork();
		((MonoBehaviour)this).InvokeRepeating("CheckForChanges", 1f, 10f);
		((MonoBehaviour)this).InvokeRepeating("TamingUpdate", 3f, 3f);
	}

	public void Update()
	{
		if (m_nview.IsValid() && m_nview.IsOwner())
		{
			float deltaTime = Time.deltaTime;
			UpdateSavedFollowTarget();
			UpdateTalk(deltaTime);
		}
	}

	public override string GetHoverName()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (!FactionManager.customFactions.TryGetValue(((Character)this).m_faction, out var value))
		{
			return GetName();
		}
		return "<color=" + value.hoverColor + ">" + GetName() + "</color>";
	}

	public override string GetHoverText()
	{
        //IL_0105: Unknown result type (might be due to invalid IL or missing references)
        //IL_010a: Unknown result type (might be due to invalid IL or missing references)
        //IL_0228: Unknown result type (might be due to invalid IL or missing references)
        //IL_022d: Unknown result type (might be due to invalid IL or missing references)
        if (Player.m_localPlayer == null)
        {
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(GetText());
		bool flag = ZInput.IsGamepadActive() && !ZInput.IsMouseActive();
		if (((Character)this).IsTamed())
		{
			stringBuilder.Append(" ( " + GetStatusString() + " )");
			if (IsFollowing())
			{
				stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $norseman_stay");
			}
			else
			{
				stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $norseman_follow");
			}
			if (CheckAccess(Player.m_localPlayer.GetPlayerID()))
			{
				if (flag)
				{
					stringBuilder.Append("\n[<color=yellow><b>$KEY_AltKeys + $KEY_Use</b></color>] $hud_rename");
					stringBuilder.Append("\n[<color=yellow><b>" + ZInput.instance.GetBoundKeyString("JoyLTrigger", false) + "$KEY_Use </b></color> $piece_container_open");
				}
				else
				{
					stringBuilder.Append("\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $hud_rename");
                    string text = NorsemenPlugin.AltKey.ToString();
                    stringBuilder.Append("\n[<color=yellow><b>" + text + " + $KEY_Use</b></color>] $piece_container_open");
				}
			}
			else
			{
				stringBuilder.Append("\n$piece_noaccess");
			}
		}
		else
		{
			if (NorsemanConfigs.IsTameable(prefabName))
			{
				int tameness = GetTameness();
				if (tameness <= 0)
				{
					stringBuilder.Append(" ( $hud_wild, " + GetStatusString() + " )");
				}
				else
				{
					stringBuilder.Append($" $hud_tameness {tameness}%, {GetStatusString()} )");
				}
			}
			if (!((BaseAI)m_vikingAI).CanSeeTarget((Character)(object)Player.m_localPlayer) && NorsemenPlugin.CanSteal)
			{
				if (flag)
				{
					stringBuilder.Append("\n[<color=yellow><b>" + ZInput.instance.GetBoundKeyString("JoyLTrigger", false) + " + $KEY_Use</b></color>] $norseman_steal");
				}
				else
				{
                    string text2 = NorsemenPlugin.AltKey.ToString();
                    stringBuilder.Append("\n[<color=yellow><b>" + text2 + " + $KEY_Use</b></color>] $norseman_steal");
				}
			}
		}
		return Localization.instance.Localize(stringBuilder.ToString());
	}

	public bool Interact(Humanoid user, bool hold, bool alt)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		Player val = (Player)(object)((user is Player) ? user : null);
		if (val == null)
		{
			return false;
		}
		string value = m_nview.GetZDO().GetString(ZDOVars.s_ownerName, "");
		if (string.IsNullOrEmpty(value))
		{
			m_nview.GetZDO().Set(ZDOVars.s_owner, val.GetPlayerID());
			m_nview.GetZDO().Set(ZDOVars.s_ownerName, val.GetPlayerName());
		}
		if (((Character)this).IsTamed())
		{
			bool flag = CheckAccess(val.GetPlayerID());
			if (ZInput.GetKey(NorsemenPlugin.AltKey, true) || ZInput.GetButton("JoyLTrigger"))
			{
				if (!flag)
				{
                    ((Character)val).Message(
						MessageHud.MessageType.Center,
						"$msg_cantopen",
						0,
						null,
						false);
                }
				else
				{
                    m_nview.InvokeRPC("RPC_RequestOpen", new object[1] { val.GetPlayerID() });
                }
			}
			else if (alt)
			{
				if (!flag)
				{
					return true;
				}
				TextInput.instance.RequestText((TextReceiver)(object)this, "$hud_rename", 10);
			}
			else
			{
				Command(user);
			}
		}
		else if (!((BaseAI)m_vikingAI).CanSeeTarget((Character)(object)user) && NorsemenPlugin.CanSteal && (ZInput.GetKey(NorsemenPlugin.AltKey, true) || ZInput.GetButton("JoyLTrigger")))
		{
			m_nview.InvokeRPC("RPC_RequestOpen", new object[1] { val.GetPlayerID() });
		}
		return true;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (!((Humanoid)this).CanConsumeItem(item, false))
		{
			return true;
		}
		if (OnConsumedItem(item))
		{
			user.GetInventory().RemoveItem(item, 1);
		}
		return true;
	}

    protected override void OnDamaged(HitData hit)
	{
	}

	public void OnAggravated(BaseAI.AggravatedReason reason)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		m_aggravatedReason = reason;
		if ((int)reason != 0)
		{
			if ((int)reason == 2)
			{
				Say(TalkManager.GetTalk(TalkManager.TalkType.Thieved), "emote_roar", alertedFX);
			}
		}
		else
		{
			Say(TalkManager.GetTalk(TalkManager.TalkType.Damaged), "emote_roar", alertedFX);
		}
	}

	public static List<Viking> GetAllVikings()
	{
		return instances;
	}

	public static List<Viking> GetVikings(Vector3 pos, float range)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		List<Viking> list = new List<Viking>();
		List<Viking> allVikings = GetAllVikings();
		for (int i = 0; i < allVikings.Count; i++)
		{
			Viking viking = allVikings[i];
			float num = Vector3.Distance(pos, ((Component)viking).transform.position);
			if (num < range)
			{
				list.Add(viking);
			}
		}
		return list;
	}

	public static Viking GetNearestViking(Vector3 pos, float maxRange)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		Viking result = null;
		List<Viking> allVikings = GetAllVikings();
		for (int i = 0; i < allVikings.Count; i++)
		{
			Viking viking = allVikings[i];
			float num2 = Vector3.Distance(pos, ((Component)viking).transform.position);
			if (num2 < num && num2 < maxRange)
			{
				result = viking;
				num = num2;
			}
		}
		return result;
	}

	public void TamingUpdate()
	{
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		if (!NorsemanConfigs.IsTameable(prefabName) || !m_nview.IsValid() || !m_nview.IsOwner() || IsHungry() || m_baseAI.IsAlerted())
		{
			return;
		}
		if (((Character)this).IsTamed())
		{
			int level = ((Character)this).GetLevel();
			if (level >= m_maxLevel)
			{
				((MonoBehaviour)this).CancelInvoke("TamingUpdate");
				return;
			}
			long ticks = ZNet.instance.GetTime().Ticks;
			long num = m_nview.GetZDO().GetLong(VikingVars.lastLevelUpTime, 0L);
			if (num == 0)
			{
				return;
			}
			long ticks2 = ticks - num;
			double totalSeconds = new TimeSpan(ticks2).TotalSeconds;
			float num2 = m_secondsToLevelUp * (float)level;
			if (!(totalSeconds < (double)num2))
			{
				((Character)this).SetLevel(level + 1);
				m_skillLevelupEffects.Create(((Component)this).transform.position, Quaternion.identity, (Transform)null, 1f, -1);
				m_nview.GetZDO().Set(VikingVars.lastLevelUpTime, ticks);
				Player closestPlayer = Player.GetClosestPlayer(((Component)this).transform.position, 20f);
                if (closestPlayer != null)
                {
					string text = GetText() + " $norseman_leveledup";
                    ((Character)closestPlayer).Message(
						(MessageHud.MessageType)2,
						text,
						0,
						(Sprite)null,
						false);
                }
			}
		}
		else
		{
			((MonsterAI)m_vikingAI).SetDespawnInDay(false);
			((MonsterAI)m_vikingAI).SetEventCreature(false);
			DecreaseRemainingTime(3f);
			float remainingTime = GetRemainingTime();
			if (remainingTime <= 0f)
			{
				Tame();
			}
			else
			{
				m_sootheEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			}
		}
	}

	public void Tame()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (!NorsemanConfigs.IsTameable(prefabName))
		{
			return;
		}
		Game.instance.IncrementPlayerStat((PlayerStatType)48, 1f);
		if (m_nview.IsValid() && m_nview.IsOwner() && !((Character)this).IsTamed())
		{
			((MonsterAI)m_vikingAI).MakeTame();
			m_tamedEffect.Create(((Component)this).transform.position, ((Component)this).transform.rotation);
			Player closestPlayer = Player.GetClosestPlayer(((Component)this).transform.position, 30f);
            if (closestPlayer != null)
            {
                ((Character)closestPlayer).Message(
					(MessageHud.MessageType)2,
					((Character)this).m_name + " $hud_tamedone",
					0,
					(Sprite)null,
					false);
            }
			m_nview.GetZDO().Set(VikingVars.lastLevelUpTime, ZNet.instance.GetTime().Ticks);
		}
	}

	public void DecreaseRemainingTime(float time)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (!m_nview.IsValid())
		{
			return;
		}
		float remainingTime = GetRemainingTime();
		s_nearbyPlayers.Clear();
		Player.GetPlayersInRange(((Component)this).transform.position, m_tamingSpeedMultiplierRange, s_nearbyPlayers);
		foreach (Player s_nearbyPlayer in s_nearbyPlayers)
		{
            if (((Character)s_nearbyPlayer).GetSEMan().HaveStatusAttribute((StatusEffect.StatusAttribute)8))
            {
				time *= m_tamingBoostMultiplier;
			}
		}
		float num = remainingTime - time;
		if ((double)num < 0.0)
		{
			num = 0f;
		}
		m_nview.GetZDO().Set(ZDOVars.s_tameTimeLeft, num);
	}

	public int GetTameness()
	{
		return (int)((1.0 - (double)Mathf.Clamp01(GetRemainingTime() / m_tamingTime)) * 100.0);
	}

	public float GetRemainingTime()
	{
		if (!m_nview.IsValid())
		{
			return 0f;
		}
		return m_nview.GetZDO().GetFloat(ZDOVars.s_tameTimeLeft, m_tamingTime);
	}

	public void RPC_SetText(long sender, string text)
	{
		if (m_nview.IsValid() && m_nview.IsOwner() && ((Character)this).IsTamed())
		{
			m_nview.GetZDO().Set(ZDOVars.s_tamedName, text);
		}
	}

	public string GetName()
	{
		return Localization.instance.Localize(GetText());
	}

	public string GetText()
	{
		if (!m_nview.IsValid())
		{
			return string.Empty;
		}
		string text = m_nview.GetZDO().GetString(ZDOVars.s_tamedName, "");
		return string.IsNullOrEmpty(text) ? ((Character)this).m_name : text;
	}

	public void SetText(string text)
	{
		if (m_nview.IsValid())
		{
			m_nview.InvokeRPC("RPC_SetText", new object[1] { text });
		}
	}

	public string GetStatusString()
	{
		if (((BaseAI)m_vikingAI).IsAlerted())
		{
			return "$hud_tamefrightened";
		}
		if (IsHungry())
		{
			return "$hud_tamehungry";
		}
		return ((Character)this).IsTamed() ? "$hud_tamehappy" : "$hud_tameinprogress";
	}

    public override void RaiseSkill(Skills.SkillType skill, float value = 1f)
    {
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (!((Character)this).IsTamed() || (int)m_levelUpOwnerSkill == 0)
		{
			return;
		}
		GameObject followTarget = GetFollowTarget();
        if (followTarget == null)
        {
			return;
		}
		Character component = followTarget.GetComponent<Character>();
        if (component != null)
        {
			Skills skills = component.GetSkills();
            if (skills != null)
            {
				skills.RaiseSkill(m_levelUpOwnerSkill, value * m_levelUpFactor);
			}
		}
	}

	public bool IsHungry()
	{
        if (m_nview == null)
        {
			return false;
		}
		if (!NorsemanConfigs.IsTameable(prefabName))
		{
			return false;
		}
		ZDO zDO = m_nview.GetZDO();
		if (zDO == null)
		{
			return false;
		}
		DateTime dateTime = new DateTime(zDO.GetLong(ZDOVars.s_tameLastFeeding, 0L));
		return (ZNet.instance.GetTime() - dateTime).TotalSeconds > (double)m_fedDuration;
	}

	public void ResetFeedingTimer()
	{
		m_nview.GetZDO().Set(ZDOVars.s_tameLastFeeding, ZNet.instance.GetTime().Ticks);
	}

    public bool StartWork(ItemDrop.ItemData item)
    {
        if ((InAttack() && HaveQueuedChain()) ||
            InDodge() ||
            !CanMove() ||
            IsKnockedBack() ||
            IsStaggering() ||
            InMinorAction())
        {
            return false;
        }

        if (item == null)
        {
            return false;
        }
        m_isWorking = true;
        return StartAttack(null, false);
    }

    public void CheckLastWork()
	{
		string text = m_nview.GetZDO().GetString(VikingVars.lastWorkTargetResource, "");
		if (!string.IsNullOrEmpty(text))
		{
			long ticks = m_nview.GetZDO().GetLong(VikingVars.lastWorkTime, ZNet.instance.GetTime().Ticks);
			DateTime dateTime = new DateTime(ticks);
			double totalSeconds = (ZNet.instance.GetTime() - dateTime).TotalSeconds;
			int num = (int)(totalSeconds / (double)NorsemanConfigs.GetWorkResourceInterval(prefabName));
			if (num <= 0)
			{
				num = 1;
			}
			((Humanoid)this).GetInventory().AddItem(text, num, 1, 0, 0L, "", false);
			NorsemenPlugin.LogDebug($"[{GetName()}] received {text} x{num}");
			ResetLastWork();
		}
	}

	public void ResetLastWork()
	{
		m_nview.GetZDO().Set(VikingVars.lastWorkTime, ZNet.instance.GetTime().Ticks);
		m_nview.GetZDO().Set(VikingVars.lastWorkTargetResource, string.Empty);
	}

	public Viking()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Expected O, but got Unknown
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Expected O, but got Unknown
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Expected O, but got Unknown
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Expected O, but got Unknown
		m_dodgeEffects = new EffectList();
		m_hairColor = Vector3.one;
		m_skinColor = Vector3.one;
		m_isElf = false;
		m_maxRange = 15f;
		m_greetRange = 10f;
		m_byeRange = 15f;
		m_offset = 2f;
		m_minTalkInterval = 1.5f;
		m_hideDialogDelay = 5f;
		m_randomTalkInterval = 10f;
		m_randomTalkChance = 1f;
		m_queuedTexts = new Queue<QueuedSay>();
		m_greetEmotes = new List<string> { "emote_wave", "emote_bow" };
		m_randomEmote = new List<string>
		{
			"emote_dance", "emote_despair", "emote_cry", "emote_point", "emote_flex", "emote_challenge", "emote_cheer", "emote_blowkiss", "emote_comehere", "emote_laugh",
			"emote_roar", "emote_shrug"
		};
		m_lastDataString = string.Empty;
		m_vikingAI = null;
		m_tombstone = null;
		m_spawnEffects = new EffectList();
		m_skillLevelupEffects = new EffectList();
		m_equipStartEffects = new EffectList();
		m_perfectDodgeEffects = new EffectList();
		prefabName = "";
		m_fedDuration = 300f;
		m_tamingTime = 1800f;
		m_tamingSpeedMultiplierRange = 60f;
		m_tamingBoostMultiplier = 2f;
		m_levelUpFactor = 1f;
		m_maxLevel = 3;
		m_secondsToLevelUp = 1800f;
		
	}
}
