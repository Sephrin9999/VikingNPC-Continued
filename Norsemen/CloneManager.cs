using System.Collections.Generic;
using UnityEngine;

namespace Norsemen;

public static class CloneManager
{
	public static GameObject root;

	internal static readonly Dictionary<string, GameObject> clones;

	internal static readonly Dictionary<string, GameObject> norsemen;

	static CloneManager()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		clones = new Dictionary<string, GameObject>();
		norsemen = new Dictionary<string, GameObject>();
		root = new GameObject("Norsemen_prefab_root");
		Object.DontDestroyOnLoad((Object)(object)root);
		root.SetActive(false);
	}

	public static Transform GetRootTransform()
	{
		return root.transform;
	}
}
