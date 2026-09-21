using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DropTable;
using Object = UnityEngine.Object;
namespace Norsemen;

public class VikingAI : MonsterAI
{
    [HarmonyPatch(typeof(TreeBase), "RPC_Damage")]
    private static class TreeBase_RPC_Damage_Patch
    {
        private static void Postfix(
            TreeBase __instance,
            HitData hit,
            ZNetView ___m_nview)
        {
            if (___m_nview != null &&
                ___m_nview.IsOwner() &&
                hit != null &&
                hit.GetAttacker() is Viking &&
                hit.GetTotalDamage() <= 0f)
            {
                Traverse.Create(__instance)
                    .Method("Shake")
                    .GetValue();
            }
        }
    }

    [HarmonyPatch(typeof(FishingFloat), "GetOwner")]
    private static class FishingFloat_GetOwner_Patch
    {
        private static void Postfix(
            ref Character __result,
            ZNetView ___m_nview)
        {
            // Vanilla already found a player owner.
            if (__result != null)
            {
                return;
            }

            if (___m_nview == null || !___m_nview.IsValid())
            {
                return;
            }

            long rodOwner = ___m_nview.GetZDO().GetLong(ZDOVars.s_rodOwner, 0L);

            foreach (Viking instance in Viking.instances)
            {
                ZNetView vikingNView = instance.GetComponent<ZNetView>();

                if (vikingNView == null || !vikingNView.IsValid())
                {
                    continue;
                }

                ZDO zdo = vikingNView.GetZDO();

                if (zdo != null && zdo.m_uid.UserID == rodOwner)
                {
                    __result = instance;
                    break;
                }
            }
        }
    }

    public Emotion m_behaviour = Emotion.Aggressive;

	public Movement m_moveType = Movement.Patrol;

	public float m_behaviourUpdateTimer;

	public float m_blockTimer;

	private float m_blockCheckInterval = 2f;

	public TreeBase m_tree;

	private float m_dodgeTimer = 0f;

	private float m_dodgeCooldown = 5f;

	private float m_dodgeCheckInterval = 0.2f;

	private float m_dodgeCheckTimer = 0f;

	private float m_dodgeDistance = 15f;

	private float m_dodgeThreatDistance = 5f;

	public Fish m_fish;

    public ItemDrop.ItemData m_bait;

	public Heightmap.Biome m_currentBiome;

	public float m_biomeTimer;

	public float m_lastMoveAwayFromMountainTimer;

	public bool m_haveNonMountainPosition;

	public Vector3 m_moveAwayFromMountainPosition;

	public float m_lastMoveAwayFromTar;

	public bool m_haveNonTarPosition;

	public Vector3 m_moveAwayFromTarPosition;

	public Player m_followPlayer;

	public MineRock m_mineRock;

	public MineRock5 m_mineRock5;

	public Destructible m_destructible;

	public Vector3 m_lastMineRock5Point;

	public float m_shipAttachTimer;

	public float m_crouchTimer;

	private float m_crouchCheckInterval = 2f;

	private float m_workTargetSearchTimer;

	private float m_workTargetSearchInterval = 30f;

	private bool hasWorkTarget;

	public Viking m_viking = null;

	public float m_lastWorkActionTime;

	public float m_workInterval = 10f;

	public float m_lastResourceGainedTime;

    private Vector3 m_vikingLastKnownTargetPos = Vector3.zero;
    private bool m_vikingBeenAtLastPos;
    private float m_vikingTimeSinceAttacking;
    private float m_vikingUnableToAttackTargetTimer;
    private float m_vikingUpdateTargetTimer;
    private float m_vikingInterceptTime;
    private float m_vikingPauseTimer;
    private ItemDrop m_vikingConsumeTarget;
    private float m_vikingConsumeSearchTimer;
    private static int m_vikingItemMask = 0;
    private Character m_vikingTargetCreature;
    private StaticTarget m_vikingTargetStatic;
 
    private float m_vikingTimeSinceSensedTargetCreature;
    //private float m_vikingMaxChaseDistance;
    private float m_vikingLastDespawnInDayCheck;
    private bool m_vikingDespawnInDay;
    private float m_vikingLastEventCreatureCheck;
    private bool m_vikingEventCreature;
    private static readonly System.Reflection.MethodInfo UpdateSleepMethod =
        AccessTools.Method(typeof(MonsterAI), "UpdateSleep");
    private static readonly AccessTools.FieldRef<ZNetScene, Dictionary<ZDO, ZNetView>> ZNetSceneInstances =
    AccessTools.FieldRefAccess<ZNetScene, Dictionary<ZDO, ZNetView>>("m_instances");

    private static readonly System.Reflection.MethodInfo SelectBestAttackMethod =
    AccessTools.Method(typeof(MonsterAI), "SelectBestAttack");
    public bool UpdateAttack(float dt, ItemDrop.ItemData itemData, bool doAttack, bool canHearTarget, bool canSeeTarget, bool isTamed)
    {
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		if (itemData == null)
		{
			return false;
		}
        ItemDrop.ItemData.AiTarget aiTargetType = itemData.m_shared.m_aiTargetType;
        ItemDrop.ItemData.AiTarget val = aiTargetType;
        if ((int)val != 0)
		{
            if ((int)val - 1 <= 1) 
            {
				return UpdateFriendAttack(dt, itemData, doAttack, isTamed);
			}
			return false;
		}
		return UpdateEnemyAttack(dt, itemData, doAttack, canHearTarget, canSeeTarget, isTamed);
	}

    private bool CanConsume(ItemDrop.ItemData item)
    {
        foreach (ItemDrop consumeItem in m_consumeItems)
        {
            if (consumeItem.m_itemData.m_shared.m_name == item.m_shared.m_name)
            {
                return true;
            }
        }

        return false;
    }

    private ItemDrop FindClosestConsumableItem(float maxRange)
    {
        if (m_vikingItemMask == 0)
        {
            m_vikingItemMask = LayerMask.GetMask("item");
        }

        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            maxRange,
            m_vikingItemMask);

        ItemDrop closestItem = null;
        float closestDistance = 999999f;

        foreach (Collider collider in colliders)
        {
            if (!collider.attachedRigidbody)
            {
                continue;
            }

            ItemDrop itemDrop =
                collider.attachedRigidbody.GetComponent<ItemDrop>();

            if (itemDrop == null)
            {
                continue;
            }

            ZNetView nview = itemDrop.GetComponent<ZNetView>();

            if (nview == null ||
                !nview.IsValid() ||
                !CanConsume(itemDrop.m_itemData))
            {
                continue;
            }

            float distance = Vector3.Distance(
                itemDrop.transform.position,
                transform.position);

            if (closestItem == null || distance < closestDistance)
            {
                closestItem = itemDrop;
                closestDistance = distance;
            }
        }

        if (closestItem != null &&
            HavePath(closestItem.transform.position))
        {
            return closestItem;
        }

        return null;
    }

    private bool UpdateEnemyAttack(
		float dt,
		ItemDrop.ItemData itemData,
		bool doAttack,
		bool canHearTarget,
		bool canSeeTarget,
		bool isTamed)
	{
			if (GetStaticTarget() != null)
			{
				return HandleStaticTarget(dt, itemData, doAttack, isTamed);
			}

			if (GetTargetCreature() != null)
			{
				return HandleCreatureTarget(
					dt,
					itemData,
					doAttack,
					canHearTarget,
					canSeeTarget,
					isTamed);
			}

			return false;
    }

    public static float GetWeaponRange(ItemDrop.ItemData itemData)
    {
        //IL_0007: Unknown result type (might be due to invalid IL or missing references)
        //IL_000c: Unknown result type (might be due to invalid IL or missing references)
        //IL_000d: Unknown result type (might be due to invalid IL or missing references)
        //IL_000e: Unknown result type (might be due to invalid IL or missing references)
        //IL_000f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0011: Unknown result type (might be due to invalid IL or missing references)
        //IL_0013: Invalid comparison between Unknown and I4
        //IL_0017: Unknown result type (might be due to invalid IL or missing references)
        //IL_001a: Invalid comparison between Unknown and I4
        Skills.SkillType skillType = itemData.m_shared.m_skillType;

        if ((int)skillType >= 8 && (int)skillType <= 10 || (int)skillType == 14)
        {
            return UnityEngine.Random.Range(15f, 30f);
        }

        return itemData.m_shared.m_attack.m_attackRange;
    }

    private bool HandleStaticTarget(
		float dt,
		ItemDrop.ItemData itemData,
		bool doAttack,
		bool isTamed)
    {
        StaticTarget target = GetStaticTarget();
        if (target == null)
        {
            return false;
        }

        Vector3 targetPoint = target.FindClosestPoint(transform.position);
        float distance = Vector3.Distance(targetPoint, transform.position);

        bool inRange = distance < GetWeaponRange(itemData);
        bool canSee = CanSeeTarget(target);

        if (inRange && canSee)
        {
            Vector3 center = target.GetCenter();

            LookAt(center);

            if (itemData.m_shared.m_aiAttackMaxAngle == 0f)
            {
                ZLog.LogError(
                    "AI Attack Max Angle for " +
                    itemData.m_shared.m_name +
                    " is 0!");
            }

            bool lookingAt = IsLookingAt(
                center,
                itemData.m_shared.m_aiAttackMaxAngle,
                itemData.m_shared.m_aiInvertAngleCheck);

            if (lookingAt && doAttack)
            {
                DoWeaponAttack(null, isFriend: false);
                return true;
            }

            StopMoving();
            return true;
        }

        if (isTamed && m_moveType == Movement.Guard)
        {
            return false;
        }

        MoveTo(dt, targetPoint, 0f, IsAlerted());
        ChargeStop();

        return true;
    }

    private bool HandleCreatureTarget(
		float dt,
		ItemDrop.ItemData itemData,
		bool doAttack,
		bool canHearTarget,
		bool canSeeTarget,
		bool isTamed)
    {
        Character targetCreature = GetTargetCreature();

        if (targetCreature == null)
        {
            return false;
        }

        if ((canHearTarget || canSeeTarget) ||
            (((BaseAI)this).HuntPlayer() && targetCreature.IsPlayer()))
        {
            return HandleDetectedCreature(
                dt,
                itemData,
                doAttack,
                canSeeTarget,
                isTamed);
        }

        return HandleLostCreature(dt, isTamed);
    }

    private bool HandleDetectedCreature(
		float dt,
		ItemDrop.ItemData itemData,
		bool doAttack,
		bool canSeeTarget,
		bool isTamed)
    {
        Character targetCreature = GetTargetCreature();

        if (targetCreature == null)
        {
            return false;
        }

        m_vikingBeenAtLastPos = false;
        m_vikingLastKnownTargetPos = targetCreature.transform.position;

        float num =
            Vector3.Distance(m_vikingLastKnownTargetPos, transform.position)
            - targetCreature.GetRadius();

        float num2 =
            m_alertRange * targetCreature.GetStealthFactor();

        if (canSeeTarget && num < num2)
        {
            SetAlerted(true);
        }

        float weaponRange = GetWeaponRange(itemData);
        bool flag = num < weaponRange;

        bool flag2 =
            !flag ||
            !canSeeTarget ||
            !IsAlerted();

        if ((flag2 & isTamed) && m_moveType == Movement.Guard)
        {
            return true;
        }

        if (flag2)
        {
            Vector3 val = CalculateInterceptPosition(num);

            MoveTo(
                dt,
                val,
                0f,
                IsAlerted());

            if (m_vikingTimeSinceAttacking > 15f)
            {
                m_vikingUnableToAttackTargetTimer = 15f;
            }

            return true;
        }

        StopMoving();

        bool flag3 = IsAlerted();

        if (flag & canSeeTarget & flag3)
        {
            if (VikingPheromoneFleeCheck(targetCreature))
            {
                Flee(dt, targetCreature.transform.position);

                m_vikingUpdateTargetTimer =
					UnityEngine.Random.Range(
							m_fleePheromoneMin,
							m_fleePheromoneMax);

                
            }
            else
            {
                LookAt(targetCreature.GetTopPoint());

                bool flag4 = IsLookingAt(
                    m_vikingLastKnownTargetPos,
                    itemData.m_shared.m_aiAttackMaxAngle,
                    itemData.m_shared.m_aiInvertAngleCheck);

                if (doAttack & flag4)
                {
                    DoWeaponAttack(targetCreature, false);
                    return true;
                }
            }
        }
        else if (flag3)
        {
            UpdateDodge(dt, targetCreature);
        }

        return false;
    }

    private bool VikingPheromoneFleeCheck(Character target)
    {
        if (target is Player player)
        {
            foreach (StatusEffect statusEffect in player.GetSEMan().GetStatusEffects())
            {
                if (statusEffect is SE_Stats seStats && seStats.m_pheromoneFlee)
                {
                    Character pheromoneTarget =
                        seStats.m_pheromoneTarget?.GetComponent<Character>();

                    if (pheromoneTarget != null &&
                        pheromoneTarget.m_name == m_viking.m_name)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private Vector3 CalculateInterceptPosition(float distanceToTarget)
    {
        Character targetCreature = GetTargetCreature();

        if (targetCreature == null)
        {
            return m_vikingLastKnownTargetPos;
        }

        Vector3 velocity = targetCreature.GetVelocity();
        Vector3 val = velocity * m_vikingInterceptTime;
        Vector3 val2 = m_vikingLastKnownTargetPos;

        if (distanceToTarget > val.magnitude / 4f)
        {
            val2 += val;
        }

        return val2;
    }

    private bool HandleLostCreature(float dt, bool isTamed)
    {
        ChargeStop();

        bool flag = !isTamed || m_moveType == Movement.Patrol;

        if (m_vikingBeenAtLastPos)
        {
            if (flag)
            {
                RandomMovement(dt, m_vikingLastKnownTargetPos, false);
            }

            if (m_vikingTimeSinceAttacking > 15f)
            {
                m_vikingUnableToAttackTargetTimer = 15f;
            }

            return true;
        }

        if (flag && MoveTo(
            dt,
            m_vikingLastKnownTargetPos,
            0f,
            IsAlerted()))
        {
            m_vikingBeenAtLastPos = true;
            return true;
        }

        return false;
    }

    private bool UpdateFriendAttack(float dt, ItemDrop.ItemData itemData, bool doAttack, bool isTamed)
    {
        Character val = ((int)itemData.m_shared.m_aiTargetType == 1)
            ? HaveHurtFriendInRange(m_viewRange)
            : HaveFriendInRange(m_viewRange);

        bool flag = !isTamed || m_moveType == Movement.Patrol;

        if (val == null)
        {
            if (flag)
            {
                RandomMovement(dt, transform.position, true);
            }

            return false;
        }

        float num = Vector3.Distance(val.transform.position, transform.position);
        float weaponRange = GetWeaponRange(itemData);

        if (num < weaponRange)
        {
            if (doAttack)
            {
                StopMoving();
                LookAt(val.transform.position);
                DoWeaponAttack(val, isFriend: true);
                return true;
            }

            if (flag)
            {
                RandomMovement(dt, val.transform.position, false);
                return true;
            }
        }
        else if (flag)
        {
            MoveTo(dt, val.transform.position, 0f, IsAlerted());
            return true;
        }

        return false;
    }

    public bool DoWeaponAttack(Character target, bool isFriend)
	{
        ItemDrop.ItemData currentWeapon = m_viking.GetCurrentWeapon();
        if (currentWeapon == null || !((BaseAI)this).CanUseAttack(currentWeapon))
		{
			return false;
		}
		bool flag = !string.IsNullOrEmpty(currentWeapon.m_shared.m_secondaryAttack.m_attackAnimation) && UnityEngine.Random.value > 0.5f;
		if (!((Character)m_viking).StartAttack(target, flag))
		{
			return false;
		}
        m_vikingTimeSinceAttacking = 0f;
        return true;
	}

    public bool UpdateAvoidFire(float dt, bool isTamed)
    {
        if (m_viking.IsHoldingTorch())
        {
            return false;
        }

        if (isTamed && m_moveType == Movement.Guard)
        {
            return false;
        }

        Character targetCreature = GetTargetCreature();

        if ((m_afraidOfFire || m_avoidFire) &&
            AvoidFire(dt, targetCreature, m_afraidOfFire))
        {
            return true;
        }

        return false;
    }

    public bool UpdateBaseAI(float dt)
    {
        if (!m_nview.IsValid())
        {
            return false;
        }

        if (!m_nview.IsOwner())
        {
            return false;
        }

        m_timeSinceHurt -= dt;

        return true;
    }

    public void SetEmotion(int state)
    {
        if (state != (int)m_behaviour)
        {
            m_behaviour = (Emotion)state;
            m_nview.GetZDO().Set(VikingVars.behaviour, state, false);
        }
    }

    public void SetMovement(int state)
    {
        if (state != (int)m_moveType)
        {
            m_moveType = (Movement)state;
            m_nview.GetZDO().Set(VikingVars.patrol, state, false);
        }
    }

    public void UpdateBehaviour(float dt)
    {
        m_behaviourUpdateTimer -= dt;

        if (!(m_behaviourUpdateTimer < 1f))
        {
            return;
        }

        m_behaviourUpdateTimer = 0f;

        SetEmotion(
            m_nview.GetZDO().GetInt(VikingVars.behaviour, 0));

        SetMovement(
            m_nview.GetZDO().GetInt(VikingVars.patrol, 0));
    }

    public void UpdateBlock(float dt, Character target, bool canSeeTarget)
	{
		m_blockTimer += dt;
		if (m_blockTimer > m_blockCheckInterval)
		{
			m_blockTimer = 0f;
			UpdateBlockDecision(target, canSeeTarget);
		}
	}

	public void UpdateBlockDecision(Character target, bool canSeeTarget)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (!((BaseAI)this).IsAlerted())
		{
			m_viking.SetBlocking(block: false);
			return;
		}
        if (target == null)
        {
            m_viking.SetBlocking(block: false);
            return;
        }
        if (((Character)m_viking).GetHealthPercentage() < 0.25f)
		{
			m_viking.SetBlocking(block: true);
			return;
		}
		if (target.IsFlying())
		{
			m_viking.SetBlocking(block: false);
			return;
		}
		float num = Vector3.Distance(((Component)target).transform.position, ((Component)this).transform.position);
        BaseAI targetAI = target.GetComponent<BaseAI>();

        bool flag =
            (targetAI != null && targetAI.IsAlerted()) ||
            target.IsPlayer();
        if (canSeeTarget)
		{
			if (num < 10f)
			{
				if (flag)
				{
					m_viking.SetBlocking(UnityEngine.Random.value > 0.8f);
				}
				else
				{
					m_viking.SetBlocking(UnityEngine.Random.value > 0.5f);
				}
			}
            else if ((targetAI != null && targetAI.IsCharging()) || target.IsDrawingBow())
            {
				m_viking.SetBlocking(UnityEngine.Random.value > 0.6f);
			}
			else
			{
				m_viking.SetBlocking(UnityEngine.Random.value > 0.25f);
			}
		}
		else
		{
			m_viking.SetBlocking(block: false);
		}
	}
    private bool VikingDespawnInDay()
    {
        if (Time.time - m_vikingLastDespawnInDayCheck > 4f)
        {
            m_vikingLastDespawnInDayCheck = Time.time;

            if (m_nview != null && m_nview.IsValid())
            {
                m_vikingDespawnInDay =
                    m_nview.GetZDO().GetBool(
                        ZDOVars.s_despawnInDay,
                        m_vikingDespawnInDay);
            }
        }

        return m_vikingDespawnInDay;
    }

    private bool VikingIsEventCreature()
    {
        if (Time.time - m_vikingLastEventCreatureCheck > 4f)
        {
            m_vikingLastEventCreatureCheck = Time.time;

            if (m_nview != null && m_nview.IsValid())
            {
                m_vikingEventCreature =
                    m_nview.GetZDO().GetBool(
                        ZDOVars.s_eventCreature,
                        m_vikingEventCreature);
            }
        }

        return m_vikingEventCreature;
    }

    public void UpdateChargeAttack(float dt, bool hasItem, bool hasTarget, bool doAttack, ItemDrop.ItemData itemData)
    {
		bool flag = hasItem & hasTarget & doAttack;
        bool flag2 = !m_character.InAttack();
        bool flag3 = itemData?.m_shared.m_attack != null && !itemData.m_shared.m_attack.IsDone() && !string.IsNullOrEmpty(itemData.m_shared.m_attack.m_drawAnimationState);
		if ((((BaseAI)this).IsCharging() | flag) & flag2 & flag3)
		{
			((BaseAI)this).ChargeStart(itemData?.m_shared.m_attack.m_drawAnimationState);
		}
	}

    public bool UpdateLogging(float dt, ItemDrop.ItemData axe, bool isFollowing)
    {
        //IL_0090: Unknown result type (might be due to invalid IL or missing references)
        //IL_004d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0058: Unknown result type (might be due to invalid IL or missing references)
        //IL_00cb: Unknown result type (might be due to invalid IL or missing references)
        if (m_tree == null)
        {
			return false;
		}
        if (m_tree.gameObject == null)
        {
			ResetWorkTargets();
			return false;
		}
		if (isFollowing)
		{
			float num = Vector3.Distance(((Component)m_tree).transform.position, ((Component)this).transform.position);
			if (num > 20f)
			{
				ResetWorkTargets();
				return false;
			}
		}
        if (!MoveTo(dt, m_tree.transform.position, axe.m_shared.m_attack.m_attackRange, false))
        {
			return true;
		}
		((BaseAI)this).StopMoving();
        LookAt(m_tree.transform.position);
        DoChopping(axe, m_tree);
		m_lastWorkActionTime = Time.time;
        m_nview.GetZDO().Set(VikingVars.lastWorkTime, ZNet.instance.GetTime().Ticks);
        return true;
	}

    public void DoChopping(ItemDrop.ItemData axe, TreeBase tree)
    {
        if (m_viking.StartWork(axe) && tree.m_logPrefab != null)
        {
            TreeLog component = tree.m_logPrefab.GetComponent<TreeLog>();

            if (component != null)
            {
                if (component.m_subLogPrefab != null)
                {
                    component = component.m_subLogPrefab.GetComponent<TreeLog>();
                }

                List<DropTable.DropData> drops =
                    component.m_dropWhenDestroyed.m_drops;

                AddResources(drops);
            }
        }

        if (tree.m_logPrefab == null)
        {
            return;
        }

        TreeLog component2 = tree.m_logPrefab.GetComponent<TreeLog>();

        if (component2 != null)
        {
            if (component2.m_subLogPrefab != null)
            {
                component2 = component2.m_subLogPrefab.GetComponent<TreeLog>();
            }

            List<DropTable.DropData> drops2 =
                component2.m_dropWhenDestroyed.m_drops;

            AddResources(drops2);
        }
    }

    public bool UpdateCircleTarget(float dt, bool isTamed)
    {
        if (isTamed && m_moveType == Movement.Guard)
        {
            return false;
        }

        Character targetCreature = GetTargetCreature();

        if (m_circleTargetInterval > 0f && targetCreature != null)
        {
            m_vikingPauseTimer += dt;

            if (m_vikingPauseTimer > m_circleTargetInterval)
            {
                if (m_vikingPauseTimer >
                    m_circleTargetInterval + m_circleTargetDuration)
                {
                    m_vikingPauseTimer =
                        UnityEngine.Random.Range(
                            0f,
                            m_circleTargetInterval / 10f);
                }

                RandomMovementArroundPoint(
                    dt,
                    targetCreature.transform.position,
                    m_circleTargetDistance,
                    IsAlerted());

                return true;
            }
        }

        return false;
    }

    public bool UpdateCirculate(float dt, bool hasItem, bool doAttack, bool isTamed)
    {
        if (isTamed && m_moveType == Movement.Guard)
        {
            return false;
        }

        bool flag = m_character.IsFlying()
            ? m_circulateWhileChargingFlying
            : m_circulateWhileCharging;

        if (flag && hasItem && !doAttack && !m_character.InAttack())
        {
            Character targetCreature = GetTargetCreature();

            if (targetCreature != null)
            {
                RandomMovementArroundPoint(
                    dt,
                    targetCreature.transform.position,
                    m_randomMoveRange,
                    IsAlerted());

                return true;
            }

            StaticTarget targetStatic = GetStaticTarget();

            if (targetStatic != null)
            {
                RandomMovementArroundPoint(
                    dt,
                    targetStatic.transform.position,
                    m_randomMoveRange,
                    IsAlerted());

                return true;
            }
        }

        return false;
    }

    public bool UpdateConsume(float dt, Viking character, bool hasTarget, bool isTamed)
	{
		if (base.m_consumeItems == null || base.m_consumeItems.Count == 0)
		{
			return false;
		}
		if (!((BaseAI)this).IsAlerted() && !hasTarget && UpdateConsumeSearch(character, isTamed, dt))
		{
			return true;
		}
		return false;
	}

	public bool IsFoodItem(ItemDrop.ItemData item)
	{
		return base.m_consumeItems.Exists((ItemDrop i) => i.m_itemData.m_shared.m_name == item.m_shared.m_name);
	}

    public bool EatInventoryFood(Viking viking)
    {
        Inventory inventory = ((Humanoid)viking).GetInventory();

        List<ItemDrop.ItemData> allItemsOfType =
            inventory.GetAllItemsOfType(
                ItemDrop.ItemData.ItemType.Consumable,
                false);

        ItemDrop.ItemData val = null;

        if (allItemsOfType.Count > 0)
        {
            foreach (ItemDrop.ItemData item in allItemsOfType)
            {
                if (IsFoodItem(item))
                {
                    val = item;
                    break;
                }
            }
        }

        if (val == null)
        {
            return false;
        }

        viking.OnConsumedItem(val);
        inventory.RemoveItem(val, 1);

        return true;
    }

    public bool UpdateConsumeSearch(Viking viking, bool isTamed, float dt)
    {
        m_vikingConsumeSearchTimer += dt;

        if (m_vikingConsumeSearchTimer <= m_consumeSearchInterval)
        {
            return false;
        }

        m_vikingConsumeSearchTimer = 0f;

        if (!viking.IsHungry())
        {
            return false;
        }

        if (isTamed && EatInventoryFood(viking))
        {
            return false;
        }

        if (isTamed && m_moveType == Movement.Guard)
        {
            return false;
        }

        m_vikingConsumeTarget =
            FindClosestConsumableItem(m_consumeSearchRange);

        if (m_vikingConsumeTarget == null)
        {
            return false;
        }

        if (!MoveTo(
            dt,
            m_vikingConsumeTarget.transform.position,
            m_consumeRange,
            false))
        {
            return true;
        }

        LookAt(m_vikingConsumeTarget.transform.position);

        if (Vector3.Distance(
                transform.position,
                m_vikingConsumeTarget.transform.position) <= m_consumeRange)
        {
            Inventory inventory = viking.GetInventory();

            viking.ConsumeItem(
                inventory,
                m_vikingConsumeTarget.m_itemData,
                true);

            m_vikingConsumeTarget = null;
        }

        return true;
    }

    public void UpdateTargets(
		Viking viking,
		bool isTamed,
		float dt,
		out bool canHearTarget,
		out bool canSeeTarget)
    {
        m_vikingUnableToAttackTargetTimer -= dt;
        m_vikingUpdateTargetTimer -= dt;

        if (m_vikingUpdateTargetTimer <= 0f && !m_viking.InAttack())
        {
            UpdateTargetSearch(isTamed);
        }

        Character targetCreature = GetTargetCreature();

        if (targetCreature != null && isTamed)
        {
            CheckTamedTargetDistance();
        }

        ValidateCurrentTarget();

        UpdateTargetSensing(
            dt,
            out canHearTarget,
            out canSeeTarget);

        CheckGiveUpChase(dt);
    }

    private void UpdateTargetSearch(bool isTamed)
    {
        m_vikingUpdateTargetTimer =
            Player.IsPlayerInRange(transform.position, 50f) ? 2f : 6f;

        if (isTamed && m_behaviour == Emotion.Passive)
        {
            SetTarget(null);
        }
        else
        {
            Character target = FindEnemy();

            if (target != null)
            {
                SetTarget(target);
            }
        }

        Character targetCreature = GetTargetCreature();

        bool hasPlayerTarget =
            targetCreature != null &&
            targetCreature.IsPlayer();

        bool flag =
            targetCreature != null &&
            m_vikingUnableToAttackTargetTimer > 0f &&
            HavePath(targetCreature.transform.position);

        bool flag2 =
            m_attackPlayerObjects &&
            (!m_aggravatable ||
             IsAggravated()) &&
            !ZoneSystem.instance.GetGlobalKey((GlobalKeys)25);

        bool flag3 =
            targetCreature == null ||
            flag ||
            !isTamed;

        if (flag2 & flag3)
        {
            FindStaticTargets(hasPlayerTarget);
        }
    }

    private void SetTarget(Character attacker)
    {
        if (attacker != null &&
            m_vikingTargetCreature == null &&
            (!attacker.IsPlayer() || !m_viking.IsTamed()))
        {
            m_vikingTargetCreature = attacker;
            m_vikingLastKnownTargetPos = attacker.transform.position;
            m_vikingBeenAtLastPos = false;
            m_vikingTargetStatic = null;
        }
        else if (attacker == null)
        {
            m_vikingTargetCreature = null;
        }
    }

    



    private void FindStaticTargets(bool hasPlayerTarget)
    {
        StaticTarget target = FindClosestStaticPriorityTarget();

        if (target != null)
        {
            m_vikingTargetStatic = target;
            m_vikingTargetCreature = null;
        }

        bool havePath = false;

        if (m_vikingTargetStatic != null)
        {
            Vector3 closestPoint =
                m_vikingTargetStatic.FindClosestPoint(transform.position);

            havePath = HavePath(closestPoint);
        }

        if ((m_vikingTargetStatic == null || !havePath) &&
            IsAlerted() &&
            hasPlayerTarget)
        {
            StaticTarget randomTarget = FindRandomStaticTarget(10f);

            if (randomTarget != null)
            {
                m_vikingTargetStatic = randomTarget;
                m_vikingTargetCreature = null;
            }
        }
    }

    private void CheckTamedTargetDistance()
    {
        Character targetCreature = GetTargetCreature();

        if (targetCreature == null)
        {
            return;
        }

        float alertRange = m_alertRange;
        Vector3 patrolPoint = Vector3.zero;

        if (GetPatrolPoint(out patrolPoint))
        {
            if (Vector3.Distance(
                    targetCreature.transform.position,
                    patrolPoint) > alertRange)
            {
                SetTarget(null);
            }

            return;
        }

        GameObject followTarget = GetFollowTarget();

        if (followTarget != null &&
            Vector3.Distance(
                targetCreature.transform.position,
                followTarget.transform.position) > alertRange)
            SetTarget(null);
        
    }

    private void ValidateCurrentTarget()
    {
        Character targetCreature = GetTargetCreature();

        if (targetCreature == null)
        {
            return;
        }

        if (targetCreature.IsDead())
        {
            SetTarget(null);
        }
        else if (!IsEnemy(targetCreature))
        {
            SetTarget(null);
        }
        else if (m_skipLavaTargets && targetCreature.AboveOrInLava())
        {
            SetTarget(null);
        }
    }

    private void UpdateTargetSensing(
		float dt,
		out bool canHearTarget,
		out bool canSeeTarget)
    {
        canHearTarget = false;
        canSeeTarget = false;

        Character targetCreature = GetTargetCreature();

        if (!hasWorkTarget && targetCreature != null)
        {
            canHearTarget = CanHearTarget(targetCreature);
            canSeeTarget = CanSeeTarget(targetCreature);

            if (canSeeTarget || canHearTarget)
            {
                m_vikingTimeSinceSensedTargetCreature = 0f;
            }

            if (targetCreature.IsPlayer())
            {
                targetCreature.OnTargeted(
                    canSeeTarget | canHearTarget,
                    IsAlerted());
            }

            SetTargetInfo(targetCreature.GetZDOID());
        }
        else
        {
            SetTargetInfo(ZDOID.None);
        }

        m_vikingTimeSinceSensedTargetCreature += dt;
    }

    private void CheckGiveUpChase(float dt)
    {
        Character targetCreature = GetTargetCreature();

        if (!IsAlerted() && targetCreature == null)
        {
            return;
        }

        m_vikingTimeSinceAttacking += dt;

        if (!HuntPlayer() ||
            targetCreature == null ||
            !targetCreature.IsPlayer())
        {
            bool lostTarget =
                m_vikingTimeSinceSensedTargetCreature > 30f;

            float giveUpTime = 60f;

            bool attackTimeout =
                m_vikingTimeSinceAttacking > giveUpTime;

            bool exceededChaseDistance =
                 m_maxChaseDistance > 0f &&
                 m_vikingTimeSinceSensedTargetCreature > 1f &&
                 targetCreature != null &&
                 Vector3.Distance(
                     m_spawnPoint,
                     targetCreature.transform.position) > m_maxChaseDistance;

            if (lostTarget || attackTimeout || exceededChaseDistance)
            {
                SetAlerted(false);
                SetTarget(null);

                m_vikingTargetStatic = null;
                m_vikingTimeSinceAttacking = 0f;
                m_vikingUpdateTargetTimer = 5f;
            }
        }
    }

    public bool UpdateDespawn(float dt, bool canSeeTarget)
    {
        Character targetCreature = GetTargetCreature();

        if (VikingDespawnInDay() &&
            EnvMan.IsDay() &&
            (targetCreature == null || !canSeeTarget))
        {
            MoveAwayAndDespawn(dt, true);
            return true;
        }

        if (VikingIsEventCreature() &&
            !RandEventSystem.HaveActiveEvent())
        {
            SetHuntPlayer(false);

            if (targetCreature == null && !IsAlerted())
            {
                MoveAwayAndDespawn(dt, false);
                return true;
            }
        }

        return false;
    }

    public bool UpdateDodge(float dt, Character target)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		if (m_dodgeTimer > 0f)
		{
			m_dodgeTimer -= dt;
		}
		m_dodgeCheckTimer -= dt;
		if (m_dodgeCheckTimer > 0f)
		{
			return false;
		}
		m_dodgeCheckTimer = m_dodgeCheckInterval;
		if (m_dodgeTimer > 0f)
		{
			return false;
		}
        if (target == null)
        {
			return false;
		}
		if (!((BaseAI)this).IsAlerted() || !m_viking.CanDodge())
		{
			return false;
		}
		float num = Vector3.Distance(((Component)target).transform.position, ((Component)this).transform.position);
		if (num > m_dodgeDistance)
		{
			return false;
		}
		if (ShouldDodgeTarget(target, num))
		{
			Vector3 val = ((Component)target).transform.position - ((Component)this).transform.position;
            Vector3 dir = CalculateDodgeDirection(val.normalized, num);
            m_viking.Dodge(dir);
			m_dodgeTimer = m_dodgeCooldown;
			return true;
		}
		return false;
	}

	private bool ShouldDodgeTarget(Character target, float distance)
	{
		if (!target.InAttack() && !target.IsDrawingBow())
		{
			return false;
		}
		float num;
		if (distance < m_dodgeThreatDistance)
		{
			num = Mathf.Lerp(0.8f, 0.6f, distance / m_dodgeThreatDistance);
		}
		else
		{
			float num2 = (distance - m_dodgeThreatDistance) / (m_dodgeDistance - m_dodgeThreatDistance);
			num = Mathf.Lerp(0.6f, 0.1f, num2);
		}
		return UnityEngine.Random.value < num;
	}

	private Vector3 CalculateDodgeDirection(Vector3 toTargetNormalized, float distance)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (distance < m_dodgeThreatDistance)
		{
			return -toTargetNormalized;
		}
		Vector3 val = Vector3.Cross(Vector3.up, toTargetNormalized);
		float num = ((UnityEngine.Random.value > 0.5f) ? 1f : (-1f));
		Vector3 val2 = -toTargetNormalized + val * num;
        return val2.normalized;
    }

	public bool UpdateFishing(float dt, ItemDrop.ItemData fishingRod, bool isFollowing)
	{
        //IL_00d7: Unknown result type (might be due to invalid IL or missing references)
        //IL_0094: Unknown result type (might be due to invalid IL or missing references)
        //IL_009f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0107: Unknown result type (might be due to invalid IL or missing references)
        if (m_fish == null)
        {
			return false;
		}
        if (m_fish.gameObject == null)
        {
			ResetWorkTargets();
			return false;
		}
		if (m_bait == null || !((Humanoid)m_viking).GetInventory().ContainsItem(m_bait))
		{
			ResetWorkTargets();
			((Humanoid)m_viking).UnequipItem(fishingRod, true);
			return false;
		}
		if (isFollowing)
		{
			float num = Vector3.Distance(((Component)m_fish).transform.position, ((Component)this).transform.position);
			if (num > 20f)
			{
				ResetWorkTargets();
				return false;
			}
		}
        if (!MoveTo(dt, m_fish.transform.position, 15f, false))
        {
			return true;
		}
		((BaseAI)this).StopMoving();
        LookAt(m_fish.transform.position);
        CastFishingRod(fishingRod, m_fish);
		m_lastWorkActionTime = Time.time;
        m_nview.GetZDO().Set(VikingVars.lastWorkTime, ZNet.instance.GetTime().Ticks);
        return true;
	}

    public void CastFishingRod(ItemDrop.ItemData fishingRod, Fish fish)
    {
        ItemDrop fishItemDrop = fish.GetComponent<ItemDrop>();

        if (fishItemDrop == null)
        {
            return;
        }

        if (m_viking.StartWork(fishingRod) &&
            m_viking.GetInventory().CanAddItem(fishItemDrop.m_itemData, 1))
        {
            m_viking.GetInventory().AddItem(
                fishItemDrop.m_itemData.m_dropPrefab.name,
                1,
                fishItemDrop.m_itemData.m_quality,
                0,
                0L,
                "",
                false);

            m_nview.GetZDO().Set(
                VikingVars.lastWorkTargetResource,
                fishItemDrop.m_itemData.m_dropPrefab.name);
        }
    }

    public bool UpdateFlee(float dt, bool isTamed, bool isAlerted)
	{
		if (isTamed && m_moveType == Movement.Guard)
		{
			return false;
		}
		if (UpdateFleeNotAlerted(dt, isAlerted))
		{
			return true;
		}
		if (UpdateFleeLowHealth(dt))
		{
			return true;
		}
		if (UpdateFleeLava(dt))
		{
			return true;
		}
		UpdateBiome(dt);
		if (UpdateFleeMountain(dt, 50f))
		{
			return true;
		}
		if (UpdateFleeTar(dt, 50f))
		{
			return true;
		}
		return false;
	}

    public bool UpdateFleeNotAlerted(float dt, bool isAlerted)
    {
        Character targetCreature = GetTargetCreature();

        if (m_fleeIfNotAlerted &&
            !HuntPlayer() &&
            targetCreature != null &&
            !isAlerted &&
            Vector3.Distance(
                targetCreature.transform.position,
                transform.position) < m_alertRange)
        {
            Flee(dt, targetCreature.transform.position);
            return true;
        }

        return false;
    }

    public bool UpdateFleeLowHealth(float dt)
    {
        Character targetCreature = GetTargetCreature();

        if (m_fleeIfLowHealth > 0f &&
            m_timeSinceHurt < m_fleeTimeSinceHurt &&
            targetCreature != null &&
            m_viking.GetHealthPercentage() < m_fleeIfLowHealth)
        {
            Flee(dt, targetCreature.transform.position);
            return true;
        }

        return false;
    }

    public bool UpdateFleeLava(float dt)
    {
        Character targetCreature = GetTargetCreature();

        if (m_fleeInLava &&
            m_viking.InLava() &&
            (targetCreature == null || targetCreature.AboveOrInLava()))
        {
            Flee(
                dt,
                m_viking.transform.position - m_viking.transform.forward);

            return true;
        }

        return false;
    }

    public void UpdateBiome(float dt)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		m_biomeTimer += dt;
		if (!(m_biomeTimer <= 1f))
		{
			m_biomeTimer = 0f;
			m_currentBiome = Heightmap.FindBiome(((Component)this).transform.position);
		}
	}

	public bool UpdateFleeMountain(float dt, float maxRange)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between Unknown and I4
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Invalid comparison between Unknown and I4
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		if ((int)m_currentBiome != 4)
		{
			return false;
		}
        HitData.DamageModifiers damageModifiers =
			 m_viking.GetDamageModifiers(null);

        HitData.DamageModifier modifier =
            damageModifiers.GetModifier(HitData.DamageType.Frost);
        if ((int)modifier == 1 || (int)modifier - 3 <= 2)
        {
			return false;
		}
		float num = (m_haveNonMountainPosition ? 2f : 0.5f);
		float num2 = Time.time - m_lastMoveAwayFromMountainTimer;
		if (num2 > num)
		{
			m_lastMoveAwayFromMountainTimer = Time.time;
			m_haveNonMountainPosition = false;
			for (int i = 0; i < 10; i++)
			{
				Vector3 val = ((Component)this).transform.position + Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward * UnityEngine.Random.Range(4f, maxRange);
                Heightmap.Biome val2 = Heightmap.FindBiome(val);
				if ((int)val2 != 4)
				{
					val.y = ZoneSystem.instance.GetSolidHeight(val);
					m_moveAwayFromMountainPosition = val;
					m_haveNonMountainPosition = true;
				}
			}
		}
		if (!m_haveNonMountainPosition)
		{
			return false;
		}
		((BaseAI)this).MoveTowards(m_moveAwayFromMountainPosition - ((Component)this).transform.position, true);
		return true;
	}

	public bool UpdateFleeTar(float dt, float maxRange)
	{
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if (!((Character)m_viking).GetSEMan().HaveStatusEffect(SEMan.s_statusEffectTared))
		{
			return false;
		}
		float num = (m_haveNonTarPosition ? 2f : 0.5f);
		float num2 = Time.time - m_lastMoveAwayFromTar;
		if (num2 > num)
		{
			m_lastMoveAwayFromTar = Time.time;
			m_haveNonTarPosition = false;
			for (int i = 0; i < 10; i++)
			{
				Vector3 val = ((Component)this).transform.position + Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward * UnityEngine.Random.Range(4f, maxRange);
				float liquidLevel = Floating.GetLiquidLevel(val, 1f, (LiquidType)1);
				if (!(val.y < liquidLevel))
				{
					m_moveAwayFromTarPosition = val;
					m_haveNonTarPosition = true;
				}
			}
		}
		if (!m_haveNonTarPosition)
		{
			return false;
		}
		((BaseAI)this).MoveTowards(m_moveAwayFromTarPosition - ((Component)this).transform.position, true);
		return true;
	}

	public bool UpdateFollow(float dt)
	{
        //IL_0007: Unknown result type (might be due to invalid IL or missing references)
        //IL_0017: Unknown result type (might be due to invalid IL or missing references)
        //IL_00df: Unknown result type (might be due to invalid IL or missing references)
        GameObject followTarget = GetFollowTarget();

        if (followTarget == null)
        {
            return false;
        }

        float num = Vector3.Distance(transform.position, followTarget.transform.position);
        bool flag = num > 5f;
        if (m_followPlayer != null)
        {
            flag |= m_followPlayer.IsRunning();
        }
        if (num < 3f)
		{
			return false;
		}
		if (num > 15f)
		{
            if (m_followPlayer != null && m_followPlayer.IsTeleporting())
            {
				((BaseAI)this).StopMoving();
				return true;
			}
			if (!((Character)m_viking).IsEncumbered())
			{
				TeleportToFollow();
				return true;
			}
		}
		if (((BaseAI)this).IsAlerted())
		{
			return false;
		}
        MoveTo(dt, followTarget.transform.position, 0f, flag);
        return true;
	}

    public void TeleportToFollow()
    {
        GameObject followTarget = GetFollowTarget();

        if (followTarget == null)
        {
            return;
        }

        m_viking.TeleportTo(
            followTarget.transform.position + followTarget.transform.forward * -2f,
            transform.rotation,
            false);
    }

    public bool UpdateHurt(float dt, bool isTamed)
    {
        if (isTamed && m_moveType == Movement.Guard)
        {
            return false;
        }

        bool flag =
            m_vikingTimeSinceAttacking > 30f &&
            m_timeSinceHurt < 20f;

        Character targetCreature = GetTargetCreature();

        if (m_fleeIfHurtWhenTargetCantBeReached &&
            targetCreature != null &&
            flag)
        {
            Flee(dt, targetCreature.transform.position);

            m_vikingLastKnownTargetPos = transform.position;
            m_vikingUpdateTargetTimer = 1f;

            return true;
        }

        return false;
    }

    public bool UpdateInventory(bool isTamed)
    {
        if (!m_viking.IsInUse())
        {
            return false;
        }

        if (isTamed)
        {
            if (m_viking.m_currentPlayer != null)
            {
                LookAt(m_viking.m_currentPlayer.GetTopPoint());
            }

            StopMoving();
            return true;
        }

        if (m_viking.m_currentPlayer != null)
        {
            float value = UnityEngine.Random.value;
            float chance = 0.1f;

            if (value < chance)
            {
                LookAt(m_viking.m_currentPlayer.GetEyePoint());
            }

            if (CanSeeTarget(m_viking.m_currentPlayer))
            {
                SetAggravated(true, (AggravatedReason)2);
            }
        }

        return false;
    }

	public bool UpdateMineRockMining(float dt, ItemDrop.ItemData pickaxe, bool isFollowing)
	{
        //IL_0090: Unknown result type (might be due to invalid IL or missing references)
        //IL_004d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0058: Unknown result type (might be due to invalid IL or missing references)
        //IL_00cb: Unknown result type (might be due to invalid IL or missing references)
        if (m_mineRock == null)
        {
            return false;
        }

        if (m_mineRock.gameObject == null)
        {
            ResetWorkTargets();
            return false;
        }
        if (isFollowing)
		{
            float num = Vector3.Distance(m_mineRock.transform.position, transform.position);
            if (num > 20f)
			{
				ResetWorkTargets();
				return false;
			}
		}
        if (!MoveTo(dt, m_mineRock.transform.position, pickaxe.m_shared.m_attack.m_attackRange, false))
        {
			return true;
		}
        StopMoving();
        LookAt(m_mineRock.transform.position);
        DoPickaxe(pickaxe, m_mineRock);
		m_lastWorkActionTime = Time.time;
        m_nview.GetZDO().Set(VikingVars.lastWorkTime, ZNet.instance.GetTime().Ticks);
        return true;
	}

    public bool UpdateMineRock5Mining(float dt, ItemDrop.ItemData pickaxe, bool isFollowing)
    {
        if (m_mineRock5 == null)
        {
            return false;
        }

        if (m_mineRock5.gameObject == null)
        {
            ResetWorkTargets();
            return false;
        }

        Vector3 closestPoint;

        if (m_lastMineRock5Point != Vector3.zero)
        {
            float distance = Vector3.Distance(
                m_lastMineRock5Point,
                transform.position);

            if (distance < 10f)
            {
                closestPoint = m_lastMineRock5Point;
            }
            else
            {
                FindNearestPoint(m_mineRock5, out closestPoint);
            }
        }
        else
        {
            FindNearestPoint(m_mineRock5, out closestPoint);
        }

        m_lastMineRock5Point = closestPoint;

        if (isFollowing)
        {
            float distance = Vector3.Distance(
                m_lastMineRock5Point,
                transform.position);

            if (distance > 20f)
            {
                ResetWorkTargets();
                return false;
            }
        }

        if (!MoveTo(
            dt,
            closestPoint,
            pickaxe.m_shared.m_attack.m_attackRange,
            false))
        {
            return true;
        }

        StopMoving();
        LookAt(closestPoint);

        DoPickaxe(pickaxe, m_mineRock5);

        m_lastWorkActionTime = Time.time;
        m_nview.GetZDO().Set(
            VikingVars.lastWorkTime,
            ZNet.instance.GetTime().Ticks);

        return true;
    }

    public void FindNearestPoint(MineRock5 mineRock5, out Vector3 closestPoint)
    {
        closestPoint = mineRock5.transform.position;

        Collider[] colliders = mineRock5.GetComponentsInChildren<Collider>();

        if (colliders == null || colliders.Length == 0)
        {
            return;
        }

        float closestDistance = float.MaxValue;

        foreach (Collider collider in colliders)
        {
            if (collider == null)
            {
                continue;
            }

            MeshCollider meshCollider = collider as MeshCollider;

            Vector3 point;

            if (meshCollider == null || meshCollider.convex)
            {
                point = collider.ClosestPoint(transform.position);
            }
            else
            {
                point = collider.ClosestPointOnBounds(transform.position);
            }

            float distance = Vector3.Distance(point, transform.position);

            if (distance < closestDistance)
            {
                closestPoint = point;
                closestDistance = distance;
            }
        }
    }

    public bool UpdateDestructibleMining(float dt, ItemDrop.ItemData pickaxe, bool isFollowing)
    {
        if (m_destructible == null)
        {
            return false;
        }

        if (m_destructible.gameObject == null)
        {
            ResetWorkTargets();
            return false;
        }

        Vector3 targetPoint;

        if (m_destructible.m_spawnWhenDestroyed != null)
        {
            Collider collider = m_destructible.GetComponentInChildren<Collider>();

            if (collider == null)
            {
                targetPoint = m_destructible.transform.position;
            }
            else
            {
                MeshCollider meshCollider = collider as MeshCollider;

                targetPoint = (meshCollider == null || meshCollider.convex)
                    ? collider.ClosestPoint(transform.position)
                    : collider.ClosestPointOnBounds(transform.position);
            }
        }
        else
        {
            targetPoint = m_destructible.transform.position;
        }

        if (isFollowing)
        {
            float distance = Vector3.Distance(targetPoint, transform.position);

            if (distance > 20f)
            {
                ResetWorkTargets();
                return false;
            }
        }

        if (!MoveTo(dt, targetPoint, pickaxe.m_shared.m_attack.m_attackRange, false))
        {
            return true;
        }

        StopMoving();
        LookAt(m_destructible.transform.position);

        DoPickaxe(pickaxe, m_destructible);

        m_lastWorkActionTime = Time.time;
        m_nview.GetZDO().Set(
            VikingVars.lastWorkTime,
            ZNet.instance.GetTime().Ticks);

        return true;
    }

    public void DoPickaxe(ItemDrop.ItemData pickaxe, MineRock mineRock)
    {
        bool started = m_viking.StartWork(pickaxe);

        NorsemenPlugin.LogDebug(
            $"[{m_viking.GetName()}] DoPickaxe MineRock - StartWork={started}, " +
            $"Pickaxe={pickaxe?.m_shared?.m_name}, " +
            $"Range={pickaxe?.m_shared?.m_attack?.m_attackRange}");

        if (started)
        {
            List<DropTable.DropData> drops = mineRock.m_dropItems.m_drops;
            AddResources(drops);
        }
    }

    public void DoPickaxe(ItemDrop.ItemData pickaxe, MineRock5 mineRock5)
    {
        if (m_viking.StartWork(pickaxe))
        {
            List<DropTable.DropData> drops = mineRock5.m_dropItems.m_drops;
            AddResources(drops);
        }
    }

    public void DoPickaxe(ItemDrop.ItemData pickaxe, Destructible destructible)
    {
        if (!m_viking.StartWork(pickaxe))
        {
            return;
        }

        List<DropTable.DropData> list = null;

        if (destructible.m_spawnWhenDestroyed != null)
        {
            MineRock5 component =
                destructible.m_spawnWhenDestroyed.GetComponent<MineRock5>();

            if (component != null)
            {
                list = component.m_dropItems.m_drops;
            }
        }
        else
        {
            DropOnDestroyed component2 =
                destructible.GetComponent<DropOnDestroyed>();

            if (component2 != null)
            {
                list = component2.m_dropWhenDestroyed.m_drops;
            }
        }

        if (list != null)
        {
            AddResources(list);
        }
    }

    public bool UpdateNoMonsterArea(float dt, bool isTamed)
    {
        if (isTamed)
        {
            return false;
        }

        Character targetCreature = GetTargetCreature();

        if (targetCreature != null)
        {
            EffectArea noMonsterArea =
                EffectArea.IsPointInsideNoMonsterArea(targetCreature.transform.position);

            if (noMonsterArea != null)
            {
                Flee(dt, targetCreature.transform.position);
                return true;
            }
        }
        else
        {
            EffectArea noMonsterArea =
                EffectArea.IsPointCloseToNoMonsterArea(transform.position);

            if (noMonsterArea != null)
            {
                Flee(dt, noMonsterArea.transform.position);
                return true;
            }
        }

        return false;
    }

    public bool UpdateAttach()
    {
        if (!m_viking.m_attached)
        {
            return false;
        }

        if (m_viking.m_attachPoint != null)
        {
            transform.position = m_viking.m_attachPoint.position;
            transform.rotation = m_viking.m_attachPoint.rotation;

            m_body.useGravity = false;

            Rigidbody componentInParent =
                m_viking.m_attachPoint.GetComponentInParent<Rigidbody>();

            Vector3 velocity;

            if (componentInParent == null)
            {
                velocity = Vector3.zero;
            }
            else
            {
                velocity = componentInParent.GetPointVelocity(transform.position);
                velocity.y = 0f;
            }

            m_body.linearVelocity = velocity;
            m_body.angularVelocity = Vector3.zero;

            

            return true;
        }

        m_viking.AttachStop();
        return false;
    }

    public void UpdateAttachShip(float dt)
    {
        m_shipAttachTimer += dt;

        if (m_shipAttachTimer < 10f)
        {
            return;
        }

        m_shipAttachTimer = 0f;

        GameObject followTarget = GetFollowTarget();

        if (m_viking.IsAttachedToShip())
        {
            Player player;

            if (followTarget == null ||
                !followTarget.TryGetComponent<Player>(out player))
            {
                m_viking.AttachStop();
                return;
            }

            Ship ship = m_viking.GetStandingOnShip();

            if (ship == null || !ship.IsPlayerInBoat(player))
            {
                m_viking.AttachStop();

                m_viking.TeleportTo(
                    player.transform.position + player.transform.forward * 2f,
                    transform.rotation,
                    false);
            }
        }
        else
        {
            Player player;

            if (followTarget == null ||
                !followTarget.TryGetComponent<Player>(out player))
            {
                return;
            }

            Ship ship = player.GetStandingOnShip();

            if (ship == null || !ship.IsPlayerInBoat(player))
            {
                return;
            }

            Chair[] chairs = ship.GetComponentsInChildren<Chair>();

            if (chairs == null)
            {
                return;
            }

            foreach (Chair chair in chairs)
            {
                Player closestPlayer =
                    Player.GetClosestPlayer(chair.transform.position, 0.1f);

                Viking nearestViking =
                    Viking.GetNearestViking(chair.transform.position, 0.1f);

                if (closestPlayer == null && nearestViking == null)
                {
                    m_viking.AttachStart(
                        chair.m_attachPoint,
                        null,
                        false,
                        false,
                        chair.m_inShip,
                        chair.m_attachAnimation,
                        chair.m_detachOffset,
                        null);

                    break;
                }
            }
        }
    }

    public bool UpdateSleeping(float dt)
    {
        UpdateSleepMethod.Invoke(this, new object[] { dt });
        return IsSleeping();
    }

    public void UpdateCrouch(float dt, Character target, bool canSeeTarget)
    {
        m_crouchTimer += dt;

        if (m_crouchTimer > m_crouchCheckInterval)
        {
            UpdateCrouchDecision(target, canSeeTarget);
            m_crouchTimer = 0f;
        }

        bool flag = m_viking.m_crouchToggled && m_viking.CanCrouch();

        m_animator.SetBool("crouching", flag);
    }

    private void UpdateCrouchDecision(Character target, bool canSeeTarget)
    {
        if (m_followPlayer != null && m_followPlayer.IsCrouching())
        {
            m_viking.m_crouchToggled = true;
            return;
        }

        if (target == null)
        {
            m_viking.m_crouchToggled = false;
            return;
        }

        if (!IsAlerted())
        {
            m_viking.m_crouchToggled = false;
            return;
        }

        float distance = Vector3.Distance(
            target.transform.position,
            transform.position);

        if (canSeeTarget)
        {
            if (distance > 20f && distance < 40f)
            {
                m_viking.m_crouchToggled =
                    UnityEngine.Random.value < 0.7f;
            }
            else if (distance >= 40f)
            {
                m_viking.m_crouchToggled =
                    UnityEngine.Random.value < 0.8f;
            }
            else
            {
                m_viking.m_crouchToggled =
                    UnityEngine.Random.value < 0.2f;
            }
        }
        else
        {
            m_viking.m_crouchToggled =
                UnityEngine.Random.value < 0.6f;
        }
    }

    public void OnWorkConfigChanged(object sender, EventArgs args)
	{
		ResetWorkTargets();
	}

	public void ResetWorkTargets()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		m_mineRock = null;
		m_mineRock5 = null;
		m_destructible = null;
		m_tree = null;
		m_lastMineRock5Point = Vector3.zero;
		m_fish = null;
		m_bait = null;
		hasWorkTarget = false;
	}

	public void FindWorkTargets(float dt)
	{
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		if (hasWorkTarget || m_viking.IsInUse())
		{
			return;
		}
		m_workTargetSearchTimer += dt;
		if (m_workTargetSearchTimer < m_workTargetSearchInterval)
		{
			return;
		}
		m_workTargetSearchTimer = 0f;
		bool flag = NorsemanConfigs.CanVikingMine(m_viking.prefabName);
		bool flag2 = NorsemanConfigs.CanVikingLumber(m_viking.prefabName);
		bool flag3 = NorsemanConfigs.CanVikingFish(m_viking.prefabName);
		bool flag4 = (NorsemanConfigs.DoesWorkRequireFood(m_viking.prefabName) || !m_viking.IsHungry()) && (flag | flag2 | flag3);
		ResetWorkTargets();
		if (!flag4 || (m_viking.m_pickaxe == null && m_viking.m_axe == null && m_viking.m_fishingRod == null))
		{
			return;
		}
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		float num3 = float.MaxValue;
		float num4 = float.MaxValue;
		float num5 = float.MaxValue;
		MineRock val = null;
		MineRock5 val2 = null;
		TreeBase val3 = null;
		Destructible val4 = null;
		Fish val5 = null;
        List<ZNetView> list = ZNetSceneInstances(ZNetScene.instance).Values.ToList();
        for (int i = 0; i < list.Count; i++)
		{
			ZNetView val6 = list[i];
			float num6 = Vector3.Distance(((Component)this).transform.position, ((Component)val6).transform.position);
			if (num6 > 50f)
			{
				continue;
			}
			if ((m_viking.m_pickaxe != null) & flag)
			{
				MineRock component = ((Component)val6).GetComponent<MineRock>();
				if ((Object)(object)component != (Object)null)
				{
					if (m_viking.m_pickaxe.m_shared.m_toolTier >= component.m_minToolTier && component.m_dropItems.m_drops.IsOreVein() && num6 < num)
					{
						num = num6;
						val = component;
					}
					continue;
				}
				MineRock5 component2 = ((Component)val6).GetComponent<MineRock5>();
				if ((Object)(object)component2 != (Object)null)
				{
					if (m_viking.m_pickaxe.m_shared.m_toolTier >= component2.m_minToolTier && component2.m_dropItems.m_drops.IsOreVein() && num6 < num2)
					{
						num2 = num6;
						val2 = component2;
					}
					continue;
				}
				Destructible component3 = ((Component)val6).GetComponent<Destructible>();
				if ((Object)(object)component3 != (Object)null)
				{
					if (m_viking.m_pickaxe.m_shared.m_toolTier < component3.m_minToolTier)
					{
						continue;
					}
                    if ((Object)(object)component3.m_spawnWhenDestroyed != (Object)null)
                    {
                        component2 = component3.m_spawnWhenDestroyed.GetComponent<MineRock5>();

                        if ((Object)(object)component2 != (Object)null)
                        {
                            if (!component2.m_dropItems.m_drops.IsOreVein())
                            {
                                continue;
                            }

                            if (num6 < num4)
                            {
                                val4 = component3;
                                num4 = num6;
                            }

                            continue;
                        }
                    }
                    DropOnDestroyed component4 = ((Component)component3).GetComponent<DropOnDestroyed>();
					if ((Object)(object)component4 != (Object)null)
					{
						if (component4.m_dropWhenDestroyed.m_drops.IsOreVein() && num6 < num4)
						{
							val4 = component3;
							num4 = num6;
						}
						continue;
					}
				}
			}
			if ((m_viking.m_axe != null) & flag2)
			{
				TreeBase component5 = ((Component)val6).GetComponent<TreeBase>();
				if ((Object)(object)component5 != (Object)null)
				{
					if (m_viking.m_axe.m_shared.m_toolTier >= component5.m_minToolTier && num6 < num3)
					{
						num3 = num6;
						val3 = component5;
					}
					continue;
				}
			}
			if (!((m_viking.m_fishingRod != null) & flag3))
			{
				continue;
			}
			Fish component6 = ((Component)val6).GetComponent<Fish>();
			if (!((Object)(object)component6 != (Object)null))
			{
				continue;
			}

            bool flag5 = false;
            foreach (Fish.BaitSetting bait in component6.m_baits)
            {
				if (((Humanoid)m_viking).GetInventory().HaveItem(bait.m_bait.m_itemData.m_shared.m_name, true))
				{
					flag5 = true;
					m_bait = ((Humanoid)m_viking).GetInventory().GetItem(bait.m_bait.m_itemData.m_shared.m_name, -1, false);
					break;
				}
			}
			if (flag5 && num6 < num5)
			{
				num5 = num6;
				val5 = component6;
			}
		}
		float num7 = Mathf.Min(new float[5] { num, num2, num3, num4, num5 });
		if (Math.Abs(num7 - num) < 1f && (Object)(object)val != (Object)null)
		{
			m_mineRock = val;
			hasWorkTarget = true;
			((Humanoid)m_viking).EquipItem(m_viking.m_pickaxe, true);
			NorsemenPlugin.LogDebug("[" + m_viking.GetName() + "] found an ore deposit: " + ((Object)m_mineRock).name);
		}
		else if (Math.Abs(num7 - num2) < 1f && (Object)(object)val2 != (Object)null)
		{
			m_mineRock5 = val2;
			hasWorkTarget = true;
			((Humanoid)m_viking).EquipItem(m_viking.m_pickaxe, true);
			NorsemenPlugin.LogDebug("[" + m_viking.GetName() + "] found an ore deposit: " + ((Object)m_mineRock5).name);
		}
		else if (Math.Abs(num7 - num4) < 1f && (Object)(object)val4 != (Object)null)
		{
			m_destructible = val4;
			hasWorkTarget = true;
			((Humanoid)m_viking).EquipItem(m_viking.m_pickaxe, true);
			NorsemenPlugin.LogDebug("[" + m_viking.GetName() + "] found an ore deposit: " + ((Object)m_destructible).name);
		}
		else if (Math.Abs(num7 - num3) < 1f && (Object)(object)val3 != (Object)null)
		{
			m_tree = val3;
			hasWorkTarget = true;
			((Humanoid)m_viking).EquipItem(m_viking.m_axe, true);
			NorsemenPlugin.LogDebug("[" + m_viking.GetName() + "] found a tree: " + ((Object)m_tree).name);
		}
		else if (Math.Abs(num7 - num5) < 1f && (Object)(object)val5 != (Object)null)
		{
			m_fish = val5;
			hasWorkTarget = true;
			((Humanoid)m_viking).EquipItem(m_viking.m_fishingRod, true);
			NorsemenPlugin.LogDebug("[" + m_viking.GetName() + "] found a fish: " + ((Object)m_fish).name);
		}
	}

    protected override void Awake()
    {
        base.Awake();
        m_vikingInterceptTime = UnityEngine.Random.Range(m_interceptTimeMin, m_interceptTimeMax);
        m_viking = ((Component)this).GetComponent<Viking>();
		bool flag = ((Character)m_viking).IsTamed();
		((BaseAI)this).m_aggravatable = ((BaseAI)this).m_aggravatable && !flag;
	}

	public override bool UpdateAI(float dt)
	{
		if (!UpdateBaseAI(dt))
		{
			return false;
		}
		bool flag = ((Character)m_viking).IsTamed();
		if (flag)
		{
			UpdateBehaviour(dt);
			UpdateAttachShip(dt);
			if (UpdateAttach())
			{
				return true;
			}
		}
		if (UpdateInventory(flag))
		{
			return true;
		}
		if (!flag && ((BaseAI)this).HuntPlayer())
		{
			SetAlerted(true);
		}
		UpdateTargets(m_viking, flag, dt, out var canHearTarget, out var canSeeTarget);
		if (!flag && UpdateDespawn(dt, canSeeTarget))
		{
			return true;
		}
		if (UpdateFlee(dt, flag, ((BaseAI)this).IsAlerted()))
		{
			return true;
		}
		if (UpdateAvoidFire(dt, flag))
		{
			return true;
		}
		if (UpdateNoMonsterArea(dt, flag))
		{
			return true;
		}
        bool flag2 = GetFollowTarget() != null;
        if (flag2 && UpdateFollow(dt))
		{
			return true;
		}
		if (UpdateHurt(dt, flag))
		{
			return true;
		}
        bool flag3 = GetStaticTarget() != null || GetTargetCreature() != null;
        if (UpdateConsume(dt, m_viking, flag3, flag))
		{
			return true;
		}
		bool flag4 = !flag || m_moveType != Movement.Guard;
		if (!flag3 & flag4)
		{
			FindWorkTargets(dt);
			if (hasWorkTarget && UpdateWork(dt, m_viking.m_pickaxe, m_viking.m_axe, m_viking.m_fishingRod, flag2))
			{
				return true;
			}
		}
		if (UpdateCircleTarget(dt, flag))
		{
			return true;
		}
        ItemDrop.ItemData val = (ItemDrop.ItemData)SelectBestAttackMethod.Invoke(
            this,
            new object[] { (Humanoid)m_viking, dt });
        bool flag5 = val != null;
		bool flag6 = val != null && Time.time - val.m_lastAttackTime > val.m_shared.m_aiAttackInterval;
        bool flag7 = m_character.GetTimeSinceLastAttack() >= base.m_minAttackInterval;
        bool doAttack = (val != null) & flag6 & flag7;
		UpdateChargeAttack(dt, flag5, flag3, doAttack, val);
		if (UpdateCirculate(dt, flag5, doAttack, flag))
		{
			return true;
		}
        Character targetCreature = GetTargetCreature();
        UpdateCrouch(dt, targetCreature, canSeeTarget);
        UpdateBlock(dt, targetCreature, canSeeTarget);
        if (UpdateAttack(dt, val, doAttack, canHearTarget, canSeeTarget, flag))
		{
			return true;
		}
		if (!flag2 && (!flag || m_moveType == Movement.Patrol))
		{
            IdleMovement(dt);
        }
		else
		{
            StopMoving();
        }
		if (!flag5 || !flag3)
		{
			((BaseAI)this).ChargeStop();
		}
		return true;
	}

	public bool UpdateWork(float dt, ItemDrop.ItemData pickaxe, ItemDrop.ItemData axe, ItemDrop.ItemData fishingRod, bool isFollowing)
	{
		if (m_viking.IsInUse())
		{
			return false;
		}
		if (((BaseAI)this).IsAlerted())
		{
			ResetWorkTargets();
			return false;
		}
		if (pickaxe == null && axe == null && fishingRod == null)
		{
			return false;
		}
		if (NorsemanConfigs.DoesWorkRequireFood(m_viking.prefabName) && m_viking.IsHungry() && ((Character)m_viking).IsTamed())
		{
			return false;
		}
		if (!(Time.time - m_lastWorkActionTime > m_workInterval))
		{
			((BaseAI)this).StopMoving();
			return true;
		}
		bool flag = NorsemanConfigs.CanVikingMine(m_viking.prefabName);
		if ((pickaxe != null) & flag)
		{
			if (UpdateMineRockMining(dt, pickaxe, isFollowing))
			{
				return true;
			}
			if (UpdateMineRock5Mining(dt, pickaxe, isFollowing))
			{
				return true;
			}
			if (UpdateDestructibleMining(dt, pickaxe, isFollowing))
			{
				return true;
			}
		}
		bool flag2 = NorsemanConfigs.CanVikingLumber(m_viking.prefabName);
		if (((axe != null) & flag2) && UpdateLogging(dt, axe, isFollowing))
		{
			return true;
		}
		bool flag3 = NorsemanConfigs.CanVikingFish(m_viking.prefabName);
		if (((fishingRod != null) & flag3) && UpdateFishing(dt, fishingRod, isFollowing))
		{
			return true;
		}
		return false;
	}

	public void AddResources(List<DropTable.DropData> drops)
	{
        //IL_004e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0053: Unknown result type (might be due to invalid IL or missing references)
        //IL_0054: Unknown result type (might be due to invalid IL or missing references)
        //IL_0082: Unknown result type (might be due to invalid IL or missing references)
        if (drops.Count <= 0)
        {
			return;
		}
		float num = Time.time - m_lastResourceGainedTime;
		if (!(num < NorsemanConfigs.GetWorkResourceInterval(m_viking.prefabName)))
		{
            DropData val = drops[UnityEngine.Random.Range(0, drops.Count)];
            string name = ((Object)val.m_item).name;
			m_nview.GetZDO().Set(VikingVars.lastWorkTargetResource, name);
			if (((Humanoid)m_viking).GetInventory().CanAddItem(val.m_item, 1))
			{
				((Humanoid)m_viking).GetInventory().AddItem(name, 1, 1, 0, 0L, "", false);
				m_lastResourceGainedTime = Time.time;
			}
		}
	}
}
