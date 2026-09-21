using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace Norsemen;

[PublicAPI]
public static class AssetBundleManager
{
	private static readonly Dictionary<string, AssetBundle> CachedBundles = new Dictionary<string, AssetBundle>();

	public static T LoadAsset<T>(string assetBundle, string prefab) where T : Object
	{
		AssetBundle assetBundle2 = GetAssetBundle(assetBundle);
		return (assetBundle2 == null) ? default(T) : assetBundle2.LoadAsset<T>(prefab);
	}

	public static AssetBundle GetAssetBundle(string fileName)
	{
		if (CachedBundles.TryGetValue(fileName, out var value))
		{
			return value;
		}
		AssetBundle val = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault((AssetBundle b) => ((Object)b).name == fileName);
		if (val != null)
		{
			CachedBundles[fileName] = val;
			return val;
		}
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		string name = executingAssembly.GetManifestResourceNames().Single((string str) => str.EndsWith(fileName));
		using Stream stream = executingAssembly.GetManifestResourceStream(name);
		AssetBundle val2 = AssetBundle.LoadFromStream(stream);
		CachedBundles[fileName] = val2;
		return val2;
	}
}
