using System.Collections.Generic;
using UnityEngine;

namespace Norsemen;

public class EffectListRef
{
	public class EffectDataRef
	{
		public readonly string prefabName;

		public int variant;

		public bool attach;

		public bool follow;

		public bool inheritParentRotation;

		public bool inheritParentScale;

		public bool multiplyParentVisualScale;

		public bool randomRotation;

		public bool scale;

		public string childTransform = string.Empty;

		private EffectList.EffectData _data;

		public EffectDataRef(string prefabName)
		{
			this.prefabName = prefabName;
		}

		public EffectList.EffectData ToEffectData()
		{
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Expected O, but got Unknown
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Expected O, but got Unknown
			if (_data != null)
			{
				return _data;
			}
			GameObject prefab = Helpers.GetPrefab(prefabName);
			if (prefab == null)
			{
				NorsemenPlugin.LogError("Effect Data Reference invalid: " + prefabName);
				return new EffectList.EffectData();
			}
			_data = new EffectList.EffectData
			{
				m_prefab = prefab,
				m_variant = variant,
				m_attach = attach,
				m_follow = follow,
				m_inheritParentRotation = inheritParentRotation,
				m_inheritParentScale = inheritParentScale,
				m_multiplyParentVisualScale = multiplyParentVisualScale,
				m_randomRotation = randomRotation,
				m_scale = scale,
				m_childTransform = childTransform
			};
			return _data;
		}
	}

	public List<EffectDataRef> dataRefs = new List<EffectDataRef>();

	private readonly List<EffectList.EffectData> data = new List<EffectList.EffectData>();

	private EffectList _effects;

	public EffectList Effects
	{
		get
		{
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Expected O, but got Unknown
			if (_effects != null)
			{
				return _effects;
			}
			foreach (EffectDataRef dataRef in dataRefs)
			{
				data.Add(dataRef.ToEffectData());
			}
			_effects = new EffectList();
			_effects.m_effectPrefabs = data.ToArray();
			return _effects;
		}
	}

	public GameObject[] Create(Vector3 basePos, Quaternion baseRot, Transform baseParent = null, float scale = 1f, int variant = -1)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Effects.Create(basePos, baseRot, baseParent, scale, variant);
	}

	public EffectListRef(params string[] effects)
	{
		Add(effects);
	}

	public EffectListRef(params EffectDataRef[] refs)
	{
		dataRefs.AddRange(refs);
	}

	public EffectListRef()
	{
	}

	public void Add(params string[] effects)
	{
		foreach (string prefabName in effects)
		{
			dataRefs.Add(new EffectDataRef(prefabName));
		}
	}

	public void Add(params EffectDataRef[] refs)
	{
		dataRefs.AddRange(refs);
	}
}
