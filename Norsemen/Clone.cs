using System;
using UnityEngine;

namespace Norsemen;

public class Clone
{
	private GameObject Prefab;

	private readonly string PrefabName;

	private readonly string NewName;

	private bool Loaded;

	public event Action<GameObject> OnCreated;

	public Clone(string prefabName, string newName)
	{
		PrefabName = prefabName;
		NewName = newName;
		PrefabManager.Clones.Add(this);
	}

	internal void Create()
	{
		if (!Loaded)
		{
			GameObject prefab = Helpers.GetPrefab(PrefabName);
			if (prefab != null)
			{
				Prefab = UnityEngine.Object.Instantiate<GameObject>(prefab, CloneManager.GetRootTransform(), false);
				Prefab.name = NewName;
				PrefabManager.RegisterPrefab(Prefab);
				OnCreated?.Invoke(Prefab);
				CloneManager.clones[Prefab.name] = Prefab;
				Loaded = true;
			}
		}
	}
}
