using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Norsemen;

public class Norseman
{
	public class ExtraRagdoll : MonoBehaviour
	{
		public GameObject m_elfEars;

		public Material[] m_elfEarMats;

		public void SetElfEars(bool isElf, Color color)
		{
            //IL_0021: Unknown result type (might be due to invalid IL or missing references)
            if (m_elfEars != null)
            {
				m_elfEars.SetActive(isElf);
				SetElfEarColor(color);
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
	}

	private static GameObject elfEars = AssetBundleManager.LoadAsset<GameObject>("norsemen_bundle", "ElvenEars");

	public static GameObject tombstone;

	public GameObject Prefab;

	public readonly string name;

	private bool Loaded;

	public Viking m_viking;

	public VikingAI m_ai;

    public readonly Character.Faction faction;

    public List<string> defaultItems;

	public readonly Heightmap.Biome biome;

	public readonly NorsemanConfigs configs;

	public readonly SpawnManager.SpawnInfo spawnInfo;

	public float baseHealth;

	public float baseArmor;

	private static GameObject ragdoll;

	public event Action<Norseman> OnCreated;

	public Norseman(Heightmap.Biome biome, string name, Faction faction)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		Prefab = null;
		m_viking = null;
		m_ai = null;
		defaultItems = new List<string>();
		baseHealth = 50f;
		baseArmor = 0f;
		this.biome = biome;
		this.name = name;
		this.faction = faction.faction;
		configs = new NorsemanConfigs(name);
		configs.biome = biome;
		spawnInfo = new SpawnManager.SpawnInfo(this)
		{
			m_spawnInterval = 1000f,
			m_spawnDistance = 50f,
			m_maxLevel = 3,
			m_minAltitude = 10f,
			m_spawnChance = 5f
		};
		PrefabManager.Norsemen.Add(this);
	}

	internal void Create()
	{
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Expected O, but got Unknown
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Expected O, but got Unknown
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Expected O, but got Unknown
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Expected O, but got Unknown
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		if (Loaded)
		{
			return;
		}
		GameObject playerPrefab = FejdStartup.instance.m_playerPrefab;
		Player component = playerPrefab.GetComponent<Player>();
        Prefab = UnityEngine.Object.Instantiate<GameObject>(
			playerPrefab,
			CloneManager.GetRootTransform(),
			false
		);
        spawnInfo.m_prefab = Prefab;
        Prefab.name = name;
		Prefab.Remove<Player>();
		Prefab.Remove<PlayerController>();
		Prefab.Remove<Talker>();
		Prefab.Remove<Skills>();
		Prefab.GetComponent<ZNetView>().m_persistent = true;
		m_viking = Prefab.AddComponent<Viking>();
		m_viking.CopyFieldsFrom<Viking, Player>(component);
        ((Character)m_viking).m_eye = Utils.FindChild(
			Prefab.transform,
			"EyePos",
			Utils.IterativeSearchType.DepthFirst
		);
        ((Character)m_viking).m_faction = faction;
		((Character)m_viking).m_health = 50f;
		if (defaultItems.Count > 0)
		{
			((Humanoid)m_viking).m_defaultItems = defaultItems.Select(Helpers.GetPrefab).ToArray();
		}
        if (tombstone != null)
        {
			m_viking.m_tombstone = tombstone;
		}
		if (ragdoll == null)
		{
			ragdoll = CreateRagdoll();
		}
		if (ragdoll != null)
		{
            ((Character)m_viking).m_deathEffects.m_effectPrefabs = new EffectList.EffectData[2]
            {
                new EffectList.EffectData
                {
					m_prefab = ragdoll
				},
                new EffectList.EffectData
                {
					m_prefab = Helpers.GetPrefab("vfx_player_death")
				}
			};
		}
		((Character)m_viking).m_name = "$enemy_norseman_rs";
		m_ai = Prefab.AddComponent<VikingAI>();
		((MonsterAI)m_ai).m_attackPlayerObjects = false;
		((BaseAI)m_ai).m_aggravatable = true;
		((BaseAI)m_ai).m_passiveAggresive = true;
		((BaseAI)m_ai).m_avoidWater = true;
        ((BaseAI)m_ai).m_alertedEffects.m_effectPrefabs = new EffectList.EffectData[1]
        {
            new EffectList.EffectData
            {
				m_prefab = Helpers.GetPrefab("sfx_dverger_vo_alerted")
			}
		};
        ((BaseAI)m_ai).m_idleSound.m_effectPrefabs = new EffectList.EffectData[1]
        {
            new EffectList.EffectData
            {
				m_prefab = Helpers.GetPrefab("sfx_dverger_vo_idle")
			}
		};
		((BaseAI)m_ai).m_idleSoundInterval = 20f;
		((BaseAI)m_ai).m_idleSoundChance = 0.5f;
        ((BaseAI)m_ai).m_pathAgentType = Pathfinding.AgentType.HumanoidAvoidWater;
        ((BaseAI)m_ai).m_moveMinAngle = 90f;
		((BaseAI)m_ai).m_smoothMovement = true;
		((BaseAI)m_ai).m_randomCircleInterval = 2f;
		((BaseAI)m_ai).m_randomMoveInterval = 15f;
		((BaseAI)m_ai).m_randomMoveRange = 20f;
		((BaseAI)m_ai).m_skipLavaTargets = true;
		((BaseAI)m_ai).m_avoidLava = true;
		((BaseAI)m_ai).m_avoidLavaFlee = true;
		((BaseAI)m_ai).m_avoidFire = true;
		((BaseAI)m_ai).m_fleeRange = 25f;
		((BaseAI)m_ai).m_fleeAngle = 45f;
		((BaseAI)m_ai).m_fleeInterval = 2f;
		((MonsterAI)m_ai).m_alertRange = 20f;
		((MonsterAI)m_ai).m_fleeIfHurtWhenTargetCantBeReached = true;
		((MonsterAI)m_ai).m_fleeUnreachableSinceAttacking = 30f;
		((MonsterAI)m_ai).m_fleeUnreachableSinceHurt = 30f;
		((MonsterAI)m_ai).m_fleeIfLowHealth = 0.2f;
		((MonsterAI)m_ai).m_fleeTimeSinceHurt = 20f;
		((MonsterAI)m_ai).m_fleeInLava = true;
		((MonsterAI)m_ai).m_circulateWhileCharging = true;
		((MonsterAI)m_ai).m_privateAreaTriggerTreshold = 4;
		((MonsterAI)m_ai).m_interceptTimeMax = 2f;
		((MonsterAI)m_ai).m_interceptTimeMin = 0f;
		((MonsterAI)m_ai).m_maxChaseDistance = 200f;
		((MonsterAI)m_ai).m_minAttackInterval = 1.5f;
		((MonsterAI)m_ai).m_circleTargetInterval = 8f;
		((MonsterAI)m_ai).m_circleTargetDuration = 6f;
		((MonsterAI)m_ai).m_circleTargetDistance = 8f;
		((MonsterAI)m_ai).m_consumeRange = 1f;
		((MonsterAI)m_ai).m_consumeSearchRange = 10f;
		((MonsterAI)m_ai).m_consumeSearchInterval = 10f;
        //((BaseAI)m_ai).SetPatrolPoint();
        if (elfEars != null)
		{
			VisEquipment component2 = Prefab.GetComponent<VisEquipment>();
			SkinnedMeshRenderer bodyModel = component2.m_bodyModel;
			GameObject gameObject = ((Component)elfEars.transform.Find("attach_skin")).gameObject;
            GameObject val = UnityEngine.Object.Instantiate<GameObject>(gameObject, ((Component)bodyModel).transform.parent, true);
            val.name = "elf_ears";
            Collider[] colliders = val.GetComponentsInChildren<Collider>();

            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }
            val.transform.localPosition = Vector3.zero;
			val.transform.localRotation = Quaternion.identity;
			List<Material> list = new List<Material>();
			SkinnedMeshRenderer[] componentsInChildren = val.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer val2 in componentsInChildren)
			{
				val2.rootBone = bodyModel.rootBone;
				val2.bones = bodyModel.bones;
				if (Utils.CustomStartsWith((val2).name, "007"))
				{
					Helpers.Add(list, ((Renderer)val2).materials);
				}
			}
			m_viking.m_elfEars = val;
			m_viking.m_elfEarMats = list.ToArray();
		}
		configs.Setup(this);
		OnCreated?.Invoke(this);
		CloneManager.norsemen[Prefab.name] = Prefab;
		PrefabManager.RegisterPrefab(Prefab);
		Loaded = true;
	}

	private static GameObject CreateRagdoll()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		GameObject prefab = Helpers.GetPrefab("Player_ragdoll");
		if (prefab == null)
		{
			return null;
		}
		GameObject val = UnityEngine.Object.Instantiate<GameObject>(prefab, CloneManager.root.transform, false);
		(val).name = "Norseman_ragdoll";
		Ragdoll val2 = default(Ragdoll);
		if (val.TryGetComponent<Ragdoll>(out val2))
		{
			val2.m_ttl = 8f;
			val2.m_removeEffect.m_effectPrefabs = new EffectList.EffectData[1]
			{
                new EffectList.EffectData
                {
					m_prefab = Helpers.GetPrefab("vfx_corpse_destruction_small")
				}
			};
			val2.m_float = true;
			val2.m_dropItems = false;
		}
		if (elfEars != null)
		{
			ExtraRagdoll extraRagdoll = val.AddComponent<ExtraRagdoll>();
			VisEquipment val3 = default(VisEquipment);
			if (val.TryGetComponent<VisEquipment>(out val3))
			{
				GameObject gameObject = ((Component)elfEars.transform.Find("attach_skin")).gameObject;
				GameObject val4 = UnityEngine.Object.Instantiate<GameObject>(gameObject);
				val4.name = "elf_ears";
                Collider[] colliders = val4.GetComponentsInChildren<Collider>();

                for (int i = 0; i < colliders.Length; i++)
                {
                    colliders[i].enabled = false;
                }
                SkinnedMeshRenderer bodyModel = val3.m_bodyModel;
				val4.transform.SetParent(((Component)bodyModel).transform.parent);
				val4.transform.localPosition = Vector3.zero;
				val4.transform.localRotation = Quaternion.identity;
				List<Material> list = new List<Material>();
				SkinnedMeshRenderer[] componentsInChildren = val4.GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach (SkinnedMeshRenderer val5 in componentsInChildren)
				{
					val5.rootBone = bodyModel.rootBone;
					val5.bones = bodyModel.bones;
					if (Utils.CustomStartsWith(val5.name, "007"))
					{
						Helpers.Add(list, ((Renderer)val5).materials);
					}
				}
				extraRagdoll.m_elfEars = val4;
				extraRagdoll.m_elfEarMats = list.ToArray();
			}
		}
		PrefabManager.RegisterPrefab(val);
		return val;
	}
}
