using System;
using HarmonyLib;

namespace Norsemen;

[HarmonyPatch(typeof(ZNet), "OnNewConnection")]
public static class RegisterAndCheckVersion
{
	private static void Prefix(ZNetPeer peer, ref ZNet __instance)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		NorsemenPlugin.LogDebug("Registering version RPC handler");
		peer.m_rpc.Register<ZPackage>("Norsemen_VersionCheck", (Action<ZRpc, ZPackage>)RpcHandlers.RPC_Norsemen_Version);
		NorsemenPlugin.LogDebug("Invoking version check");
		ZPackage val = new ZPackage();
        val.Write(NorsemenPlugin.ModVersion);
        peer.m_rpc.Invoke("Norsemen_VersionCheck", new object[1] { val });
	}
}
